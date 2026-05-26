using System.Collections.Generic;

public class RandomModificatorsDisable : AbstractModificator
{
    public ModificatorTypes TargetType;
    public int DisabledModificatorsAmount = 1;

    private List<AbstractModificator> _currentDisabledModificators = new();

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        if (SceneList.GetCurrentSceneIsGameplay())
        {
            RandomizeDisabledModificators();
        }
    }

    public override void OnModificatorChoiseFinished(AbstractModificatorCardsManager choise)
    {
        base.OnModificatorChoiseFinished(choise);

        RandomizeDisabledModificators();
    }

    private void RandomizeDisabledModificators()
    {
        //getting valid modificators for disable
        List<AbstractModificator> possibleModificatorsForDisable = new();
        foreach (AbstractModificator modificator in ModificatorsManager.Instance.CurrentModificators)
        {
            if (
                modificator.ModificatorType == TargetType &&
                !modificator.TryGetComponent(out RandomModificatorsDisable rmd) &&
                (!modificator.DisabledModificator || _currentDisabledModificators.Contains(modificator))
                )
            {
                possibleModificatorsForDisable.Add(modificator);
            }
        }
        //select random valid modificators to disable
        List<AbstractModificator> disableModificators = new();
        for (int i = 0; i < DisabledModificatorsAmount; i++)
        {
            if (possibleModificatorsForDisable.Count == 0) break;

            int randomIndex = NumberMath.PickRandomInRangeNoSeed(0, possibleModificatorsForDisable.Count - 1);

            disableModificators.Add(possibleModificatorsForDisable[randomIndex]);
            possibleModificatorsForDisable.RemoveAt(randomIndex);
        }

        //disabling random valid modificators and enable old disabled modificators
        foreach (AbstractModificator newDisableModificator in disableModificators)
        {
            newDisableModificator.DisabledModificator = true;
        }
        foreach (AbstractModificator oldDisabledModificator in _currentDisabledModificators)
        {
            if (!disableModificators.Contains(oldDisabledModificator))
            {
                oldDisabledModificator.DisabledModificator = false;
            }
        }
        _currentDisabledModificators = disableModificators;
    }
}