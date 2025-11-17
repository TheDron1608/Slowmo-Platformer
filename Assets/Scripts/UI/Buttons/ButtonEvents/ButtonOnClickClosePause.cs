using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonOnClickClosePause : MonoBehaviour
{
    public void ClosePause()
    {
        GameplayUIManager.GetInstance().Pause.Paused = false;
    }
}
