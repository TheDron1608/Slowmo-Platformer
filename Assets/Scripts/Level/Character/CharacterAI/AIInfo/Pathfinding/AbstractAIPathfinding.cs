using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class AbstractAIPathfinding : AbstractAIInfo
{
    public struct PathChainElement
    {
        public Vector2Int TargetPosition;

        public PathChainElement (Vector2Int targetPosition)
        {
            TargetPosition = targetPosition;
        }

        public static void Debug_DrawChain(LinkedList<PathChainElement> chain, Color color, float duration, PathChainElement? singleChain = null)
        {
            if (chain.Count <= 1) return;
            var currentChain = chain.First.Next;
            while (currentChain.Next != null)
            {
                Debug.DrawLine(
                    currentChain.Value.TargetPosition + new Vector2(0.5f, 0.5f),
                    currentChain.Previous.Value.TargetPosition + new Vector2(0.5f, 0.5f),
                    color,
                    duration
                    );
                currentChain = currentChain.Next;
            }
        }

        public static void Debug_DrawChain(LinkedList<PathChainElement> chain, Color color, float duration, PathChainElement singleChain)
        {
            if (chain.Count <= 1) return;
            var currentChain = chain.Find(singleChain);
            if (currentChain.Previous == null) return;
            Debug.DrawLine(
                currentChain.Value.TargetPosition + new Vector2(0.5f, 0.5f),
                currentChain.Previous.Value.TargetPosition + new Vector2(0.5f, 0.5f),
                color,
                duration
                );
        }
    }

    private LinkedList<PathChainElement> _pathChain = new();
    private Vector2? _pathTarget;

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
}