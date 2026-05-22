using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements.InputSystem;

public class GameplayInitializer : MonoBehaviour
{
    private void Start()
    {
        foreach (AbstractModificator modificator in ModificatorsManager.Instance.CurrentModificators)
        {
            if (!modificator.DisabledModificator)
            {
                modificator.OnLevelPreGenerated();
            }
        }

        UIManager.Instance.GameplayScreenOverlay.Show();
        UIManager.Instance.ModificatorsScreenOverlay.Show();
        UIManager.Instance.ArtifactModificatorsScreenOverlay.Show();
        UIManager.Instance.DifficultyScreenOverlay.Show();

        WorldGenerationManager.Instance?.GenerateLevel();

        SpawnManager.Instance?.SpawnPlayerCharacterAtStartPosition();

        foreach (AbstractModificator modificator in ModificatorsManager.Instance.CurrentModificators)
        {
            if (!modificator.DisabledModificator)
            {
                modificator.OnLevelGenerated();
            }
        }
    }
}
