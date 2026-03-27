using UnityEngine.InputSystem;
using UnityEngine;
using Unity.VisualScripting;
using Unity.Mathematics;

public class PlayerInputSpecial : AbstractAISpecial
{
    const float GAMEPAD_AIM_TIME_SCALE = 0.1f;
    const float GAMEPAD_AIM_VISUAL_MOVE_SPEED = 15f;

    private static bool _isGamepadAiming = false;
    private static bool IsGamepadAiming
    {
        get => _isGamepadAiming;
        set
        {
            if (_isGamepadAiming == value) return; 

            _isGamepadAiming = value;
            if (_isGamepadAiming)
            {
                TimeManager.Instance.CurrentTimeScale *= GAMEPAD_AIM_TIME_SCALE;
            }
            else
            {
                TimeManager.Instance.CurrentTimeScale /= GAMEPAD_AIM_TIME_SCALE;
            }
        }
    }

    public InputActionReference SpecialActionReference;
    public InputActionReference GamepadAimActionReference;
    public float GamePadTeleportRange = 15f;
    public float MaxDistanceToTeleportIntoCharacter = 5f;
    public Color ValidTeleporationColor = Color.white;
    public Color InvalidTeleportationColor = Color.grey;

    [SerializeField] private GameObject _gamepadTeleportTargetUI;

    private Vector2? _teleportationGamepadAimTarget = null;
    private Vector2? _teleportationGamepadAimCurrent = null;

    private CharacterComponentsManager _bloodTeleportationTarget = null;


