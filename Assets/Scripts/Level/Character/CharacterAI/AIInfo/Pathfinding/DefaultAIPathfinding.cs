using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Profiling;

public class DefaultAIPathfinding : AbstractAIPathfinding
{
    public bool CanJumpToTarget = true;
    public Color Debug_PathColor = new Color(1, 1, 1, 0);
    public bool AdvancedPathinding = false;

    const int DEFAULT_ITERATIONS_LIMIT = 16;
    const int ADVANCED_ITERATIONS_LIMIT = 64;

    private class PathChainElementPrecalculated
    {
        public Vector2Int StartPosition;
        public Vector2Int TargetPosition;
        public TileManager.NavigationPlatformInfo Platform;
        public PathChainElementPrecalculated PrevElement = null;
        public readonly float DistanceToTarget;

        public PathChainElementPrecalculated(Vector2Int startPosition, Vector2Int targetPosition, TileManager.NavigationPlatformInfo platform, Vector2 pathTarget)
        {
            StartPosition = startPosition;
            TargetPosition = targetPosition;
            Platform = platform;
            DistanceToTarget = Vector2.Distance(targetPosition, pathTarget);
        }

        public void Debug_DrawChain(Color color, float duration, bool recursiveInvoke = false)
        {
            Debug.DrawLine(StartPosition + new Vector2(0.5f, 0.5f), TargetPosition + new Vector2(0.5f, 0.5f), color, duration);
            if (recursiveInvoke)
            {
                PrevElement?.Debug_DrawChain(color, duration, true);
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

    public override bool GetIsAbleToReachPathTarget()
    {
        return
            base.GetIsAbleToReachPathTarget() &&
            (
                !PathTarget.HasValue || PathChain?.Last == null ||
                PathTarget.Value.Position == PathChain?.Last.Value.TargetPosition
            );
    }

    protected override void OnUpdateInfo()
    {
        PathChain = new();

        if (!PathTarget.HasValue)
        {
            return;
        }
        Profiler.BeginSample("DefaultAIPathfinding.UpdateInfo");

        Vector2Int characterTilePosition = TileManager.PositionToTilePosition(transform.position);

        if (LayerManager.Instance.GetZLayerOfGameObject(gameObject) != PathTarget.Value.Layer)
        {
            if (!AdvancedPathinding)
            {
                Profiler.EndSample();
                return;
            }

            List<OnInteractEnterMultiZDoor> validDoors = new();
            foreach (var door in LayerManager.Instance.GetZLayerOfGameObject(gameObject).FurnitureContainer.GetComponentsInChildren<OnInteractEnterMultiZDoor>(false))
            {
                if (door.Exit != null && door.Exit.ZLayer == PathTarget.Value.Layer)
                {
                    validDoors.Add(door);
                }
            }
            if (validDoors.Count == 0)
            {
                Profiler.EndSample();
                return;
            }

            OnInteractEnterMultiZDoor validDoor = validDoors.OrderBy(door => Vector2.Distance(door.transform.position, CharComponents.transform.position)).First();

            if (TryGeneratePathChainOnSingleLayer(
                    characterTilePosition,
                    TileManager.PositionToTilePosition(validDoor.transform.position),
                    validDoor.ZLayer,
                    false,
                    validDoor,
                    out var pathToValidDoor
                    ) &&
                TryGeneratePathChainOnSingleLayer(
                    TileManager.PositionToTilePosition(validDoor.Exit.transform.position),
                    PathTarget.Value.Position,
                    PathTarget.Value.Layer,
                    CanJumpToTarget,
                    null,
                    out var pathFromValidDoorToTarget
                    )
                )
            {

                PathChain.AddRange(pathToValidDoor);
                PathChain.AddRange(pathFromValidDoorToTarget);
            }
        }
        else
        {
            TryGeneratePathChainOnSingleLayer(
                characterTilePosition,
                PathTarget.Value.Position,
                PathTarget.Value.Layer,
                CanJumpToTarget,
                null,
                out var newPathChain
                );

            PathChain = newPathChain;
        }

    }

    private bool TryGeneratePathChainOnSingleLayer(
        Vector2Int from,
        Vector2Int to,
        ZIndexLayer layer,
        bool CanJumpToTarget,
        Interactable interactWithObjectAtFinish,
        out LinkedList<PathChainElement> result
        )
    {
        TileManager tileManager = layer.TileManager;
        List<TileManager.NavigationPlatformInfo> platforms = new(tileManager.NavigationPlatforms);
        TileManager.NavigationPlatformInfo startPlatform = null;
        TileManager.NavigationPlatformInfo targetPlatform = null;
        int maxJumpHeight = CharComponents.CharacterJumping.GetJumpHeight();
        int maxJumpWidth = CharComponents.CharacterJumping.GetJumpWidth();

        startPlatform = tileManager.GetPlatformUnderPoint(from);
        if (startPlatform == null)
        {
            result = null;
            Profiler.EndSample();
            return false;
        }

        targetPlatform = tileManager.GetNearestReachablePlatform(to, maxJumpHeight, maxJumpWidth);
        platforms[platforms.IndexOf(startPlatform)] = null;

        PathChainElementPrecalculated currentChain;
        int iterations = 0;
        currentChain = new(
            from,
            from,
            startPlatform,
            to
            );
        List<PathChainElementPrecalculated> pathTree = new();
        List<PathChainElementPrecalculated> requiredCalculateChains = new() { currentChain };
        PathChainElementPrecalculated nearestChain = currentChain;
        PathChainElementPrecalculated nearestJumpToChain = null;
        Vector2Int pathTargetVec2Int = TileManager.PositionToTilePosition(to);

        while (iterations < (AdvancedPathinding ? ADVANCED_ITERATIONS_LIMIT : DEFAULT_ITERATIONS_LIMIT))
        {
            if (CanJumpToTarget)
            {

                TileManager.NavigationPlatformTransitionInfo newJumpToTargetTransition = tileManager.TryGetValidJumpTargetPositionFromPlatfromToPoint(
                    currentChain.TargetPosition,
                    pathTargetVec2Int,
                    currentChain.Platform,
                    maxJumpHeight,
                    maxJumpWidth
                    );

                if (newJumpToTargetTransition != null)
                {
                    PathChainElementPrecalculated newJumpToTargetChain = new(
                        newJumpToTargetTransition.StartConntection,
                        newJumpToTargetTransition.EndConnection,
                        currentChain.Platform,
                        to
                        );

                    PathChainElementPrecalculated newJumpToTargetSubChain = new(
                        currentChain.TargetPosition,
                        newJumpToTargetChain.StartPosition,
                        currentChain.Platform,
                        to
                        );

                    newJumpToTargetChain.PrevElement = newJumpToTargetSubChain;
                    newJumpToTargetSubChain.PrevElement = currentChain;
                    nearestJumpToChain = newJumpToTargetChain;

                    break;
                }
            }

            if (currentChain.Platform == targetPlatform)
            {
                nearestChain = currentChain;
                break;
            }

            foreach (var possibleReachablePlatform in currentChain.Platform.PreCalculatedReachablePlatforms)
            {
                if (possibleReachablePlatform == null) continue;

                TileManager.NavigationPlatformTransitionInfo transition = tileManager.TryCreateTransition(currentChain.Platform, possibleReachablePlatform, currentChain.StartPosition, maxJumpHeight, maxJumpWidth);
                if (transition != null)
                {
                    PathChainElementPrecalculated newMoveChain = new(
                        currentChain.TargetPosition,
                        transition.StartConntection,
                        possibleReachablePlatform,
                        to
                        );
                    newMoveChain.PrevElement = currentChain;

                    PathChainElementPrecalculated newJumpChain = new(
                        transition.StartConntection,
                        transition.EndConnection,
                        possibleReachablePlatform,
                        to
                        );
                    newJumpChain.PrevElement = newMoveChain;

                    requiredCalculateChains.Add(newJumpChain);
                }

            }

            if (currentChain.DistanceToTarget < nearestChain.DistanceToTarget)
            {
                nearestChain = currentChain;
            }

            requiredCalculateChains.Remove(currentChain);

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

        if (nearestJumpToChain != null)
        {
            currentChain = nearestJumpToChain;
        }
        else
        {
            Vector3Int finalChainElementTargetPosVec3 = tileManager.GetNearestPlatformPositionToPoint(nearestChain.Platform, to);
            PathChainElementPrecalculated finalChainElement = new(
                nearestChain.TargetPosition,
                new Vector2Int(finalChainElementTargetPosVec3.x, finalChainElementTargetPosVec3.y + 1),
                nearestChain.Platform,
                to
                );
            finalChainElement.PrevElement = nearestChain;
            currentChain = finalChainElement;
        }


        result = new();

        iterations = 0;
        do
        {
            result.AddFirst(currentChain.ConvertToPathChainElement());

            currentChain = currentChain.PrevElement;

            iterations++;
            if (iterations > (AdvancedPathinding ? ADVANCED_ITERATIONS_LIMIT : DEFAULT_ITERATIONS_LIMIT)) throw new UnityException("iterations limit is reached, pathfinding system probably created invinite loop or too big");
        }
        while (currentChain != null && currentChain.TargetPosition != from);

        if (interactWithObjectAtFinish)
        {
            var lastChain = result.Last();
            lastChain.RequiredIteractableToContinue = interactWithObjectAtFinish;
            result.RemoveLast();
            result.AddLast(lastChain);
        }

        Profiler.EndSample();
        return result.Last.Value.TargetPosition == to;
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