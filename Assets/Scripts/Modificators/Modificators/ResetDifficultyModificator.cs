
public class ResetDifficultyModificator : AbstractModificator
{
    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        DifficultyManager.Instance.ForceResetDifficulty();

        if (SceneList.GetCurrentSceneIsGameplay())
        {
            if (UIManager.Instance.DifficultyCurseChoiseScreenOverlay.IsShown())
            {
                UIManager.Instance.DifficultyCurseChoiseScreenOverlay.DifficultyCurseChoiseUI.RequestSceneChangeOnFinish(SceneList.GAMEPLAY);
            }
            else
            {
                UIManager.Instance.LoadSceneWithEffect(SceneList.GAMEPLAY);
            }
        }
    }
}