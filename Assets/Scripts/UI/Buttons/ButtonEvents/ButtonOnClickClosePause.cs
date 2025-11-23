using UnityEngine;

public class ButtonOnClickClosePause : MonoBehaviour
{
    public void ClosePause()
    {
        GameplayUIManager.GetInstance().Pause.Paused = false;
    }
}
