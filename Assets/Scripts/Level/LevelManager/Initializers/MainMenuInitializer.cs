using UnityEngine;
using UnityEngine.UI;

public class MainMenuInitializer : MonoBehaviour
{
    [SerializeField] private SavesButtonsList _saves;
    [SerializeField] private CityAnimatorParameters _cityParams;

    private void Start()
    {
        if (SessionManager.Instance.CurrentSession != null)
        {
            Button currentSaveBtn = 
                _saves.SaveButtonsList
                .Find(e => e.GetSessionData() == SessionManager.Instance.CurrentSession)
                ?.GetComponentInChildren<Button>();

            if (currentSaveBtn != null)
            {
                currentSaveBtn.onClick.Invoke();
            }
            else
            {
                SessionManager.Instance.CurrentSession = null;
            }

            _cityParams.BreakIntro();
        }
    }
}