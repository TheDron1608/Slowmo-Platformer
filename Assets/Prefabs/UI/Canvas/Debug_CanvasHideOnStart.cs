using UnityEngine;

public class Debug_CanvasHideOnStart : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CanvasGroup cg;
        TryGetComponent<CanvasGroup>(out cg);
        cg.alpha = 0f;
    }
}
