using UnityEngine;

public class SwapTrackTargetOnReachCurrentTrackTarget : MonoBehaviour
{
    public float Delay;
    public UIElementTrackTarget SwapFrom;
    public UIElementTrackTarget SwapTo;

    private float _swapFromReachedTime = 0f;

    private void Start()
    {
        SwapFrom.enabled = true;
        SwapTo.enabled = false;
    }

    private void Update()
    {
        if (!SwapFrom.isActiveAndEnabled) return;

        if (SwapFrom.GetIsReachedTrackTarget())
        {
            _swapFromReachedTime += Time.unscaledDeltaTime;
        }
        if (_swapFromReachedTime >= Delay)
        {
            SwapFrom.enabled = false;
            SwapTo.enabled = true;
        }
    }
}