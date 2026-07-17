using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class HealthCostingTeleportation : AbstractCharacterSpecial
{
    const float TELEPORATION_VISUAL_EFFECT_DURATION_SECONDS = 0.1f;
    const float TELEPORTATION_VISUAL_EFFECT_SPRITE_SIZE_UNITS = 2f;

    const int PARTICLES_AMOUNT = 4;
    const float PARTICLE_MIN_VELOCITY = 0.5f;
    const float PARTICLE_MAX_VELOCITY = 2f;
    const float PARTICLE_MIN_ANGULAR_VELOCITY = -360f;
    const float PARTICLE_MAX_ANGULAR_VELOCITY = 360f;

    public List<AbstractParticle> ParticlesOnTeleport = new();
    [SerializeField] private GameObject _visualEffect;

    private Coroutine _teleporationVisualEffectCoroutine;

    public bool TryTeleport(Vector2 position, ZIndexLayer layer)
    {
        if (!IsAbleToDoSpecial) return false;

        Vector2 oldPosition = CharComponents.Center.transform.position;

        if (layer.MultiTileMapsContainer.GetHasTileBehaviourAt(position, TileBehaviour.TileBehaviourType.FOREBGROUND))
        {
            return false;
        }

        CharComponents.transform.position = position;
        LayerManager.Instance.ChangeZIndexForGameObject(layer, CharComponents.gameObject);

        if (_teleporationVisualEffectCoroutine != null)
        {
            StopCoroutine(_teleporationVisualEffectCoroutine);
        }
        _teleporationVisualEffectCoroutine = StartCoroutine(TeleporationVisualEffect(oldPosition, position, layer));

        InvokeUse();

        return true;
    }

    private IEnumerator TeleporationVisualEffect(Vector2 from, Vector2 to, ZIndexLayer layer)
    {
        ParticleSpawner.SpawnInstantlyMultipleParticles(
            ParticlesOnTeleport,
            from,
            Vector2.one,
            0f,
            PARTICLE_MIN_VELOCITY,
            PARTICLE_MAX_VELOCITY,
            PARTICLE_MIN_ANGULAR_VELOCITY,
            PARTICLE_MAX_ANGULAR_VELOCITY,
            CharComponents.CharacterEffectsReceiver.EffectMaterial,
            layer,
            PARTICLES_AMOUNT,
            0f
            );

        float maxWidth = Vector2.Distance(from, to) / TELEPORTATION_VISUAL_EFFECT_SPRITE_SIZE_UNITS;
        Vector2 targetRotation = (from - to).normalized;

        LayerManager.Instance.ChangeZIndexForGameObject(layer, _visualEffect);
        _visualEffect.transform.rotation = VectorMath.Vec2ToQuarterninon2D(targetRotation);
        _visualEffect.GetComponent<Renderer>().sharedMaterial = CharComponents.CharacterEffectsReceiver.EffectMaterial;
        _visualEffect.SetActive(true);

        for (float t = 0f; t < TELEPORATION_VISUAL_EFFECT_DURATION_SECONDS; t += Time.deltaTime)
        {
            float currentWidth = maxWidth * (1f - (t / TELEPORATION_VISUAL_EFFECT_DURATION_SECONDS)) * TELEPORTATION_VISUAL_EFFECT_SPRITE_SIZE_UNITS;

            _visualEffect.transform.position = VectorMath.Vec2ToVec3(to + (targetRotation * currentWidth / 2f), layer.transform.position.z);
            _visualEffect.transform.localScale = new Vector3(currentWidth, 1f, 1f);

            yield return new WaitForEndOfFrame();
        }

        _visualEffect.SetActive(false);
        _visualEffect.transform.SetParent(transform);

        _teleporationVisualEffectCoroutine = null;
    }

    private void OnDestroy()
    {
        if (_visualEffect != null && !_visualEffect.IsDestroyed())
        {
            Destroy(_visualEffect.gameObject);
        }
    }
}