using System.Collections;
using UnityEngine;

public class DefaultAIInteracting : AbstractAIInteracting
{
    public float OpenDoorDelaySeconds = 0.5f;

    private void OnEnable()
    {
        CharComponents.CharacterCollision.OnCollisionChanged += CharacterCollision_OnCollisionChanged;
    }

    private void OnDisable()
    {
        CharComponents.CharacterCollision.OnCollisionChanged -= CharacterCollision_OnCollisionChanged;
    }

    private void CharacterCollision_OnCollisionChanged(object sender, CharacterCollision.OnCollisionChangedEventArgs e)
    {
        if ((e.Collider?.TryGetComponent(out OnInteractToggleOpenDoor door) ?? false))
        {
            StartCoroutine(AwaitTimeThenOpenDoor(door));
        }
    }

    private IEnumerator AwaitTimeThenOpenDoor(OnInteractToggleOpenDoor door)
    {
        yield return new WaitForSeconds(OpenDoorDelaySeconds);
        if (!door.IsOpen)
        {
            Debug.Log(gameObject.name);
            CharComponents.CharacterInteract.TryInteract(door);
        }
    }
}
