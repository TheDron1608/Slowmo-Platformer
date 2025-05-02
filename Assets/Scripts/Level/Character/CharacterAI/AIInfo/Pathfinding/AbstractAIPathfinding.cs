using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class AbstractAIPathfinding : AbstractAIInfo
{
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

        public PathChainElement (Vector2Int targetPosition, PathChainElementType type)
        {
            TargetPosition = targetPosition;
            Type = type;
        }
    }

    private LinkedList<PathChainElement> _pathChain = new();
    private Vector2? _pathTarget;

    protected Vector2Int? _startTarget;

    public event EventHandler OnPathUpdated;

    public LinkedList<PathChainElement> PathChain
    {
        get => _pathChain;
        protected set => _pathChain = value;
    }

    public Vector2? PathTarget
    {
        get => _pathTarget;
        set
        {
            if (_pathTarget != value)
            {
                _pathTarget = value;
                TryUpdateInfo();
                OnPathUpdated?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public void Debug_DrawChain(Color color, float duration, PathChainElement? singleChain = null)
    {
        if (PathChain == null || PathChain.Count == 0) return;
        var currentChain = PathChain.First;
        do
        {
            if (singleChain.HasValue && currentChain.Value.TargetPosition != singleChain.Value.TargetPosition)
            {
                currentChain = currentChain.Next;
                continue;
            }

            Debug.DrawLine(
                currentChain.Value.TargetPosition + new Vector2(0.5f, 0.5f),
                (currentChain.Previous?.Value.TargetPosition ?? _startTarget.Value) + new Vector2(0.5f, 0.5f),
                color,
                duration
                );
            Debug.DrawLine(
                currentChain.Value.TargetPosition + new Vector2(0.4f, 0.4f),
                currentChain.Value.TargetPosition + new Vector2(0.6f, 0.6f),
                color,
                duration
                );
            Debug.DrawLine(
                currentChain.Value.TargetPosition + new Vector2(0.4f, 0.6f),
                currentChain.Value.TargetPosition + new Vector2(0.6f, 0.4f),
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