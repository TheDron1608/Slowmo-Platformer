using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class DefaultAIPathfinding : AbstractAIPathfinding
{
    public GameObject Debug_DrawPathToObject;

    const int PATHINDING_ITERATIONS_LIMIT = 64;

    private class PathChainElementPreCalculated
    {
        public TileManager.NavigationPlatformInfo Platform;
        public Vector2Int EnterPosition;
        public TileManager.NavigationPlatformTransitionInfo Transition = null;
        public PathChainElementPreCalculated PreviousElement = null;
        public float DistanceToTarget;

        public PathChainElementPreCalculated(TileManager.NavigationPlatformInfo platform, Vector2Int enterPosition, TileManager.NavigationPlatformTransitionInfo transition, PathChainElementPreCalculated previousElement, Vector2 pathTarget)
        {
            Platform = platform;
            EnterPosition = enterPosition;
            Transition = transition;
            PreviousElement = previousElement;
            DistanceToTarget = Vector2.Distance(new Vector2(Platform.Position.x + (float)Platform.Width / 2, Platform.Position.y), pathTarget);
        }

        public void Debug_DrawChain(Color color, float duration)
        {
            Platform.Debug_DrawPlatform(color, duration);
            Transition?.Debug_DrawTransition(color, duration);
            PreviousElement?.Debug_DrawChain(color, duration);
        }
    }

    private class PathChainElement
    {
        public Vector2Int StartPosition;
        public Vector2Int TargetPosition;
        public bool IsJump;
        public PathChainElement NextElement;

        public PathChainElement(Vector2Int startPosition, Vector2Int targetPosition, bool isJump, PathChainElement nextElement)
        {
            StartPosition = startPosition;
            TargetPosition = targetPosition;
            IsJump = isJump;
            NextElement = nextElement;
        }

        public void Debug_DrawChain(Color color, float duration, bool recursiveInvoke = false)
        {
            Debug.DrawLine(StartPosition + new Vector2(0.5f, 0.5f), TargetPosition + new Vector2(0.5f, 0.5f), color, duration);
            if (recursiveInvoke)
            {
                NextElement?.Debug_DrawChain(color, duration, true);
            }
        }
    }

    private List<PathChainElement> _pathChain = new();

    protected override void OnUpdateInfo()
    {
        if (PathTarget == null) return;

        TileManager tileManager = LayerManager.Instance.GetZLayerOfGameObject(gameObject).TileManager;
        List<TileManager.NavigationPlatformInfo> platforms = new(tileManager.NavigationPlatforms);
        TileManager.NavigationPlatformInfo startPlatform = null;
        TileManager.NavigationPlatformInfo targetPlatform = null;
        int maxJumpHeight = (int)math.floor(CharComponents.CharacterJumping.GetJumpHeight());
        int maxJumpWidth = (int)math.floor(CharComponents.CharacterJumping.GetJumpWidth());

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
        targetPlatform = tileManager.GetNearestReachablePlatform(PathTarget, 3, 4);
        platforms.Remove(startPlatform);

        //startPlatform.Debug_DrawPlatform(Color.red, 1f);
        //targetPlatform.Debug_DrawPlatform(Color.blue, 1f);


        PathChainElementPreCalculated currentChain = new(
            startPlatform,
            new Vector2Int((int)CharComponents.transform.position.x, startPlatform.Position.y + 1),
            null,
            null,
            PathTarget
            );
        int iterations = 0;
        List<PathChainElementPreCalculated> pathTree = new();
        List<PathChainElementPreCalculated> requiredCalculateChains = new() { currentChain };
        PathChainElementPreCalculated nearestChain = currentChain;

        while (iterations < PATHINDING_ITERATIONS_LIMIT)
        {
            for (int j = 0; j < platforms.Count; j++)
            {
                TileManager.NavigationPlatformTransitionInfo transition = tileManager.TryCreateTransition(currentChain.Platform, platforms[j], currentChain.EnterPosition, 3, 4);
                if (transition != null)
                {
                    PathChainElementPreCalculated newChain = new(
                        platforms[j],
                        transition.EndConnection,
                        transition,
                        currentChain,
                        PathTarget
                        );

                    requiredCalculateChains.Add(newChain);
                    platforms.RemoveAtSwapBack(j);
                    j--;
                }

            }

            if (currentChain.DistanceToTarget < nearestChain.DistanceToTarget)
            {
                nearestChain = currentChain;
            }

            requiredCalculateChains.Remove(currentChain);

            currentChain.Debug_DrawChain(Color.red, .25f);

            if (currentChain.Platform == targetPlatform)
            {
                nearestChain = currentChain;
                break;
            }

            if (requiredCalculateChains.Count > 0)
            {
                currentChain = requiredCalculateChains[0];
                float leastDistance = currentChain.DistanceToTarget;
                foreach (PathChainElementPreCalculated chain in requiredCalculateChains)
                {
                    if (chain.DistanceToTarget < leastDistance)
                    {
                        currentChain = chain;
                    }
                }
            }
            else
            {
                break;
            }

            iterations++;
        }

        _pathChain.Clear();

        iterations = 0;
        while (currentChain.Platform != startPlatform)
        {
            currentChain = currentChain.PreviousElement;
            iterations++;
            if (iterations > PATHINDING_ITERATIONS_LIMIT) throw new UnityException("iterations limit is reached, pathfinding system probably created invinite loop or too big");
        }

        
        nearestChain.Debug_DrawChain(Color.green, .25f);
    }

    protected override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        if (Debug_DrawPathToObject != null)
        {
            PathTarget = Debug_DrawPathToObject.transform.position;
        }
    }
}