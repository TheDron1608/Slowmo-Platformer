using UnityEngine;

public class PassiveDamage : AbstractDamagableObjectEffect
{
    public float DamagePerSecond = 0f;

    private void FixedUpdate()
    {
        AffectedDamagableObject.ApplyDamage(DamagePerSecond * Time.fixedDeltaTime, null, 0f);
    }
}
