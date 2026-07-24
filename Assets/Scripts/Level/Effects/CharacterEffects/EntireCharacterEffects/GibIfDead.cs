
using UnityEngine;

public class GibIfDead : Gib
{
    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return base.ApplyCondition(affectWho, sender) && affectWho.GetComponent<AbstractCharacterComponent>().CharComponents.CharacterHealth.Died;
    }
}