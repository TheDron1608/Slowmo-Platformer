using UnityEngine;

public class ModificatorChoiseInitializer : MonoBehaviour
{

    private void Awake()
    {
        foreach (AbstractModificator modificator in ModificatorsManager.Instance.CurrentModificators)
        {
            modificator.OnModificatorChoiseStarted();
        }
    }

    private void Start()
    {
        UIManager.Instance.ModificatorsScreenOverlay.Show();
    }
}
