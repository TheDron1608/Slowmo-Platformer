using System;
using System.Collections;
using UnityEngine;

public class CanvasAplha : MonoBehaviour
{
    const float ALPHA_CHANGE_SPEED_MULTIPLIER = 2f;

    private CanvasGroup _canvasGroupComponent;
    private void Start()
    {
        if (!TryGetComponent<CanvasGroup>(out _canvasGroupComponent))
        {
            throw new UnityException($"Canvas {gameObject.name} must contain CanvasGroup component");
        }
    }

    public void HideCanvas()
    {
        StartCoroutine(SetCanvasAlpha(false));
    }
    public void ShowCanvas()
    {
        StartCoroutine(SetCanvasAlpha(true));
    }

    private IEnumerator SetCanvasAlpha(bool turnVisible)
    {
        if (turnVisible)
        {
            _canvasGroupComponent.interactable = true;
            while (_canvasGroupComponent.alpha < 1)
            {
                _canvasGroupComponent.alpha += Time.deltaTime * ALPHA_CHANGE_SPEED_MULTIPLIER;
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            while (_canvasGroupComponent.alpha > 0)
            {
                _canvasGroupComponent.alpha -= Time.deltaTime * ALPHA_CHANGE_SPEED_MULTIPLIER;
                yield return new WaitForEndOfFrame();
            }
            _canvasGroupComponent.interactable = false;
        }
    }
}
