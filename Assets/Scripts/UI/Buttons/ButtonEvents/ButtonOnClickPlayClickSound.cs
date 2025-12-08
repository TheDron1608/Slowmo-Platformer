using UnityEngine;
using UnityEngine.UI;

public class ButtonOnClickPlayClickSound : MonoBehaviour
{
    public AbstractSoundPlayer SoundOnClick;
    [SerializeField]
    private Button _button;

    private void Start()
    {
        _button.onClick.AddListener(PlayClickSound);
    }

    public void PlayClickSound()
    {
        SoundOnClick.PlaySound();
    }
}
