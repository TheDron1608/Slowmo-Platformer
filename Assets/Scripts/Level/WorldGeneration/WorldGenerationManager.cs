using System.Collections.Generic;
using System.Linq;
using UnityEditor.MemoryProfiler;
using UnityEngine;

public class WorldGenerationManager : MonoBehaviour
{
    const int GENERATION_FAIL_ITERATIONS_LIMIT = 4;

    public List<Chunk> Chunks = new();
    public int Seed;

    private Random.State _randomState;

    public void GenerateWorld(MultiTileMapsContainer container, Vector3Int position, int chunksAmount, Vector3Int prefferedPosition)
    {
        Random.State oldState = Random.state;
        Random.state = _randomState;

        NumberMath.PickRandomItem(Chunks).ForceGenerateChunk(container, position, out ChunkConnection[] firstChunkConnections);

        for (int i = 1; i < chunksAmount; i++)
        {
            if (container.GetHasAnyTileAt(prefferedPosition)) break;

            foreach (
                ChunkConnection avaibleConnection in 
                container.GetComponentsInChildren<ChunkConnection>(false).OrderBy(
                    (ChunkConnection connection) => Vector3.Distance(connection.transform.position, prefferedPosition)
                    )
                )
            {
                bool successfullGenerating = false;
                for (int j = 0; j < GENERATION_FAIL_ITERATIONS_LIMIT; j++)
                {
                    if (NumberMath.PickRandomItem(Chunks).TryAddChunk(container, avaibleConnection, out ChunkConnection newChunkConnection))
                    {
                        newChunkConnection.gameObject.SetActive(false);
                        avaibleConnection.gameObject.SetActive(false);
                        Destroy(newChunkConnection.gameObject);
                        Destroy(avaibleConnection.gameObject);
                        successfullGenerating = true;
                        break;
                    }
                }
                if (successfullGenerating) break;
            }
        }

        foreach (ChunkConnection unclosedConnections in container.GetComponentsInChildren<ChunkConnection>(false))
        {
            unclosedConnections.CloseChunkConnection();
        }

        Random.state = oldState;
    }

    private void Awake()
    {
        _randomState = Random.state;
        Random.InitState(Seed);

        foreach (var layer in LayerManager.Instance.ZLayers)
        {
            GenerateWorld(layer.MultiTileMapsContainer, Vector3Int.zero, 256, new Vector3Int(50, 50));
        }
    }
}
