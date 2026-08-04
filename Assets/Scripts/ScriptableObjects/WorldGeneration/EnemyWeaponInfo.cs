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

    /// <typeparam name="T">all returned items must contain T component</typeparam>
    public Holdable PickRandomWeapon<T>()
    {
        List<Holdable> filteredPool = new();
        foreach (var holdable in PossibleWeapon)
        {
            if (holdable.TryGetComponent(out T t))
            {
                filteredPool.Add(holdable);
            }
        }
        return NumberMath.PickRandomItem(filteredPool);
    }
}
