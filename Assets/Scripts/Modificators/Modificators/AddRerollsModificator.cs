
public class AddRerollsModificator : AbstractModificator, IMultiplierableEffect
{
    public int AddBlessPickRerolls = 0;
    public int AddCursePickRerolls = 0;
    public int AddDifficultyCursePickRerolls = 0;

    private float _effectMultiplier = 1f;

    public float EffectMultiplier
    {
        get => _effectMultiplier;
        set => _effectMultiplier = value;
    }

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        ModificatorsManager.Instance.BlessPickRerolls += (int)(AddBlessPickRerolls * EffectMultiplier);
        ModificatorsManager.Instance.CursePickRerolls += (int)(AddCursePickRerolls * EffectMultiplier);
        ModificatorsManager.Instance.DifficultyCursePickRerolls += (int)(AddDifficultyCursePickRerolls * EffectMultiplier);
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorRemoved();

        if (ModificatorsManager.Instance != null)
        {
            ModificatorsManager.Instance.BlessPickRerolls -= (int)(AddBlessPickRerolls * EffectMultiplier);
            ModificatorsManager.Instance.CursePickRerolls -= (int)(AddCursePickRerolls * EffectMultiplier);
            ModificatorsManager.Instance.DifficultyCursePickRerolls -= (int)(AddDifficultyCursePickRerolls * EffectMultiplier);
        }
    }
}