using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class DefaultAIPathfinding : AbstractAIPathfinding
{
    public bool CanJumpToTarget = true;
    public Color Debug_PathColor = new Color(1, 1, 1, 0);

    const int PATHINDING_ITERATIONS_LIMIT = 64;

    private class PathChainElementPrecalculated
    {
        public Vector2Int StartPosition;
        public Vector2Int TargetPosition;
        public TileManager.NavigationPlatformInfo Platform;
        public PathChainElementPrecalculated PrevElement = null;
        public PathChainElementPrecalculated NextElement = null;
        public readonly float DistanceToTarget;

        public PathChainElementPrecalculated(Vector2Int startPosition, Vector2Int targetPosition, TileManager.NavigationPlatformInfo platform, Vector2 pathTarget)
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

        public PathChainElement ConvertToPathChainElement()
        {
            PathChainElement.PathChainElementType type;
            if (StartPosition.y < TargetPosition.y)
            {
                type = PathChainElement.PathChainElementType.MOVE_OFF_PLATFORM_UP;
            }
            else if (StartPosition.y > TargetPosition.y)
            {
                type = PathChainElement.PathChainElementType.MOVE_OFF_PLATFORM_DOWN;
            }
            else if (Platform.GetPositionInOnPlatform(StartPosition) ^ Platform.GetPositionInOnPlatform(TargetPosition))
            {
                type = PathChainElement.PathChainElementType.MOVE_OFF_PLATFORM_MIDDLE;
            }
            else
            {
                type = PathChainElement.PathChainElementType.MOVE_ON_PLATFORM;
            }

            return new PathChainElement(TargetPosition, type);
        }
    }

    protected override void OnUpdateInfo()
    {
        if (PathTarget.Value == null)
        {
            PathChain.Clear();
            return;
        }

        TileManager tileManager = LayerManager.Instance.GetZLayerOfGameObject(gameObject).TileManager;
        List<TileManager.NavigationPlatformInfo> platforms = new(tileManager.NavigationPlatforms);
        TileManager.NavigationPlatformInfo startPlatform = null;
        TileManager.NavigationPlatformInfo targetPlatform = null;
        int maxJumpHeight = CharComponents.CharacterJumping.GetJumpHeight();
        int maxJumpWidth = CharComponents.CharacterJumping.GetJumpWidth();
        Vector2Int characterTilePosition = TileManager.PositionToTilePosition(CharComponents.transform.position);

        for (int i = 0; i < platforms.Count; i++)
        {
            if (platforms[i].GetPositionInOnPlatform(characterTilePosition))
            {
                startPlatform = platforms[i];
                break;
            }
        }
        if (startPlatform == null) return;

        targetPlatform = tileManager.GetNearestReachablePlatform(PathTarget.Value, maxJumpHeight, maxJumpWidth);
        platforms[platforms.IndexOf(startPlatform)] = null;
        _startTarget = characterTilePosition;

        PathChainElementPrecalculated currentChain;
        int iterations = 0;
        if (startPlatform != targetPlatform)
        {
            currentChain = new(
                TileManager.PositionToTilePosition(CharComponents.transform.position),
                new Vector2Int((int)CharComponents.transform.position.x, startPlatform.Position.y + 1),
                startPlatform,
                PathTarget.Value
                );
            List<PathChainElementPrecalculated> pathTree = new();
            List<PathChainElementPrecalculated> requiredCalculateChains = new() { currentChain };
            PathChainElementPrecalculated nearestChain = currentChain;

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
                        PathChainElementPrecalculated newMoveChain = new(
                            currentChain.TargetPosition,
                            transition.StartConntection,
                            platforms[j],
                            PathTarget.Value
                            );
                        newMoveChain.PrevElement = currentChain;
                        currentChain.NextElement = newMoveChain;

                        PathChainElementPrecalculated newJumpChain = new(
                            transition.StartConntection,
                            transition.EndConnection,
                            platforms[j],
                            PathTarget.Value
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
                    foreach (PathChainElementPrecalculated chain in requiredCalculateChains)
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

            Vector3Int finalChainElementTargetPosVec3 = tileManager.GetNearestPlatformPositionToPoint(nearestChain.Platform, PathTarget.Value);
            PathChainElementPrecalculated finalChainElement = new(
                nearestChain.TargetPosition,
                new Vector2Int(finalChainElementTargetPosVec3.x, finalChainElementTargetPosVec3.y + 1),
                nearestChain.Platform,
                PathTarget.Value
                );
            finalChainElement.PrevElement = nearestChain;
            nearestChain.NextElement = finalChainElement;
            currentChain = finalChainElement;
        }

        else
        {
            Vector3Int finalChainElementTargetPosVec3 = tileManager.GetNearestPlatformPositionToPoint(targetPlatform, PathTarget.Value);
            currentChain = new(
                TileManager.PositionToTilePosition(CharComponents.transform.position),
                new Vector2Int(finalChainElementTargetPosVec3.x, finalChainElementTargetPosVec3.y + 1),
                targetPlatform,
                PathTarget.Value
                );
        }


        PathChain.Clear();

        Vector2Int pathTargetVec2Int = TileManager.PositionToTilePosition(PathTarget.Value);
        iterations = 0;
        do
        {
            PathChain.AddFirst(currentChain.ConvertToPathChainElement());

            if (CanJumpToTarget)
            {

                TileManager.NavigationPlatformTransitionInfo newTransition = tileManager.TryGetValidJumpTargetPositionFromPlatfromToPoint(
                    currentChain.StartPosition,
                    pathTargetVec2Int,
                    currentChain.Platform,
                    maxJumpHeight + 1,
                    maxJumpWidth
                    );

                if (newTransition != null && newTransition.StartConntection != characterTilePosition)
                {
                    PathChainElementPrecalculated newChain = new(
                        newTransition.StartConntection,
                        newTransition.EndConnection,
                        currentChain.Platform,
                        PathTarget.Value
                        );
                    newChain.PrevElement = currentChain;
                    currentChain.TargetPosition = newChain.StartPosition;
                    currentChain.NextElement = newChain;

                    PathChain.Clear();
                    PathChain.AddFirst(newChain.ConvertToPathChainElement());
                    PathChain.AddFirst(currentChain.ConvertToPathChainElement());
                }
            }

            currentChain = currentChain.PrevElement;

            iterations++;
            if (iterations > PATHINDING_ITERATIONS_LIMIT) throw new UnityException("iterations limit is reached, pathfinding system probably created invinite loop or too big");
        }
        while (currentChain != null && currentChain.Platform != startPlatform);
    }

    protected override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        if (Debug_PathColor.a != 0f)
        {
            Debug_DrawChain(Debug_PathColor, Time.fixedDeltaTime);
        }
    }
}