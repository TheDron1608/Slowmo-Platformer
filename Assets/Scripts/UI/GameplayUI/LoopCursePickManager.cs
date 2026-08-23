using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class LoopCursePickManager : AbstractModificatorCardsManager
{
    const float TRADE_DELAY = 0.75f;

    public List<AbstractModificator> LoopModificators = new();

    [SerializeField] private Sprite FirstCardSprite;
    [SerializeField] private Sprite MiddleCardSprite;
    [SerializeField] private Sprite LastCardSprite;
    [SerializeField] private Sprite SingleCardSprite;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private UIElementTrackTarget _scoreTrackTarget;
    [SerializeField] private Transform _showScoreTransform;
    [SerializeField] private Transform _hideScoreTransform;

    public static LoopCursePickManager Instance;

    public override string GetAnalyticsChoiseTypeName()
    {
        return "LoopPick";
    }

    protected override void OnAwake()
    {
        base.OnAwake();

        PicksLeft = ModificatorsManager.Instance?.ModifiactorsPickAmount ?? 1;
        _scoreText.text = (ScoreManager.Instance.TradableScore + ScoreManager.Instance.CurrentCombo * ScoreManager.Instance.CurrentMultiplier).ToString("0");

        if (Instance != null && !Instance.IsDestroyed()) throw new UnityException("Limit of 1 CursePickManager instance per scene");
        Instance = this;

        RerollsLeft = ModificatorsManager.Instance.CursePickRerolls;
    }

    private void Start()
    {
        Trade();
    }

    public void Trade()
    {
        for (int i = 0; i < LoopModificators.Count; i++)
        {
            bool validMod = true;
            foreach (AbstractModificator currentMod in ModificatorsManager.Instance.CurrentModificators)
            {
                if (LoopModificators[i].GetIsRestrictedWith(currentMod))
                {
                    validMod = false;
                    break;
                }
            }

            if (validMod)
            {
                Sprite cardBg = null;
                if (LoopModificators.Count <= 1) cardBg = SingleCardSprite;
                else if (i == 0) cardBg = FirstCardSprite;
                else if (i == LoopModificators.Count - 1) cardBg = LastCardSprite;
                else cardBg = MiddleCardSprite;

                ModificatorCardsCluster newCluster = Instantiate(_clusterInstance);
                newCluster.AddModificator(LoopModificators[i], cardBg);
                newCluster.AddStatusOnPick = AbstractModificator.ModificatorStatuses.PERMANENT;
                AddCard(newCluster);
            }
        }

        if (Cards.Count == 0) FinishTrade();
    }


    public override void FinishTrade()
    {
        base.FinishTrade();
        UIManager.Instance.LoadSceneWithEffect(SceneList.GAMEPLAY);
    }

    public override void ForceReroll()
    {
        base.ForceReroll();
        Trade();
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

            StartCoroutine(FinishTradeAfterDelay());
        }
        else
        {
            SetAllCardsInteractable(true);
        }
    }

    private IEnumerator FinishTradeAfterDelay()
    {
        yield return new WaitForSeconds(TRADE_DELAY);
        FinishTrade();
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}