using UnityEngine;

public class PLevelInitializer : MonoBehaviour
{
    private void Start()
    {
        UIManager.Instance.GameplayScreenOverlay.Show();

        WorldGenerationManager.Instance?.GenerateLevel();

        SpawnManager.Instance?.SpawnPlayerCharacterAtStartPosition();
    }
}
