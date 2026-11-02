using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterHoldableSpeicalModificator : AbstractGlobalSpecialModificator, IInvertableTeamModificator
{
    const int MAX_AUTO_CONVERTS_ITERATIONS = 50;

    public static CharacterHoldableSpeicalModificator Instance = null;

    public int ScoreCost = 25;
    public int StartScore = 100;
    public Holdable SpecialHoldableInstance;
    public List<AbstractEffect> TargetEffects = new();
    public List<AbstractEffect> InvertTargetEffects = new();
    public TeamManager.Teams TrackedTeam = TeamManager.Teams.PLAYER;
    public float MaxCharacterDistanceFromHoldable = 10f;
    public float MaxCharacterDistanceFromHoldableOnUnholded = 22.5f;
    public List<AbstractEffect> EffectsOnTooFarFromHoldable = new();
    public List<AbstractEffect> EffectsOnEnemyHoldingSpecialHoldable = new();
    public float SlowmoOnConvert = 1f;
    public Material ComboMaterialOnAbleToUse;

    private Holdable _currentSpecialHoldable = null;
    private List<CharacterComponentsManager> _convertedCharacters = new();
    private bool _invertTeam = false;
    private bool _anyConvertedCharacterIsAlive = true;
    public bool InvertTeam
    {
        get => _invertTeam;
        set
        {
            if (_invertTeam == value) return;
            _invertTeam = value;

            if (!DisabledModificator)
            {
                OnModificatorRemoved();
                OnModificatorAdded();
            }
        }
    }

    public Holdable CurrentSpecialHoldable
    {
        get => _currentSpecialHoldable;
    }

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        if (Instance != null) throw new UnityException("limit of 1 Instance of CharacterHoldableSpecialModificator");
        Instance = this;

        SpawnManager.Instance.KeepHoldableOnFinishLevel = false;
        ScoreManager.Instance.AddScore(StartScore);
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

        Instance = null;
    }

    public override void OnLevelGenerated()
    {
        base.OnLevelGenerated();

        GameOverManager.Instance.ExtraAllDeadGameOverConditions.Add(ExtraGameOverCondition);

        CharacterTeam targetCharacter = TeamManager.Instance.GetTeamDataByTeam(InvertTeam ? IInvertableTeamModificator.GetInvertedTeam(TrackedTeam) : TrackedTeam).GetTeamMembers().FirstOrDefault();
        if (InvertTeam && BossInitializer.Instance?.Boss != null)
        {
            targetCharacter = BossInitializer.Instance.Boss.CharacterTeam;
            BossInitializer.Instance.GibMinionsOnBossDeath = false;
        }
        if (targetCharacter != null)
        {
            _currentSpecialHoldable = targetCharacter.CharComponents.CharacterHolding.GiveNewHoldable(SpecialHoldableInstance);
            _currentSpecialHoldable.OnGiven += CurrentSpecialHoldable_OnGiven;
            _currentSpecialHoldable.OnThrown += CurrentSpecialHoldable_OnThrown;
            if (_currentSpecialHoldable.TryGetComponent(out CameraTrackObject track)) track.enabled = !InvertTeam;
        }
        _convertedCharacters = new() { targetCharacter.CharComponents };
    }

    protected override bool SpecialActivateCondition()
    {
        return base.SpecialActivateCondition() && GetAvaibleScore() >= ScoreCost;
    }

    private float GetAvaibleScore()
    {
        return ScoreManager.Instance.TradableScore + ScoreManager.Instance.CurrentCombo * ScoreManager.Instance.CurrentMultiplier;
    }


    public override bool OnSpecialActivated()
    {
        if (InvertTeam) return false;

        if (_currentSpecialHoldable != null && !_currentSpecialHoldable.IsDestroyed())
        {
            CharacterComponentsManager nearestEnemy = null;
            float nearestEnemyDistance = _currentSpecialHoldable.CurrentHolder == null ? MaxCharacterDistanceFromHoldableOnUnholded : MaxCharacterDistanceFromHoldable;
            foreach (Transform characterTransform in LayerManager.Instance.GetZLayerOfGameObject(_currentSpecialHoldable.gameObject).CharactersContainer)
            {
                float currentEnemyDistance = Vector2.Distance(characterTransform.position, _currentSpecialHoldable.transform.position);
                if (
                    currentEnemyDistance < nearestEnemyDistance &&
                    characterTransform.TryGetComponent(out AbstractCharacterComponent character) &&
                    !character.CharComponents.CharacterTeam.GetIsAllyToAnotherTeam(TrackedTeam) &&
                    !_convertedCharacters.Contains(character.CharComponents)
                    )
                {
                    nearestEnemy = character.CharComponents;
                    nearestEnemyDistance = currentEnemyDistance;
                }

            }

            if (nearestEnemy != null)
            {
                nearestEnemy.CharacterEffectsReceiver.ApplyEffect(InvertTeam ? InvertTargetEffects : TargetEffects, _currentSpecialHoldable);

                if (_currentSpecialHoldable.CurrentHolder == null)
                {
                    nearestEnemy.CharacterHolding.ForceGrab(_currentSpecialHoldable);
                }
                else if (_currentSpecialHoldable.CurrentHolder.CharComponents.CharacterTeam.Team != TrackedTeam)
                {
                    _currentSpecialHoldable.CurrentHolder.CharComponents.CharacterEffectsReceiver.ApplyEffect(EffectsOnEnemyHoldingSpecialHoldable, nearestEnemy);
                    nearestEnemy.CharacterHolding.ForceGrab(_currentSpecialHoldable);
                }

                _convertedCharacters.Add(nearestEnemy);

                if (!InvertTeam) TimeManager.Instance.TryTemporalSlowTime(SlowmoOnConvert);

                ScoreManager.Instance.ResetCombo(ScoreManager.ResetComboReasons.MODIFICATOR_FORCE_RESET);
                ScoreManager.Instance.TradableScore -= ScoreCost;

                return true;
            }
        }

        return false;
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
        if (e.CharComponents.UITrack != null && !InvertTeam)
        {
            e.CharComponents.UITrack.TrackIsDying = true;
        }
    }

    private void CurrentSpecialHoldable_OnThrown(object sender, Holdable.OnThrownEventArgs e)
    {
        if (e.Thrower.CharComponents.UITrack != null && !InvertTeam)
        {
            e.Thrower.CharComponents.UITrack.TrackIsDying = false;
        }
    }

    private void FixedUpdate()
    {
        _anyConvertedCharacterIsAlive = false;
        foreach (var character in _convertedCharacters)
        {
            if (!character.IsDestroyed() && !character.CharacterHealth.Died)
            {
                _anyConvertedCharacterIsAlive = true;
                break;
            }
        }

        if (!_anyConvertedCharacterIsAlive)
        {
            for (int i = 0; i < MAX_AUTO_CONVERTS_ITERATIONS; i++)
            {
                if (!TryActivateSpecial()) break;
            }
        }

        if (InvertTeam)
        {
            if (
                _currentSpecialHoldable.CurrentOrLastHolder == null ||
                _currentSpecialHoldable.CurrentOrLastHolder.IsDestroyed() ||
                _currentSpecialHoldable.CurrentOrLastHolder.CharComponents.CharacterEffectsReceiver.GetHasEffect<ILethalEffect>() ||
                _currentSpecialHoldable.CurrentHolder?.CharComponents.CharacterTeam.Team == TrackedTeam
                )
            {
                CharacterComponentsManager nearestEnemy = null;
                float nearestEnemyDistance = float.MaxValue;
                foreach (Transform characterTransform in LayerManager.Instance.GetZLayerOfGameObject(_currentSpecialHoldable.gameObject).CharactersContainer)
                {
                    float currentEnemyDistance = Vector2.Distance(characterTransform.position, _currentSpecialHoldable.transform.position);
                    if (
                        currentEnemyDistance < nearestEnemyDistance &&
                        characterTransform.TryGetComponent(out AbstractCharacterComponent character) &&
                        !character.CharComponents.CharacterTeam.GetIsAllyToAnotherTeam(TrackedTeam) &&
                        character.CharComponents != _currentSpecialHoldable.CurrentHolder?.CharComponents &&
                        !_convertedCharacters.Contains(character.CharComponents)
                        )
                    {
                        nearestEnemy = character.CharComponents;
                        nearestEnemyDistance = currentEnemyDistance;
                    }

                }

                if (nearestEnemy != null)
                {
                    nearestEnemy.CharacterEffectsReceiver.ApplyEffect(InvertTeam ? InvertTargetEffects : TargetEffects, _currentSpecialHoldable);

                    if (_currentSpecialHoldable.CurrentHolder?.CharComponents.CharacterTeam.Team == TrackedTeam)
                    {
                        _currentSpecialHoldable.CurrentHolder.CharComponents.CharacterEffectsReceiver.ApplyEffect(EffectsOnEnemyHoldingSpecialHoldable, nearestEnemy);
                    }

                    nearestEnemy.CharacterHolding.ForceGrab(_currentSpecialHoldable);

                    _convertedCharacters.Add(nearestEnemy);

                    TimeManager.Instance.TryTemporalSlowTime(SlowmoOnConvert);
                }

                if (BossInitializer.Instance != null) BossInitializer.Instance.Boss = nearestEnemy;
            }
        }

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
            if (GetAvaibleScore() >= ScoreCost)
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
            (GetAvaibleScore() < ScoreCost && !_anyConvertedCharacterIsAlive) ||
            _currentSpecialHoldable == null ||
            _currentSpecialHoldable.IsDestroyed();
    }
}