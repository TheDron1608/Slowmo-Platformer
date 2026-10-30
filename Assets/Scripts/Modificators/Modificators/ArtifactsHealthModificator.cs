using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class ArtifactsHealthModificator : AbstractCharactersModificator, IInvertableTeamModificator
{
    const int MAX_ADD_MODS_ATTERMPS = 30;

    public int ArtifactsCapacity = 5;
    public int MinArtifactPrice = 50;
    public float ArtifactPricePerCombo = 1.5f;
    public float SlowTimeOnGainArtifact = 2f;
    public float HealthPerArtifactPrice = 0.01f;
    public TeamManager.Teams HealTeam = TeamManager.Teams.PLAYER;
    public float StartArtifactPriceIfIsCharacterModificator = 100f;
    public float StartArtifactPriceIfInverted = 250f;

    private List<AbstractModificator> _currentArtifacts = new();
    private float? _lastCharacterHealth = null;

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        if (Status == ModificatorStatuses.CHARACTER_DEFAULT)
        {
            ShuffleNewArtifacts(StartArtifactPriceIfIsCharacterModificator);
        }

        if (InvertTeam)
        {
            ShuffleNewArtifacts(StartArtifactPriceIfInverted);
        }
    }

    protected override void OnCharacterAffected(CharacterComponentsManager character)
    {
        character.CharacterEffectsReceiver.OnEffectAdded += CharacterEffectsReceiver_OnEffectAdded;
    }

    protected override void OnCharacterRemovedAffect(CharacterComponentsManager character)
    {
        character.CharacterEffectsReceiver.OnEffectAdded -= CharacterEffectsReceiver_OnEffectAdded;
    }

    public override void OnLevelGenerated()
    {
        base.OnLevelGenerated();

        if (_lastCharacterHealth != null && _lastCharacterHealth > 0f)
        {
            foreach (CharacterTeam character in TeamManager.Instance.GetTeamDataByTeam(InvertTeam ? IInvertableTeamModificator.GetInvertedTeam(Team) : Team).GetTeamMembers())
            {
                character.CharComponents.CharacterHealth.ApplyMaxHealth(_lastCharacterHealth.Value, null, true);
            }
        }
    }

    public override void OnLevelFinished()
    {
        base.OnLevelFinished();

        float characterMaxHealth = 0;
        foreach (CharacterTeam character in TeamManager.Instance.GetTeamDataByTeam(InvertTeam ? IInvertableTeamModificator.GetInvertedTeam(Team) : Team).GetTeamMembers())
        {
            if (characterMaxHealth < character.CharComponents.CharacterHealth.CurrentHealth) characterMaxHealth = character.CharComponents.CharacterHealth.CurrentHealth;
        }
        _lastCharacterHealth = characterMaxHealth > 0f ? characterMaxHealth : null;
    }

    private void CharacterEffectsReceiver_OnEffectAdded(object sender, ObjectEffectsReceiver.EffectAddedEventArgs e)
    {
        if (e.Effect is ILethalEffect leathalE && e.Effect is ICharacterEffect characterE)
        {
            ShuffleNewArtifacts(ScoreManager.Instance.CurrentCombo * ScoreManager.Instance.CurrentMultiplier * ArtifactPricePerCombo);
            ScoreManager.Instance.ResetCombo(ScoreManager.ResetComboReasons.MODIFICATOR_FORCE_RESET);

            float totalModsPrice = 0f;
            foreach (AbstractModificator artifact in _currentArtifacts)
            {
                if (artifact.Type != ModificatorTypes.NEUTRAL)
                {
                    totalModsPrice += artifact.ModificatorPrice;
                }
            }

            characterE.AffectedCharacter.CharacterHealth.ApplyMaxHealth(totalModsPrice * HealthPerArtifactPrice + characterE.AffectedCharacter.CharacterHealth.CurrentHealth, null, true);
            if (characterE.AffectedCharacter.CharacterHealth.CurrentHealth > 0f)
            {
                characterE.AffectedCharacter.CharacterHealth.Ressurect();
            }
        }
    }

    public void ShuffleNewArtifacts(float price)
    {
        while (_currentArtifacts.Count > 0)
        {
            ModificatorsManager.Instance.RemoveModificator(_currentArtifacts[0]);
            _currentArtifacts.RemoveAt(0);
        }

        float addedModsPrice = 0f;
        int totalAddedMods = 0;
        for (int failedAttempts = 0; failedAttempts < MAX_ADD_MODS_ATTERMPS; failedAttempts++)
        {
            float targetPrice = price - addedModsPrice;
            List<AbstractModificator> randomMods = ModificatorsManager.Instance.PickRandomModificators(
                ModificatorTypes.NEGATIVE,
                0,
                targetPrice > MinArtifactPrice ? math.max(targetPrice / ArtifactsCapacity, MinArtifactPrice) : targetPrice,
                false,
                false,
                true,
                _currentArtifacts,
                false,
                0f,
                null,
                true
                );

            if (randomMods.Count > 0) failedAttempts = 0;

            foreach (AbstractModificator randomMod in randomMods)
            {
                if (randomMod.Type != ModificatorTypes.NEUTRAL)
                {
                    addedModsPrice += randomMod.ModificatorPrice;
                    totalAddedMods++;
                }
                _currentArtifacts.Add(ModificatorsManager.Instance.AddModificator(randomMod, ModificatorStatuses.ARTIFACT));
            }

            if (totalAddedMods >= ArtifactsCapacity || addedModsPrice >= price) break;
        }

        TimeManager.Instance.TryTemporalSlowTime(SlowTimeOnGainArtifact);
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorRemoved();

        while (_currentArtifacts.Count > 0)
        {
            ModificatorsManager.Instance.RemoveModificator(_currentArtifacts[0]);
            _currentArtifacts.RemoveAt(0);
        }
    }
}