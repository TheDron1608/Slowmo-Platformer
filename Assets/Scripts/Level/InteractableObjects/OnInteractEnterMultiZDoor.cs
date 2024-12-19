using System;
using UnityEngine;

public class OnInteractEnterMultiZDoor : Interactable
{
    const float MAX_SPEED_NOT_BREAKING_INTERACTION = .5f;

    private MultiZLayerCamera _multiZLayerCameraComponent;
    private ZIndexLayer _zLayer;

    public ZIndexLayer ZLayer
    {
        get => _zLayer;
        private set => _zLayer = value;
    }

    public OnInteractEnterMultiZDoor Exit;

    private void Awake()
    {
        if (!Camera.main.TryGetComponent(out _multiZLayerCameraComponent)) throw new UnityException("MainCamera does not has MultiZLayerCamera component");
        ZLayer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);
    }

    protected override bool InteractCondition(GameObject interactor)
    {
        return interactor.GetComponent<CharacterCollisionInfo>().IsCollidingFloor();
    }

    protected override void OnFinishInteract(GameObject interactor)
    {
        LayerManager.Instance.ChangeZIndexForGameObject(Exit.ZLayer, interactor, Exit.gameObject);
    }
}
