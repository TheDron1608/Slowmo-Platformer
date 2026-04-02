using UnityEngine;

public class DebugOnlyGameObject : MonoBehaviour
{
#if DEBUG_BUILD
    private void Awake()
    {
        gameObject.SetActive(true);
    }
#else 
    private void Awake()
    {
        gameObject.SetActive(false);
    }
#endif
}
