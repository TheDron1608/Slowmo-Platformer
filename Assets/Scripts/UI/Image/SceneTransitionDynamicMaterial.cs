using UnityEngine;
using UnityEngine.UI;

public class SceneTransitionDynamicMaterial : MonoBehaviour
{
    [SerializeField] private Image _image;

    private void Awake()
    {
        if (SessionManager.Instance?.CurrentSelectedPlayer != null)
        {
            _image.material = SessionManager.Instance.CurrentSelectedPlayer.InfoBgMaterial;
        }
        _image.enabled = true;
    }
}