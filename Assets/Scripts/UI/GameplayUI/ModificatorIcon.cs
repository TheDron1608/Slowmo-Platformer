using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ModificatorIcon : Selectable, IModificatorInfo
{
    const float TRIGGER_ANIMATION_DURATION = 0.75f;
    const float RAISE_ANIMATION_MULTIPLIER = 7.5f;
    const float RAISED_OFFSET = -50f;
    const float IMAGE_ON_HOVER_SCALE_MULTIPLIER = 1.25f;
    const float IMAGE_ON_HOVER_SCALEUP_SPEED_MULTIPLIER = 15f;

    public AbstractModificator ModificatorInstance;
    [SerializeField] private RectTransform _iconContainer;
    [SerializeField] private Image _titleImage;
    [SerializeField] private Image _bgImage;
    [SerializeField] private Image _disableIconImage;

    private float _multiplier = 1f;
    private float _currentTriggerAnimationProgress = 0f;
    private Coroutine _triggerAnimationCoroutine;
    private bool _disabledModificator = false;
    private AbstractModificator _currentModificator;
    private bool _raising = false;

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

    public bool DisabledModificator
    {
        get => _disabledModificator;
        set
        {
            if (_disabledModificator == value) return;
            _disabledModificator = value;

            _disableIconImage.enabled = _disabledModificator;
        }
    }

    public AbstractModificator CurrentModificator
    {
        get => _currentModificator;
        set => _currentModificator = value;
    }

    public bool Raising
    {
        get => _raising;
        set => _raising = value;
    }

    public AbstractModificator.ModificatorStatuses Status
    {
        get => CurrentModificator.Status;
    }

    public Image TitleImage
    {
        get => _titleImage;
    }
    public Image BgImage
    {
        get => _bgImage;
    }

    public ModificatorLocalization Localization
    {
        get => CurrentModificator.Localization;
    }

    public float ModificatorPrice
    {
        get => CurrentModificator.ModificatorPrice;
    }

    public bool Multiplierable
    {
        get => CurrentModificator.Multiplierable;
    }

    public float ModificatorMultiplier
    {
        get => CurrentModificator.ModificatorMultiplier;
    }

    protected override void Start()
    {
        _currentModificator.CurrentIcon = this;
    }

    private void Update()
    {
        if (_triggerAnimationCoroutine != null) return;

        _iconContainer.localPosition = new Vector3(
            _iconContainer.localPosition.x,
            math.lerp(_iconContainer.localPosition.y, (_raising ? RAISED_OFFSET : 0f), Time.deltaTime * RAISE_ANIMATION_MULTIPLIER),
            _iconContainer.localPosition.z
            );
    }

    public void TriggerAnimation()
    {
        if (_raising) return;

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
                math.sin(_currentTriggerAnimationProgress / TRIGGER_ANIMATION_DURATION * math.PI) * RAISED_OFFSET,
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
        else if (BlessPickManager.Instance != null)
        {
            BlessPickManager.Instance.SetIconDisplayedDescription(this);
        }
        else if (UIManager.Instance.DifficultyCurseChoiseScreenOverlay.IsShown())
        {
            UIManager.Instance.DifficultyCurseChoiseScreenOverlay.DifficultyCurseChoiseUI
                .SetDisplayedInfo(new List<IModificatorInfo> { this });
        }
        else if (UIManager.GamePaused())
        {
            UIManager.Instance.ModificatorsScreenOverlay?.GetModificatorsUI().SetSelectedModificatorInfo(this);
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

    protected override void OnDestroy()
    {
        if (_currentModificator != null)
        {
            _currentModificator.CurrentIcon = null;
        }
    }
}