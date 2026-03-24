using UnityEngine;

public class MultiplyGlobalRandomChanceModificator : AbstractModificator
{
    public float RandomChanceProcMultiplier = 1f;
    public float GoodRandomChanceProcMultiplier = 1f;
    public float BadRandomChanceProcMultiplier = 1f;

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        RandomManager.Instance.RandomChanceProcMultiplier *= RandomChanceProcMultiplier;
        RandomManager.Instance.GoodRandomChanceProcMultiplier *= GoodRandomChanceProcMultiplier;
        RandomManager.Instance.BadRandomChanceProcMultiplier *= BadRandomChanceProcMultiplier;

        if (RandomChanceProcMultiplier != 1f || BadRandomChanceProcMultiplier != 1f)
        {
            RandomManager.Instance.OnBadRandomChanceProcd += Instance_OnRandomChanceProcd;
        }
        if (RandomChanceProcMultiplier != 1f || GoodRandomChanceProcMultiplier != 1f)
        {
            RandomManager.Instance.OnGoodRandomChanceProcd += Instance_OnRandomChanceProcd;
        }
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorRemoved();

        if (RandomManager.Instance == null) return;

        RandomManager.Instance.RandomChanceProcMultiplier /= RandomChanceProcMultiplier;
        RandomManager.Instance.GoodRandomChanceProcMultiplier /= GoodRandomChanceProcMultiplier;
        RandomManager.Instance.BadRandomChanceProcMultiplier /= BadRandomChanceProcMultiplier;

        if (RandomChanceProcMultiplier != 1f || BadRandomChanceProcMultiplier != 1f)
        {
            RandomManager.Instance.OnBadRandomChanceProcd -= Instance_OnRandomChanceProcd;
        }
        if (RandomChanceProcMultiplier != 1f || GoodRandomChanceProcMultiplier != 1f)
        {
            RandomManager.Instance.OnGoodRandomChanceProcd -= Instance_OnRandomChanceProcd;
        }
    }

    private void Instance_OnRandomChanceProcd(object sender, System.EventArgs e)
    {
        TryTriggerIconAnimation();
    }
}