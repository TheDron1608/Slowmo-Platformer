using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TradeArtifactAbilityModificator : AbstractGlobalSpecialModificator, IInvertableTeamModificator
{
    const int MAX_ADD_MODS_ATTERMPS = 30;

    public int ArtifactsCapacity = 5;
    public float ArtifactPricePerCombo = 1.5f;
    public float SlowTimeOnGainArtifact = 2f;
    public float HealAmountPerArtifactPrice = 0.01f;
    public TeamManager.Teams HealTeam = TeamManager.Teams.PLAYER;
    public float StartArtifactPriceIfIsCharacterModificator = 100f;
    public float StartArtifactPriceIfInverted = 250f;
    public int StartArtifactsIfInverted = 3;

    private List<AbstractModificator> _currentArtifacts = new();
    private bool _invertTeam = false;
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

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        if (Status == ModificatorStatuses.CHARACTER_DEFAULT)
        {
            TryAddArtifact(StartArtifactPriceIfIsCharacterModificator);
        }

        if (InvertTeam)
        {
            for (int i = 0; i < MAX_ADD_MODS_ATTERMPS; i++)
            {
                TryAddArtifact(StartArtifactPriceIfInverted);
                if (_currentArtifacts.Count >= StartArtifactsIfInverted) break;
            }
        }
    }

    public override bool OnSpecialActivated()
    {
        if (TryAddArtifact(ScoreManager.Instance.CurrentCombo * ScoreManager.Instance.CurrentMultiplier * ArtifactPricePerCombo))
        {
            ScoreManager.Instance.ResetCombo(ScoreManager.ResetComboReasons.USED_ABILITY);
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool TryAddArtifact(float price)
    {
        AbstractModificator randomModificator = ModificatorsManager.Instance.PickRandomModificators(
            ModificatorTypes.NEGATIVE,
            0,
            price,
            false,
            false,
            false,
            _currentArtifacts,
            true
            ).FirstOrDefault();

        if (randomModificator != null)
        {
            TimeManager.Instance.TryTemporalSlowTime(SlowTimeOnGainArtifact);

            _currentArtifacts.Add(ModificatorsManager.Instance.AddModificator(randomModificator, ModificatorStatuses.ARTIFACT));

            if (_currentArtifacts.Count > 5)
            {
                ModificatorsManager.Instance.RemoveModificator(_currentArtifacts[0]);
                _currentArtifacts.RemoveAt(0);
            }

            return true;
        }

        return false;
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

    private void FixedUpdate()
    {
        float charactersHealAmountThisFrame = 
            _currentArtifacts.Sum(e => e.ModificatorPrice * (1f - (e.GetSpoilProgress() ?? 0f))) * -HealAmountPerArtifactPrice * Time.fixedDeltaTime;

        if (TeamManager.Instance != null)
        {
            foreach (
                CharacterTeam playerTeamMember in 
                TeamManager.Instance.GetTeamDataByTeam(InvertTeam ? IInvertableTeamModificator.GetInvertedTeam(HealTeam) : HealTeam).GetTeamMembers()
                )
            {
                playerTeamMember.CharComponents.CharacterHealth.ApplyDamage(charactersHealAmountThisFrame, null);
            }
        }
    }
}