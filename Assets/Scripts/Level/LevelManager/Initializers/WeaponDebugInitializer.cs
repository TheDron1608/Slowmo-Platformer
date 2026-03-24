using System.Collections.Generic;
using UnityEngine;

public class WeaponDebugInitializer : MonoBehaviour
{
    public List<AbstractModificator> StartModificators;
    public CharacterComponentsManager TrackedCharacter;

    private void Start()
    {
        foreach (AbstractModificator modificator in StartModificators)
        {
            ModificatorsManager.Instance.AddModificator(modificator, AbstractModificator.ModificatorStatuses.PERMANENT);
        }

        UIManager.Instance.GameplayScreenOverlay.Show();
        UIManager.Instance.ModificatorsScreenOverlay.Show();

        GameplayUIManager.GetInstance().AddTrackedCharacter(TrackedCharacter);

        foreach (AbstractModificator modificator in ModificatorsManager.Instance.CurrentModificators)
        {
            if (!modificator.DisabledModificator)
            {
                modificator.OnLevelPreGenerated();
            }
        }
        TeamManager.Instance.OnLevelPreGenerated();

        foreach (ZIndexLayer layer in LayerManager.Instance.ZLayers)
        {
            layer.Debug_ArtificalInvokeOnObjectSpawnedForAll();
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
