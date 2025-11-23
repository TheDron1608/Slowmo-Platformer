using UnityEngine.InputSystem;

public class PlayerInputReloading : AbstractAIReloading
{
    public InputActionReference ReloadActionReference;


    private void Start()
    {
        ReloadActionReference.action.started += ReloadActionReference_OnActionStarted;
    }

    private void ReloadActionReference_OnActionStarted(InputAction.CallbackContext context)
    {
        if (UIManager.GamePaused()) return;
        HandleReload();
    }

    //RELOAD
    private void HandleReload()
    {
        CharComponents.CharacterReloading.TryReload();
    }

    private void Update()
    {
        UpdateAutoReload();
    }

    private void UpdateAutoReload()
    {
        if (
            CharComponents.CharacterHolding.CurrentHoldObject != null &&
            CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out RangedWeapon rangedWeapon) &&
            !rangedWeapon.IsReloading &&
            rangedWeapon.GetIsNeedReload()
            )
        {
            CharComponents.CharacterReloading.TryReload();
        }
    }

    private void OnDestroy()
    {
        ReloadActionReference.action.started -= ReloadActionReference_OnActionStarted;
    }
}