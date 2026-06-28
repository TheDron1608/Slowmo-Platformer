using UnityEngine;

public class ButtonOnClickGoToUrl : MonoBehaviour
{
    public string Url = "";

    public void GoToLink()
    {
        Application.OpenURL(Url);
    }
}
