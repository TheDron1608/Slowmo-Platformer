using UnityEngine;
using UnityEngine.UI;

public class ButtonOnClickOpenCloseSetting : MonoBehaviour
{
    public static Button LastOpenSettingButton = null;

    public GameObject SettingPrefab;

    public void OpenSetting()
    {
        UIManager.Instance.SettingOverlay.Show();
        GameObjectUtility.TryGetComponentInSelfOrChild(gameObject, out LastOpenSettingButton);
    }

    public void CloseSetting()
    {
        UIManager.Instance.SettingOverlay.Hide();

        LastOpenSettingButton?.Select();
        LastOpenSettingButton = null;
    }
}
