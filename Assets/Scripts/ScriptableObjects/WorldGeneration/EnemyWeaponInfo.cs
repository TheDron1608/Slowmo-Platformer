using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyWeaponInfo", menuName = "WorldGeneration/EnemyWeaponInfo")]
public class EnemyWeaponInfo : ScriptableObject
{
    public List<Holdable> PossibleWeapon;

    public Holdable PickRandomWeapon()
    {
        return NumberMath.PickRandomItem(PossibleWeapon);           
    }
}
