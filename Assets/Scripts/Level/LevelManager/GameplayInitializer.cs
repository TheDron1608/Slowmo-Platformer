using UnityEngine;

public class GameplayInitializer : MonoBehaviour
{
    private void Start()
    {
        UIManager.Instance.GameplayScreenOverlay.Show();
        UIManager.Instance.ModificatorsScreenOverlay.Show();

        WorldGenerationManager.Instance?.GenerateLevel();

        SpawnManager.Instance?.SpawnPlayerCharacterAtStartPosition();
    }
}
