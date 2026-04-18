using Unity.VisualScripting;
using UnityEngine;

public class MaterialManager : MonoBehaviour
{
    public static MaterialManager Instance;

    [SerializeField]
    public Material defaultmaterial;
    [SerializeField]
    public Material SelectMaterial;

    void Start()
    {
        if (Instance != null && !Instance.IsDestroyed()) throw new UnityException("Limit of 1 Instance of MeterialManager objects");
        Instance = this;

        DontDestroyOnLoad(this);
    }







    private void OnDestroy()
    {
        Instance = null;
    }
}
