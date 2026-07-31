using UnityEngine;

public class AbstractLevelFinishDoor : AnimatedInteractable, INavPointersScreenOverlayTrackableObject
{
    [SerializeField] private float _offsetForPointerPosition;

    protected override void OnStartInteact(GameObject interactor)
    {
        base.OnStartInteact(interactor);

        MusicManager.Instance.TargetMusicVolume = 0f;
    }

    protected override void OnFinishInteract(GameObject interactor)
    {
        base.OnFinishInteract(interactor);

        for(int i = 0; i < ModificatorsManager.Instance.CurrentModificators.Count; i++)
        {
            ModificatorsManager.Instance.CurrentModificators[i].OnLevelFinished();
        }
    }

    public float GetOffsetForPointerPosition()
    {
        return _offsetForPointerPosition;
    }

    public bool PointingCondition()
    {
        return true;
    }
}
