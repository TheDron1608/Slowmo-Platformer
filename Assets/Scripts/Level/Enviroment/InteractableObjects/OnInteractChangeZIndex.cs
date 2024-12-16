using UnityEngine;
using UnityEngine.TextCore.Text;

public class OnInteractChangeZIndex : MonoBehaviour, IInteractable
{
    private MultiZLayerCamera _multiZLayerCameraComponent;

    public GameObject ExitObject;

    private void Awake()
    {
        if (!Camera.main.TryGetComponent(out _multiZLayerCameraComponent)) throw new UnityException("MainCamera does not has MultiZLayerCamera component");
    }

    public void Interact(GameObject interactor)
    {
        interactor.transform.parent = ExitObject.transform.parent;
        interactor.transform.position = ExitObject.transform.position;
        interactor.layer = ExitObject.layer;

        _multiZLayerCameraComponent.CurrentZIndex = ExitObject.layer;
    }
}
