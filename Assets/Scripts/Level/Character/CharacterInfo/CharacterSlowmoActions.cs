using Unity.VisualScripting;
using UnityEngine;

public class CharacterSlowmoActions : AbstractCharacterComponent
{
    public float SlowmoOnKill = 0.6f;
    public float SlowmoOnEnterZDoor = 1f;
    public float SlowmoOnSpawn = 2f;

    private void Start()
    {
        CharComponents.CharacterAttacking.OnEffectApplied += CharacterAttacking_OnEffectApplied;
        CharComponents.CharacterInteract.OnInteracted += CharacterInteract_OnInteracted;

        TimeManager.Instance.TryTemporalSlowTime(SlowmoOnSpawn);
    }

    private void CharacterAttacking_OnEffectApplied(object sender, IEffectApplier.OnEffectAppliedEventArgs e)
    {
        if (
            e.Effect is ILethalEffect &&
            e.Receiver.TryGetComponent(out AbstractCharacterComponent killedCharacter) &&
            (e.Sender as MonoBehaviour).TryGetComponent(out AbstractCharacterComponent killerCharacter)
            )
        {
            TimeManager.Instance.TryTemporalSlowTime(SlowmoOnKill);
        }
    }

    private void CharacterInteract_OnInteracted(object sender, Interactable e)
    {
        if (e is OnInteractEnterMultiZDoor door)
        {
            TimeManager.Instance.TryTemporalSlowTime(SlowmoOnEnterZDoor);
        }
    }

    private void OnDestroy()
    {
        CharComponents.CharacterAttacking.OnEffectApplied -= CharacterAttacking_OnEffectApplied;
        CharComponents.CharacterInteract.OnInteracted -= CharacterInteract_OnInteracted;
    }
}
