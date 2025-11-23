/// <summary>
/// removes all effects of same type when applied
/// </summary>
public abstract class AbstractOverwritingEffect : AbstractEffect
{
    protected override void OnApply()
    {
        base.OnApply();
        AffectedObject.RemoveEffect(this);
    }
}
