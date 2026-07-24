using UnityEngine;

[AllowEffectWithSenderReceiveNull]
public class AffectObjectsOnRadius : AbstractEffectWithSender, IMultiplierableEffect
{
    public AbstractEffect Effect;
    public float Radius;
    public bool AffectThroughWalls = true;

    private float _effectMultiplier = 1f;

    public float EffectMultiplier
    {
        get => _effectMultiplier;
        set => _effectMultiplier = value;
    }

    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        ZIndexLayer layer = LayerManager.Instance.GetZLayerOfGameObject(AffectedObject.gameObject);
        Vector2 centerPosition;
        if (AffectedObject.TryGetComponent(out Collider2D collider))
        {
            centerPosition = GameObjectUtility.GetCenterOfCollider(collider);
        }
        else
        {
            centerPosition = AffectedObject.transform.position;
        }

        foreach (Transform character in layer.CharactersContainer)
        {
            TryAffectObject(character, centerPosition, layer, sender);
        }
        foreach (Transform holdable in layer.HoldablesContainer)
        {
            TryAffectObject(holdable, centerPosition, layer, sender);
        }
        foreach (Transform furntiure in layer.FurnitureContainer)
        {
            TryAffectObject(furntiure, centerPosition, layer, sender);
        }
        foreach (Transform characterTransform in layer.InteractableEnviromentContainer)
        {
            TryAffectObject(characterTransform, centerPosition, layer, sender, true);
        }
    }

    private void TryAffectObject(Transform obj, Vector2 centerPosition, ZIndexLayer layer, MonoBehaviour sender, bool forceAffectThroughWalls = false)
    {
        float distance = Vector2.Distance(centerPosition, obj.position);
        if (
            distance < Radius &&
            obj.gameObject.activeSelf &&
            obj != AffectedObject.transform &&
            obj != sender.transform &&
            obj.TryGetComponent(out ObjectEffectsReceiver effectReceiver) &&
            (AffectThroughWalls || forceAffectThroughWalls || Physics2D.Linecast(
                centerPosition,
                obj.position,
                1 << layer.EnviromentLayer
                ).collider == null)
            )
        {
            effectReceiver.ApplyEffect(Effect, sender, EffectMultiplier * (1f - distance / Radius));
        }
    }

    public override bool Equals(AbstractEffect other)
    {
        return
            base.Equals(other) &&
            Radius == (other as AffectObjectsOnRadius).Radius &&
            (Effect?.Equals((other as AffectObjectsOnRadius).Effect) ?? Effect == (other as AffectObjectsOnRadius).Effect) &&
            AffectThroughWalls == (other as AffectObjectsOnRadius).AffectThroughWalls;
    }
}