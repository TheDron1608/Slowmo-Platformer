using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonOnClickPickCards : MonoBehaviour
{
    public void PickCards()
    {
        if (GameObjectUtility.TryGetComponentInParentRecursive(transform, out ModificatorCardsCluster cluster))
        {
            cluster.Pick();
        }
    }
}
