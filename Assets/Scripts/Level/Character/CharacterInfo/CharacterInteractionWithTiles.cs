using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class CharacterInteractionWithTiles : AbstractCharacterComponent
{
    const float BASE_STICK_ON_WALL_STRINGHT_MULTIPLIER = 15f;

    public bool IsAbleToStickOnWalls = true;
    public float StickOnWallStringhtMultiplier = 1f;

    private bool _isCurrentAbleToStickOnWalls = true;

    public bool IsCurrentAbleToStickOnWalls
    {
        get => _isCurrentAbleToStickOnWalls;
        set => _isCurrentAbleToStickOnWalls = value;
    }

    private void FixedUpdate()
    {
        UpdateStickyTileInteractionProcess();
    }

    private void UpdateStickyTileInteractionProcess()
    {
        if (
            IsCurrentAbleToStickOnWalls && 
            IsAbleToStickOnWalls && 
            CharComponents.CharacterRigidBody.linearVelocityY < 0f && 
            CharComponents.CharacterCollision.GetIsStickingOnWall()
            )
        {
            CharComponents.CharacterRigidBody.linearVelocityY = math.lerp(CharComponents.CharacterRigidBody.linearVelocityY, 0f, Time.fixedDeltaTime * BASE_STICK_ON_WALL_STRINGHT_MULTIPLIER * StickOnWallStringhtMultiplier);
        }
    }
}