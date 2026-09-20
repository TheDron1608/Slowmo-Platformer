using System.Collections.Generic;
using UnityEngine;

public class WeaponDebugInitializer : MonoBehaviour
{
    public List<AbstractModificator> StartModificators;
    public CharacterComponentsManager TrackedCharacter;
    public int StartCombo = 0;
    public bool LimitFPS = false;
    public int MaxFPS = 999;

    private void Start()
    {
        if (LimitFPS) Application.targetFrameRate = MaxFPS;

        ScoreManager.Instance.CurrentCombo = StartCombo;

        UIManager.Instance.GameplayScreenOverlay.Show();
        UIManager.Instance.ModificatorsScreenOverlay.Show();
        UIManager.Instance.ArtifactModificatorsScreenOverlay.Show();
        UIManager.Instance.DifficultyScreenOverlay.Show();

        GameplayUIManager.GetInstance().AddTrackedCharacter(TrackedCharacter.UITrack);

        foreach (AbstractModificator modificator in ModificatorsManager.Instance.CurrentModificators)
        {
            if (!modificator.DisabledModificator)
            {
                modificator.OnLevelPreGenerated();
            }
        }

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

        foreach (AbstractModificator modificator in StartModificators)
        {
            ModificatorsManager.Instance.AddModificator(modificator, AbstractModificator.ModificatorStatuses.PERMANENT);
        }
    }
}
