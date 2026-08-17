using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(2)]
public class ParallaxManager : MonoBehaviour
{
    const string PARALLAX_CONTAINER_NAME = "ParallaxContainer";
    const string SKY_PLANE_NAME = "SkyPlane";

    public static ParallaxManager Instance = null;
    
    public GameObject ParallaxInstance;

    private Parallax[] _currentParallaxes;
    private Transform _currentParallaxContainer = null;
    private GameObject _currentMultiParallax;
    private Image _skyPlane; 
    private Material _skyMaterial = null;

    public Material SkyMaterial
    {
        get => _skyMaterial;
        set
        {
            _skyMaterial = value;
            UpdateSky();
        }
    }

    public void SetParallaxMaterialDependedOnDifficulty(DifficultyManager.DifficultyStage difficulty)
    {
        SetParallaxMaterial(difficulty.BackgroundEnviromentMaterial);
    }

    public void SetParallaxMaterial(Material value)
    {
        if (_currentParallaxes == null) return;
        foreach (var parallax in _currentParallaxes)
        {
            parallax.ParallaxMaterial = value;
        }
    }

    public void UpdateParallax()
    {
        if (_currentParallaxContainer != null && !_currentParallaxContainer.IsDestroyed())
        {
            if (_currentMultiParallax != null && !_currentMultiParallax.IsDestroyed())
            {
                Destroy(_currentMultiParallax);
            }

            if (ParallaxInstance != null)
            {
                _currentMultiParallax = Instantiate(ParallaxInstance, _currentParallaxContainer);
                _currentParallaxes = _currentParallaxContainer.GetComponentsInChildren<Parallax>();
            }
        }
    }

    public void UpdateSky()
    {
        if (_skyPlane != null && !_skyPlane.IsDestroyed())
        {
            _skyPlane.material = SkyMaterial;
        }
    }

    private void Awake()
    {
        if (Instance != null) throw new UnityException("Limit of 1 ParallaxManager per scene");

        DontDestroyOnLoad(this);
        Instance = this;

        SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
    }

    private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
    {
        _currentParallaxContainer = GameObject.Find(PARALLAX_CONTAINER_NAME)?.transform;
        _skyPlane = GameObject.Find(SKY_PLANE_NAME)?.GetComponent<Image>();

        UpdateParallax();
        UpdateSky();
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;

        Instance = null;
    }
}