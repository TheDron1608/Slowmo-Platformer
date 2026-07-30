using Unity.VisualScripting;
using UnityEngine;

public class StuckTrackTarget : MonoBehaviour
{
    public static StuckTrackTarget CreateTrack(IStuckableObject stuckedObject, Transform stuckTo)
    {
        GameObject newGO = new("StuckTrackTarget_" + stuckedObject);
        newGO.transform.SetParent(stuckTo);
        newGO.transform.position = (stuckedObject as MonoBehaviour).transform.position;
        newGO.transform.rotation = (stuckedObject as MonoBehaviour).transform.rotation;

        StuckTrackTarget newGOStuckTrackTarget = newGO.AddComponent<StuckTrackTarget>();
        newGOStuckTrackTarget.StuckedObject = stuckedObject;
        newGOStuckTrackTarget.StuckTo = stuckTo;

        return newGOStuckTrackTarget;
    }

    public IStuckableObject StuckedObject;
    public Transform StuckTo;

    private void Update()
    {
        if (StuckedObject.StuckedToCollider?.transform != StuckTo || StuckedObject.StuckedToCollider.IsDestroyed() || (StuckedObject as MonoBehaviour).IsDestroyed())
        {
            Destroy(gameObject);
        }
        else
        {
            (StuckedObject as MonoBehaviour).transform.position = transform.position;
            (StuckedObject as MonoBehaviour).transform.rotation = transform.rotation;
        }
    }
}