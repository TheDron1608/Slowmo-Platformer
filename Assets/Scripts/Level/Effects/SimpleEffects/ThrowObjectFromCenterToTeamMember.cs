using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ThrowObjectFromCenterToTeamMember : AbstractEffect
{
    const float SPAWN_OBJECT_EXTRA_OFFSET = 0.5f;

    public List<Rigidbody2D> ThrowPhysicsObjects = new();
    public float ThrowForceMult = 1f;
    public TeamManager.Teams ThrowAtTeam = TeamManager.Teams.PLAYER;

    protected override void OnApply()
    {
        base.OnApply();

        ZIndexLayer layer = LayerManager.Instance.GetZLayerOfGameObject(AffectedObject.gameObject);

        float nearestCharacterDistance = float.MaxValue;
        AbstractCharacterComponent nearestCharacter = null;
        foreach (Transform characterT in layer.CharactersContainer)
        {
            if (characterT.IsDestroyed() || !characterT.gameObject.activeSelf) continue;

            float distance = Vector2.Distance(AffectedObject.transform.position, characterT.position);
            if (
                distance < nearestCharacterDistance &&
                characterT.TryGetComponent(out AbstractCharacterComponent character) &&
                character.CharComponents.CharacterTeam.Team == ThrowAtTeam
                )
            {
                nearestCharacter = character;
                nearestCharacterDistance = distance;
            }
        }

        if (nearestCharacter != null)
        {
            Vector2 throwDirection = (nearestCharacter.CharComponents.Center.transform.position - AffectedObject.transform.position).normalized;

            List<GameObject> newObjects = layer.TrySpawnObject(
                NumberMath.PickRandomItem(ThrowPhysicsObjects).gameObject,
                AffectedObject.transform.position + VectorMath.Vec2ToVec3(throwDirection) * SPAWN_OBJECT_EXTRA_OFFSET,
                null,
                null
                );

            if (newObjects.Count > 0 && newObjects[0].TryGetComponent(out Rigidbody2D rb))
            {
                rb.linearVelocity += throwDirection * ThrowForceMult * nearestCharacterDistance;
            }
        }

        RemoveSelf();
    }

    public override bool Equals(AbstractEffect other)
    {
        return
            base.Equals(other) &&
            ThrowPhysicsObjects == (other as ThrowObjectFromCenterToTeamMember).ThrowPhysicsObjects &&
            ThrowAtTeam == (other as ThrowObjectFromCenterToTeamMember).ThrowAtTeam &&
            ThrowForceMult == (other as ThrowObjectFromCenterToTeamMember).ThrowForceMult;
    }
}