using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders.Simulation;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public abstract class AbstractModificatorCardsManager : MonoBehaviour
{
    public Transform CardSpawnPosition;
    public Transform CardsContainer;
    public Transform CardTrackTargetsContainer;
    public Transform CardsInfoContainer;

    [SerializeField] protected ModificatorCardsCluster _clusterInstance;
    [SerializeField] protected PickNothingCard _pickNothingCardInstance;
    [SerializeField] protected ModificatorVisualInfo _cardInfoInstance;

    private List<AbstractCardItem> _cards = new();

    public event EventHandler<AbstractCardItem> OnAddedItem;
    public event EventHandler<AbstractCardItem> OnRemovedItem;

    public List<AbstractCardItem> Cards
    {
        get => _cards;
        protected set => _cards = value;
    }

    public void AddCard(AbstractCardItem cluster)
    {
        cluster.transform.SetParent(CardsContainer);
        cluster.transform.position = CardSpawnPosition.transform.position;

        UIElementTrackTarget.CreateTrackTarget(CardTrackTargetsContainer, cluster);

        _cards.Add(cluster);

        OnAddedItem?.Invoke(this, cluster);
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
            bool foundTrackTarget = false;
            foreach (Transform trackTargetTransform in CardTrackTargetsContainer)
            {
                if (
                    trackTargetTransform.TryGetComponent(out UIElementTrackTarget trackTarget) &&
                    trackTarget.TrackingUIElement == _cards[removedCardIndex].transform
                    )
                {
                    _cards[removedCardIndex].SetInteractable(false);

                    foundTrackTarget = true;
                    trackTarget.transform.SetParent(CardSpawnPosition);
                    trackTarget.transform.localPosition = Vector3.zero;

                    StartCoroutine(AwaitReachTrackTargetThenDestroy(trackTarget, _cards[removedCardIndex].transform));
                }
            }

            OnRemovedItem?.Invoke(this, _cards[removedCardIndex]);

            if (!foundTrackTarget && _cards.Count > removedCardIndex)
            {
                Destroy(_cards[removedCardIndex].gameObject);
            }

            _cards.RemoveAt(removedCardIndex);

        }
    }
    private IEnumerator AwaitReachTrackTargetThenDestroy(UIElementTrackTarget trackTarget, Transform trackedUIElement)
    {
        while (
            !trackTarget.IsDestroyed() && 
            !trackedUIElement.IsDestroyed() &&
            Vector2.Distance(trackTarget.transform.position, trackedUIElement.position) > 0.05f
            )
        {
            yield return new WaitForEndOfFrame();
        }

        if (!trackedUIElement.IsDestroyed())
        {
            Destroy(trackedUIElement.gameObject);
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
                ModificatorVisualInfo newInfo = Instantiate(_cardInfoInstance, CardsInfoContainer);
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
    public virtual void FinishTrade()
    {
        ClearAllCards();
    }
}