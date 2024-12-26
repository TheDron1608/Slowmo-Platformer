using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class CharacterInteractionWithTiles : MonoBehaviour
{
    const float BASE_STICK_ON_WALL_STRINGHT_MULTIPLIER = 15f;

    public bool CanStickOnWalls = true;
    public float StickOnWallStringhtMultiplier = 1f;

    private CharacterCollisionInfo _collisionCharacterInfoComponent;
    private Rigidbody2D _rigidBodyComponent;

    private void Awake()
    {
        if (!TryGetComponent<CharacterCollisionInfo>(out _collisionCharacterInfoComponent)) throw new UnityException("CollisionCharacterInfo component not found");
        if (!TryGetComponent<Rigidbody2D>(out _rigidBodyComponent)) throw new UnityException("RigidBody2D component not found");
    }

    private void Start()
    {
        _collisionCharacterInfoComponent.OnTileBehavioutTypeCollisionChanged += CollisionCharacterInfoComponent_OnTileBehavioutTypeCollisionChanged;
    }

    private void CollisionCharacterInfoComponent_OnTileBehavioutTypeCollisionChanged(object sender, CharacterCollisionInfo.OnTileBehavioutTypeCollisionChangedEventArgs e)
    {
        UpdateTileInteractions();
    }

    private void UpdateTileInteractions()
    {
        UpdateStickyTileInteraction();
    }

    private void UpdateStickyTileInteraction()
    {
        if (!CanStickOnWalls) return;

        if (
            _collisionCharacterInfoComponent.GetTileBehaviourTypeFromLeftWall() == TileBehaviour.TileBehaviourType.STICKY ||
            _collisionCharacterInfoComponent.GetTileBehaviourTypeFromRightWall() == TileBehaviour.TileBehaviourType.STICKY
            )
        {
            StartCoroutine(UpdateStickyTileInteractionProcess());
        }
    }

    private IEnumerator UpdateStickyTileInteractionProcess()
    {
        while (_collisionCharacterInfoComponent.GetIsStickingOnWall())
        {
            if (_rigidBodyComponent.linearVelocityY < 0f)
            {
                _rigidBodyComponent.linearVelocityY = math.lerp(_rigidBodyComponent.linearVelocityY, 0f, Time.deltaTime * BASE_STICK_ON_WALL_STRINGHT_MULTIPLIER * StickOnWallStringhtMultiplier);
            }

            yield return new WaitForFixedUpdate();
        }
    }
}