using UnityEngine;
using UnityEngine.Tilemaps;

public class TileBehaviour : MonoBehaviour
{
    public enum TileBehaviourType
    {
        FOREBGROUND,
        BACKGROUND,
        WINDOWS,
        BACKGROUND_DECORATIONS,
        OVERGROUND,
        OVERGROUND_DECORATIONS,
        CHUNK_MASK,
        HALLUCINATION_TILES,
        OVERGROUND_HALLUCINATION_TILES
    }

    public enum EnviromentMaterialType
    { 
        PRIMARY,
        SECONDARY,
        BACKGROUND
    }

    public TileBehaviourType BehaviourType;
    public EnviromentMaterialType MaterialType;

    public void SetMaterialDependOnDifficulty(DifficultyManager.DifficultyStage difficulty)
    {
        switch (MaterialType)
        {
            case EnviromentMaterialType.PRIMARY:
                GetComponent<TilemapRenderer>().sharedMaterial = difficulty.PrimaryEnviromentMaterial;
                break;
            case EnviromentMaterialType.SECONDARY:
                GetComponent<TilemapRenderer>().sharedMaterial = difficulty.SecondaryEnviromentMaterial;
                break;
            case EnviromentMaterialType.BACKGROUND:
                GetComponent<TilemapRenderer>().sharedMaterial = difficulty.BackgroundEnviromentMaterial;
                break;
        }
    }
}
