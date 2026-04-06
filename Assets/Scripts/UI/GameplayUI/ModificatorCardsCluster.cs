using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ModificatorCardsCluster : AbstractCardItem
{
    const float CLUSTER_HAND_BASE_ROTATION = 30f;
    const float CLUSTER_HARD_ROTATION_CHANGE_SPEED = 5f;

    [SerializeField] private RectTransform _cardsContainer;

    public List<ModificatorCard> Cards = new();

    private AbstractModificator.ModificatorStatuses _addStatusOnPick;
    private float _currentClusterRotation = CLUSTER_HAND_BASE_ROTATION;

    public AbstractModificator.ModificatorStatuses AddStatusOnPick
    {
        get => _addStatusOnPick;
        set => _addStatusOnPick = value;
    }

    protected override void Start()
    {
        base.Start();

        if (
            EventSystem.current != null &&
            CurrentDeviceTracker.GetGamepadIsConnected() &&
            (
                EventSystem.current.currentSelectedGameObject == null ||
                EventSystem.current.currentSelectedGameObject.IsDestroyed()
            )
            )
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }

    private void Update()
    {
        if (EventSystem.current == null) return;

        _currentClusterRotation = math.lerp(
            _currentClusterRotation,
            CLUSTER_HAND_BASE_ROTATION * (EventSystem.current.currentSelectedGameObject == gameObject ? Cards.Count - 1f : 1f),
            Time.deltaTime * CLUSTER_HARD_ROTATION_CHANGE_SPEED
            );

        for (int i = 0; i < Cards.Count; i++)
        {
            Cards[i].transform.SetParent(_cardsContainer.transform);
            float targetRotation = Cards.Count == 1 ? 0f : ((i / (Cards.Count - 1f)) - 0.5f) * _currentClusterRotation;
            Cards[i].transform.rotation = Cards[i].transform.parent.rotation;
            Cards[i].transform.Rotate(new Vector3(0, 0f, 1f), targetRotation);
            Cards[i].transform.position = _cardsContainer.transform.position + Vector3.left * (targetRotation / 60f) * math.abs(Cards[i].GetComponent<RectTransform>().rect.width);
        }
    }

    public void AddModificator(AbstractModificator modificator)
    {
        ModificatorCard newCard = ModificatorsManager.Instance.CreateModificatorCard(modificator, transform);
        newCard.CurrentCluster = this;
        Cards.Add(newCard);
        Cards.Sort((a, b) => a.ModificatorInstance.ModificatorPrice.CompareTo(b.ModificatorInstance.ModificatorPrice));
    }

    public void RemoveModificator(AbstractModificator modificator)
    {
        ModificatorCard card = Cards.Find(e => 
        e.ModificatorInstance == modificator ||
        e.ModificatorInstance.OriginalModificator == modificator
        );

        if (card != null)
        {
            Cards.Remove(card);
            Destroy(card.gameObject);
        }
    }

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
        if (GameObjectUtility.TryGetComponentInParentRecursive(transform, out AbstractModificatorCardsManager container))
        {
            container.SetClusterDisplayedDescription(this);
        }
        ShowOverrideCurrentModificators();
    }

    private void ShowOverrideCurrentModificators()
    {
        foreach (AbstractModificator currentModificator in ModificatorsManager.Instance.CurrentModificators)
        {
            if (currentModificator.CurrentIcon == null) continue;

            if (Cards.Any(e => e.ModificatorInstance.GetIsOverriding(currentModificator)))
            {
                currentModificator.CurrentIcon.Raising = true;
                currentModificator.CurrentIcon.DisabledModificator = true;
            }
            else
            {
                currentModificator.CurrentIcon.Raising = false;
                currentModificator.CurrentIcon.DisabledModificator = currentModificator.DisabledModificator;
            }
        }
    }

    private void HideOverrideCurrentModificators()
    {
        foreach (AbstractModificator currentModificator in ModificatorsManager.Instance.CurrentModificators)
        {
            if (currentModificator.CurrentIcon == null) continue;

            currentModificator.CurrentIcon.Raising = false;
            currentModificator.CurrentIcon.DisabledModificator = currentModificator.DisabledModificator;
        }
    }

    public override void Pick()
    {
        HideOverrideCurrentModificators();

        if (GameObjectUtility.TryGetComponentInParentRecursive(transform, out AbstractModificatorCardsManager container))
        {
            foreach (ModificatorCard card in Cards)
            {
                ModificatorsManager.Instance.AddModificator(card.ModificatorInstance, AddStatusOnPick);
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

    protected override void OnDestroy()
    {
        if (EventSystem.current == null) return;
        if (
            EventSystem.current.currentSelectedGameObject == gameObject &&
            CursePickManager.Instance?.Cards.Count > 0
            )
        {
            EventSystem.current.SetSelectedGameObject(CursePickManager.Instance.Cards.First().gameObject);
        }
    }
}