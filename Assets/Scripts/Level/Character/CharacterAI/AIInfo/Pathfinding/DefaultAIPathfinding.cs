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
    public Color Debug_PathColor = new Color(1, 1, 1, 0);

    const int PATHINDING_ITERATIONS_LIMIT = 64;

    protected override void OnUpdateInfo()
    {
        if (PathTarget.Value == null) return;

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

        //startPlatform.Debug_DrawPlatform(Color.red, 1f);
        //targetPlatform.Debug_DrawPlatform(Color.blue, 1f);

        PathChainElement currentChain;
        int iterations = 0;
        if (startPlatform != targetPlatform)
        {
            currentChain = new(
                TileManager.PositionToTilePosition(CharComponents.transform.position),
                new Vector2Int((int)CharComponents.transform.position.x, startPlatform.Position.y + 1),
                startPlatform,
                PathTarget.Value
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
                            PathTarget.Value
                            );
                        newMoveChain.PrevElement = currentChain;
                        currentChain.NextElement = newMoveChain;

                        PathChainElement newJumpChain = new(
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

            Vector3Int finalChainElementTargetPosVec3 = tileManager.GetNearestPlatformPositionToPoint(nearestChain.Platform, PathTarget.Value);
            PathChainElement finalChainElement = new(
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
            PathChain.Add(currentChain);

            if (CanJumpToTarget)
            {

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
                        PathTarget.Value
                        );
                    newChain.PrevElement = currentChain;
                    currentChain.TargetPosition = newChain.StartPosition;
                    currentChain.NextElement = newChain;
                    currentChain = newChain;

                    PathChain.Add(currentChain);

                    break;
                }
            }

            currentChain = currentChain.PrevElement;

            iterations++;
            if (iterations > PATHINDING_ITERATIONS_LIMIT) throw new UnityException("iterations limit is reached, pathfinding system probably created invinite loop or too big");
        }
        while (currentChain != null && currentChain.Platform != startPlatform);

        if (Debug_PathColor.a != 0 && PathChain.Count > 0)
        {
            PathChain[0].Debug_DrawChain(Debug_PathColor, UPDATE_AI_DELAY_SECONDS, true, true);
        }
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