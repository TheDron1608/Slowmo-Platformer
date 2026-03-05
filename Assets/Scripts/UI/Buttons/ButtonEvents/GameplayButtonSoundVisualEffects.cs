using UnityEngine.SceneManagement;

public class GameplayButtonSoundVisualEffects : ButtonSoundVisualEffects
{
    protected override bool SelectCondition()
    {
        return
            base.SelectCondition() &&
            (SceneManager.GetActiveScene().name != "Gameplay" || (TimeManager.Instance?.Paused ?? true));
    }
}
