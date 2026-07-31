using System;
using System.Collections;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(-1)]
public class UIManager : MonoBehaviour
{
    const string GLITCH_EFFECT_RENDER_FEATURE_NAME = "GlitchEffect";
    const string GLITCH_EFFECT_MATERIAL_INTENCITY_PROP_NAME = "_Intencity";
    const float GLITCH_EFFECT_CHANGE_SPEED = 25f;

    public enum LiveTimeLeftTypes
    {
        DEFAULT,
        SWORD_PLAYER
    }

    public bool ShowFPS
    {
        get => _showFPS;
        set
        {
            if (value == _showFPS) return;
            _showFPS = value;
            if (_showFPS)
            {
                FPSCountScreenOverlay.Show();
            }
            else
            {
                FPSCountScreenOverlay.Hide();
            }
        }
    }

    public class ScreenOverlayInstance : MonoBehaviour
    {
        public int OverlayOrder;
    }

    [Serializable]
    public class ScreenOverlay
    {
        public int OverlayOrder = 0;

        [SerializeField]
        protected GameObject _screenOverlayPrefab;

        protected ScreenOverlayInstance _currentScreenOverlay;
        private Transform _screenOverlayContainer;

        public void SetShown(bool value)
        {
            if (value)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        public virtual void Show()
        {
            if (_currentScreenOverlay != null) return;

            GameObject currentScreenOverlayGO = Instantiate(_screenOverlayPrefab, Instance._screenOverlayContainer.transform);
            currentScreenOverlayGO.transform.SetSiblingIndex(OverlayOrder);

            _currentScreenOverlay = currentScreenOverlayGO.AddComponent<ScreenOverlayInstance>();
            _currentScreenOverlay.OverlayOrder = OverlayOrder;

            if (_currentScreenOverlay.TryGetComponent(out AnimatedImage animatedImageComponent))
            {
                animatedImageComponent.AnimationFinished += AnimatedImage_OnAnimationFinished;
            }

            Instance.UpdateScreenOverlaysOrder();
        }


        public virtual void Hide()
        {
            if (_currentScreenOverlay == null) return;

            Destroy(_currentScreenOverlay.gameObject);
        }

        public ScreenOverlayInstance GetCurrentScreenOverlay()
        {
            return _currentScreenOverlay;
        }

        public bool IsShown()
        {
            return _currentScreenOverlay != null;
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
        const string SHADER_FILL_AMOUNT_PROP_NAME = "_FillAmount";

        private float _fillAmount = 0f;

        public float FillAmount
        {
            get => _fillAmount;
            set
            {
                if (_fillAmount == value) return;
                _currentScreenOverlay.GetComponentInChildren<Image>().material.SetFloat(SHADER_FILL_AMOUNT_PROP_NAME, value);
                _fillAmount = value;
            }
        }

        public override void Show()
        {
            base.Show();
            _currentScreenOverlay?.GetComponentInChildren<Image>().material.SetFloat(SHADER_FILL_AMOUNT_PROP_NAME, _fillAmount);
        }
    }

    [Serializable]
    public class TextableScreenOverlay : ScreenOverlay
    {
        private TextMeshProUGUI _currentTextContainer;

        public string TextContent
        {
            get => _currentTextContainer?.text;
            set
            {
                if (_currentScreenOverlay != null) _currentTextContainer.text = value;
            }
        }

        public override void Show()
        {
            base.Show();
            _currentTextContainer = _currentScreenOverlay.GetComponentInChildren<TextMeshProUGUI>();
        }

        public void Show(string text)
        {
            Show();
            TextContent = text;
        }

        public override void Hide()
        {
            base.Hide();
            _currentTextContainer = null;
        }
    }

    [Serializable]
    public class GameOverUIScreenOverlay : ScreenOverlay
    {
        private GameOverUIManager _currentGameOverUI = null;

        public GameOverUIManager GetGameOverUI()
        {
            return _currentGameOverUI;
        }

        public void Show(GameOverUIManager.GameOverReasons reason)
        {
            Show();
            GetGameOverUI().GameOverReason = reason;
        }

        public override void Show()
        {
            base.Show();
            _currentGameOverUI = _currentScreenOverlay.GetComponent<GameOverUIManager>();
        }

        public override void Hide()
        {
            base.Hide();
            _currentGameOverUI = null;
        }
    }

    [Serializable]
    public class GameplayUIScreenOverlay : ScreenOverlay
    {
        private GameplayUIManager _currentGameplayUI = null;

        public GameplayUIManager GetGameplayUI()
        {
            return _currentGameplayUI;
        }

        public override void Show()
        {
            base.Show();
            _currentGameplayUI = _currentScreenOverlay.GetComponent<GameplayUIManager>();
        }

        public override void Hide()
        {
            base.Hide();
            _currentGameplayUI = null;
        }
    }

    [Serializable]
    public class ModificatorsUIScreenOverlay : ScreenOverlay
    {
        private ModificatorsUI _currentModificartorsUI = null;

        public ModificatorsUI GetModificatorsUI()
        {
            return _currentModificartorsUI;
        }

        public override void Show()
        {
            base.Show();

            _currentModificartorsUI = _currentScreenOverlay.GetComponent<ModificatorsUI>();
            if (ModificatorsManager.Instance != null && !ModificatorsManager.Instance.IsDestroyed())
            {
                foreach (AbstractModificator modificator in ModificatorsManager.Instance.CurrentModificators)
                {
                    if (modificator.CurrentIcon == null)
                    {
                        _currentModificartorsUI.AddModificatorIcon(modificator, true);
                    }
                }
            }
        }

        public override void Hide()
        {
            base.Hide();
            _currentModificartorsUI = null;
        }
    }

    [Serializable]
    public class DifficultyCurseChoiseUIScreenOverlay : ScreenOverlay
    {
        private DifficultyCurseChoiseUI _difficultyCurseChoiseUI = null;

        public DifficultyCurseChoiseUI DifficultyCurseChoiseUI
        {
            get => _difficultyCurseChoiseUI;
        }

        public void DebugShow()
        {
            Show();
            DifficultyCurseChoiseUI.InitDebugCurseOptions();
        }

        public void Show(float curseMinPrice, float curseMaxPrice, int pickAmount, int optionsAmount)
        {
            Show();
            DifficultyCurseChoiseUI.InitCurseOptions(curseMinPrice, curseMaxPrice, pickAmount, optionsAmount);
        }

        public override void Show()
        {
            base.Show();
            _difficultyCurseChoiseUI = _currentScreenOverlay.GetComponent<DifficultyCurseChoiseUI>();
        }

        public override void Hide()
        {
            base.Hide();
            _difficultyCurseChoiseUI = null;
        }
    }

    [Serializable]
    public class CharacterUnlockedMessageScreenOverlay : ScreenOverlay
    {
        public CharacterUnlockedMessageUI _message;

        public void Show(PlayerCharacterInfo unlockedCharacter)
        {
            Show();
            _message.SetUnlockedCharacterInfo(unlockedCharacter);
        }

        public override void Show()
        {
            base.Show();
            _message = _currentScreenOverlay.GetComponent<CharacterUnlockedMessageUI>();
        }
    }

    [Serializable]
    public class BlindScreenOverlay : ScreenOverlay
    {
        private BlindnessOverlay _blindScreen = null;

        public BlindnessOverlay GetBlindScreen()
        {
            return _blindScreen;
        }

        public void Show(float constantDuration, float fadeOutDuration)
        {
            Show();
            _blindScreen.ConstantDuration = constantDuration;
            _blindScreen.FadeOutDuration = fadeOutDuration;
        }

        public override void Show()
        {
            base.Show();
            _blindScreen = _currentScreenOverlay.GetComponent<BlindnessOverlay>();
            _blindScreen.Restart();
        }
    }

    public ScreenOverlay InputBindingScreenOverlay;
    public ScreenOverlay SceneStartScreenOverlay;
    public ScreenOverlay SceneEndScreenOverlay;
    public FillableScreenOverlay DamagedScreenOverlay;
    public GameplayUIScreenOverlay GameplayScreenOverlay;
    public GameOverUIScreenOverlay GameOverScreenOverlay;
    public ModificatorsUIScreenOverlay ModificatorsScreenOverlay;
    public ModificatorsUIScreenOverlay ArtifactModificatorsScreenOverlay;
    public ScreenOverlay DifficultyScreenOverlay;
    public DifficultyCurseChoiseUIScreenOverlay DifficultyCurseChoiseScreenOverlay;
    public TextableScreenOverlay LivingTimeLeftScreenOverlay;
    public TextableScreenOverlay SwordPlayerLiveTimeLeftScreenOverlay;
    public FillableScreenOverlay SlowmoOverlay;
    public ScreenOverlay FPSCountScreenOverlay;
    public ScreenOverlay SettingOverlay;
    public CharacterUnlockedMessageScreenOverlay UnlockedCharacterMessageOverlay;
    public BlindScreenOverlay BlindnessOverlay;
    public ScreenOverlay NavPointersScreenOverlay;

    [Header("Render")]
    [SerializeField] private Renderer2DData _renderData;
    public float TargetGlitchIntencity = 0f;

    public static UIManager Instance;

    private GameObject _screenOverlayContainer;
    private AsyncOperation _sceneLoadingProcess; //used only at LoadSceneWithEffect and LoadSceneWithEffect_OnScreenOverlayAnimationFinished functions
    private bool _showFPS = false;
    private ScreenOverlay[] _allOverlays;
    private FullScreenPassRendererFeature _glitchRenderFeature;
    private float _currentGlitchEffectIntencity = 0f;

    public bool IsLoadingScene()
    {
        return _sceneLoadingProcess != null;
    }

    public static bool GamePaused()
    {
        return
            ((!Instance.GameplayScreenOverlay.GetGameplayUI()?.Pause?.IsDestroyed()) ?? false) &&
            (
                Instance.GameplayScreenOverlay.GetGameplayUI().Pause.gameObject.activeSelf ||
                Instance.GameOverScreenOverlay.GetCurrentScreenOverlay() != null
            );
    }

    public TextableScreenOverlay GetLiveTimeLeftScreenOverlayByType(LiveTimeLeftTypes type)
    {
        switch (type)
        {
            case LiveTimeLeftTypes.DEFAULT:
                return LivingTimeLeftScreenOverlay;
            case LiveTimeLeftTypes.SWORD_PLAYER:
                return SwordPlayerLiveTimeLeftScreenOverlay;
        }
        throw new UnityException("not found value for type: " + type.ToString());
    }

    private void Awake()
    {
        DontDestroyOnLoad(this);

        _glitchRenderFeature = _renderData.rendererFeatures.Find(e => e.name == GLITCH_EFFECT_RENDER_FEATURE_NAME) as FullScreenPassRendererFeature;

        _allOverlays = new ScreenOverlay[]
        {
            InputBindingScreenOverlay,
            SceneStartScreenOverlay,
            SceneEndScreenOverlay,
            DamagedScreenOverlay,
            GameplayScreenOverlay,
            GameOverScreenOverlay,
            ModificatorsScreenOverlay,
            ArtifactModificatorsScreenOverlay,
            DifficultyScreenOverlay,
            DifficultyCurseChoiseScreenOverlay,
            LivingTimeLeftScreenOverlay,
            SwordPlayerLiveTimeLeftScreenOverlay,
            SlowmoOverlay,
            FPSCountScreenOverlay,
            SettingOverlay,
            UnlockedCharacterMessageOverlay,
            BlindnessOverlay,
            NavPointersScreenOverlay
        };

        if (Instance != null && !Instance.IsDestroyed()) throw new UnityException("Limit of 1 Instance of UIManager objects");
        Instance = this;

        _screenOverlayContainer = GameObject.FindGameObjectWithTag("ScreenOverlayContainer");

        SceneManager.activeSceneChanged += SceneManager_OnActiveSceneChanged;
    }

    private void UpdateScreenOverlaysOrder()
    {

        ScreenOverlayInstance[] sortedOverlays = _screenOverlayContainer.GetComponentsInChildren<ScreenOverlayInstance>();

        sortedOverlays = sortedOverlays.OrderBy((ScreenOverlayInstance overlayInstance) => overlayInstance.OverlayOrder).ToArray();

        for (int i = 0; i < sortedOverlays.Length; i++)
        {
            sortedOverlays[i].transform.SetSiblingIndex(i);
        }
    }

    private void SceneManager_OnActiveSceneChanged(Scene arg0, Scene arg1)
    {
        _screenOverlayContainer = GameObject.FindGameObjectWithTag("ScreenOverlayContainer");

        foreach (ScreenOverlay overlay in _allOverlays)
        {
            overlay.Hide();
        }

        SceneStartScreenOverlay.Show();

        if (ShowFPS)
        {
            FPSCountScreenOverlay.Show();
        }
    }

    private void Update()
    {
        _currentGlitchEffectIntencity = Mathf.Lerp(_currentGlitchEffectIntencity, TargetGlitchIntencity, Time.deltaTime * GLITCH_EFFECT_CHANGE_SPEED);
        _glitchRenderFeature.passMaterial.SetFloat(GLITCH_EFFECT_MATERIAL_INTENCITY_PROP_NAME, _currentGlitchEffectIntencity);
    }

    private void OnDestroy()
    {
        Instance = null;
        SceneManager.activeSceneChanged -= SceneManager_OnActiveSceneChanged;
    }

    public void LoadSceneWithEffect(string sceneName)
    {
        if (_sceneLoadingProcess != null) return;

        if (MusicManager.Instance != null) MusicManager.Instance.TargetMusicVolume = 0f;
        if (TimeManager.Instance != null) TimeManager.Instance.Paused = true;

        Instance.SceneEndScreenOverlay.Show();

        _sceneLoadingProcess = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        _sceneLoadingProcess.allowSceneActivation = false;
        StartCoroutine(LoadSceneAfterAwaitMusicFinish());
    }

    private IEnumerator LoadSceneAfterAwaitMusicFinish()
    {
        if (MusicManager.Instance != null)
        {
            yield return new WaitForSecondsRealtime(MusicManager.VOLUME_CHANGE_DURATION);
        }

        if (TimeManager.Instance != null) TimeManager.Instance.Paused = false;

        _sceneLoadingProcess.allowSceneActivation = true;
        _sceneLoadingProcess = null;
    }
}
