using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class ModificatorCardsCluster : MonoBehaviour, IPointerEnterHandler
{
    const float CLUSTER_HAND_MAX_ROTATION = 15f;

    [SerializeField] private RectTransform _cardsContainer;

    public List<ModificatorCard> Cards = new();

    private void Start()
    {
        UpdateClustersPosition();
    }

    public void AddCard(ModificatorCard card)
    {
        Cards.Add(Instantiate(card));
        UpdateClustersPosition();
    }

    public void RemoveCard(ModificatorCard card)
    {
        Cards.Remove(card);
        Destroy(card.gameObject);
        UpdateClustersPosition();
    }

    private void UpdateClustersPosition()
    {
        for (int i = 0; i < Cards.Count; i++)
        {
            Cards[i].transform.SetParent(_cardsContainer.transform);
            float targetRotation = ((i / (Cards.Count - 1f)) - 0.5f) * CLUSTER_HAND_MAX_ROTATION;
            Cards[i].transform.Rotate(new Vector3(0, 0f, 1f), targetRotation);
            Cards[i].transform.position = _cardsContainer.position + Vector3.left * (targetRotation / 60f) * Cards[i].GetComponent<RectTransform>().rect.width;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (GameObjectUtility.TryGetComponentInParentRecursive(transform, out ModificatorsContainer container))
        {
            container.SetClusterDisplayedDescription(this);
        }
    }
}