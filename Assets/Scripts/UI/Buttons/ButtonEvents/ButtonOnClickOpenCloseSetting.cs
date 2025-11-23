using UnityEngine;
using UnityEngine.UI;

public class ButtonOnClickOpenCloseSetting : MonoBehaviour
{
    public static GameObject SettingInstance;
    public static Button LastButtonOpenedSetting;

    public GameObject SettingPrefab;

    public void OpenSetting()
    {
        if (GameObjectUtility.TryGetComponentInParentRecursive(transform, out Canvas canvas))
        {
            if (SettingInstance == null)
            {
                SettingInstance = Instantiate(SettingPrefab);
            }
            SettingInstance.transform.SetParent(canvas.transform, false);
            SettingInstance.gameObject.SetActive(true);

            GameObjectUtility.TryGetComponentInSelfOrChild(gameObject, out LastButtonOpenedSetting);
        }
        else
        {
            throw new UnityException("Canvas component not found in any parent of " + transform.name);
        }
    }

    public void CloseSetting()
    {
        if (CurrentDeviceTracker.GetGamepadIsConnected()) LastButtonOpenedSetting?.Select();
        LastButtonOpenedSetting = null;

        SettingInstance.gameObject.SetActive(false);
        SettingInstance.transform.SetParent(null, false);
        DontDestroyOnLoad(SettingPrefab.gameObject);
    }
}
