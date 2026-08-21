using Unity.VisualScripting;
using UnityEngine;

public class GameplayInitializer : MonoBehaviour
{
    private void Start()
    {
        foreach (AbstractModificator modificator in ModificatorsManager.Instance.CurrentModificators)
        {
            //re-enable modificators with errors
            if (!modificator.IsDestroyed())
            {
                modificator.enabled = true;
            }
        }

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
        UIManager.Instance.NavPointersScreenOverlay.Show();

        WorldGenerationManager.Instance?.GenerateLevel();

        //spawn player failed, restart level
        if (!SpawnManager.Instance?.SpawnPlayerCharacterAtStartPosition())
        {
            UIManager.Instance.LoadSceneWithEffect(SceneList.GAMEPLAY);
            return;
        }


        foreach (AbstractModificator modificator in ModificatorsManager.Instance.CurrentModificators)
        {
            if (!modificator.DisabledModificator)
            {
                modificator.OnLevelGenerated();
            }
        }
    }
}
