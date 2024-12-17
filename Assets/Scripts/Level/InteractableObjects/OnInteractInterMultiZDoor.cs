using UnityEngine;
using UnityEngine.TextCore.Text;

public class OnInteractInterMultiZDoor : MonoBehaviour, IInteractable
{
    private MultiZLayerCamera _multiZLayerCameraComponent;
    private ZIndexLayer _zLayer;
    
    public ZIndexLayer ZLayer
    {
        get => _zLayer;
        private set => _zLayer = value;
    }

    public OnInteractInterMultiZDoor Exit;

    private void Awake()
    {
        if (!Camera.main.TryGetComponent(out _multiZLayerCameraComponent)) throw new UnityException("MainCamera does not has MultiZLayerCamera component");
        ZLayer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);
    }

    public void Interact(GameObject interactor)
    {
        LayerManager.Instance.ChangeZIndexForGameObject(Exit.ZLayer, interactor, Exit.gameObject);

        //_multiZLayerCameraComponent.CurrentZIndex = Exit.ZLayer.ZIndex;
    }
}
