using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ModificatorsContainer : MonoBehaviour
{
    const float CHANGE_SCENE_DELAY_AFTER_SPEND_ALL_PICKS = 0.5f;
    const float SHOW_CARDS_DELAY = 0.5f;


    public Transform CardSpawnPosition;
    public Transform CardsContainer;
    public Transform CardTrackTargetsContainer;
    public Transform CardsInfoContainer;
    [SerializeField] private ModificatorCardsCluster _clusterInstance;
    [SerializeField] private ModificatorVisualInfo _cardInfoInstance;

    private int _picksLeft = 1;

    private List<ModificatorCardsCluster> _modificatorCardsClusters = new();
    private Coroutine _changeSceneDelayAfterSpendAllPicksCoroutine = null;

    public event EventHandler<ModificatorCardsCluster> OnAddedItem;
    public event EventHandler<ModificatorCardsCluster> OnRemovedItem;

    public static ModificatorsContainer Instance;

    public List<ModificatorCardsCluster> ModificatorCardsClusters
    {
        get => _modificatorCardsClusters;
    }

    private void Awake()
    {
        _picksLeft = ModificatorsManager.Instance?.ModifiactorsPickAmount ?? 1;
        StartCoroutine(ShowCardsAfterDelay());
        if (Instance != null) throw new UnityException("Limit of 1 ModificatorsContainer instance per scene");
        Instance = this;
    }

    private IEnumerator ShowCardsAfterDelay()
    {
        if (ModificatorsManager.Instance != null)
        {
            yield return new WaitForSeconds(SHOW_CARDS_DELAY);
            AddModificatorCardsCluster(ModificatorsManager.Instance.PickRandomModifcators());
        }
    }

    public void AddModificatorCardsCluster(ModificatorCardsCluster cluster)
    {
        cluster.transform.SetParent(CardsContainer);
        cluster.transform.position = CardSpawnPosition.transform.position;

        UIElementTrackTarget.CreateTrackTarget(CardTrackTargetsContainer, cluster.transform);

        _modificatorCardsClusters.Add(cluster);

        OnAddedItem?.Invoke(this, cluster);
    }

    public void AddModificatorCardsCluster(List<ModificatorCardsCluster> clusters, float delay = 0.1f)
    {
        StartCoroutine(AddModificatorCardsClusterCoroutine(clusters));
    }
    private IEnumerator AddModificatorCardsClusterCoroutine(List<ModificatorCardsCluster> clusters, float delay = 0.1f)
    {
        foreach (ModificatorCardsCluster cluster in clusters)
        {
            AddModificatorCardsCluster(cluster);
            yield return new WaitForSeconds(delay);
        }
    }

    public void RemoveModificatorCardsCluster(ModificatorCardsCluster card)
    {
        int removedCardIndex = _modificatorCardsClusters.IndexOf(card);

        if (removedCardIndex != -1)
        {
            bool foundTrackTarget = false;
            foreach (Transform trackTargetTransform in CardTrackTargetsContainer)
            {
                if (
                    trackTargetTransform.TryGetComponent(out UIElementTrackTarget trackTarget) &&
                    trackTarget.TrackingUIElement == _modificatorCardsClusters[removedCardIndex].transform
                    )
                {
                    _modificatorCardsClusters[removedCardIndex].SetInteractable(false);

                    foundTrackTarget = true;
                    trackTarget.transform.SetParent(CardSpawnPosition);
                    trackTarget.transform.localPosition = Vector3.zero;

                    StartCoroutine(AwaitReachTrackTargetThenDestroy(trackTarget, _modificatorCardsClusters[removedCardIndex].transform));
                }
            }

            OnRemovedItem?.Invoke(this, _modificatorCardsClusters[removedCardIndex]);

            if (!foundTrackTarget && _modificatorCardsClusters.Count > removedCardIndex)
            {
                Destroy(_modificatorCardsClusters[removedCardIndex].gameObject);
            }

            _modificatorCardsClusters.RemoveAt(removedCardIndex);

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
        foreach (Transform child in CardsInfoContainer)
        {
            Destroy(child.gameObject);
        }

        if (cluster != null)
        {
            foreach (ModificatorCard card in cluster.Cards)
            {
                ModificatorVisualInfo newInfo = Instantiate(_cardInfoInstance, CardsInfoContainer);
                newInfo.Card = card;
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
            newInfo.Icon = icon;
        }
    }

    public void SpendPicksLeft(int amount = 1)
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
            _changeSceneDelayAfterSpendAllPicksCoroutine = StartCoroutine(ChangeSceneDelayAfterSpendAllPicks());
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

    private IEnumerator ChangeSceneDelayAfterSpendAllPicks()
    {
        yield return new WaitForSeconds(CHANGE_SCENE_DELAY_AFTER_SPEND_ALL_PICKS);
        UIManager.Instance.LoadSceneWithEffect(SceneList.GAMEPLAY);
    }

    public void SetAllCardsInteractable(bool value)
    {
        foreach (ModificatorCardsCluster cluster in _modificatorCardsClusters)
        {
            cluster.SetInteractable(value);
        }
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}