    public Vector3? GetMouseWorldPositionOnCharacterLayer()
    {
        RaycastHit[] mouseHits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition));
        for (int i = 0; i < mouseHits.Length; i++)
        {
            if (mouseHits[i].collider.gameObject == LayerManager.Instance.GetZLayerOfGameObject(gameObject).gameObject)
            {
                return mouseHits[i].point;
            }
        }
        return null;
    }

    private void Start()
    {
        SpecialActionReference.action.started += SpecialActionRereference_OnActionStarted;
        SpecialActionReference.action.canceled += SpecialActionReference_OnActionCanceled;
    }

    private void SpecialActionRereference_OnActionStarted(InputAction.CallbackContext context)
    {
        if (UIManager.GamePaused()) return;
        HandleStartSpecial();
    }
    private void SpecialActionReference_OnActionCanceled(InputAction.CallbackContext context)
    {
        if (UIManager.GamePaused()) return;
        HandleStopSpecial();
    }

    private void HandleStartSpecial()
    {
        if (CharComponents.CharacterSpecial == null) return;

        //BLEED TELEPORTATION
        if (
            CharComponents.CharacterSpecial.TryGetComponent(out CharacterBleedTeleportation bleedTeleporation) && 
            bleedTeleporation.GetHasEnoughForCost() && 
            !bleedTeleporation.IsTeleporting
            )
        {
            if (CurrentDeviceTracker.GetGamepadIsConnected())
            {
                IsGamepadAiming = true;
            }
            else
            {
                ZIndexLayer layer = LayerManager.Instance.GetZLayerOfGameObject(CharComponents.gameObject);
                Vector3? mousePos = CurrentDeviceTracker.GetMouseWorldPositionOnLayer(layer);
                if (!mousePos.HasValue) return;

                CharacterComponentsManager closesetCharacter = null;
                float closestCharacterDistance = MaxDistanceToTeleportIntoCharacter;
                foreach (Transform characterTrasnform in CharComponents.CharacterCollision.CurrentZLayer.CharactersContainer)
                {
                    if (
                        characterTrasnform.gameObject.activeSelf && 
                        characterTrasnform.TryGetComponent(out CharacterComponentsManager character) &&
                        character != CharComponents &&
                        !CharComponents.CharacterTeam.GetIsAllyToAnotherTeam(character.CharacterTeam)
                        )
                    {
                        float distance = Vector2.Distance(mousePos.Value, characterTrasnform.position);
                        if (distance < closestCharacterDistance)
                        {
                            closestCharacterDistance = distance;
                            closesetCharacter = character;
                        }
                    }
                }

                bleedTeleporation.TryTeleport(closesetCharacter);
            }
        }

        //TELEPORTATION
        else if (
            CharComponents.CharacterSpecial.TryGetComponent(out CharacterTeleportation characterTeleportation) && 
            characterTeleportation.GetHasEnoughForCost()
            )
        {
            if (CurrentDeviceTracker.GetGamepadIsConnected())
            {
                IsGamepadAiming = true;
            }
            else
            {
                ZIndexLayer layer = LayerManager.Instance.GetZLayerOfGameObject(CharComponents.gameObject);
                Vector3? mousePos = CurrentDeviceTracker.GetMouseWorldPositionOnLayer(layer);
                if (!mousePos.HasValue) return;

                characterTeleportation.TryTeleport(mousePos.Value, layer);
            }
        }

        //HOOKING
        else if (
            CharComponents.CharacterSpecial.TryGetComponent(out CharacterHook characterHook) &&
            characterHook.GetHasEnoughForCost()
            )
        {
            characterHook.TryHook(CharComponents.CharacterAiming.GetTargetAimNormalized());
        }
    }

    private void HandleStopSpecial()
    {
        if (CharComponents.CharacterSpecial == null) return;

        //BLEED TELEPORTATION
        if (CharComponents.CharacterSpecial.TryGetComponent(out CharacterBleedTeleportation characterBleedTeleportation))
        {
            if (_bloodTeleportationTarget != null && _isGamepadAiming)
            {
                IsGamepadAiming = false;
                _teleportationGamepadAimTarget = null;
                characterBleedTeleportation.TryTeleport(_bloodTeleportationTarget);
            }
        }

        //TELEPORATAION
        else if (CharComponents.CharacterSpecial.TryGetComponent(out CharacterTeleportation characterTeleportation))
        {
            if (_isGamepadAiming && _teleportationGamepadAimTarget.HasValue && GamepadAimActionReference.action.ReadValue<Vector2>() != Vector2.zero)
            {
                characterTeleportation.TryTeleport(_teleportationGamepadAimTarget.Value, CharComponents.CharacterCollision.CurrentZLayer);
            }
        }

        //HOOKING
        else if (CharComponents.CharacterSpecial.TryGetComponent(out CharacterHook characterHook))
        {
            characterHook.TryStopHook();
        }

        IsGamepadAiming = false;
        _teleportationGamepadAimTarget = null;
    }

    private void Update()
    {
        bool isValidAimThisFrame = true;

        if (CharComponents.CharacterSpecial == null || !_isGamepadAiming || UIManager.GamePaused())
        {
            _teleportationGamepadAimTarget = null;
        }
        else
        {
            if (CharComponents.CharacterSpecial.TryGetComponent(out CharacterBleedTeleportation characterBleedTeleportation))
            {
                ZIndexLayer layer = LayerManager.Instance.GetZLayerOfGameObject(CharComponents.gameObject);
                Vector2 targetDirection = GamepadAimActionReference.action.ReadValue<Vector2>();
                Vector2 targetPos = CharComponents.Center.transform.position + VectorMath.Vec2ToVec3(targetDirection * GamePadTeleportRange);

                CharacterComponentsManager closesetCharacter = null;
                float closestCharacterDistance = MaxDistanceToTeleportIntoCharacter;
                foreach (Transform characterTrasnform in CharComponents.CharacterCollision.CurrentZLayer.CharactersContainer)
                {
                    if (
                        characterTrasnform.gameObject.activeSelf && characterTrasnform.TryGetComponent(out CharacterComponentsManager character) &&
                        character != CharComponents &&
                        !CharComponents.CharacterTeam.GetIsAllyToAnotherTeam(character.CharacterTeam)
                        )
                    {
                        float distance = Vector2.Distance(targetPos, characterTrasnform.position);
                        if (distance < closestCharacterDistance)
                        {
                            closestCharacterDistance = distance;
                            closesetCharacter = character;
                        }
                    }
                }

                if (closesetCharacter != null)
                {
                    _bloodTeleportationTarget = closesetCharacter;
                    _teleportationGamepadAimTarget = closesetCharacter.Center.transform.position;
                }
                else
                {
                    _bloodTeleportationTarget = null;
                    _teleportationGamepadAimTarget = targetPos;
                    isValidAimThisFrame = false;
                }
            }

            else if (CharComponents.CharacterSpecial.TryGetComponent(out CharacterTeleportation characterTeleportation))
            {
                ZIndexLayer layer = LayerManager.Instance.GetZLayerOfGameObject(CharComponents.gameObject);
                Vector2 targetDirection = GamepadAimActionReference.action.ReadValue<Vector2>();
                Vector2 targetPos = CharComponents.Center.transform.position + VectorMath.Vec2ToVec3(targetDirection * GamePadTeleportRange);

                if (!layer.MultiTileMapsContainer.GetHasTileBehaviourAt(targetPos, TileBehaviour.TileBehaviourType.FOREBGROUND))
                {
                    _teleportationGamepadAimTarget = targetPos;
                }
                else
                {
                    RaycastHit2D[] hits = Physics2D.RaycastAll(
                        targetPos,
                        -targetDirection,
                        GamePadTeleportRange,
                        1 << layer.EnviromentLayer
                        );
                    foreach (RaycastHit2D hit in hits)
                    {
                        if (!layer.MultiTileMapsContainer.GetHasTileBehaviourAt(hit.point, TileBehaviour.TileBehaviourType.FOREBGROUND))
                        {
                            _teleportationGamepadAimTarget = TileManager.PositionToTilePosition(hit.point) + Vector2.one * 0.5f;
                            break;
                        }
                    }
                }
            }
        }

        if (_teleportationGamepadAimTarget != null && IsGamepadAiming)
        {
            if (_teleportationGamepadAimCurrent.HasValue)
            {
                _teleportationGamepadAimCurrent = math.lerp(_teleportationGamepadAimCurrent.Value, _teleportationGamepadAimTarget.Value, Time.unscaledDeltaTime * GAMEPAD_AIM_VISUAL_MOVE_SPEED);
            }
            else
            {
                _teleportationGamepadAimCurrent = _teleportationGamepadAimTarget;
            }
        }
        else
        {
            _teleportationGamepadAimCurrent = null;
        }

        SetGamepadTeleportUITarget(_teleportationGamepadAimCurrent, isValidAimThisFrame);
    }

    private void SetGamepadTeleportUITarget(Vector2? target, bool isValid)
    {
        if (!target.HasValue || Vector2.Distance(CharComponents.Center.transform.position, target.Value) < 0.5f)
        {
            _gamepadTeleportTargetUI.SetActive(false);
            _gamepadTeleportTargetUI.transform.SetParent(transform);
        }
        else
        {
            if (_gamepadTeleportTargetUI.TryGetComponent(out SpriteRenderer teleTargetRenderer))
            {
                teleTargetRenderer.color = isValid ? ValidTeleporationColor : InvalidTeleportationColor;
            }

            _gamepadTeleportTargetUI.SetActive(true);
            LayerManager.Instance.ChangeZIndexForGameObject(CharComponents.CharacterCollision.CurrentZLayer, _gamepadTeleportTargetUI);

            float distance = Vector2.Distance(CharComponents.Center.transform.position, target.Value);
            Vector2 targetAim = (target.Value - VectorMath.Vec3ToVec2(CharComponents.Center.transform.position)).normalized;

            _gamepadTeleportTargetUI.transform.localScale = new Vector3(distance, 1f, 1f);
            _gamepadTeleportTargetUI.transform.position = CharComponents.Center.transform.position + VectorMath.Vec2ToVec3(targetAim) * distance / 2f;
            _gamepadTeleportTargetUI.transform.rotation = VectorMath.Vec2ToQuarterninon2D(targetAim);
        }
    }

    private void OnDestroy()
    {
        SpecialActionReference.action.started += SpecialActionRereference_OnActionStarted;
        SpecialActionReference.action.canceled += SpecialActionReference_OnActionCanceled;

        if (CharComponents != null && !CharComponents.IsDestroyed() && CharComponents.CharacterSpecial == this)
        {
            CharComponents.CharacterSpecial = null;
        }
        if (_gamepadTeleportTargetUI != null && !_gamepadTeleportTargetUI.IsDestroyed())
        {
            Destroy(_gamepadTeleportTargetUI);
        }

        IsGamepadAiming = false;
    }
}
