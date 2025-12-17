using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
            float targetRotation = Cards.Count == 1 ? 0f : ((i / (Cards.Count - 1f)) - 0.5f) * CLUSTER_HAND_MAX_ROTATION;
            Cards[i].transform.Rotate(new Vector3(0, 0f, 1f), targetRotation);
            Cards[i].transform.position = _cardsContainer.transform.position + Vector3.left * (targetRotation / 60f) * math.abs(Cards[i].GetComponent<RectTransform>().rect.width);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (GameObjectUtility.TryGetComponentInParentRecursive(transform, out ModificatorsContainer container))
        {
            container.SetClusterDisplayedDescription(this);
        }
    }

    public void Pick()
    {
        if (GameObjectUtility.TryGetComponentInParentRecursive(transform, out ModificatorsContainer container))
        {
            foreach (ModificatorCard card in Cards)
            {
                ModificatorsManager.Instance.AddModificator(card.ModificatorInstance);
            }

            foreach (UIElementTrackTarget trackTarget in container.GetComponentsInChildren<UIElementTrackTarget>())
            {
                if (trackTarget.TrackingUIElement.gameObject == gameObject)
                {
                    trackTarget.transform.SetParent(container.CardSpawnPosition);
                    trackTarget.transform.localPosition = Vector3.zero;
                    container.SetClusterDisplayedDescription(null);
                    break;
                }
            }

            container.SpendPicksLeft();
        }
    }

    public void SetInteractable(bool value)
    {
        foreach (ModificatorCard card in Cards)
        {
            card.GetComponent<Button>().interactable = value;
        }
    }
}