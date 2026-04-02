using UnityEngine;

public class ButtonOnClickSelectDebugSession : MonoBehaviour
{
    //called when clicked
    public void OnClick()
    {
        SessionManager.Instance.CurrentSession = new();
    }
}
