using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ModificatorIcon : Selectable
{
    const float TRIGGER_ANIMATION_DURATION = 0.75f;
    const float TRIGGER_ANIMATION_OFFSET = -50f;
    const string ICON_TITLE_GO_NAME = "IconTitle";
    const string DISABLE_ICON_GO_NAME = "DisableTitle";
    const float IMAGE_ON_HOVER_SCALE_MULTIPLIER = 1.25f;
    const float IMAGE_ON_HOVER_SCALEUP_SPEED_MULTIPLIER = 15f;

    public AbstractModificator ModificatorInstance;
    [SerializeField] private RectTransform _iconContainer;

    private float _multiplier = 1f;
    private string _localizedTitle;
    private string _localizedDescription;
    private float _currentTriggerAnimationProgress = 0f;
    private Coroutine _triggerAnimationCoroutine;
    private Image _iconTitleImage;
    private Image _disableIconImage;
    private Image _bgImage;
    private bool _disabledIcon = false;
    private AbstractModificator _currentModificator;

    public float Multiplier
    {
        get => _multiplier;
        set
        {
            _multiplier = value;
            if (TryGetComponent(out ModificatorLocalizationMultiplierableVariables localizedVars))
            {
                localizedVars.UpdateLocalizedValues();
            }
        }
    }

    public string LocalizedTitle
    {
        get => _localizedTitle;
        set => _localizedTitle = value;
    }

    public string LocalizedDescription
    {
        get => _localizedDescription;
        set => _localizedDescription = value;
    }

    public bool DisabledIcon
    {
        get => _disabledIcon;
        set
        {
            if (_disabledIcon == value) return;
            _disabledIcon = value;

            _disableIconImage.enabled = _disabledIcon;
        }
    }

    public AbstractModificator CurrentModificator
    {
        get => _currentModificator;
        set => _currentModificator = value;
    }

    protected override void Awake()
    {
        base.Awake();
        _iconTitleImage = GameObjectUtility.FindGameObjectInChildrenByName(transform, ICON_TITLE_GO_NAME)?.GetComponent<Image>();
        _disableIconImage = GameObjectUtility.FindGameObjectInChildrenByName(transform, DISABLE_ICON_GO_NAME)?.GetComponent<Image>();
        _bgImage = transform.GetComponentInChildren<Image>();
        targetGraphic = _bgImage;
    }

    public void TriggerAnimation()
    {
        if (_triggerAnimationCoroutine != null)
        {
            StopCoroutine(_triggerAnimationCoroutine);
        }
        _triggerAnimationCoroutine = StartCoroutine(TriggerAnimationCoroutine());
    }

    private IEnumerator TriggerAnimationCoroutine()
    {
        if (_currentTriggerAnimationProgress > TRIGGER_ANIMATION_DURATION / 2)
        {
            _currentTriggerAnimationProgress = TRIGGER_ANIMATION_DURATION - _currentTriggerAnimationProgress;
        }

        while (_currentTriggerAnimationProgress < TRIGGER_ANIMATION_DURATION)
        {
            _iconContainer.localPosition = new Vector3(
                _iconContainer.localPosition.x,
                math.sin(_currentTriggerAnimationProgress / TRIGGER_ANIMATION_DURATION * math.PI) * TRIGGER_ANIMATION_OFFSET,
                _iconContainer.localPosition.z
                );

            _currentTriggerAnimationProgress += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        _currentTriggerAnimationProgress = 0f;
        _triggerAnimationCoroutine = null;
    }

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
        ShowInfo();
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);
        HideInfo();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        ShowInfo();
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        HideInfo();
    }

    private void ShowInfo()
    {
        if (CursePickManager.Instance != null)
        {
            CursePickManager.Instance.SetIconDisplayedDescription(this);
        }
        else
        {
            if (UIManager.GamePaused())
            {
                UIManager.Instance.ModificatorsScreenOverlay?.GetModificatorsUI().SetSelectedModificatorInfo(LocalizedTitle, LocalizedDescription, DisabledIcon);
                UIManager.Instance.ModificatorsScreenOverlay?.GetModificatorsUI().SetSelectedModificatorInfoEnabled(true);
            }
        }
    }

    private void HideInfo()
    {
        if (CursePickManager.Instance != null)
        {
            CursePickManager.Instance.SetIconDisplayedDescription(null);
        }
        else
        {
            UIManager.Instance.ModificatorsScreenOverlay?.GetModificatorsUI().SetSelectedModificatorInfoEnabled(false);
        }
    }
}