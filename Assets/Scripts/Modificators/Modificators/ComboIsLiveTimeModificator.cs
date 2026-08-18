using System.Collections.Generic;
using Unity.Mathematics;

public class ComboIsLiveTimeModificator : AbstractModificator, IInvertableTeamModificator
{
    public TeamManager.Teams AffectTeam = TeamManager.Teams.PLAYER;
    public float ResetScoreEncountSpeedMultiplierMultiplier = 1f;
    public float ComboMultiplierOnReset = 0.5f;
    public int MinCombo = -10;
    public List<AbstractEffect> PlayerCharacterKillEffectsOnReachMinCombo = new();

    private int _oldMinComboValue;
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

        _oldMinComboValue = ScoreManager.Instance.MinCombo;
        ScoreManager.Instance.MinCombo = math.min(ScoreManager.Instance.MinCombo, MinCombo);

        ScoreManager.Instance.ResetScoreEncountSpeedMultiplier *= ResetScoreEncountSpeedMultiplierMultiplier;
        ScoreManager.Instance.OverrideResetComboEvent = OverrideScoreEncounterResetComboEvent;
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorRemoved();

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScoreEncountSpeedMultiplier /= ResetScoreEncountSpeedMultiplierMultiplier;
            ScoreManager.Instance.OverrideResetComboEvent = null;
            ScoreManager.Instance.MinCombo = _oldMinComboValue;
        }
    }

    public void OverrideScoreEncounterResetComboEvent(ScoreManager scoreManager)
    {
        if (scoreManager.CurrentCombo > 0)
        {
            scoreManager.CurrentCombo = (int)math.floor(scoreManager.CurrentCombo * ComboMultiplierOnReset);
        }
        else
        {
            scoreManager.CurrentCombo--;
        }

        if (scoreManager.CurrentCombo == scoreManager.MinCombo)
        {
            foreach (
                AbstractCharacterComponent playerCharacter in 
                TeamManager.Instance.GetTeamDataByTeam(InvertTeam ? IInvertableTeamModificator.GetInvertedTeam(AffectTeam) : AffectTeam).GetTeamMembers()
                )
            {
                playerCharacter.CharComponents.CharacterEffectsReceiver.ApplyEffect(PlayerCharacterKillEffectsOnReachMinCombo, null, 1f, true);
            }
        }
        else
        {
            scoreManager.RestoreComboLastTime();
        }
    }
}