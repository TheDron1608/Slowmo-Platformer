
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class Sniper : MonoBehaviour
{
    const float SCOPE_ALIGNING_UP_FORCE_MULT = 10f;
    const float HIT_RADIUS = 1.5f;
    const float PLAYER_HIT_RADIUS = 0.5f;
    const float ATTACKING_SCALE = 0.75f;
    const float ATTACKING_SCALE_SPEED_MULT = 10f;
    const float CAMERA_SHAKE_ON_ATTACK = 0.175f;
    const float TRIGGER_ON_EXTRA_TEAM_DISTANCE = 5f;
    const float SNIPER_SPAWN_OFFSET_Y = 25f;

    public float MoveSpeed;
    public float MaxMoveSpeed;
    public float AttackDelay;
    public List<AbstractEffect> EffectsOnHit;
    public TeamManager.Teams PrimalTargetTeam = new();
    public List<TeamManager.Teams> ExtraTargetTeams = new();
    public AbstractSoundPlayer AttackSound;

    private Vector2 _currentVeclocity = Vector2.zero;
    private float _currentAimingTime = 0f;
    private bool _isAttacking = false;
    private Vector3 _currentAttackingPosition;

    public Sniper SpawnSelf()
    {
        List<CharacterTeam> targets = TeamManager.Instance.GetTeamDataByTeam(PrimalTargetTeam).GetTeamMembers();
        if (targets.Count > 0)
        {
            List<GameObject> result = LayerManager.Instance.GetZLayerOfGameObject(targets[0].gameObject).TrySpawnObject(
                gameObject, 
                targets[0].transform.position + Vector3.up * SNIPER_SPAWN_OFFSET_Y, 
                null, 
                null
                );
            if (result.Count > 0 && result[0].TryGetComponent(out Sniper sniperResult))
            {
                return sniperResult;
            }
        }

        return null;
    }

    private void FixedUpdate()
    {
        ZIndexLayer layer = Camera.main.GetComponent<MultiZLayerCamera>().CurrentZLayer;

        if (_isAttacking)
        {
            MoveTo(layer, _currentAttackingPosition);

            _currentAimingTime += Time.deltaTime;
            if (_currentAimingTime > AttackDelay)
            {
                foreach (Transform character in layer.CharactersContainer)
                {
                    TryAffectObject(character, layer);
                }
                foreach (Transform holdable in layer.HoldablesContainer)
                {
                    TryAffectObject(holdable, layer);
                }
                foreach (Transform furntiure in layer.FurnitureContainer)
                {
                    TryAffectObject(furntiure, layer);
                }
                foreach (Transform characterTransform in layer.InteractableEnviromentContainer)
                {
                    TryAffectObject(characterTransform, layer);
                }

                layer.MultiTileMapsContainer.DestroyTileAt(new Vector3Int((int)math.floor(transform.position.x), (int)math.floor(transform.position.y), 0), true, true);
                Camera.main.GetComponent<ShakableObject>().Shake(CAMERA_SHAKE_ON_ATTACK);
                AttackSound.PlaySound(false, transform.position);

                _isAttacking = false;
                _currentAimingTime = 0f;
                _currentVeclocity = Vector2.zero;
            }
        }

        else
        {
            transform.SetParent(layer.OtherContainer);

            float nearestCharacterDistance = float.MaxValue;
            AbstractCharacterComponent nearestCharacter = null;
            foreach (Transform characterT in layer.CharactersContainer)
            {
                if (characterT.IsDestroyed() || !characterT.gameObject.activeSelf) continue;

                float distance = Vector2.Distance(transform.position, characterT.position);
                if (
                    distance < nearestCharacterDistance &&
                    characterT.TryGetComponent(out AbstractCharacterComponent character) &&
                    (
                        character.CharComponents.CharacterTeam.Team == PrimalTargetTeam ||
                        (distance < TRIGGER_ON_EXTRA_TEAM_DISTANCE && ExtraTargetTeams.Contains(character.CharComponents.CharacterTeam.Team))
                    ) &&
                    !character.CharComponents.CharacterEffectsReceiver.GetHasEffect<ILethalEffect>(true)
                    )
                {
                    nearestCharacterDistance = distance;
                    nearestCharacter = character;
                }
            }

            if (nearestCharacter != null)
            {
                LayerManager.Instance.ChangeZIndexForGameObject(layer, gameObject);

                MoveTo(layer, nearestCharacter.transform.position);

                transform.rotation = VectorMath.Vec2ToQuaternion2DNoMirroring(_currentVeclocity + Vector2.up * MoveSpeed * SCOPE_ALIGNING_UP_FORCE_MULT);
            }

            if (nearestCharacterDistance < HIT_RADIUS)
            {
                _isAttacking = true;
                _currentAttackingPosition = nearestCharacter.CharComponents.Center.transform.position;
            }
        }

        transform.localScale = Vector3.one * math.lerp(
            transform.localScale.x,
            _isAttacking ? ATTACKING_SCALE : 1f,
            Time.deltaTime * ATTACKING_SCALE_SPEED_MULT
            );
    }

    private void MoveTo(ZIndexLayer layer, Vector3 to)
    {
        transform.position = VectorMath.Vec2ToVec3(Vector2.SmoothDamp(
            transform.position,
            to,
            ref _currentVeclocity,
            1f / MoveSpeed,
            MaxMoveSpeed
            ), layer.transform.position.z);

        transform.rotation = VectorMath.Vec2ToQuaternion2DNoMirroring(_currentVeclocity + Vector2.up * MoveSpeed * SCOPE_ALIGNING_UP_FORCE_MULT);
    }

    private void TryAffectObject(Transform obj, ZIndexLayer layer)
    {
        float distance = Vector2.Distance(transform.position, obj.position);
        if (
            distance < HIT_RADIUS &&
            obj.gameObject.activeSelf &&
            obj.TryGetComponent(out ObjectEffectsReceiver effectReceiver) &&
            (
                !obj.TryGetComponent(out AbstractCharacterComponent character) ||
                character.CharComponents.CharacterTeam.Team != TeamManager.Teams.PLAYER ||
                distance < PLAYER_HIT_RADIUS
            )
            )
        {
            effectReceiver.ApplyEffect(EffectsOnHit, null);
            if (character != null) character.CharComponents.CharacterHealth.ApplyProjectileHit(null, false);
        }
    }
}