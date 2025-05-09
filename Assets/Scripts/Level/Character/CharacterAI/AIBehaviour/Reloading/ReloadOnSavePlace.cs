using System.Collections;
using UnityEngine;

public class ReloadOnSavePlace : AbstractAIReloading
{
    public float AwaitTimeInSavePlaceToStartReload = 1f;
    public float SaveDistance = 4.5f;

    private Coroutine AwaitTimeInSavePlaceCoroutine;

    private void FixedUpdate()
    {
        if (
            CharComponents.CharacterHolding.CurrentHoldObject != null &&
            CharComponents.CharacterHolding.CurrentHoldObject.TryGetComponent(out RangedWeapon rangedWeapon)
            )
        {
            if (
                rangedWeapon.GetIsNeedReload() && 
                (CharComponents.CharacterAIManager.CurrentActiveStateBehaviour.NearestEnemyInfo == null || CharComponents.CharacterAIManager.CurrentActiveStateBehaviour.NearestEnemyInfo.NearestEnemyDistance.Value >= SaveDistance)
                )
            {
                if (AwaitTimeInSavePlaceCoroutine == null)
                {
                    AwaitTimeInSavePlaceCoroutine = StartCoroutine(AwaitTimeInSavePlaceThenReload());
                }
            }
            else if (CharComponents.CharacterReloading.GetIsReloading())
            {
                if (AwaitTimeInSavePlaceCoroutine != null)
                {
                    StopCoroutine(AwaitTimeInSavePlaceCoroutine);
                    AwaitTimeInSavePlaceCoroutine = null;
                }
                CharComponents.CharacterReloading.TryFinishReload();
            }
        }
    }

    private IEnumerator AwaitTimeInSavePlaceThenReload()
    {
        yield return new WaitForSeconds(AwaitTimeInSavePlaceToStartReload);
        CharComponents.CharacterReloading.TryReload();
    }
}
