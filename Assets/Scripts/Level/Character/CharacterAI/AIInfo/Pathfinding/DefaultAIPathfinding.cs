using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class DefaultAIPathfinding : AbstractAIPathfinding
{
    public bool CanJumpToTarget = true;
    public GameObject Debug_DrawPathToObject;

    const int PATHINDING_ITERATIONS_LIMIT = 64;

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

    protected override void OnUpdateInfo()
    {
        if (PathTarget == null) return;

        TileManager tileManager = LayerManager.Instance.GetZLayerOfGameObject(gameObject).TileManager;
        List<TileManager.NavigationPlatformInfo> platforms = new(tileManager.NavigationPlatforms);
        TileManager.NavigationPlatformInfo startPlatform = null;
        TileManager.NavigationPlatformInfo targetPlatform = null;
        int maxJumpHeight = CharComponents.CharacterJumping.GetJumpHeight();
        int maxJumpWidth = CharComponents.CharacterJumping.GetJumpWidth();

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
        targetPlatform = tileManager.GetNearestReachablePlatform(PathTarget, maxJumpHeight, maxJumpWidth);
        platforms[platforms.IndexOf(startPlatform)] = null;

        //startPlatform.Debug_DrawPlatform(Color.red, 1f);
        //targetPlatform.Debug_DrawPlatform(Color.blue, 1f);

        PathChainElement currentChain;
        int iterations = 0;
        if (startPlatform != targetPlatform)
        {
            currentChain = new(
                new Vector2Int((int)math.floor(CharComponents.transform.position.x), (int)math.floor(CharComponents.transform.position.y)),
                new Vector2Int((int)CharComponents.transform.position.x, startPlatform.Position.y + 1),
                startPlatform,
                PathTarget
                );
            List<PathChainElement> pathTree = new();
            List<PathChainElement> requiredCalculateChains = new() { currentChain };
            PathChainElement nearestChain = currentChain;

            while (iterations < PATHINDING_ITERATIONS_LIMIT)
            {
                if (currentChain.Platform == targetPlatform)
                {
                    nearestChain = currentChain;
                    break;
                }

                for (int j = 0; j < platforms.Count; j++)
                {
                    if (platforms[j] == null) continue;

                    TileManager.NavigationPlatformTransitionInfo transition = tileManager.TryCreateTransition(currentChain.Platform, platforms[j], currentChain.StartPosition, maxJumpHeight, maxJumpWidth);
                    if (transition != null)
                    {
                        PathChainElement newMoveChain = new(
                            currentChain.TargetPosition,
                            transition.StartConntection,
                            platforms[j],
                            PathTarget
                            );
                        newMoveChain.PrevElement = currentChain;
                        currentChain.NextElement = newMoveChain;

                        PathChainElement newJumpChain = new(
                            transition.StartConntection,
                            transition.EndConnection,
                            platforms[j],
                            PathTarget
                            );
                        newJumpChain.PrevElement = newMoveChain;

                        requiredCalculateChains.Add(newJumpChain);
                        platforms[j] = null;

                        //newMoveChain.Debug_DrawChain(Color.red, .25f);
                        //newJumpChain.Debug_DrawChain(Color.red, .25f);
                    }

                }

                if (currentChain.DistanceToTarget < nearestChain.DistanceToTarget)
                {
                    nearestChain = currentChain;
                }

                requiredCalculateChains.Remove(currentChain);

                //currentChain.Debug_DrawChain(Color.red, .25f);

                if (requiredCalculateChains.Count > 0)
                {
                    currentChain = requiredCalculateChains[0];
                    float leastDistance = currentChain.DistanceToTarget;
                    foreach (PathChainElement chain in requiredCalculateChains)
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

            Vector3Int finalChainElementTargetPosVec3 = tileManager.GetNearestPlatformPositionToPoint(nearestChain.Platform, PathTarget);
            PathChainElement finalChainElement = new(
                nearestChain.TargetPosition,
                new Vector2Int(finalChainElementTargetPosVec3.x, finalChainElementTargetPosVec3.y + 1),
                nearestChain.Platform,
                PathTarget
                );
            finalChainElement.PrevElement = nearestChain;
            nearestChain.NextElement = finalChainElement;
            currentChain = finalChainElement;
        }

        else
        {
            Vector3Int finalChainElementTargetPosVec3 = tileManager.GetNearestPlatformPositionToPoint(targetPlatform, PathTarget);
            currentChain = new(
                new Vector2Int((int)math.round(CharComponents.transform.position.x), (int)math.floor(CharComponents.transform.position.y)),
                new Vector2Int(finalChainElementTargetPosVec3.x, finalChainElementTargetPosVec3.y + 1),
                targetPlatform,
                PathTarget
                );
        }


        _pathChain.Clear();

        iterations = 0;
        do
        {
            _pathChain.Add(currentChain);

            if (CanJumpToTarget)
            {
                Vector2Int pathTargetVec2Int = new Vector2Int((int)math.floor(PathTarget.x), (int)math.round(PathTarget.y));

                TileManager.NavigationPlatformTransitionInfo newTransition = tileManager.TryGetValidJumpTargetPositionFromPlatfromToPoint(
                    currentChain.StartPosition,
                    pathTargetVec2Int,
                    currentChain.Platform,
                    3,
                    4
                    );

                if (newTransition != null)
                {
                    PathChainElement newChain = new(
                        newTransition.StartConntection,
                        newTransition.EndConnection,
                        currentChain.Platform,
                        PathTarget
                        );
                    newChain.PrevElement = currentChain;
                    currentChain.TargetPosition = newChain.StartPosition;
                    currentChain.NextElement = newChain;
                    currentChain = newChain;

                    break;
                }
            }

            currentChain = currentChain.PrevElement;

            iterations++;
            if (iterations > PATHINDING_ITERATIONS_LIMIT) throw new UnityException("iterations limit is reached, pathfinding system probably created invinite loop or too big");
        }
        while (currentChain != null && currentChain.Platform != startPlatform);

        _pathChain[0].Debug_DrawChain(Color.green, .25f, true, true);
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