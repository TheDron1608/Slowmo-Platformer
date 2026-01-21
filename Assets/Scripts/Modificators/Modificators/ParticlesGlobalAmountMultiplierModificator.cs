using UnityEngine;

public class ParticlesGlobalAmountMultiplierModificator : AbstractModificator
{
    public float PhysicsParticlesGlobalSpawnAmountMultiplier = 1f;
    public float FluidParticlesGlobalSpawnAmountMultiplier = 1f;
    public float CloudParticlesGlobalSpawnAmountMultiplier = 1f;

    public override void OnLevelPreGenerated()
    {
        base.OnLevelPreGenerated();

        ParticlesManager.Instance.PhysicsParticlesGlobalSpawnAmountMultiplier *= PhysicsParticlesGlobalSpawnAmountMultiplier;
        ParticlesManager.Instance.FluidParticlesGlobalSpawnAmountMultiplier *= FluidParticlesGlobalSpawnAmountMultiplier;
        ParticlesManager.Instance.CloudParticlesGlobalSpawnAmountMultiplier *= CloudParticlesGlobalSpawnAmountMultiplier;
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorRemoved();

        if (ParticlesManager.Instance != null)
        {
            ParticlesManager.Instance.PhysicsParticlesGlobalSpawnAmountMultiplier /= PhysicsParticlesGlobalSpawnAmountMultiplier;
            ParticlesManager.Instance.FluidParticlesGlobalSpawnAmountMultiplier /= FluidParticlesGlobalSpawnAmountMultiplier;
            ParticlesManager.Instance.CloudParticlesGlobalSpawnAmountMultiplier /= CloudParticlesGlobalSpawnAmountMultiplier;
        }
    }
}