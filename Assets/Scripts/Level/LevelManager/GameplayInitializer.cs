using UnityEngine;

public class GameplayInitializer : MonoBehaviour
{
    private void Start()
    {
        foreach (AbstractModificator modificator in ModificatorsManager.Instance.CurrentModificators)
        {
            modificator.OnLevelPreGenerated();
        }
        TeamManager.Instance.OnLevelPreGenerated();

        UIManager.Instance.GameplayScreenOverlay.Show();
        UIManager.Instance.ModificatorsScreenOverlay.Show();

        WorldGenerationManager.Instance?.GenerateLevel();

        SpawnManager.Instance?.SpawnPlayerCharacterAtStartPosition();

        foreach (AbstractModificator modificator in ModificatorsManager.Instance.CurrentModificators)
        {
            modificator.OnLevelGenerated();
        }
    }
}
