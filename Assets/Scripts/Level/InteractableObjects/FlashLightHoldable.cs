using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Holdable))]
public class FlashLightHoldable : MonoBehaviour
{
    [SerializeField] private LightManagerManagedLightSource _lightComponent;

    private void Awake()
    {
        if (TryGetComponent(out Holdable holdable) && !holdable.IsDestroyed())
        {
            holdable.OnGiven += FlashLightHoldable_OnGiven;
            holdable.OnThrown += FlashLightHoldable_OnThrown;
        }
    }

    private void FlashLightHoldable_OnGiven(object sender, CharacterHoldingObjects e)
    {
        _lightComponent.ForceDisableLight = false;
    }
    private void FlashLightHoldable_OnThrown(object sender, Holdable.OnThrownEventArgs e)
    {
        _lightComponent.ForceDisableLight = true;
    }

    private void OnDestroy()
    {
        if (TryGetComponent(out Holdable holdable) && !holdable.IsDestroyed())
        {
            holdable.OnGiven -= FlashLightHoldable_OnGiven;
            holdable.OnThrown -= FlashLightHoldable_OnThrown;
        }
    }
}
