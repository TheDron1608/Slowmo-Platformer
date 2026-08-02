
using UnityEngine;

public class BreakableEquipment : BreakableObject
{
    public override void BreakObject(MonoBehaviour breaker)
    {
        base.BreakObject(breaker);
        GetComponent<CharacterEquipmentPart>().TryUnequipPart();
    }
}
