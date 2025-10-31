using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SimpleAIPathfinding : AbstractAIPathfinding
{
    protected override void OnUpdateInfo()
    {
        PathChain = new();

        if (PathTarget.HasValue)
        {
            PathChain.AddLast(new PathChainElement(TileManager.PositionToTilePosition(PathTarget.Value.Position), PathChainElement.PathChainElementType.MOVE_ON_PLATFORM));
        }
    }
}