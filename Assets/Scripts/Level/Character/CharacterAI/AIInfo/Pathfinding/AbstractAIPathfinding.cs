using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractAIPathfinding : AbstractAIInfo
{

    public class PathChainElement
    {
        public Vector2Int StartPosition;
        public Vector2Int TargetPosition;
        public TileManager.NavigationPlatformInfo Platform;
        public PathChainElement PrevElement = null;
        public PathChainElement NextElement = null;
        public readonly float DistanceToTarget;

        public PathChainElement(Vector2Int startPosition, Vector2Int targetPosition, TileManager.NavigationPlatformInfo platform, Vector2 pathTarget)
        {
            StartPosition = startPosition;
            TargetPosition = targetPosition;
            Platform = platform;
            DistanceToTarget = Vector2.Distance(targetPosition, pathTarget);
        }

        public void Debug_DrawChain(Color color, float duration, bool recursivePrevInvoke = false, bool recursiveNextInvoke = false)
        {
            Debug.DrawLine(StartPosition + new Vector2(0.5f, 0.5f), TargetPosition + new Vector2(0.5f, 0.5f), color, duration);
            if (recursiveNextInvoke)
            {
                NextElement?.Debug_DrawChain(color, duration, false, true);
            }
            if (recursivePrevInvoke)
            {
                PrevElement?.Debug_DrawChain(color, duration, true, false);
            }
        }
    }

    private List<PathChainElement> _pathChain = new();
    private Vector2? _pathTarget;

    public List<PathChainElement> PathChain
    {
        get => _pathChain;
        protected set => _pathChain = value;
    }

    public Vector2? PathTarget
    {
        get => _pathTarget;
        set
        {
            Vector2? oldValue = _pathTarget;
            _pathTarget = value;
            if (oldValue != value)
            {
                TryUpdateInfo();
            }
        }
    }
}