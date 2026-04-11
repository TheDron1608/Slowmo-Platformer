using UnityEngine;

public class ModificatorChoiseInitializer : MonoBehaviour
{

    private void Awake()
    {
        foreach (AbstractModificator modificator in ModificatorsManager.Instance.CurrentModificators)
        {
            if (!modificator.DisabledModificator)
            {
                modificator.OnModificatorChoiseStarted();
            }
        }
    }

    private void Start()
    {
        UIManager.Instance.ModificatorsScreenOverlay.Show();
        UIManager.Instance.DifficultyScreenOverlay.Show();
    }
}
