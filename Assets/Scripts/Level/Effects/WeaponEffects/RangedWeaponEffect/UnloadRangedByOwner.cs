using System.Collections;
using UnityEngine;

[AllowEffectWithSenderReceiveNull]
public class UnloadRangedByOwner : AbstractRangedWeaponEffectWithSender
{
    const float UPDATES_PER_FRAME = 10f;

    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        RangedWeapon.AmmoLeft = 0;
        RangedWeapon.LoadedSpentAmmoLeft += RangedWeapon.LoadedLivingAmmoLeft;
        RangedWeapon.LoadedLivingAmmoLeft = 0;
        StartCoroutine(UnloadWeaponTilNotThrownOrEmpty());
    }

    private IEnumerator UnloadWeaponTilNotThrownOrEmpty()
    {
        while(RangedWeapon.LoadedSpentAmmoLeft > 0)
        {
            if (RangedWeapon.Unloaded)
            {
                RangedWeapon.TryCloseMag();
            }
            else
            {
                RangedWeapon.TryUnload();
            }
            yield return new WaitForSeconds(1f / UPDATES_PER_FRAME);
        }

        RemoveSelf();
    }
}
