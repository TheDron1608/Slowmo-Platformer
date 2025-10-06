using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1)]
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

            if (_currentScreenOverlay.TryGetComponent<AnimatedImage>(out AnimatedImage animatedImageComponent)) {
                animatedImageComponent.AnimationFinished += AnimatedImage_OnAnimationFinished;
            }
        }


        public void Hide()
        {
            if (_currentScreenOverlay == null) return;

            Destroy(_currentScreenOverlay);
        }

        private void AnimatedImage_OnAnimationFinished(object sender, EventArgs e)
        {
            ScreenOverlayAnimationFinished?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler ScreenOverlayAnimationFinished;
    }



    public UIManagerSceenOverlay InputBindingScreenOverlay;
    public UIManagerSceenOverlay SceneStartScreenOverlay;
    public UIManagerSceenOverlay SceneEndScreenOverlay;

    public static UIManager Instance;

    private GameObject _screenOverlayContainer;
    private AsyncOperation _sceneLoadingProcess; //used only at LoadSceneWithEffect and LoadSceneWithEffect_OnScreenOverlayAnimationFinished functions

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
        SceneEndScreenOverlay.Hide();
        SceneStartScreenOverlay.Show();
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public void LoadSceneWithEffect(string sceneName)
    {
        UIManager.Instance.SceneEndScreenOverlay.Show();

        UIManager.Instance.SceneEndScreenOverlay.ScreenOverlayAnimationFinished += LoadSceneWithEffect_OnScreenOverlayAnimationFinished;

        _sceneLoadingProcess = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        _sceneLoadingProcess.allowSceneActivation = false;
    }

    private void LoadSceneWithEffect_OnScreenOverlayAnimationFinished(object sender, EventArgs e)
    {
        _sceneLoadingProcess.allowSceneActivation = true;
    }
}
