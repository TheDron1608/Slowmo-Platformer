using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public int CurrentScore = 0;
    public int CurrentCombo = 0;

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
