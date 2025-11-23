using UnityEngine;

public class CharacterHealthbarTrack : AbstractCharacterComponent
{
    private void OnEnable()
    {
        GameplayUIManager.GetInstance()?.AddTrackedCharacter(CharComponents);
        Camera.main?.GetComponent<CameraTrack>().TrackTargets.Add(CharComponents.transform);
    }

    private void OnDisable()
    {
        GameplayUIManager.GetInstance()?.RemoveTrackedCharacter(CharComponents);
        Camera.main?.GetComponent<CameraTrack>().TrackTargets.Remove(CharComponents.transform);
    }
}
