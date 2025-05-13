using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class AbstractAIPathfinding : AbstractAIInfo
{
    public struct PathPosition
    {
        public Vector2Int Position;
        public ZIndexLayer Layer;

        public PathPosition(Vector2Int position, ZIndexLayer layer)
        {
            Position = position;
            Layer = layer;
        }
        public PathPosition(Vector2 position, ZIndexLayer layer)
        {
            Position = TileManager.PositionToTilePosition(position);
            Layer = layer;
        }
        public PathPosition(Vector2Int position, GameObject pathFinder)
        {
            Position = position;
            Layer = LayerManager.Instance.GetZLayerOfGameObject(pathFinder);
        }
        public PathPosition(Vector2 position, GameObject pathFinder)
        {
            Position = TileManager.PositionToTilePosition(position);
            Layer = LayerManager.Instance.GetZLayerOfGameObject(pathFinder);
        }

        public override bool Equals(object obj)
        {
            return obj is PathPosition position &&
                   Position.Equals(position.Position) &&
                   EqualityComparer<ZIndexLayer>.Default.Equals(Layer, position.Layer);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Position, Layer);
        }

        public static bool operator == (PathPosition a, PathPosition b)
        {
            return a.Position == b.Position && a.Layer == b.Layer;
        }
        public static bool operator != (PathPosition a, PathPosition b)
        {
            return !(a == b);
        }
    }

    public struct PathChainElement
    {
        public enum PathChainElementType
        {
            MOVE_ON_PLATFORM,
            MOVE_OFF_PLATFORM_DOWN,
            MOVE_OFF_PLATFORM_MIDDLE,
            MOVE_OFF_PLATFORM_UP
        }

        public Vector2Int TargetPosition;
        public PathChainElementType Type;
        public Interactable RequiredIteractableToContinue;

        public PathChainElement (Vector2Int targetPosition, PathChainElementType type)
        {
            TargetPosition = targetPosition;
            Type = type;
            RequiredIteractableToContinue = null;
        }
        public PathChainElement(Vector2Int targetPosition, PathChainElementType type, Interactable requiredInteractableToContinue)
        {
            TargetPosition = targetPosition;
            Type = type;
            RequiredIteractableToContinue = requiredInteractableToContinue;
        }
    }

    private LinkedList<PathChainElement> _pathChain = new();
    private PathPosition? _pathTarget;

    public event EventHandler OnPathUpdated;

    public LinkedList<PathChainElement> PathChain
    {
        get => _pathChain;
        protected set => _pathChain = value;
    }

    public PathPosition? PathTarget
    {
        get => _pathTarget;
        set
        {
            _pathTarget = value;
            TryUpdateInfo();
            OnPathUpdated?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Debug_DrawChain(Color color, float duration, PathChainElement? singleChain = null)
    {
        if (PathChain == null || PathChain.Count <= 1) return;
        var currentChain = PathChain.First.Next;
        do
        {
            if (singleChain.HasValue && currentChain.Value.TargetPosition != singleChain.Value.TargetPosition)
            {
                currentChain = currentChain.Next;
                continue;
            }

            Debug.DrawLine(
                currentChain.Value.TargetPosition + new Vector2(0.5f, 0.5f),
                currentChain.Previous.Value.TargetPosition + new Vector2(0.5f, 0.5f),
                color,
                duration
                );
            
            if (singleChain.HasValue && currentChain.Value.TargetPosition != singleChain.Value.TargetPosition)
            {
                break;
            }

            currentChain = currentChain.Next;
        }
        while (currentChain != null);
    }
}