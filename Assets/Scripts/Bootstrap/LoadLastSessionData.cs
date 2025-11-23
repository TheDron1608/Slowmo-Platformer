using UnityEngine;
using UnityEngine.InputSystem;

public class LoadLastSessionData : MonoBehaviour
{
    private void Start()
    {
        LoadCurrentWindowOptions();
        LoadCurrentKeyBinding();
        LoadCurrentSoundVolume();
    }

    private void LoadCurrentKeyBinding()
    {
        string keybindData = JSONFileManager.ReadJSON(JSONFileManager.Instance.ControlsFileName);
        if (keybindData == null || keybindData == "") return;

        InputSystem.actions.LoadBindingOverridesFromJson(keybindData);
    }

    private void LoadCurrentWindowOptions()
    {
        string windowDataStr = JSONFileManager.ReadJSON(JSONFileManager.Instance.WindowFileName);
        if (windowDataStr == null || windowDataStr == "") return;

        JSONFileManager.WindowOptionsSaveData windowDataObj = JsonUtility.FromJson<JSONFileManager.WindowOptionsSaveData>(windowDataStr);

        windowDataObj.ApplyOptions();
    }

    private void LoadCurrentSoundVolume()
    {
        SoundManager.Instance.LoadSoundFromJSON();
    }
}
