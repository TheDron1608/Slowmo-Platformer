using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class ComboDependedRegenMultiplier : AbstractCharacterEffect, IEntireCharacterEffect
{
    public float RegenMultPerCombo = 5f;
    public float RegenReductionPerSecond = 1f;
    public Color ExtraInfoColorRegen = Color.lightGreen;
    public Color ExtraInfoColorNegativeRegen = Color.darkRed;

    private float _currentRegenMult = 1f;

    protected override void OnApply()
    {
        base.OnApply();

        UpdateRegenMult(1f + ScoreManager.Instance.LastCombo * RegenMultPerCombo);
        ScoreManager.Instance.OnResetCombo += ScoreManager_OnResetCombo;
    }

    private void FixedUpdate()
    {
        UpdateRegenMult(_currentRegenMult - RegenReductionPerSecond * Time.fixedDeltaTime);
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        ScoreManager.Instance.OnResetCombo -= ScoreManager_OnResetCombo;

        TextMeshProUGUI comboExtraInfo = UIManager.Instance?.GameplayScreenOverlay.GetGameplayUI()?.Combo?.ExtraInfo;
        if (comboExtraInfo != null)
        {
            comboExtraInfo.text = "";
        }
    }

    private void ScoreManager_OnResetCombo(object sender, System.EventArgs e)
    {
        UpdateRegenMult(math.min(_currentRegenMult, 1f) + ScoreManager.Instance.LastCombo * RegenMultPerCombo);
    }

    private void UpdateRegenMult(float newRegenMult)
    {
        AffectedCharacter.CharacterHealth.HealMultiplier = AffectedCharacter.CharacterHealth.HealMultiplier / _currentRegenMult * newRegenMult;
        _currentRegenMult = newRegenMult;

        TextMeshProUGUI comboExtraInfo = UIManager.Instance?.GameplayScreenOverlay.GetGameplayUI()?.Combo?.ExtraInfo;
        if (comboExtraInfo != null)
        {
            comboExtraInfo.text = $"regen\nx{math.round(newRegenMult * 100f)}%";
            comboExtraInfo.color = newRegenMult >= 1f ? ExtraInfoColorRegen : ExtraInfoColorNegativeRegen;
        }
    }

    public override bool Equals(AbstractEffect other)
    {
        return base.Equals(other) && RegenMultPerCombo == (other as ComboDependedRegenMultiplier).RegenMultPerCombo;
    }
}
