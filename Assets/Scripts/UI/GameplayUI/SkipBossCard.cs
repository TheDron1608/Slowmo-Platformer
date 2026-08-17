
public class SkipBossCard : PickNothingCard
{
    public override void Pick()
    {
        base.Pick();

        BossInitializer.Instance?.SkipFight();
    }
}