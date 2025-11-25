using UnityEngine;

public class PassiveDamage : AbstractDamagableObjectEffect
{
    public float DamagePerSecond = 0f;
    public bool AllowOnDead = false;
    public bool AllowOnDying = true;

    private void FixedUpdate()
    {
        if (
            (AllowOnDead || (!AffectedObject.GetComponent<ObjectEffectsReceiver>()?.GetHasEffect<ILethalEffect>() ?? true)) &&
            (AllowOnDying || (!AffectedObject.GetComponent<ObjectEffectsReceiver>()?.GetHasEffect<ILethalEffect>(true) ?? true))
            )
        {
            AffectedDamagableObject.ApplyDamage(DamagePerSecond * Time.fixedDeltaTime, null, 0f);
        }
    }
}
