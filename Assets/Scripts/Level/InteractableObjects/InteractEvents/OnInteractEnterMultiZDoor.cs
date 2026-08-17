using UnityEngine;

public class OnInteractEnterMultiZDoor : AnimatedInteractable, INavPointersScreenOverlayTrackableObject
{
    [SerializeField] private float _offsetForPointerPosition;

    private ZIndexLayer _zLayer;

    private static OnInteractEnterMultiZDoor LastPlayerExitDoor = null; //this static property and will not reset on reboot game, cuz no need in it

    public ZIndexLayer ZLayer
    {
        get => _zLayer;
        private set => _zLayer = value;
    }

    public OnInteractEnterMultiZDoor Exit;

    protected override void OnAwake()
    {
        base.OnAwake();

        ZLayer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);
    }

    protected override void OnFinishInteract(GameObject interactor)
    {
        base.OnFinishInteract(interactor);
        LayerManager.Instance.ChangeZIndexForGameObject(Exit.ZLayer, interactor, Exit.gameObject);

        if (interactor.TryGetComponent(out AbstractCharacterComponent character) && character.CharComponents.CharacterTeam.Team == TeamManager.Teams.PLAYER)
        {
            LastPlayerExitDoor = Exit;
        }
    }

    public float GetOffsetForPointerPosition()
    {
        return _offsetForPointerPosition;
    }

    public bool PointingCondition()
    {
        return LastPlayerExitDoor != this && enabled;
    }
}
