using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonOnClickChangeFloor : MonoBehaviour
{
    [SerializeField]
    private FloorInfo _floor;

    [SerializeField]
    private TextMeshProUGUI _textContainer;

    [SerializeField]
    private CityAnimatorParameters _cityAnimator;

    [SerializeField]
    private GameObject _levelButtonsContainer;

    [SerializeField]
    private ButtonOnInitUpdateSelfLevelInfo _levelButtonSample;
    
    private ButtonEnabledOnSessionProgress _levelButtonSampleEnableConditionComponent;

    private void Start()
    {
        SessionManager.Instance.CurrentSessionChanged += SessionManager_OnCurrentSessionChanged;

        if (!_levelButtonSample.TryGetComponent<ButtonEnabledOnSessionProgress>(out _levelButtonSampleEnableConditionComponent))
        {
            throw new UnityException("ButtonEnabledOnSessionProgress component not found");
        }
    }

    private void SessionManager_OnCurrentSessionChanged(object sender, EventArgs e)
    {
        if (SessionManager.Instance.CurrentSession != null && SessionManager.Instance.CurrentSession.FloorProgress >= _floor.Floor)
        {
            _textContainer.text = _floor.Name();
        }
    }

    //called when clicked
    public void ChangeFloor()
    {
        UpdateCityAnimator();
        UpdateLevelButtonsContainer();
    }

    private void UpdateCityAnimator()
    {
        _cityAnimator.CurrentLevel = _floor.Floor;
    }

    private void UpdateLevelButtonsContainer()
    {
        foreach (Transform child in _levelButtonsContainer.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < _floor.Levels.Count; i++)
        {
            _levelButtonSampleEnableConditionComponent.RequiredFloorProgress = _floor.Floor;
            _levelButtonSampleEnableConditionComponent.RequiredLevelProgress = _floor.Levels[i].Level;
            ButtonOnInitUpdateSelfLevelInfo newLevelButton = Instantiate(_levelButtonSample, _levelButtonsContainer.transform);
            
            newLevelButton.LevelInfo = _floor.Levels[i];
        }
    }
}
