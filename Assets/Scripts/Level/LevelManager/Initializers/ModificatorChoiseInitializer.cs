using System;
using UnityEngine;

public class ModificatorChoiseInitializer : MonoBehaviour
{

    private void Awake()
    {
        foreach (AbstractModificator modificator in ModificatorsManager.Instance.CurrentModificators)
        {
            if (!modificator.DisabledModificator)
            {
                if (CursePickManager.Instance != null)
                {
                    try
                    {
                        modificator.OnModificatorChoiseStarted(CursePickManager.Instance);
                    }
                    catch (Exception exc)
                    {
                        Debug.LogError("failed OnModificatorChoiseStarted for modificator: " + modificator.name + " message: " + exc.Message);
                    }
                }
                else if (BlessPickManager.Instance != null)
                {
                    try
                    {
                        modificator.OnModificatorChoiseStarted(CursePickManager.Instance);
                    }
                    catch (Exception exc)
                    {
                        Debug.LogError("failed OnModificatorChoiseStarted for modificator: " + modificator.name + " message: " + exc.Message);
                    }
                }
                else if (MoneyPickManager.Instance != null)
                {
                    try
                    {
                        modificator.OnModificatorChoiseStarted(CursePickManager.Instance);
                    }
                    catch (Exception exc)
                    {
                        Debug.LogError("failed OnModificatorChoiseStarted for modificator: " + modificator.name + " message: " + exc.Message);
                    }
                }
            }
        }
    }

    private void Start()
    {
        UIManager.Instance.ModificatorsScreenOverlay.Show();
        UIManager.Instance.ArtifactModificatorsScreenOverlay.Show();
        UIManager.Instance.DifficultyScreenOverlay.Show();
    }
}
