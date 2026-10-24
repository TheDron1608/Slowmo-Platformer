using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class MoneyPickManager : AbstractModificatorCardsManager
{
    const float TRADE_FINISH_DELAY_AFTER_SPEND_ALL_PICKS = 0.5f;

    public static MoneyPickManager Instance;

    public TextMeshProUGUI CurrentMoneyText;

    private Coroutine _changeSceneDelayAfterSpendAllPicksCoroutine = null;

    public override string GetAnalyticsChoiseTypeName()
    {
        return "MoneyPick";
    }

    protected override void OnAwake()
    {
        base.OnAwake();

        PicksLeft = ModificatorsManager.Instance?.ModifiactorsPickAmount ?? 1;

        if (Instance != null && !Instance.IsDestroyed()) throw new UnityException("Limit of 1 MoneyPickManager instance per scene");
        Instance = this;

        RerollsLeft = ModificatorsManager.Instance.BlessPickRerolls;
    }

    private void Start()
    {
        PicksLeft = 999;
        ForceReroll();
    }


    public override void FinishTrade(bool pickNothing = false)
    {
        base.FinishTrade(pickNothing);
        UIManager.Instance.LoadSceneWithEffect(SceneList.GAMEPLAY);
    }

    public override void ForceReroll()
    {
        base.ForceReroll();

        if (ModificatorsManager.Instance != null)
        {
            //show sellable modifiers
            List<AbstractModificator> addedModificators = new();
            for (int i = 0; i < ModificatorsManager.Instance.MaxModificatorOptions; i++)
            {
                List<AbstractModificator> addModificators = ModificatorsManager.Instance.PickRandomModificators(
                    AbstractModificator.ModificatorTypes.POSITIVE,
                    float.MinValue,
                    float.MaxValue,
                    false,
                    true,
                    true,
                    addedModificators,
                    false,
                    ModificatorsManager.Instance.BlessPickCounterMods
                    );

                if (addModificators.Count > 0)
                {
                    addedModificators.AddRange(addModificators);
                    ModificatorCardsCluster newCluster = Instantiate(_clusterInstance);
                    newCluster.AddModificator(addModificators);
                    newCluster.AddStatusOnPick = AbstractModificator.ModificatorStatuses.TRADED;
                    AddCard(newCluster);
                }
                else
                {
                    break;
                }
            }
        }

        if (RerollsLeft > 0)
        {
            AddCard(Instantiate(_rerollCardInstace));
        }

        AddCard(Instantiate(_pickNothingCardInstance));
    }

    public override void SpendPicksLeft(int amount = 1)
    {
        PicksLeft -= amount;
        if (PicksLeft <= 0 || Cards.Count == 0)
        {
            while (Cards.Count > 0)
            {
                RemoveCard(Cards.First());
            }

            _changeSceneDelayAfterSpendAllPicksCoroutine = StartCoroutine(FinishTradeAfterDelay());
        }
        else
        {
            if (_changeSceneDelayAfterSpendAllPicksCoroutine != null)
            {
                StopCoroutine(_changeSceneDelayAfterSpendAllPicksCoroutine);
                _changeSceneDelayAfterSpendAllPicksCoroutine = null;
            }
            SetAllCardsInteractable(true);
        }
    }

    private void FixedUpdate()
    {
        CurrentMoneyText.text = (ComboMoneyModificator.Instance?.DisplayedMoney.ToString("0") ?? "0") + "$";
    }

    private IEnumerator FinishTradeAfterDelay()
    {
        yield return new WaitForSeconds(TRADE_FINISH_DELAY_AFTER_SPEND_ALL_PICKS);
        FinishTrade();
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}