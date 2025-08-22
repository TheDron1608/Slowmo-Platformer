using UnityEngine;

public class CameraFOVDependsOnScreenResolution : MonoBehaviour
{
    public float FullHDFieldOfView = 30f;
    void Start()
    {
        GetComponent<Camera>().fieldOfView = Screen.width / 1920f * FullHDFieldOfView;
    }
}
