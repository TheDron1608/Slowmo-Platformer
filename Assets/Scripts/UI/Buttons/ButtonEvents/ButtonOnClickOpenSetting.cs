using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonOnClickOpenSetting : MonoBehaviour
{
    public static GameObject SettingInstance;

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
        }
        else
        {
            throw new UnityException("Canvas component not found in any parent of " + transform.name);
        }
    }

    public void CloseSetting()
    {
        SettingInstance.gameObject.SetActive(false);
        SettingInstance.transform.SetParent(null, false);
        DontDestroyOnLoad(SettingPrefab.gameObject);
    }
}
