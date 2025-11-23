using UnityEngine;

public class OnInteractEnterMultiZDoor : AnimatedInteractable
{
    private MultiZLayerCamera _multiZLayerCameraComponent;
    private ZIndexLayer _zLayer;

    public ZIndexLayer ZLayer
    {
        get => _zLayer;
        private set => _zLayer = value;
    }

    public OnInteractEnterMultiZDoor Exit;

    protected override void OnAwake()
    {
        base.OnAwake();

        if (!Camera.main.TryGetComponent(out _multiZLayerCameraComponent)) throw new UnityException("MainCamera does not has MultiZLayerCamera component");
        ZLayer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);
    }

    protected override bool StartInteractCondition(GameObject interactor)
    {
        return interactor.GetComponent<CharacterCollision>().IsCollidingFloor();
    }

    protected override void OnFinishInteract(GameObject interactor)
    {
        base.OnFinishInteract(interactor);
        LayerManager.Instance.ChangeZIndexForGameObject(Exit.ZLayer, interactor, Exit.gameObject);
    }
}
