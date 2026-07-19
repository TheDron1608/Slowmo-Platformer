using System.Collections.Generic;
using UnityEngine;

public class RandomBottleLiquid : MonoBehaviour
{
    public float FillLiquidChance = 1f;
    public List<Material> RandomLiquidMaterial = new();
    public List<AbstractParticle> ExtraLiquidParticles = new();
    public List<AbstractEffect> ExtraThrowHitEffects = new();

    private void Start()
    {
        if (Random.value <= FillLiquidChance)
        {
            if (TryGetComponent(out DynamicMaterial dynamicMaterial))
            {
                dynamicMaterial.DefaultMaterial = NumberMath.PickRandomItem(RandomLiquidMaterial);
            }
            else if (TryGetComponent(out SpriteRenderer spriteRenderer))
            {
                spriteRenderer.sharedMaterial = NumberMath.PickRandomItem(RandomLiquidMaterial);
            }

            if (TryGetComponent(out BreakableObject breakableObject))
            {
                breakableObject.ParticlesOnBreak.AddRange(ExtraLiquidParticles);
            }

            if (TryGetComponent(out Holdable holdable))
            {
                holdable.EffectsOnThrowHit.AddRange(ExtraThrowHitEffects);
            }
        }
    }
}