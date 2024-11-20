using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonOnClickGoToScene : MonoBehaviour
{
    public string GoToSceneName;

    public void GoToScene()
    {
        SceneManager.LoadScene(GoToSceneName);
    }
}
