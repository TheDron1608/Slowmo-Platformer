using UnityEngine;

public abstract class AbstractAIPathfinding : AbstractAIInfo
{
    private Vector2 _pathTarget;

    public Vector2 PathTarget
    {
        get => _pathTarget;
        set
        {
            _pathTarget = value;
            TryUpdateInfo();
        }
    }
}