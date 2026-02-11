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

    private void Start()
    {
        CharComponents.CharacterCollision.OnCollisionChanged += CharacterCollision_OnCollisionChanged;
    }

    private void CharacterCollision_OnCollisionChanged(object sender, CharacterCollision.OnCollisionChangedEventArgs e)
    {
        UpdateStickyTileInteraction();
    }

    private void UpdateStickyTileInteraction()
    {
        if (
            IsCurrentAbleToStickOnWalls && IsAbleToStickOnWalls &&
            CharComponents.CharacterCollision.GetTileBehaviourTypeFromLeftWall() == ForegroundRuleTile.ForegroundBehaviourType.STICKY ||
            CharComponents.CharacterCollision.GetTileBehaviourTypeFromRightWall() == ForegroundRuleTile.ForegroundBehaviourType.STICKY
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

    private void OnDestroy()
    {
        CharComponents.CharacterCollision.OnCollisionChanged -= CharacterCollision_OnCollisionChanged;
    }
}