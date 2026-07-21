using Unity.Multiplayer.PlayMode;
using UnityEngine;

[AllowEffectWithSenderReceiveNull]
public class AffectCharactersOnRadius : AbstractEffectWithSender, IMultiplierableEffect
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

        foreach (Transform characterTransform in layer.CharactersContainer)
        {
            float distance = Vector2.Distance(centerPosition, characterTransform.position);
            if (
                distance < Radius &&
                characterTransform.gameObject.activeSelf &&
                characterTransform.TryGetComponent(out AbstractCharacterComponent character) &&
                (AffectThroughWalls || Physics2D.Linecast(
                    centerPosition,
                    character.CharComponents.Center.transform.position,
                    1 << layer.EnviromentLayer
                    ).collider == null)
                )
            {
                character.CharComponents.CharacterEffectsReceiver.ApplyEffect(Effect, sender, EffectMultiplier * (1f - distance / Radius));
            }
        }
    }

    public override bool Equals(AbstractEffect other)
    {
        return
            base.Equals(other) &&
            Radius == (other as AffectCharactersOnRadius).Radius &&
            (Effect?.Equals((other as AffectCharactersOnRadius).Effect) ?? Effect == (other as AffectCharactersOnRadius).Effect) &&
            AffectThroughWalls == (other as AffectCharactersOnRadius).AffectThroughWalls;
    }
}