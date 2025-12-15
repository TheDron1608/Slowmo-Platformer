using UnityEngine;
using UnityEngine.SceneManagement;

public class OnInteractEnterNextLevelDoor : AnimatedInteractable
{
    public string GameplaySceneName = "Gameplay";
    public string ModificatorChoiseSceneName = "ModificatorChoise";

    protected override void OnFinishInteract(GameObject interactor)
    {
        base.OnFinishInteract(interactor);

        if (ModificatorsManager.Instance == null || ModificatorsManager.Instance.ModifiactorsPickAmount == 0)
        {
            UIManager.Instance.LoadSceneWithEffect(GameplaySceneName);
        }
        else
        {
            UIManager.Instance.LoadSceneWithEffect(ModificatorChoiseSceneName);
        }
    }
}
