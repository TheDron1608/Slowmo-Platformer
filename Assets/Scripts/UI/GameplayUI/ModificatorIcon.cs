using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ModificatorIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    const float TRIGGER_ANIMATION_DURATION = 0.75f;
    const float TRIGGER_ANIMATION_OFFSET = -50f;
    const string ALLOWED_TO_SHOW_NONPAUSE_MODIFICATOR_INFO = "ModificatorChoise";

    public AbstractModificator ModificatorInstance;
    [SerializeField] private RectTransform _iconContainer;

    private float _multiplier = 1f;
    private string _localizedTitle;
    private string _localizedDescription;
    private float _currentTriggerAnimationProgress = 0f;
    private Coroutine _triggerAnimationCoroutine;

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

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (UIManager.GamePaused() || SceneManager.GetActiveScene().name == ALLOWED_TO_SHOW_NONPAUSE_MODIFICATOR_INFO)
        {
            UIManager.Instance.ModificatorsScreenOverlay?.GetModificatorsUI().SetSelectedModificatorInfo(LocalizedTitle, LocalizedDescription);
            UIManager.Instance.ModificatorsScreenOverlay?.GetModificatorsUI().SetSelectedModificatorInfoEnabled(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.Instance.ModificatorsScreenOverlay?.GetModificatorsUI().SetSelectedModificatorInfoEnabled(false);
    }
}