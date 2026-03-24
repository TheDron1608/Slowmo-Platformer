using System.Collections;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class CursePickManager : AbstractModificatorCardsManager
{
    const float TRADE_FINISH_DELAY_AFTER_SPEND_ALL_PICKS = 0.5f;
    const float SCORE_ENCOUNT_PER_SECOND = 100f;
    const float MAX_MODIFICATOR_APPEAR_DELAY = 1f;

    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private UIElementTrackTarget _scoreTrackTarget;
    [SerializeField] private Transform _showScoreTransform;
    [SerializeField] private Transform _hideScoreTransform;
    [SerializeField] private Transform _startButtonsContainer;
    [SerializeField] private Transform _notEnoughPointsButtonsContainer;

    private int _picksLeft = 1;
    private Coroutine _changeSceneDelayAfterSpendAllPicksCoroutine = null;
    private Coroutine _tradeCoroutine = null;

    public static CursePickManager Instance;

    private void Awake()
    {
        _picksLeft = ModificatorsManager.Instance?.ModifiactorsPickAmount ?? 1;
        _scoreText.text = ScoreManager.Instance.TradableScore.ToString("0");

        if (Instance != null) throw new UnityException("Limit of 1 CursePickManager instance per scene");
        Instance = this;
    }

    private void Start()
    {
        ShowScore();
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
        _startButtonsContainer.gameObject.SetActive(false);
        UIManager.Instance.LoadSceneWithEffect(SceneList.GAMEPLAY);
    }

    private IEnumerator TradeCoroutine()
    {
        if (ModificatorsManager.Instance != null && ScoreManager.Instance != null)
        {
            float modificatorAppearDelay = math.min(
                ScoreManager.Instance.TradableScore / SCORE_ENCOUNT_PER_SECOND / ModificatorsManager.Instance.MaxModificatorOptions, 
                MAX_MODIFICATOR_APPEAR_DELAY
                );
            float encountedScore = 0f;
            float lastAddedCardScore = math.min(1f, ScoreManager.Instance.TradableScore);
            float delayTime = Time.deltaTime;

            _scoreText.text = ScoreManager.Instance.TradableScore.ToString("0");
            _startButtonsContainer.gameObject.SetActive(false);
            ShowScore();

            while (encountedScore < ScoreManager.Instance.TradableScore)
            {
                encountedScore += Time.deltaTime * SCORE_ENCOUNT_PER_SECOND;
                _scoreText.text = math.max(ScoreManager.Instance.TradableScore - encountedScore, 0f).ToString("0");

                delayTime += Time.deltaTime;
                if (delayTime > modificatorAppearDelay)
                {
                    ModificatorCardsCluster newCluster = ModificatorsManager.Instance.PickRandomModificators(
                        AbstractModificator.ModificatorTypes.NEGATIVE,
                        lastAddedCardScore,
                        encountedScore + 1f
                        );

                    if (newCluster != null && newCluster.Cards.Count > 0)
                    {
                        newCluster.AddStatusOnPick = AbstractModificator.ModificatorStatuses.CURSE;
                        newCluster.SetInteractable(false);
                        AddModificatorCardsCluster(newCluster);
                        if (ModificatorCardsClusters.Count > ModificatorsManager.Instance.MaxModificatorOptions)
                        {
                            RemoveModificatorCardsCluster(ModificatorCardsClusters.First());
                        }
                    }
                    else
                    {
                        Debug.LogWarning("not found any at range: " + lastAddedCardScore + "-" +encountedScore);
                    }

                    lastAddedCardScore = encountedScore;
                    delayTime = 0f;
                }

                yield return new WaitForEndOfFrame();
            }

            if (ModificatorCardsClusters.Count == 0)
            {
                _scoreText.text = ScoreManager.Instance.TradableScore.ToString("0");
                ShowTradeFailedUI();
            }
            else
            {
                SetAllCardsInteractable(true);
                ScoreManager.Instance.TradableScore = 0;
            }
        }

        _tradeCoroutine = null;
    }

    public override void SpendPicksLeft(int amount = 1)
    {
        _picksLeft -= amount;
        if (_picksLeft <= 0)
        {
            while (ModificatorCardsClusters.Count > 0)
            {
                RemoveModificatorCardsCluster(ModificatorCardsClusters.First());
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
        _scoreTrackTarget.transform.position = _showScoreTransform.position;
    }
    public void HideScore()
    {
        _scoreTrackTarget.transform.position = _hideScoreTransform.position;
    }

    public override void SetClusterDisplayedDescription(ModificatorCardsCluster cluster)
    {
        base.SetClusterDisplayedDescription(cluster);
        HideScore();
    }

    private void ShowTradeFailedUI()
    {
        _notEnoughPointsButtonsContainer.gameObject.SetActive(true);
        _startButtonsContainer.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}