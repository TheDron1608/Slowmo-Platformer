using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BlindnessOverlay : MonoBehaviour
{
    public float ConstantDuration = 1f;
    public float FadeOutDuration = 1f;

    [SerializeField] RawImage _screenContainer;
    [SerializeField] StaticSoundPlayer _blindSound;

    private float _lifeTime = 0;

    public void Restart()
    {
        _screenContainer.enabled = false;
        _lifeTime = 0f;
        StartCoroutine(ScreenshotAtEndOfFrame());
        _blindSound.Volume = 0f;
        _blindSound.PlaySound(true);
    }

    private IEnumerator ScreenshotAtEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        _screenContainer.texture = ScreenCapture.CaptureScreenshotAsTexture();
        _screenContainer.enabled = true;
    }

    private void Update()
    {
        _lifeTime += Time.deltaTime;

        float currentStunIntencity = NumberMath.LimitFloatBetweenZeroAndOne(1f - ((_lifeTime - ConstantDuration) / FadeOutDuration));

        if (currentStunIntencity > 0f)
        {
            _screenContainer.color = new Color(
                _screenContainer.color.r,
                _screenContainer.color.g,
                _screenContainer.color.b,
                currentStunIntencity
                );

            _blindSound.Volume = currentStunIntencity;
        }
        else
        {
            _blindSound.BreakAllSounds();
            UIManager.Instance.BlindnessOverlay.Hide();
        }
    }
}