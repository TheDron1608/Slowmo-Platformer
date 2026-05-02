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
                    modificator.OnModificatorChoiseStarted(CursePickManager.Instance);
                }
                else if (BlessPickManager.Instance != null)
                {
                    modificator.OnModificatorChoiseStarted(BlessPickManager.Instance);
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
