using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TradeArtifactAbilityModificator : AbstractGlobalSpecialModificator
{
    public int ArtifactsCapacity = 5;
    public float SlowTimeOnGainArtifact = 2f;
    public float HealAmountPerArtifactPrice = 0.01f;
    public TeamManager.Teams HealTeam = TeamManager.Teams.PLAYER;

    private List<AbstractModificator> _currentArtifacts = new();

    public override bool OnSpecialActivated()
    {
        AbstractModificator randomModificator = ModificatorsManager.Instance.PickRandomModificators(
            ModificatorTypes.NEGATIVE,
            0,
            ScoreManager.Instance.CurrentCombo,
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
            ScoreManager.Instance.ResetCombo();

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