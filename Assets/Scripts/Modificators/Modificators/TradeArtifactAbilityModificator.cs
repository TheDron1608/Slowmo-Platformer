using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TradeArtifactAbilityModificator : AbstractGlobalSpecialModificator
{
    public int ArtifactsCapacity = 5;
    public float ArtifactPricePerCombo = 1.5f;
    public float SlowTimeOnGainArtifact = 2f;
    public float HealAmountPerArtifactPrice = 0.01f;
    public TeamManager.Teams HealTeam = TeamManager.Teams.PLAYER;
    public float StartArtifactPriceIfIsCharacterModificator = 100f;

    private List<AbstractModificator> _currentArtifacts = new();

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        if (Status == ModificatorStatuses.CHARACTER_DEFAULT)
        {
            TryAddArtifact(StartArtifactPriceIfIsCharacterModificator);
        }
    }

    public override bool OnSpecialActivated()
    {
        if (TryAddArtifact(ScoreManager.Instance.CurrentCombo * ScoreManager.Instance.CurrentMultiplier * ArtifactPricePerCombo))
        {
            ScoreManager.Instance.ResetCombo();
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

    private void FixedUpdate()
    {
        float charactersHealAmountThisFrame = 
            _currentArtifacts.Sum(e => e.ModificatorPrice * (1f - (e.GetSpoilProgress() ?? 0f))) * -HealAmountPerArtifactPrice * Time.fixedDeltaTime;

        if (TeamManager.Instance != null)
        {
            foreach (CharacterTeam playerTeamMember in TeamManager.Instance.GetTeamDataByTeam(HealTeam).GetTeamMembers())
            {
                playerTeamMember.CharComponents.CharacterHealth.ApplyDamage(charactersHealAmountThisFrame, null);
            }
        }
    }
}