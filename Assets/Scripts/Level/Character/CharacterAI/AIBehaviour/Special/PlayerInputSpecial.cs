using UnityEngine.InputSystem;
using UnityEngine;
using Unity.VisualScripting;

public class PlayerInputSpecial : AbstractAISpecial
{
    public InputActionReference SpecialActionReference;
    public float MaxDistanceToTeleportIntoCharacter = 5f;

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
        HandleStopSpecial();
    }

    private void HandleStartSpecial()
    {
        if (CharComponents.CharacterSpecial == null) return;

        if (CharComponents.CharacterSpecial.TryGetComponent(out CharacterBleedTeleportation bleedTeleporation))
        {
            ZIndexLayer layer = LayerManager.Instance.GetZLayerOfGameObject(CharComponents.gameObject);
            Vector3? mousePos = CurrentDeviceTracker.GetMouseWorldPositionOnLayer(layer);
            if (!mousePos.HasValue) return;

            CharacterComponentsManager closesetCharacter = null;
            float closestCharacterDistance = MaxDistanceToTeleportIntoCharacter;
            foreach (Transform characterTrasnform in CharComponents.CharacterCollision.CurrentZLayer.CharactersContainer)
            {
                if (characterTrasnform.gameObject.activeSelf && characterTrasnform.TryGetComponent(out CharacterComponentsManager character))
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

    private void HandleStopSpecial()
    {

    }

    private void OnDestroy()
    {
        SpecialActionReference.action.started += SpecialActionRereference_OnActionStarted;
        SpecialActionReference.action.canceled += SpecialActionReference_OnActionCanceled;

        if (CharComponents != null && !CharComponents.IsDestroyed() && CharComponents.CharacterSpecial == this)
        {
            CharComponents.CharacterSpecial = null;
        }
    }
}
