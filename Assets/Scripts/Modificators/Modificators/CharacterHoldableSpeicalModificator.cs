using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterHoldableSpeicalModificator : AbstractGlobalSpecialModificator
{
    public int ExtraCostPerConverted = 1;
    public int RequiredComboToReduceCost = 5;
    public Holdable SpecialHoldableInstance;
    public List<AbstractEffect> TargetEffects = new();
    public TeamManager.Teams TrackedTeam = TeamManager.Teams.PLAYER;
    public float MaxCharacterDistanceFromHoldable = 10f;
    public List<AbstractEffect> EffectsOnTooFarFromHoldable = new();
    public float SlowmoOnConvert = 1f;
    public Material ComboMaterialOnAbleToUse;

    private Holdable _currentSpecialHoldable = null;
    private List<CharacterComponentsManager> _convertedCharacters = new();
    private int _currentExtraCost = 0;
    private int _killsToReducePriceLeft = 0;
    private Material _oldComboMaterial = null;

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        SpawnManager.Instance.KeepHoldableOnFinishLevel = false;
        ScoreManager.Instance.OnAddedCombo += Instance_OnAddedCombo;
    }

    private void Instance_OnAddedCombo(object sender, System.EventArgs e)
    {
        _killsToReducePriceLeft--;
        if (_killsToReducePriceLeft <= 0)
        {
            _currentExtraCost = math.max(_currentExtraCost - 1, 0);
            _killsToReducePriceLeft = RequiredComboToReduceCost;
        }
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorRemoved();

        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.ExtraAllDeadGameOverConditions.Remove(ExtraGameOverCondition);
        }

        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.KeepHoldableOnFinishLevel = true;
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnAddedCombo += Instance_OnAddedCombo;
        }
    }

    public override void OnLevelGenerated()
    {
        base.OnLevelGenerated();

        GameOverManager.Instance.ExtraAllDeadGameOverConditions.Add(ExtraGameOverCondition);

        CharacterTeam spawnedPlayer = TeamManager.Instance.GetTeamDataByTeam(TrackedTeam).GetTeamMembers().FirstOrDefault();
        if (spawnedPlayer != null)
        {
            _currentSpecialHoldable = spawnedPlayer.CharComponents.CharacterHolding.GiveNewHoldable(SpecialHoldableInstance);
            _currentSpecialHoldable.OnGiven += CurrentSpecialHoldable_OnGiven;
            _currentSpecialHoldable.OnThrown += CurrentSpecialHoldable_OnThrown;
        }
        _convertedCharacters = new() { spawnedPlayer.CharComponents };

        _currentExtraCost = 0;
        _killsToReducePriceLeft = RequiredComboToReduceCost;
    }

    public override bool OnSpecialActivated()
    {
        if (_currentSpecialHoldable != null && !_currentSpecialHoldable.IsDestroyed())
        {
            CharacterComponentsManager nearestEnemy = null;
            float nearestEnemyDistance = MaxCharacterDistanceFromHoldable;
            foreach (Transform characterTransform in LayerManager.Instance.GetZLayerOfGameObject(_currentSpecialHoldable.gameObject).CharactersContainer)
            {
                float currentEnemyDistance = Vector2.Distance(characterTransform.position, _currentSpecialHoldable.transform.position);
                if (
                    currentEnemyDistance < nearestEnemyDistance &&
                    characterTransform.TryGetComponent(out AbstractCharacterComponent character) &&
                    !character.CharComponents.CharacterTeam.GetIsAllyToAnotherTeam(TrackedTeam) &&
                    !_convertedCharacters.Contains(character.CharComponents) &&
                    character.CharComponents.CharacterEffectsReceiver.GetHasEffect<ILethalEffect>()
                    )
                {
                    nearestEnemy = character.CharComponents;
                    nearestEnemyDistance = currentEnemyDistance;
                }

            }
            
            if (nearestEnemy != null )
            {
                nearestEnemy.CharacterEffectsReceiver.ApplyEffect(TargetEffects, _currentSpecialHoldable);

                if (_currentSpecialHoldable.CurrentHolder == null)
                {
                    nearestEnemy.CharacterHolding.ForceGrab(_currentSpecialHoldable);
                }

                _convertedCharacters.Add(nearestEnemy);

                TimeManager.Instance.TryTemporalSlowTime(SlowmoOnConvert);

                _currentExtraCost += ExtraCostPerConverted;

                return true;
            }
        }

        return false;
    }

    public override int GetTotalComboCost()
    {
        return base.GetTotalComboCost() + _currentExtraCost;
    }

    public override void OnLevelFinished()
    {
        base.OnLevelFinished();

        if (_currentSpecialHoldable != null && !_currentSpecialHoldable.IsDestroyed())
        {
            _currentSpecialHoldable.OnGiven += CurrentSpecialHoldable_OnGiven;
            _currentSpecialHoldable.OnThrown += CurrentSpecialHoldable_OnThrown;
        }
    }

    private void CurrentSpecialHoldable_OnGiven(object sender, CharacterHoldingObjects e)
    {
        if (e.CharComponents.UITrack != null)
        {
            e.CharComponents.UITrack.TrackIsDying = true;
        }
    }

    private void CurrentSpecialHoldable_OnThrown(object sender, Holdable.OnThrownEventArgs e)
    {
        if (e.Thrower.CharComponents.UITrack != null)
        {
            e.Thrower.CharComponents.UITrack.TrackIsDying = false;
        }
    }

    private void FixedUpdate()
    {
        if (
            SceneList.GetCurrentSceneIsGameplay() && 
            _currentSpecialHoldable != null && 
            !_currentSpecialHoldable.IsDestroyed() &&
            _convertedCharacters.Count > 1
            )
        {
            //gib all players if lost sword
            if (
                _currentSpecialHoldable == null ||
                _currentSpecialHoldable.IsDestroyed()
                )
            {
                for (int i = 0; i < _convertedCharacters.Count; i++)
                {
                    if (_convertedCharacters[i] != null && !_convertedCharacters[i].IsDestroyed())
                    {
                        _convertedCharacters[i].CharacterEffectsReceiver.ApplyEffect(EffectsOnTooFarFromHoldable, _currentSpecialHoldable);
                    }
                    _convertedCharacters.RemoveAt(i);
                    i--;
                }
            }
            //gib players if too far from sword
            else if (_convertedCharacters.Count > 1)
            {
                ZIndexLayer specialHoldableLayer = LayerManager.Instance.GetZLayerOfGameObject(_currentSpecialHoldable.gameObject);

                for (int i = 0; i < _convertedCharacters.Count; i++) 
                {
                    if (_convertedCharacters[i] == null || _convertedCharacters[i].IsDestroyed())
                    {
                        _convertedCharacters.RemoveAt(i);
                        i--;
                    }
                    else if (
                        _currentSpecialHoldable.CurrentHolder != _convertedCharacters[i].CharacterHolding &&
                        (
                            _convertedCharacters[i].CharacterCollision.CurrentZLayer != specialHoldableLayer ||
                            Vector2.Distance(
                                _convertedCharacters[i].transform.position, 
                                _currentSpecialHoldable.transform.position
                                ) > MaxCharacterDistanceFromHoldable
                        )
                        )
                    {
                        _convertedCharacters[i].CharacterEffectsReceiver.ApplyEffect(EffectsOnTooFarFromHoldable, _currentSpecialHoldable);
                        _convertedCharacters.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        ComboEncounter combo = UIManager.Instance?.GameplayScreenOverlay.GetGameplayUI()?.Combo;
        if (combo != null)
        {
            if (ScoreManager.Instance.CurrentCombo >= GetTotalComboCost())
            {
                combo.OverrideBgMaterial = ComboMaterialOnAbleToUse;
            }
            else
            {
                combo.OverrideBgMaterial = null;
            }
        }
    }

    private bool ExtraGameOverCondition()
    {
        return 
            ScoreManager.Instance.CurrentCombo < GetTotalComboCost() || 
            _currentSpecialHoldable == null || 
            _currentSpecialHoldable.IsDestroyed();
    }
}