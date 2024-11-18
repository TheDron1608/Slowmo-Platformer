using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Serializable]
    public class UIManagerSceenOverlay
    {
        [SerializeField]
        private GameObject _screenOverlayPrefab;

        private GameObject _currentScreenOverlay;

        public void Show()
        {
            if (_currentScreenOverlay != null) return;

            _currentScreenOverlay = Instantiate(_screenOverlayPrefab, Instance._screenOverlayContainer.transform);
        }

        public void Hide()
        {
            if (_currentScreenOverlay == null) return;

            Destroy(_currentScreenOverlay);
        }
    }



    public UIManagerSceenOverlay InputBindingScreenOverlay;

    public static UIManager Instance;

    private GameObject _screenOverlayContainer;

    private void Awake()
    {
        DontDestroyOnLoad(this);

        if (Instance != null) throw new UnityException("Limit of 1 Instance of UIManager objects");
        Instance = this;

        SceneManager.activeSceneChanged += SceneManager_OnActiveSceneChanged;
    }

    private void SceneManager_OnActiveSceneChanged(Scene arg0, Scene arg1)
    {
        _screenOverlayContainer = GameObject.FindGameObjectWithTag("ScreenOverlayContainer");
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}
