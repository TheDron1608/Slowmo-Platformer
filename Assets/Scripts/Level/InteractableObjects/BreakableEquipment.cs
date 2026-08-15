
using UnityEngine;

public class BreakableEquipment : BreakableObject
{
    protected override void OnBreakObject(MonoBehaviour breaker)
    {
        base.OnBreakObject(breaker);
        GetComponent<CharacterEquipmentPart>().TryUnequipPart();
    }
}
