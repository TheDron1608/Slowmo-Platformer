using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RandomizeTileSprites : AbstractModificator
{
    const int DRAW_OVER_EMPTY_TILES_BORDER_WIDTH = 50;

    public float RandomizeAmount = 0.5f;
    public bool DrawOverEmptyTiles = false;
    public List<Sprite> RandomSpritesPool = new();

    private Tile[] RandomTilesScriptableObjs;

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        if (LayerManager.Instance != null)
        {
            GenerateRandomizedTiles();
        }
    }

    public override void OnLevelGenerated()
    {
        base.OnLevelGenerated();

        GenerateRandomizedTiles();
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorRemoved();

        HideRandomizedTiles();
    }

    private void GenerateRandomizedTiles()
    {
        RandomTilesScriptableObjs = new Tile[RandomSpritesPool.Count];
        for (int i = 0; i < RandomTilesScriptableObjs.Length; i++)
        {
            RandomTilesScriptableObjs[i] = ScriptableObject.CreateInstance<Tile>();
            RandomTilesScriptableObjs[i].sprite = RandomSpritesPool[i];
        }

        Vector3Int currentTilePos;
        Sprite currentSprite;
        foreach (ZIndexLayer layer in LayerManager.Instance.ZLayers)
        {
            BoundsInt totalBounds = layer.MultiTileMapsContainer.GetTotalTilemapCellBounds();
            if (DrawOverEmptyTiles)
            {
                totalBounds.position -= new Vector3Int(DRAW_OVER_EMPTY_TILES_BORDER_WIDTH, DRAW_OVER_EMPTY_TILES_BORDER_WIDTH);
                totalBounds.size += new Vector3Int(DRAW_OVER_EMPTY_TILES_BORDER_WIDTH, DRAW_OVER_EMPTY_TILES_BORDER_WIDTH) * 2;
            }

            for (int y = totalBounds.yMin; y < totalBounds.yMax; y++)
            {
                for (int x = totalBounds.xMin; x < totalBounds.xMax; x++)
                {
                    currentTilePos = new(x, y);
                    currentSprite =
                        layer.MultiTileMapsContainer.GetForeground().GetSprite(currentTilePos) ??
                        layer.MultiTileMapsContainer.GetBackground().GetSprite(currentTilePos) ??
                        layer.MultiTileMapsContainer.GetBackgroundDecorations().GetSprite(currentTilePos);

                    if (currentSprite != null || DrawOverEmptyTiles)
                    {
                        layer.MultiTileMapsContainer.GetHallucinationTilemap().SetTile(
                            currentTilePos,
                            RandomManager.Instance.ProcRandomBadChanceNoTrigger(RandomizeAmount) ?
                                NumberMath.PickRandomItem(RandomTilesScriptableObjs) :
                                (
                                    layer.MultiTileMapsContainer.GetForeground().GetTile(currentTilePos) ??
                                    layer.MultiTileMapsContainer.GetBackground().GetTile(currentTilePos) ??
                                    layer.MultiTileMapsContainer.GetBackgroundDecorations().GetTile(currentTilePos)
                                )
                            );
                    }
                }
            }

            layer.MultiTileMapsContainer.ToggleHallucinationTilemapVisibility(true);
        }
    }

    private void HideRandomizedTiles()
    {
        if (LayerManager.Instance != null)
        {
            foreach (ZIndexLayer layer in LayerManager.Instance.ZLayers)
            {
                layer.MultiTileMapsContainer.ToggleHallucinationTilemapVisibility(false);
            }
        }
    }
}