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

    [SerializeField] private RectTransform _cardsContainer;
    [SerializeField] private ButtonSoundVisualEffects _svEffects;

    public List<ModificatorCard> Cards = new();

    private AbstractModificator.ModificatorStatuses _addStatusOnPick;

    public AbstractModificator.ModificatorStatuses AddStatusOnPick
    {
        get => _addStatusOnPick;
        set => _addStatusOnPick = value;
    }

    public ButtonSoundVisualEffects SVEffects
    {
        get => _svEffects;
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {

        _svEffects = GetComponent<ButtonSoundVisualEffects>();
    }
#endif

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

    //cool but disturbing visual effect

    private void UpdateCardsPositions()
    {
        for (int i = 0; i < Cards.Count; i++)
        {
            Cards[i].transform.SetAsLastSibling();
            Cards[i].transform.SetParent(_cardsContainer.transform);
            float targetRotation = Cards.Count == 1 ? 0f : ((i / (Cards.Count - 1f)) - 0.5f) * CLUSTER_HAND_BASE_ROTATION;
            Cards[i].transform.rotation = Cards[i].transform.parent.rotation;
            Cards[i].transform.Rotate(new Vector3(0, 0f, 1f), targetRotation);
            Cards[i].transform.position = _cardsContainer.transform.position + Vector3.left * (targetRotation / 60f) * math.abs(Cards[i].GetComponent<RectTransform>().rect.width);
        }
    }

    public void AddModificator(List<AbstractModificator> modificators)
    {
        foreach (AbstractModificator modificator in modificators)
        {
            AddModificator(modificator);
        }
    }

    public void AddModificator(AbstractModificator modificator, Sprite overrideCardsBg = null)
    {
        ModificatorCard newCard = ModificatorsManager.Instance.CreateModificatorCard(modificator, transform, overrideCardsBg);
        newCard.CurrentCluster = this;
        Cards.Add(newCard);
        Cards.Sort((a, b) => a.ModificatorInstance.ModificatorPrice.CompareTo(b.ModificatorInstance.ModificatorPrice));
        SVEffects.SoundOnHoverSelect.DefaultSound = ModificatorsManager.Instance.CardTierSelectSounds[Cards.Max(e => (int)e.ModificatorInstance.ModificatorTier)];
        SVEffects.SoundOnClick.DefaultSound = ModificatorsManager.Instance.CardTierPickSounds[Cards.Max(e => (int)e.ModificatorInstance.ModificatorTier)];
        UpdateCardsPositions();
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

    public override void OnDeselect(BaseEventData eventData)
    {
        if (
            GameObjectUtility.TryGetComponentInParentRecursive(transform, out AbstractModificatorCardsManager container) &&
            !container.Scrollbar.gameObject.activeSelf
            )
        {
            base.OnDeselect(eventData);
        }
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
        base.Pick();

        HideOverrideCurrentModificators();

        if (GameObjectUtility.TryGetComponentInParentRecursive(transform, out AbstractModificatorCardsManager container))
        {
            List<AbstractModificator> addedModificators = new();
            foreach (ModificatorCard card in Cards)
            {
                addedModificators.Add(ModificatorsManager.Instance.AddModificator(card.ModificatorInstance, AddStatusOnPick));
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