using UnityEngine;

public abstract class AbstractAINearestEnemyInfo : AbstractAIInfo
{
    private CharacterTeam _nearestEnemy;

    public CharacterTeam NearestEnemy
    {
        get
        {
            TryUpdateInfo();
            return _nearestEnemy;
        }
        protected set => _nearestEnemy = value;
    }
}
