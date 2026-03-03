using System;

public class ComboRegen : AbstractCharacterEffect, IEntireCharacterEffect, ITriggerableEffect
{
    public float HealOnAddCombo = 0.5f;
    public float HealOnResetCombo = -0.2f;

    public event EventHandler OnTriggered;

    protected override void OnApply()
    {
        base.OnApply();

        ScoreManager.Instance.OnAddedCombo += Instance_OnAddedCombo;
        ScoreManager.Instance.OnResetCombo += Instance_OnResetCombo;
    }

    private void Instance_OnAddedCombo(object sender, System.EventArgs e)
    {
        AffectedCharacter.CharacterHealth.ApplyDamage(-HealOnAddCombo, null);
        OnTriggered?.Invoke(this, new EventArgs());
    }

    private void Instance_OnResetCombo(object sender, System.EventArgs e)
    {
        AffectedCharacter.CharacterHealth.ApplyDamage(-HealOnResetCombo * ScoreManager.Instance.CurrentCombo, AffectedCharacter.CharacterAttacking);
        OnTriggered?.Invoke(this, new EventArgs());
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnAddedCombo -= Instance_OnAddedCombo;
            ScoreManager.Instance.OnResetCombo -= Instance_OnResetCombo;
        }
    }
}
