
using System.Collections.Generic;
using UnityEngine;

public class RandomizeModificatorsOverTimeModificator : AbstractModificator
{
    public float RandomizationDelay = 10f;

    private float _timeSinceLastRandomization = 0f;

    private void FixedUpdate()
    {
        if (!SceneList.GetCurrentSceneIsGameplay()) return;

        _timeSinceLastRandomization += Time.fixedDeltaTime;

        if (_timeSinceLastRandomization > RandomizationDelay)
        {
            _timeSinceLastRandomization = 0f;

            List<AbstractModificator> validMods = new();
            foreach (var mod in ModificatorsManager.Instance.CurrentModificators)
            {
                if (mod.Status != ModificatorStatuses.CHARACTER_DEFAULT && mod.ModificatorTier != ModificatorTiers.TIER_ULTRA && mod != this)
                {
                    validMods.Add(mod);
                }
            }

            while (validMods.Count > 0)
            {
                AbstractModificator randomMod = NumberMath.PickRandomItem(validMods);
                ModificatorStatuses randomModStatus = randomMod.Status;
                List<AbstractModificator> replaceRandomMod = ModificatorsManager.Instance.PickRandomModificators(
                    randomMod.ModificatorType,
                    randomMod.ModificatorPrice - 1f,
                    randomMod.ModificatorPrice,
                    false,
                    false,
                    false,
                    null,
                    true,
                    0f,
                    ModificatorsManager.Instance.CurrentModificators
                    );
                if (replaceRandomMod?.Count > 0)
                {
                    TryTriggerIconAnimation();
                    ModificatorsManager.Instance.RemoveModificator(randomMod);
                    ModificatorsManager.Instance.AddModificator(replaceRandomMod[0], randomModStatus);
                    return;
                }
            }
        }

    }
}