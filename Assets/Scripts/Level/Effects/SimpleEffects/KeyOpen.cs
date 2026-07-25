
using UnityEngine;

public class KeyOpen : AbstractEffectWithSender
{
    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        if (AffectedObject.TryGetComponent(out OnInteractToggleOpenDoor door))
        {
            door.enabled = true;
            door.Open(sender.gameObject);
            
            if (sender.TryGetComponent(out BreakableObject breakableObj))
            {
                breakableObj.BreakObject(null);
            }
        }
    }
}