using UnityEngine;

public class ModificatorChoiseInitializer : MonoBehaviour
{
    private void Start()
    {
        UIManager.Instance.ModificatorsScreenOverlay.Show();

        foreach (AbstractModificator modificator in ModificatorsManager.Instance.CurrentModificators)
        {
            modificator.OnModificatorChoiseStarted();
        }
    }
}
