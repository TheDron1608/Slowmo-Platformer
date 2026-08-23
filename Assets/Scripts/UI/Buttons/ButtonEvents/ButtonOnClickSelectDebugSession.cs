using UnityEngine;

public class ButtonOnClickSelectDebugSession : MonoBehaviour
{
    //called when clicked
    public void OnClick()
    {
        AnalyticsManager.Instance.CollectData = false;
        SessionManager.Instance.CurrentSession = new();
        SessionManager.Instance.CurrentSession.IsDebug = true;
    }
}
