using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ModificatorsContainer : MonoBehaviour
{
    const float CHANGE_SCENE_DELAY_AFTER_SPEND_ALL_PICKS = 0.5f;
    const float SHOW_CARDS_DELAY = 0.5f;

    public string SceneNameAfterSpendAllPicks = "Gameplay";
    public Transform CardSpawnPosition;
    public Transform CardsContainer;
    public Transform CardTrackTargetsContainer;
    public Transform CardsInfoContainer;
    [SerializeField] private ModificatorCardsCluster _clusterInstance;
    [SerializeField] private ModificatorCardInfo _cardInfoInstance;

    private int _picksLeft = 1;

    private List<ModificatorCardsCluster> _modificatorCardsClusters = new();
    private Coroutine _changeSceneDelayAfterSpendAllPicksCoroutine = null;

    public event EventHandler<ModificatorCardsCluster> OnAddedItem;
    public event EventHandler<ModificatorCardsCluster> OnRemovedItem;

    public static ModificatorsContainer Instance;

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
            OnRemovedItem?.Invoke(this, _modificatorCardsClusters[removedCardIndex]);

            Destroy(_modificatorCardsClusters[removedCardIndex].gameObject);
            _modificatorCardsClusters.RemoveAt(removedCardIndex);
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
                ModificatorCardInfo newInfo = Instantiate(_cardInfoInstance, CardsInfoContainer);
                newInfo.Card = card;
            }
        }
    }

    public void SpendPicksLeft(int amount = 1)
    {
        _picksLeft -= amount;
        if (_picksLeft <= 0)
        {
            foreach (AbstractModificator modificator in ModificatorsManager.Instance.CurrentModificators)
            {
                if (!modificator.DisabledModificator)
                {
                    modificator.OnModificatorChoiseFinished();
                }
            }
            SetAllCardsInteractable(false);
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
        UIManager.Instance.LoadSceneWithEffect(SceneNameAfterSpendAllPicks);
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