using UnityEngine;

[RequireComponent(typeof(Holdable))]
public class FlashLightHoldable : MonoBehaviour
{
    [SerializeField] private LightManagerManagedLightSource _lightComponent;

    private Holdable _holdbleComponent;



    private void Update()
    {
        _lightComponent.ForceDisableLight = _holdbleComponent.CurrentHolder == null || _holdbleComponent.IsHolstered;
    }

    private void Awake()
    {
        if (!TryGetComponent(out _holdbleComponent)) throw new UnityException("Holdable component not found");
    }
}
