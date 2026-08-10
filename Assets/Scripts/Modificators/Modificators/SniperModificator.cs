using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class SniperModificator : AbstractModificator
{
    const float SCOPE_ALIGNING_UP_FORCE_MULT = 10f;
    const float HIT_RADIUS = 1.5f;
    const float PLAYER_HIT_RADIUS = 0.5f;
    const float ATTACKING_SCALE = 0.75f;
    const float ATTACKING_SCALE_SPEED_MULT = 10f;
    const float CAMERA_SHAKE_ON_ATTACK = 0.175f;
    const float SNIPER_DISTANCE_ON_START = 50f;
    const float TRIGGER_ON_EXTRA_TEAM_DISTANCE = 5f;

    public Sprite SniperDot;
    public float MoveSpeed;
    public float MaxMoveSpeed;
    public float AttackDelay;
    public List<AbstractEffect> EffectsOnHit;
    public TeamManager.Teams PrimalTargetTeam = new();
    public List<TeamManager.Teams> ExtraTargetTeams = new();
    public AbstractSoundPlayer AttackSound;

    private SpriteRenderer _currentSniper;
    private Vector2 _currentVeclocity = Vector2.zero;
    private float _currentAimingTime = 0f;
    private bool _isAttacking = false;
    private Vector3 _currentAttackingPosition;

    public override void OnLevelGenerated()
    {
        base.OnLevelGenerated();

        AddSniper();
    }

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        if (SceneList.GetCurrentSceneIsGameplay()) AddSniper();
    }

    private void AddSniper()
    {
        GameObject sniperGO = new GameObject("SniperDot");
        sniperGO.tag = LayerManager.OTHER_TAG_NAME;
        _currentSniper = sniperGO.AddComponent<SpriteRenderer>();
        _currentSniper.sprite = SniperDot;
        _currentSniper.transform.position = VectorMath.PickRandomDirection() * SNIPER_DISTANCE_ON_START;

        _currentVeclocity = Vector2.zero;
        _currentAimingTime = 0f;
        _isAttacking = false;
    }

    private void FixedUpdate()
    {
        if (SceneList.GetCurrentSceneIsGameplay() && _currentSniper != null && !_currentSniper.IsDestroyed())
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

                    layer.MultiTileMapsContainer.DestroyTileAt(new Vector3Int((int)math.floor(_currentSniper.transform.position.x), (int)math.floor(_currentSniper.transform.position.y), 0), true, true);
                    Camera.main.GetComponent<ShakableObject>().Shake(CAMERA_SHAKE_ON_ATTACK);
                    AttackSound.PlaySound(false, _currentSniper.transform.position);

                    _isAttacking = false;
                    _currentAimingTime = 0f;
                    _currentVeclocity = Vector2.zero;
                }
            }

            else
            {
                _currentSniper.transform.SetParent(layer.OtherContainer);

                float nearestCharacterDistance = float.MaxValue;
                AbstractCharacterComponent nearestCharacter = null;
                foreach (Transform characterT in layer.CharactersContainer)
                {
                    if (characterT.IsDestroyed() || !characterT.gameObject.activeSelf) continue;

                    float distance = Vector2.Distance(_currentSniper.transform.position, characterT.position);
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
                    LayerManager.Instance.ChangeZIndexForGameObject(layer, _currentSniper.gameObject);

                    MoveTo(layer, nearestCharacter.transform.position);

                    _currentSniper.transform.rotation = VectorMath.Vec2ToQuaternion2DNoMirroring(_currentVeclocity + Vector2.up * MoveSpeed * SCOPE_ALIGNING_UP_FORCE_MULT);
                }

                if (nearestCharacterDistance < HIT_RADIUS)
                {
                    _isAttacking = true;
                    _currentAttackingPosition = nearestCharacter.CharComponents.Center.transform.position;
                }
            }

            _currentSniper.transform.localScale = Vector3.one * math.lerp(
                _currentSniper.transform.localScale.x,
                _isAttacking ? ATTACKING_SCALE : 1f,
                Time.deltaTime * ATTACKING_SCALE_SPEED_MULT
                );

        }
    }

    private void MoveTo(ZIndexLayer layer, Vector3 to)
    {
        _currentSniper.transform.position = VectorMath.Vec2ToVec3(Vector2.SmoothDamp(
            _currentSniper.transform.position,
            to,
            ref _currentVeclocity,
            1f / MoveSpeed,
            MaxMoveSpeed
            ), layer.transform.position.z);

        _currentSniper.transform.rotation = VectorMath.Vec2ToQuaternion2DNoMirroring(_currentVeclocity + Vector2.up * MoveSpeed * SCOPE_ALIGNING_UP_FORCE_MULT);
    }

    private void TryAffectObject(Transform obj, ZIndexLayer layer)
    {
        float distance = Vector2.Distance(_currentSniper.transform.position, obj.position);
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
