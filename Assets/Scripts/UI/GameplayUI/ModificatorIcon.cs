using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class ModificatorIcon : MonoBehaviour
{
    const float TRIGGER_ANIMATION_DURATION = 0.75f;
    const float TRIGGER_ANIMATION_OFFSET = -50f;

    public AbstractModificator ModificatorInstance;
    [SerializeField] private RectTransform _iconContainer;

    private float _currentTriggerAnimationProgress = 0f;
    private Coroutine _triggerAnimationCoroutine;

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
}