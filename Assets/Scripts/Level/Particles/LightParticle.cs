using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightParticle : AbstractParticle
{
    public float LifeTime = 0.5f;

    private float _lifeTimeLeft;

    public override void SetParticleAttrs(
        AbstractParticle original,
        Vector2 position,
        Vector2 direction,
        float angle,
        float velocity,
        float angularVelocity,
        Material material,
        ZIndexLayer layer,
        bool enablePhysics = true
        )
    {
        base.SetParticleAttrs(original, position, direction, angle, velocity, angularVelocity, material, layer);

        Light2D light = GetComponent<Light2D>();
        Light2D originalLight = original.GetComponent<Light2D>();
        light.pointLightOuterRadius = originalLight.pointLightOuterRadius;
        light.pointLightInnerRadius = originalLight.pointLightInnerRadius;
        light.pointLightInnerRadius = originalLight.pointLightInnerRadius;
        light.pointLightOuterRadius = originalLight.pointLightOuterRadius;
        light.intensity = originalLight.intensity;

        LifeTime = originalLight.GetComponent<LightParticle>().LifeTime;
        _lifeTimeLeft = LifeTime;
    }

    private void FixedUpdate()
    {
        _lifeTimeLeft -= Time.deltaTime;
        if (_lifeTimeLeft <= 0)
        {
            RemoveParticle();
        }
    }

    public override void RemoveParticle()
    {
        base.RemoveParticle();

        transform.parent = ParticlesManager.Instance.UnusedLightParticleContainer;
    }
}
