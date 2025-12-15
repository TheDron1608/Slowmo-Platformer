using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModificatorsContainer : MonoBehaviour
{
    public Transform CardSpawnPosition;
    public Transform CardsContainer;
    public Transform CardTrackTargetsContainer;
    public Transform CardsInfoContainer;
    [SerializeField] private ModificatorCardsCluster _clusterInstance;
    [SerializeField] private ModificatorCardInfo _cardInfoInstance;

    private List<ModificatorCardsCluster> _modificatorCardsClusters = new();

    public void AddModificatorCardsCluster(ModificatorCardsCluster cluster)
    {
        cluster.transform.SetParent(CardsContainer);
        cluster.transform.position = CardSpawnPosition.transform.position;

        UIElementTrackTarget.CreateTrackTarget(CardTrackTargetsContainer, cluster.transform);

        _modificatorCardsClusters.Add(cluster);
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
                newInfo.Title.text = card.Title;
                newInfo.Description.text = card.Description;
            }
        }
    }
}