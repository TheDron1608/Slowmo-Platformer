using UnityEngine;

public class CharacterPartPhysicsParticle : PhysicsParticle
{
    const float PARTICLE_COLLIDER_SIZE_MULTIPLIER = 0.667f;

    private CharacterPart _characterPart = null;
    public CharacterPart CharacterPart
    {
        get => _characterPart;
        set
        {
            _characterPart = value;
            UpdateCharacterPart();
        }
    }

    private void UpdateCharacterPart()
    {
        if (CharacterPart != null)
        {
            {
                GameObject newSpriteObject = new GameObject(CharacterPart.name);
                newSpriteObject.transform.SetParent(transform, false);
                newSpriteObject.tag = gameObject.tag;
                newSpriteObject.layer = gameObject.layer;
                GameObjectUtility.CopySpriteRenderer(
                    CharacterPart.GetComponent<SpriteRenderer>(),
                    newSpriteObject.AddComponent<SpriteRenderer>()
                    );
            }

            foreach (CharacterEquipmentPart equipmentOnLimbPart in CharacterPart.GetEquipedAtParts())
            {
                GameObject newEquipmentSpriteObject = new GameObject(equipmentOnLimbPart.name);
                newEquipmentSpriteObject.transform.SetParent(transform, false);
                newEquipmentSpriteObject.tag = gameObject.tag;
                newEquipmentSpriteObject.layer = gameObject.layer;
                GameObjectUtility.CopySpriteRenderer(
                    equipmentOnLimbPart.GetComponent<SpriteRenderer>(),
                    newEquipmentSpriteObject.AddComponent<SpriteRenderer>()
                    );
            }

            Collider2D copyCollider;
            if (CharacterPart is CharacterLimbPart)
            {
                copyCollider = CharacterPart.GetComponentInChildren<Collider2D>();
            }
            else if (CharacterPart is CharacterEquipmentPart equipmentPart && equipmentPart.GetEquipedAtLimb() != null)
            {
                copyCollider = equipmentPart.GetEquipedAtLimb().GetComponentInChildren<Collider2D>();
            }
            else
            {
                return;
            }

            GameObject newColliderObject = new GameObject("CharacterPartPhysicsParticleCollider");
            newColliderObject.transform.SetParent(transform, false);
            newColliderObject.tag = gameObject.tag;
            newColliderObject.layer = gameObject.layer;
            newColliderObject.transform.localPosition = copyCollider.transform.localPosition;

            GameObjectUtility.ConvertSimpleColliderToBoxCollider(
                newColliderObject.AddComponent<BoxCollider2D>(),
                copyCollider
                );
            newColliderObject.GetComponent<BoxCollider2D>().size *= PARTICLE_COLLIDER_SIZE_MULTIPLIER;
        }
    }
}
