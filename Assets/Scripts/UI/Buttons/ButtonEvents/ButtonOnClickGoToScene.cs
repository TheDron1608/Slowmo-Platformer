using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonOnClickGoToScene : MonoBehaviour
{
    public string GoToSceneName;

    public void GoToScene()
    {
        UIManager.Instance.LoadSceneWithEffect(GoToSceneName);
    }
}
