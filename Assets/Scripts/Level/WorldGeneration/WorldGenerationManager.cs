using System.Collections.Generic;
using System.Linq;
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

        NumberMath.PickRandomItem(Chunks).ForceGenerateChunk(container, position, out ChunkConnectionPosition[] firstChunkConnections);

        for (int i = 1; i < chunksAmount; i++)
        {
            if (container.GetHasAnyTileAt(prefferedPosition)) break;

            foreach (
                ChunkConnectionPosition avaibleConnection in 
                container.GetComponentsInChildren<ChunkConnectionPosition>(false).OrderBy(
                    (ChunkConnectionPosition connection) => Vector3.Distance(connection.transform.position, prefferedPosition)
                    )
                )
            {
                if (!avaibleConnection.isActiveAndEnabled) continue;

                bool successfullGenerating = false;
                for (int j = 0; j < GENERATION_FAIL_ITERATIONS_LIMIT; j++)
                {
                    if (NumberMath.PickRandomItem(Chunks).TryAddChunk(container, avaibleConnection, out ChunkConnectionPosition newChunkConnection))
                    {
                        newChunkConnection.DestroyConnection();
                        avaibleConnection.DestroyConnection();
                        successfullGenerating = true;
                        break;
                    }
                }
                if (successfullGenerating) break;
            }
        }

        foreach (ChunkConnectionPosition unclosedConnection in container.GetComponentsInChildren<ChunkConnectionPosition>(false))
        {
            if (!unclosedConnection.isActiveAndEnabled) continue;

            bool needCloseConnection = true;
            foreach (ChunkConnectionPosition unclosedConnection2 in container.GetComponentsInChildren<ChunkConnectionPosition>(false))
            {
                if (unclosedConnection != unclosedConnection2 && unclosedConnection.GetTilePosition() == unclosedConnection2.GetTilePosition())
                {
                    unclosedConnection.OnOpenedChunkConnection();
                    unclosedConnection.DestroyConnection();
                    unclosedConnection2.DestroyConnection();
                    needCloseConnection = false;
                    break;
                }
            }

            if (needCloseConnection)
            {
                unclosedConnection.OnClosedChunkConnection();
            }
        }

        Random.state = oldState;
    }

    /*private void Awake()
    {
        _randomState = Random.state;
        Random.InitState(Seed);

        foreach (var layer in LayerManager.Instance.ZLayers)
        {
            GenerateWorld(layer.MultiTileMapsContainer, Vector3Int.zero, 256, new Vector3Int(50, 50));
        }
    }*/
}
