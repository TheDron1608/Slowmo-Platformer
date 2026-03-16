using UnityEngine.SceneManagement;

public class GameplayButtonSoundVisualEffects : ButtonSoundVisualEffects
{
    protected override bool SelectCondition()
    {
        return
            base.SelectCondition() &&
            (!SceneList.GetCurrentSceneIsGameplay() || (TimeManager.Instance?.Paused ?? true));
    }
}
