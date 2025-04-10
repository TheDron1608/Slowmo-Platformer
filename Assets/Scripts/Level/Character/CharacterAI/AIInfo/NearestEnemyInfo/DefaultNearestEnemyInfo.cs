using UnityEngine;

public partial class DefaultNearestEnemyInfo : AbstractAINearestEnemyInfo
{
    protected override void OnUpdateInfo()
    {
        float minDistance = MaxEnemyDetectRange;
        ZIndexLayer currentLayer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);
        CharacterTeam result = null;
        foreach (Transform characterGameObject in currentLayer.CharactersContainer)
        {
            if (characterGameObject.TryGetComponent(out CharacterTeam characterTeam) && !NumberMath.GetListContainsAnyItemOfAnotherList(characterTeam.CharacterTeams, CharComponents.CharacterTeam.CharacterTeams))
            {
                float charDistance = Vector2.Distance(transform.position, characterGameObject.transform.position);
                if (
                    charDistance < minDistance &&
                    Physics2D.Linecast(
                        CharComponents.Center.transform.position,
                        characterTeam.CharComponents.Center.transform.position,
                        1 << currentLayer.EnviromentLayer
                        ).collider == null
                    )
                {
                    minDistance = charDistance;
                    result = characterTeam;
                }
            }
        }

        if (result != null) TimeSinceLastEnemyDetection = 0f; 

        NearestEnemy = result;
        NearestEnemyDistance = minDistance;
    }
}
