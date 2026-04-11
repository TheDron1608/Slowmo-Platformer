using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class BlessPickManager : AbstractModificatorCardsManager
{
    const float TRADE_FINISH_DELAY_AFTER_SPEND_ALL_PICKS = 0.5f;
    const float SELL_ENCOUNT_PER_SECOND = 100f;
    const float SELL_DELAY_BETWEEN_MODIFICATORS = 0.25f;

    [SerializeField] private SellCursesCard _sellCursesCardInstance;
    [SerializeField] private TextMeshProUGUI _soldPriceText;
    [SerializeField] private UIElementTrackTarget _soldPriceTrackTarget;
    [SerializeField] private Transform _showSoldPriceTransform;
    [SerializeField] private Transform _hideSoldPriceTransform;
    [SerializeField] private Transform _nothingToSellMessageContainer;
    [SerializeField] private Transform _cantBuyAnyModificatorsMessageContainer;

    private int _picksLeft = 1;
    private Coroutine _changeSceneDelayAfterSpendAllPicksCoroutine = null;
    private Coroutine _sellCursesCoroutine = null;

    public static BlessPickManager Instance;

    private void Awake()
    {
        _picksLeft = ModificatorsManager.Instance?.ModifiactorsPickAmount ?? 1;
        _soldPriceText.text = ScoreManager.Instance.TradableScore.ToString("0");

        if (Instance != null) throw new UnityException("Limit of 1 BlessPickManager instance per scene");
        Instance = this;
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


    public override void FinishTrade()
    {
        base.FinishTrade();
        UIManager.Instance.LoadSceneWithEffect(SceneList.GAMEPLAY);
    }

    private IEnumerator SellCursesCoroutine()
    {
        if (ModificatorsManager.Instance != null)
        {
            ClearAllCards();
            SetDisplayedInfo(null);
            _soldPriceText.text = ScoreManager.Instance.TradableScore.ToString("0");
            ShowScore();

            //count total traded points
            float totalAddPrice = 0;
            foreach (
                AbstractModificator modificator in
                ModificatorsManager.Instance.CurrentModificators
                    .Where(e => e.Status == AbstractModificator.ModificatorStatuses.CURSE)
                )
            {
                modificator.CurrentIcon.Raising = true;

                float modificatorAddPrice = 0;
                while (modificatorAddPrice < modificator.ModificatorPrice)
                {
                    float modificatorAddPriceThisFrame = Time.deltaTime * SELL_ENCOUNT_PER_SECOND;
                    if (modificatorAddPrice + modificatorAddPriceThisFrame > modificator.ModificatorPrice)
                    {
                        modificatorAddPriceThisFrame = modificator.ModificatorPrice - modificatorAddPrice;
                    }

                    modificatorAddPrice += modificatorAddPriceThisFrame;
                    totalAddPrice += modificatorAddPriceThisFrame;
                    _soldPriceText.text = totalAddPrice.ToString("0");

                    yield return new WaitForEndOfFrame();
                }

                modificator.CurrentIcon.Raising = false;

                yield return new WaitForSeconds(SELL_DELAY_BETWEEN_MODIFICATORS);
            }

            //show sellable modifiers
            float currentBlessMaxPrice = totalAddPrice;
            for (int i = 0; i < ModificatorsManager.Instance.MaxModificatorOptions; i++)
            {
                ModificatorCardsCluster newCluster = Instantiate(_clusterInstance);
                newCluster.AddModificator(ModificatorsManager.Instance.PickRandomModificators(AbstractModificator.ModificatorTypes.POSITIVE, currentBlessMaxPrice - 1));
                if (newCluster == null) break;

                newCluster.AddStatusOnPick = AbstractModificator.ModificatorStatuses.TRADED;
                AddCard(newCluster);

                currentBlessMaxPrice =
                    newCluster.Cards
                    .Where(e => e.ModificatorInstance.ModificatorType == AbstractModificator.ModificatorTypes.POSITIVE)
                    .OrderBy(e => e.ModificatorInstance.ModificatorPrice)
                    .First()
                    .ModificatorInstance.ModificatorPrice;
            }

            //remove traded modificators if has any option
            if (Cards.Count > 0)
            {
                ModificatorsManager.Instance.RemoveModificators(AbstractModificator.ModificatorStatuses.CURSE);
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

            HideScore();
        }

        _sellCursesCoroutine = null;
    }

    public override void SpendPicksLeft(int amount = 1)
    {
        _picksLeft -= amount;
        if (_picksLeft <= 0)
        {
            while (Cards.Count > 0)
            {
                RemoveCard(Cards.First());
            }

            foreach (AbstractModificator modificator in ModificatorsManager.Instance.CurrentModificators)
            {
                if (!modificator.DisabledModificator)
                {
                    modificator.OnModificatorChoiseFinished();
                }
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