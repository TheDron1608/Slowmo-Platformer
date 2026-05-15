using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class CursePickManager : AbstractModificatorCardsManager
{
    const float TRADE_DELAY = 0.75f;
    const float SCORE_ENCOUNT_PER_SECOND = 100f;
    const float MAX_MODIFICATOR_APPEAR_DELAY = 1f;

    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private UIElementTrackTarget _scoreTrackTarget;
    [SerializeField] private Transform _showScoreTransform;
    [SerializeField] private Transform _hideScoreTransform;

    private int _picksLeft = 1;
    private Coroutine _changeSceneDelayAfterSpendAllPicksCoroutine = null;
    private Coroutine _tradeCoroutine = null;

    public static CursePickManager Instance;

    private void Awake()
    {
        _picksLeft = ModificatorsManager.Instance?.ModifiactorsPickAmount ?? 1;
        _scoreText.text = ScoreManager.Instance.TradableScore.ToString("0");

        if (Instance != null && !Instance.IsDestroyed()) throw new UnityException("Limit of 1 CursePickManager instance per scene");
        Instance = this;
    }

    private void Start()
    {
        ShowScore();
        Trade();
    }

    public void Trade()
    {
        if (_tradeCoroutine == null)
        {
            _tradeCoroutine = StartCoroutine(TradeCoroutine());
        }
    }


    public override void FinishTrade()
    {
        base.FinishTrade();
        UIManager.Instance.LoadSceneWithEffect(SceneList.GAMEPLAY);
    }

    private IEnumerator TradeCoroutine()
    {
        if (ModificatorsManager.Instance != null && ScoreManager.Instance != null)
        {
            yield return new WaitForSeconds(TRADE_DELAY);

            List<AbstractModificator> addedModificators = new();
            float modificatorAppearDelay = math.min(
                ScoreManager.Instance.TradableScore / SCORE_ENCOUNT_PER_SECOND / ModificatorsManager.Instance.MaxModificatorOptions, 
                MAX_MODIFICATOR_APPEAR_DELAY
                );
            float encountedScore = 0f;
            float lastAddedCardScore = math.min(1f, ScoreManager.Instance.TradableScore);
            float delayTime = 0f;
            int iter = 0;

            _scoreText.text = ScoreManager.Instance.TradableScore.ToString("0");
            ShowScore();

            while (
                (
                    encountedScore < ScoreManager.Instance.TradableScore || 
                    Cards.Count < ModificatorsManager.Instance.MaxModificatorOptions
                ) &&
                iter < ModificatorsManager.Instance.MaxModificatorOptions
                )
            {
                encountedScore += Time.deltaTime * SCORE_ENCOUNT_PER_SECOND;
                _scoreText.text = math.max(ScoreManager.Instance.TradableScore - encountedScore, 0f).ToString("0");

                delayTime += Time.deltaTime;
                if (delayTime > modificatorAppearDelay)
                {
                    List<AbstractModificator> addModificators = ModificatorsManager.Instance.PickRandomModificators(
                        AbstractModificator.ModificatorTypes.NEGATIVE,
                        lastAddedCardScore * ModificatorsManager.Instance.TradeCurseProfitMult,
                        encountedScore * ModificatorsManager.Instance.TradeCurseProfitMult,
                        true,
                        false,
                        true,
                        addedModificators
                        );

                    if (addModificators.Count > 0)
                    {
                        ModificatorCardsCluster newCluster = Instantiate(_clusterInstance);
                        newCluster.AddModificator(addModificators);
                        newCluster.AddStatusOnPick = AbstractModificator.ModificatorStatuses.CURSE;
                        AddCard(newCluster);

                        addedModificators.AddRange(addModificators);
                        
                        if (Cards.Count > ModificatorsManager.Instance.MaxModificatorOptions)
                        {
                            RemoveCard(Cards.First());
                        }
                    }
                    else
                    {
                        break;
                    }

                    lastAddedCardScore = encountedScore;
                    delayTime = 0f;
                    iter++;
                }

                yield return new WaitForEndOfFrame();
            }

            if (Cards.Count == 0)
            {
                _scoreText.text = ScoreManager.Instance.TradableScore.ToString("0");
                AddCard(Instantiate(_pickNothingCardInstance));
            }
            else
            {
                if (ModificatorsManager.Instance.CanSkipCursePick)
                {
                    yield return new WaitForSeconds(modificatorAppearDelay);
                    AddCard(Instantiate(_pickNothingCardInstance));
                }

                SetAllCardsInteractable(true);
            }
        }

        _tradeCoroutine = null;
    }

    public override void SpendPicksLeft(int amount = 1)
    {
        _picksLeft -= amount;
        if (_picksLeft <= 0 || Cards.Count == 0)
        {
            while (Cards.Count > 0)
            {
                RemoveCard(Cards.First());
            }

            foreach (AbstractModificator modificator in ModificatorsManager.Instance.CurrentModificators)
            {
                if (!modificator.DisabledModificator)
                {
                    modificator.OnModificatorChoiseFinished(this);
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
        yield return new WaitForSeconds(TRADE_DELAY);
        FinishTrade();
    }

    public void ShowScore()
    {
        _scoreTrackTarget.transform.position = _showScoreTransform.position;
    }
    public void HideScore()
    {
        _scoreTrackTarget.transform.position = _hideScoreTransform.position;
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