using UnityEngine;

public class FastMessageUIManager : MonoBehaviour
{
    public float ShowDuration = 1f;
    public StaticSoundPlayer SoundOnShow;
    public GameObject TextContainer;

    private float _timeSinceShow = 0f;

    private void FixedUpdate()
    {
        _timeSinceShow += Time.fixedDeltaTime;

        if (_timeSinceShow > ShowDuration)
        {
            TextContainer.SetActive(false);

            if (!SoundOnShow.GetIsPlaying())
            {
                UIManager.Instance.FastMessageScreenOverlay.Hide();
            }
        }
    }

    private void OnEnable()
    {
        TextContainer.SetActive(true);
        SoundOnShow.PlaySound();
    }
}