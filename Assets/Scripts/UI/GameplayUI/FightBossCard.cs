
public class FightBossCard : PickNothingCard
{
    public override void Pick()
    {
        base.Pick();

        BossInitializer.Instance?.Fight();
    }
}