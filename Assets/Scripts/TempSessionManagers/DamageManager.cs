using UnityEngine;

public class DamageManager : MonoBehaviour
{
    public static DamageManager Instance;

    public float GlobalDamageMultiplier = 1f;

    private void Awake()
    {
        if (Instance != null) throw new UnityException("maximum of 1 DamageManager instance");
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}
