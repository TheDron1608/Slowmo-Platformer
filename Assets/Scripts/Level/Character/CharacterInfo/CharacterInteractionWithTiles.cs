using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class CharacterInteractionWithTiles : AbstractCharacterComponent
{
    const float BASE_STICK_ON_WALL_STRINGHT_MULTIPLIER = 15f;

    public bool IsAbleToStickOnWalls = true;
    public float StickOnWallStringhtMultiplier = 1f;

    private void Start()
    {
        CharComponents.CharacterCollision.OnTileBehavioutTypeCollisionChanged += CollisionCharacterInfoComponent_OnTileBehavioutTypeCollisionChanged;
    }

    private void CollisionCharacterInfoComponent_OnTileBehavioutTypeCollisionChanged(object sender, CharacterCollision.OnTileBehavioutTypeCollisionChangedEventArgs e)
    {
        UpdateTileInteractions();
    }

    private void UpdateTileInteractions()
    {
        UpdateStickyTileInteraction();
    }

    private void UpdateStickyTileInteraction()
    {
        if (!IsAbleToStickOnWalls) return;

        if (
            CharComponents.CharacterCollision.GetTileBehaviourTypeFromLeftWall() == TileBehaviour.TileBehaviourType.STICKY ||
            CharComponents.CharacterCollision.GetTileBehaviourTypeFromRightWall() == TileBehaviour.TileBehaviourType.STICKY
            )
        {
            StartCoroutine(UpdateStickyTileInteractionProcess());
        }
    }

    private IEnumerator UpdateStickyTileInteractionProcess()
    {
        while (CharComponents.CharacterCollision.GetIsStickingOnWall())
        {
            if (CharComponents.CharacterRigidBody.linearVelocityY < 0f)
            {
                CharComponents.CharacterRigidBody.linearVelocityY = math.lerp(CharComponents.CharacterRigidBody.linearVelocityY, 0f, Time.deltaTime * BASE_STICK_ON_WALL_STRINGHT_MULTIPLIER * StickOnWallStringhtMultiplier);
            }

            yield return new WaitForFixedUpdate();
        }
    }
}