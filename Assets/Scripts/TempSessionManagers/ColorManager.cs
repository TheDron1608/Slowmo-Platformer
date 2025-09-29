using UnityEngine;

[DefaultExecutionOrder(-1)]
public class ColorManager : MonoBehaviour
{
    public static ColorManager Instance;

    public LevelColorset ColorSet;

    private void Awake()
    {
        if (Instance != null) throw new UnityException("maximum of 1 ColorManager instance");
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}
