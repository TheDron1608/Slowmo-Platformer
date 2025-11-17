using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

[DefaultExecutionOrder(-1)]
public class UIManager : MonoBehaviour
{
    [Serializable]
    public class ScreenOverlay
    {
        [SerializeField]
        protected GameObject _screenOverlayPrefab;

        protected GameObject _currentScreenOverlay;

        public virtual void Show()
        {
            if (_currentScreenOverlay != null) return;

            _currentScreenOverlay = Instantiate(_screenOverlayPrefab, Instance._screenOverlayContainer.transform);

            if (_currentScreenOverlay.TryGetComponent(out AnimatedImage animatedImageComponent)) {
                animatedImageComponent.AnimationFinished += AnimatedImage_OnAnimationFinished;
            }
        }


        public virtual void Hide()
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

    [Serializable]
    public class FillableScreenOverlay : ScreenOverlay
    {
        const string DAMAGED_SHADER_FILL_AMOUNT_PROP_NAME = "_FillAmount";

        private float _fillAmount = 0f;

        public float FillAmount
        {
            get => _fillAmount;
            set
            {
                if (_fillAmount == value) return;
                _currentScreenOverlay.GetComponentInChildren<Image>().material.SetFloat(DAMAGED_SHADER_FILL_AMOUNT_PROP_NAME, value);
                _fillAmount = value;
            }
        }

        public override void Show()
        {
            base.Show();
            _currentScreenOverlay?.GetComponentInChildren<Image>().material.SetFloat(DAMAGED_SHADER_FILL_AMOUNT_PROP_NAME, _fillAmount);
        }
    }

    public ScreenOverlay InputBindingScreenOverlay;
    public ScreenOverlay SceneStartScreenOverlay;
    public ScreenOverlay SceneEndScreenOverlay;
    public FillableScreenOverlay DamagedScreenOverlay;

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
