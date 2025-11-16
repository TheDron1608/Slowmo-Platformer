using Unity.Mathematics;
using UnityEngine;

public class PLevelInitializer : MonoBehaviour
{
    private void Start()
    {
        WorldGenerationManager.Instance?.GenerateLevel();
        SpawnManager.Instance?.SpawnPlayerCharacterAtStartPosition();
    }
}
