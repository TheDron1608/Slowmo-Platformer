using UnityEngine;

public class StuckTrackTarget : MonoBehaviour
{
    public static StuckTrackTarget CreateTrack(Holdable stuckedObject, Transform stuckTo)
    {
        GameObject newGO = new("StuckTrackTarget_" + stuckedObject);
        newGO.transform.SetParent(stuckTo);
        newGO.transform.position = stuckedObject.transform.position;
        newGO.transform.rotation = stuckedObject.transform.rotation;

        StuckTrackTarget newGOStuckTrackTarget = newGO.AddComponent<StuckTrackTarget>();
        newGOStuckTrackTarget.StuckedObject = stuckedObject;
        newGOStuckTrackTarget.StuckTo = stuckTo;

        return newGOStuckTrackTarget;
    }

    public Holdable StuckedObject;
    public Transform StuckTo;

    private void Update()
    {
        if (StuckedObject.StuckedToCollider?.transform != StuckTo)
        {
            Destroy(gameObject);
        }
        else
        {
            StuckedObject.transform.position = transform.position;
            StuckedObject.transform.rotation = transform.rotation;
        }
    }
}