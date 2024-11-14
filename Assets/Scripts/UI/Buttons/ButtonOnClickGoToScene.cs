using UnityEngine;

public class ButtonOnClickGoToScene : MonoBehaviour
{
    public string GoToSceneName;

    public void GoToScene()
    {
        ScenePreloader.TryLoadPreloadedScenceElseLoadRegulary(GoToSceneName);
    }
}
