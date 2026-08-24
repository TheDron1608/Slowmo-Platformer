using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class TutorialBlessPickManager : AbstractModificatorCardsManager
{
    const float MAX_PRICE_REDUCTION = 0.8f;
    const float TRADE_FINISH_DELAY_AFTER_SPEND_ALL_PICKS = 0.5f;
    const float SELL_ENCOUNT_PER_SECOND = 100f;
    const float SELL_DELAY_BETWEEN_MODIFICATORS = 0.25f;

    public Color AddColorText = Color.white;
    public Color SubtractColorText = Color.red;

    public List<AbstractModificator> OverrideModificatorsPool = new();

    [SerializeField] private SellCursesCard _sellCursesCardInstance;
    [SerializeField] private TextMeshProUGUI _soldPriceText;
    [SerializeField] private UIElementTrackTarget _soldPriceTrackTarget;
    [SerializeField] private Transform _showSoldPriceTransform;
    [SerializeField] private Transform _hideSoldPriceTransform;
    [SerializeField] private Transform _nothingToSellMessageContainer;
    [SerializeField] private Transform _cantBuyAnyModificatorsMessageContainer;

    private float _tradedPrice = 0f;
    private Coroutine _changeSceneDelayAfterSpendAllPicksCoroutine = null;
    private Coroutine _sellCursesCoroutine = null;

    public static TutorialBlessPickManager Instance;

    public override string GetAnalyticsChoiseTypeName()
    {
        return "SellNegativePick";
    }

    protected override void OnAwake()
    {
        base.OnAwake();

        PicksLeft = ModificatorsManager.Instance?.ModifiactorsPickAmount ?? 1;
        _soldPriceText.text = ScoreManager.Instance.TradableScore.ToString("0");

        if (Instance != null && !Instance.IsDestroyed()) throw new UnityException("Limit of 1 BlessPickManager instance per scene");
        Instance = this;

        RerollsLeft = ModificatorsManager.Instance.BlessPickRerolls;
    }

    private void Start()
    {
        if (ModificatorsManager.Instance.CurrentModificators.Where(e => e.Status == AbstractModificator.ModificatorStatuses.CURSE).Count() == 0)
        {
            _nothingToSellMessageContainer.gameObject.SetActive(true);
        }
        else
        {
            AddCard(Instantiate(_sellCursesCardInstance));
        }

        AddCard(Instantiate(_pickNothingCardInstance));
    }

    public void SellCurses()
    {
        if (_sellCursesCoroutine == null)
        {
            _sellCursesCoroutine = StartCoroutine(SellCursesCoroutine());
        }
    }


    public override void FinishTrade(bool pickNothing = false)
    {
        base.FinishTrade(pickNothing);
        UIManager.Instance.LoadSceneWithEffect(SceneList.TUTORIAL_3);
    }

    private IEnumerator SellCursesCoroutine()
    {
        if (ModificatorsManager.Instance != null)
        {
            ModificatorsManager.Instance.ModificatorsPool = OverrideModificatorsPool;

            ClearAllCards();
            SetDisplayedInfo(null);
            _soldPriceText.text = ScoreManager.Instance.TradableScore.ToString("0");
            _tradedPrice = 0f;
            ShowScore();

            //count total traded points
            foreach (
                AbstractModificator modificator in
                ModificatorsManager.Instance.CurrentModificators
                    .Where(e => e.Status == AbstractModificator.ModificatorStatuses.CURSE)
                )
            {
                if (modificator.ModificatorType == AbstractModificator.ModificatorTypes.NEUTRAL) continue;

                modificator.CurrentIcon.Raising = true;

                if (modificator.ModificatorType == AbstractModificator.ModificatorTypes.NEGATIVE)
                {
                    _soldPriceText.color = AddColorText;
                    float modificatorAddPrice = 0;
                    while (modificatorAddPrice < modificator.ModificatorPrice)
                    {
                        float modificatorAddPriceThisFrame = Time.deltaTime * SELL_ENCOUNT_PER_SECOND;
                        if (modificatorAddPrice + modificatorAddPriceThisFrame > modificator.ModificatorPrice)
                        {
                            modificatorAddPriceThisFrame = modificator.ModificatorPrice - modificatorAddPrice;
                        }

                        modificatorAddPrice += modificatorAddPriceThisFrame;
                        _tradedPrice += modificatorAddPriceThisFrame;
                        _soldPriceText.text = _tradedPrice.ToString("0");

                        yield return new WaitForEndOfFrame();
                    }
                }

                else if (modificator.ModificatorType == AbstractModificator.ModificatorTypes.POSITIVE)
                {
                    _soldPriceText.color = SubtractColorText;
                    float modificatorRemovePrice = 0;
                    while (modificatorRemovePrice > -modificator.ModificatorPrice)
                    {
                        float modificatorRemovePriceThisFrame = -Time.deltaTime * SELL_ENCOUNT_PER_SECOND;
                        if (modificatorRemovePrice + modificatorRemovePriceThisFrame > modificator.ModificatorPrice)
                        {
                            modificatorRemovePriceThisFrame = modificator.ModificatorPrice - modificatorRemovePrice;
                        }

                        modificatorRemovePrice += modificatorRemovePriceThisFrame;
                        _tradedPrice += modificatorRemovePriceThisFrame;
                        _soldPriceText.text = _tradedPrice.ToString("0");

                        yield return new WaitForEndOfFrame();
                    }
                }

                modificator.CurrentIcon.Raising = false;

                yield return new WaitForSeconds(SELL_DELAY_BETWEEN_MODIFICATORS);
            }

            //show sellable modifiers
            List<AbstractModificator> addedModificators = new();
            for (int i = 0; i < ModificatorsManager.Instance.MaxModificatorOptions; i++)
            {
                List<AbstractModificator> addModificators = ModificatorsManager.Instance.PickRandomModificators(
                    AbstractModificator.ModificatorTypes.POSITIVE,
                    _tradedPrice * ModificatorsManager.Instance.TradeBlessProfitMult * MAX_PRICE_REDUCTION,
                    _tradedPrice * ModificatorsManager.Instance.TradeBlessProfitMult,
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

            InitSpecialCards();

            if (SessionManager.Instance?.TempSession != null)
            {
                SessionManager.Instance.TempSession.TotalSoldCurses += _tradedPrice;
                if (SessionManager.Instance.TempSession.MaxSoldCurses < _tradedPrice)
                {
                    SessionManager.Instance.TempSession.MaxSoldCurses = math.round(_tradedPrice);
                }
            }
        }

        _sellCursesCoroutine = null;
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
                    _tradedPrice * ModificatorsManager.Instance.TradeBlessProfitMult * MAX_PRICE_REDUCTION,
                    _tradedPrice * ModificatorsManager.Instance.TradeBlessProfitMult,
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

            InitSpecialCards();

            HideScore();
        }
    }

    private void InitSpecialCards()
    {
        //remove traded modificators if has any option and RemoveModifictorsOnSell is true
        if (Cards.Count > 0)
        {
            if (ModificatorsManager.Instance.RemoveModifictorsOnSell)
            {
                ModificatorsManager.Instance.RemoveModificators(AbstractModificator.ModificatorStatuses.CURSE);
            }
            else
            {
                foreach (AbstractModificator mod in ModificatorsManager.Instance.CurrentModificators)
                {
                    if (mod.Status == AbstractModificator.ModificatorStatuses.CURSE)
                    {
                        mod.Status = AbstractModificator.ModificatorStatuses.PERMANENT;
                    }
                }
            }

            if (RerollsLeft > 0)
            {
                AddCard(Instantiate(_rerollCardInstace));
            }

            if (ModificatorsManager.Instance.CanSkipBlessPick)
            {
                AddCard(Instantiate(_pickNothingCardInstance));
            }
        }
        else
        {
            AddCard(Instantiate(_pickNothingCardInstance));
            _cantBuyAnyModificatorsMessageContainer.gameObject.SetActive(true);
        }
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

    private IEnumerator FinishTradeAfterDelay()
    {
        yield return new WaitForSeconds(TRADE_FINISH_DELAY_AFTER_SPEND_ALL_PICKS);
        FinishTrade();
    }

    public void ShowScore()
    {
        _soldPriceTrackTarget.transform.position = _showSoldPriceTransform.position;
        CardsInfoContainer.gameObject.SetActive(false);
    }
    public void HideScore()
    {
        _soldPriceTrackTarget.transform.position = _hideSoldPriceTransform.position;
        CardsInfoContainer.gameObject.SetActive(true);
    }

    public override void SetDisplayedInfo(List<IModificatorInfo> infos)
    {
        base.SetDisplayedInfo(infos);

        HideScore();
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}