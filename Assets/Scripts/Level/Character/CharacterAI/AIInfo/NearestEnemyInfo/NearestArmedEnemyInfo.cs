using UnityEngine;

public class NearestArmedEnemyInfo : DefaultNearestEnemyInfo
{
    protected override bool CharacterCondition(CharacterComponentsManager character)
    {
        return base.CharacterCondition(character) && character.CharacterHolding.CurrentHoldObject != null;
    }
}
