using UnityEngine;
using UnityEngine.UI;

public class ButtonOnClickPlayClickSound : MonoBehaviour
{
    [SerializeField]
    private Button _button;

    private void Start()
    {
        _button.onClick.AddListener(PlayClickSound);
    }

    public void PlayClickSound()
    {
        SoundManager.Instance.PlaySound(SoundManager.Instance.ButtonClickSound);
    }
}
