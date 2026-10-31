using System.Collections.Generic;
using Unity.Mathematics;

public class ComboIsLiveTimeModificator : AbstractModificator, IInvertableTeamModificator
{
    const float COMBO_ENCOUNT_FINISH_VOLUME_MULT = 3f;
    const int DAMAGED_SCREEN_OVERLAY_MAX_COMBO = 8;

    public TeamManager.Teams AffectTeam = TeamManager.Teams.PLAYER;
    public float ResetScoreEncountSpeedMultiplierMultiplier = 1f;
    public float ComboMultiplierOnReset = 0.5f;
    public int MinCombo = -10;
    public int StartCombo = 10;
    public int SetComboInInvertedTeam = 0;
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

        if (InvertTeam)
        {
            ScoreManager.Instance.CurrentCombo = math.max(ScoreManager.Instance.CurrentCombo, SetComboInInvertedTeam);
        }
        else
        {
            ScoreManager.Instance.CurrentCombo += StartCombo;
        }

        if (GameplayUIManager.GetInstance()?.Combo != null)
        {
            GameplayUIManager.GetInstance().Combo.InstantComboEncount = true;
            GameplayUIManager.GetInstance().Combo.EcnountFinishVolumeMult = COMBO_ENCOUNT_FINISH_VOLUME_MULT;
        }

        ModificatorsManager.Instance.ResetComboOnCursePick = false;
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

        if (GameplayUIManager.GetInstance()?.Combo != null)
        {
            GameplayUIManager.GetInstance().Combo.InstantComboEncount = false;
            GameplayUIManager.GetInstance().Combo.EcnountFinishVolumeMult = 1.0f;
        }

        if (ModificatorsManager.Instance != null)
        {
            ModificatorsManager.Instance.ResetComboOnCursePick = true;
        }
    }

    public override void OnLevelGenerated()
    {
        base.OnLevelGenerated();

        if (GameplayUIManager.GetInstance()?.Combo != null)
        {
            GameplayUIManager.GetInstance().Combo.InstantComboEncount = true;
            GameplayUIManager.GetInstance().Combo.EcnountFinishVolumeMult = COMBO_ENCOUNT_FINISH_VOLUME_MULT;
        }
    }

    public bool OverrideScoreEncounterResetComboEvent(ScoreManager scoreManager, ScoreManager.ResetComboReasons reason)
    {
        if (InvertTeam && reason == ScoreManager.ResetComboReasons.TIME_OUT) return false;

        if (scoreManager.CurrentCombo > 0)
        {
            scoreManager.CurrentCombo = (int)math.floor(scoreManager.CurrentCombo * ComboMultiplierOnReset);
        }
        else
        {
            scoreManager.CurrentCombo--;
        }

        if (scoreManager.CurrentCombo == scoreManager.MinCombo && !InvertTeam)
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

        return true;
    }

    private void FixedUpdate()
    {
        if (UIManager.Instance.DamagedScreenOverlay.IsShown() && ScoreManager.Instance != null)
        {
            UIManager.Instance.DamagedScreenOverlay.FillAmount = 1f - NumberMath.LimitFloatBetweenZeroAndOne((float)ScoreManager.Instance.CurrentCombo / COMBO_ENCOUNT_FINISH_VOLUME_MULT);
        }
    }
}