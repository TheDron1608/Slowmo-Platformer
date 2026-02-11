using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Localization.Components;

public class ButtonOnClickSetShowFPS : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _buttonText;
    [SerializeField] private LocalizeStringEvent _showFPSLocalization;
    [SerializeField] private LocalizeStringEvent _hideFPSLocalization;

    public void ToggleShowFPS()
    {
        bool newValue = !UIManager.Instance.ShowFPS;

        UIManager.Instance.ShowFPS = newValue;

        JSONFileManager.WindowOptionsSaveData newSaveData =
            JsonUtility.FromJson<JSONFileManager.WindowOptionsSaveData>(JSONFileManager.ReadJSON(JSONFileManager.Instance.WindowFileName));

        newSaveData.ShowFPS = newValue;

        JSONFileManager.SaveJSON(JSONFileManager.Instance.WindowFileName, JsonUtility.ToJson(newSaveData));

        _showFPSLocalization.enabled = !newValue;
        _hideFPSLocalization.enabled = newValue;
        _buttonText.text = (newValue ? _hideFPSLocalization : _showFPSLocalization).StringReference.GetLocalizedString();
    }

    private void Awake()
    {
        bool currentValue = UIManager.Instance.ShowFPS;

        _showFPSLocalization.enabled = !currentValue;
        _hideFPSLocalization.enabled = currentValue;
        _buttonText.text = (currentValue ? _hideFPSLocalization : _showFPSLocalization).StringReference.GetLocalizedString();
    }
}
