using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class DefaultAIPathfinding : AbstractAIPathfinding
{
    protected override void OnUpdateInfo()
    {
        List<TileManager.NavigationPlatformInfo> platforms = LayerManager.Instance.GetZLayerOfGameObject(gameObject).TileManager.NavigationPlatforms;
        TileManager.NavigationPlatformInfo startPlatform = null;

        for (int i = 0; i < platforms.Count; i++)
        {
            if (
                platforms[i].GetIsUnderVector(CharComponents.transform.position) &&
                (startPlatform == null || startPlatform.Position.y < platforms[i].Position.y)
                )
            {
                startPlatform = platforms[i];
            }
        }
    }

    private void Start()
    {
        OnUpdateInfo();
    }
}