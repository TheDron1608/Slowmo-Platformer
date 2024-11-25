using UnityEngine;

public class ButtonOnClickResetCurrentSession : MonoBehaviour
{
    //called when clicked
    public void ResetCurrentSession()
    {
        SessionManager.Instance.ClearCurrentSession();
    }
}
