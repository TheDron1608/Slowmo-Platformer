using Unity.VisualScripting;
using UnityEngine;

public class CharacterUITrack : AbstractCharacterComponent
{
    private bool _tracked = false;
    private void OnEnable()
    {
        if (_tracked) return;

        GameplayUIManager.GetInstance()?.AddTrackedCharacter(CharComponents);
        Camera.main?.GetComponent<CameraTrack>().TrackTargets.Add(CharComponents.transform);

        _tracked = true;
    }

    private void OnDisable()
    {
        if (!_tracked || ExcludeDisableConditions()) return;

        GameplayUIManager.GetInstance()?.RemoveTrackedCharacter(CharComponents);
        Camera.main?.GetComponent<CameraTrack>().TrackTargets.Remove(CharComponents.transform);

        _tracked = false;
    }

    private bool ExcludeDisableConditions()
    {
        return
            !(CharComponents == null || CharComponents.IsDestroyed()) && //exclude if destroyed
            (CharComponents.CharacterSpecial?.GetComponent<CharacterBleedTeleportation>()?.IsTeleporting ?? false); //exclude if is teleporting
    }
}
