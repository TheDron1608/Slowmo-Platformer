using System.Collections.Generic;
using System.Linq;

public class RandomizeModificatorAtLevelFinish: AbstractModificator
{
    const float MAX_MODIFICATOR_PRICE_CHANGE = 1.1f;
    const int MAX_REPLACE_ATTEMPTS = 10;

    public override void OnLevelFinished()
    {
        base.OnLevelFinished();

        for (int i = 0; i < MAX_REPLACE_ATTEMPTS; i++)
        {
            AbstractModificator randomModificator = NumberMath.PickRandomItem(
                ModificatorsManager.Instance.CurrentModificators
                .Where(e => e.Status != ModificatorStatuses.CHARACTER_DEFAULT)
                .ToList()
                );
            AbstractModificator replaceRandomModificator = ModificatorsManager.Instance.PickRandomModificators(
                randomModificator.ModificatorType,
                randomModificator.ModificatorPrice / MAX_MODIFICATOR_PRICE_CHANGE,
                randomModificator.ModificatorPrice * MAX_MODIFICATOR_PRICE_CHANGE,
                false,
                false,
                false,
                new List<AbstractModificator> { randomModificator },
                true
                ).FirstOrDefault();

            if (randomModificator == null || replaceRandomModificator == null)
            {
                continue;
            }
            else
            {
                ModificatorsManager.Instance.RemoveModificator(randomModificator);
                ModificatorsManager.Instance.AddModificator(replaceRandomModificator, randomModificator.Status);

                break;
            }
        }

        TryTriggerIconAnimation();
    }
}