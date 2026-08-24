using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public abstract class AbstractModificatorCardsManager : MonoBehaviour
{
    const float DEFAULT_INFO_WIDTH_MULT = 2f;
    const float ADD_MODIFICATOR_VOLUME_MULT = 0.333f;

    public Transform CardSpawnPosition;
    public Transform CardsContainer;
    public Transform CardTrackTargetsContainer;
    public Transform CardsInfoContainer;
    public TextMeshProUGUI PicksLeftText;

    public LocalizedString PicksLeftLocalization;
    public LocalizedString StartTitle;
    public LocalizedString StartDesc;

    public Scrollbar Scrollbar;
    [SerializeField] protected ModificatorCardsCluster _clusterInstance;
    [SerializeField] protected PickNothingCard _pickNothingCardInstance;
    [SerializeField] protected RerollCard _rerollCardInstace;
    [SerializeField] protected ModificatorVisualInfo _cardInfoInstance;

    private List<AbstractCardItem> _cards = new();
    private int _rerollsLeft = 0;
    private Dictionary<AbstractCardItem, bool> _cardPickInfo = new();
    private int _picksLeft;
    private bool _showDefaultDesc = true;

    public event EventHandler<AbstractCardItem> OnAddedItem;
    public event EventHandler<AbstractCardItem> OnRemovedItem;

    public abstract string GetAnalyticsChoiseTypeName();

    public bool ShowDefaultDesc
    {
        get => _showDefaultDesc;
        set
        {
            if (_showDefaultDesc == value) return;
            _showDefaultDesc = value;

            if (_showDefaultDesc)
            {
                SetDefaultDisplayedInfo();
            }
            else
            {
                SetDisplayedInfo(null);
            }
        }
    }

    private void Awake()
    {
        OnAwake();
    }

    protected virtual void OnAwake()
    {
        SetDefaultDisplayedInfo();
        PicksLeftText.text = PicksLeftLocalization.GetLocalizedString() + _picksLeft.ToString();
    }

    public List<AbstractCardItem> Cards
    {
        get => _cards;
        protected set => _cards = value;
    }

    public int RerollsLeft
    {
        get => _rerollsLeft;
        protected set => _rerollsLeft = value;
    }

    public int PicksLeft
    {
        get => _picksLeft;
        set
        {
            _picksLeft = value;
            PicksLeftText.text = PicksLeftLocalization.GetLocalizedString() + _picksLeft.ToString();
            PicksLeftText.enabled = _picksLeft > 0;
        }
    }

    public Dictionary<AbstractCardItem, bool> CardPickInfo
    {
        get => _cardPickInfo;
        set => _cardPickInfo = value;
    }

    public void AddCard(AbstractCardItem card)
    {
        card.transform.SetParent(CardsContainer);
        card.transform.localScale = Vector3.one * 2f;
        card.transform.position = CardSpawnPosition.transform.position;

        UIElementTrackTarget.CreateTrackTarget(CardTrackTargetsContainer, card);

        _cards.Add(card);
        _cardPickInfo.Add(card, false);

        if (card is ModificatorCardsCluster cluster)
        {
            cluster.SVEffects.SoundOnClick.PlaySound(false, null, null, ADD_MODIFICATOR_VOLUME_MULT);
        }

        OnAddedItem?.Invoke(this, card);
    }


    public void ClearAllCards()
    {
        while (Cards.Count > 0)
        {
            RemoveCard(Cards.First());
        }
    }

    public void RemoveCard(AbstractCardItem card)
    {
        int removedCardIndex = _cards.IndexOf(card);

        if (removedCardIndex != -1)
        {
            foreach (Transform trackTargetTransform in CardTrackTargetsContainer)
            {
                if (
                    trackTargetTransform.TryGetComponent(out UIElementTrackTarget trackTarget) &&
                    trackTarget.TrackingUIElement == _cards[removedCardIndex].transform
                    )
                {
                    _cards[removedCardIndex].SetInteractable(false);

                    trackTarget.transform.SetParent(CardSpawnPosition);
                    trackTarget.transform.localPosition = Vector3.zero;
                }
            }

            OnRemovedItem?.Invoke(this, _cards[removedCardIndex]);

            _cards.RemoveAt(removedCardIndex);
        }
    }

    public void SetDefaultDisplayedInfo()
    {
        if (_showDefaultDesc)
        {
            foreach (Transform child in CardsInfoContainer)
            {
                Destroy(child.gameObject);
            }

            ModificatorVisualInfo newInfo = Instantiate(_cardInfoInstance, CardsInfoContainer);
            newInfo.GetComponent<RectTransform>().sizeDelta *= Vector3.right * DEFAULT_INFO_WIDTH_MULT;
            newInfo.Title.text = StartTitle?.GetLocalizedString() ?? "";
            newInfo.Description.text = StartDesc?.GetLocalizedString() ?? "";
        }
        else
        {
            SetDisplayedInfo(null);
        }
    }

    public void SetClusterDisplayedDescription(ModificatorCardsCluster cluster)
    {
        SetDisplayedInfo(cluster?.Cards.ConvertAll(e => e as IModificatorInfo));
    }

    public virtual void SetDisplayedInfo(List<IModificatorInfo> infos)
    {
        foreach (Transform child in CardsInfoContainer)
        {
            Destroy(child.gameObject);
        }

        if (infos != null)
        {
            foreach (IModificatorInfo info in infos)
            {
                if (info.Localization == null) continue;
                ModificatorVisualInfo newInfo = Instantiate(_cardInfoInstance, CardsInfoContainer);
                newInfo.transform.SetAsFirstSibling();
                newInfo.TargetInfo = info;
            }
        }
    }

    public void SetIconDisplayedDescription(ModificatorIcon icon)
    {
        foreach (Transform child in CardsInfoContainer)
        {
            Destroy(child.gameObject);
        }

        if (icon != null)
        {
            ModificatorVisualInfo newInfo = Instantiate(_cardInfoInstance, CardsInfoContainer);
            newInfo.TargetInfo = icon;
        }
    }

    public void SetAllCardsInteractable(bool value)
    {
        foreach (AbstractCardItem cluster in _cards)
        {
            cluster.SetInteractable(value);
        }
    }

    public abstract void SpendPicksLeft(int amount = 1);

    public virtual void FinishTrade(bool pickNothing = false)
    {
        if (GetAnalyticsChoiseTypeName() != null)
        {
            foreach (var pickInfo in _cardPickInfo)
            {
                if (pickInfo.Key is ModificatorCardsCluster cluster)
                {
                    foreach (ModificatorCard modCard in cluster.Cards)
                    {
                        AnalyticsManager.Instance?.RecordEvent(
                            new ModificatorPickChoiseAnalyticsEvent(
                                modCard.ModificatorInstance.gameObject.name,
                                !pickInfo.Value,
                                GetAnalyticsChoiseTypeName()
                                )
                            );
                    }
                }
            }
        }

        ClearAllCards();

        //collection can be modified on loop,
        //may cause bugs but where are too less mods amount affecting on finish choise modifiers to worry about
        for (int i = 0; i < ModificatorsManager.Instance.CurrentModificators.Count; i++)
        {
            if (!ModificatorsManager.Instance.CurrentModificators[i].DisabledModificator)
            {
                ModificatorsManager.Instance.CurrentModificators[i].OnModificatorChoiseFinished(this);
            }
        }
    }

    public bool TryReroll()
    {
        if (_rerollsLeft > 0)
        {
            ForceReroll();
            return true;
        }
        return false;
    }

    public virtual void ForceReroll()
    {
        _rerollsLeft--;
        RandomManager.Instance.InitNewSeed();
        ClearAllCards();
        SetDisplayedInfo(null);
    }
}