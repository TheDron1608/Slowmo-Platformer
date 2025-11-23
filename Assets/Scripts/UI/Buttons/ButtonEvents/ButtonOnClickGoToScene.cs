using UnityEngine;

public class ButtonOnClickGoToScene : MonoBehaviour
{
    public string GoToSceneName;

    public void GoToScene()
    {
        UIManager.Instance.LoadSceneWithEffect(GoToSceneName);
    }
}
