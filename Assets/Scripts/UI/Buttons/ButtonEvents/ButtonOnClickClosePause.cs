using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonOnClickClosePause : MonoBehaviour
{
    public void ClosePause()
    {
        GameplayUIManager.Instance.Pause.Paused = false;
    }
}
