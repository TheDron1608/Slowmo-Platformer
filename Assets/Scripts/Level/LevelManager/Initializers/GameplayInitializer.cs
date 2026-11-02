using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

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
                try
                {
                    modificator.OnLevelPreGenerated();
                }
                catch (Exception exc)
                {
                    Debug.LogError("OnLevelPreGenerated error for modificator: " + modificator.name + " message: " + exc.Message);
                }
            }
        }

        UIManager.Instance.GameplayScreenOverlay.Show();
        UIManager.Instance.ModificatorsScreenOverlay.Show();
        UIManager.Instance.ArtifactModificatorsScreenOverlay.Show();
        UIManager.Instance.DifficultyScreenOverlay.Show();
        UIManager.Instance.NavPointersScreenOverlay.Show();

        try
        {
            WorldGenerationManager.Instance?.GenerateLevel();
        }
        catch (Exception e)
        {
            Debug.LogError("World generation failed");
            SceneManager.LoadScene(SceneList.GAMEPLAY);
        }

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
                try
                {
                    modificator.OnLevelGenerated();
                }
                catch (Exception exc)
                {
                    Debug.LogError("OnLevelGenerated error for modificator: " + modificator.name + " message: " + exc.Message);
                }
            }
        }
    }
}
