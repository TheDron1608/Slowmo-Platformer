using System;
using System.Collections.Generic;
using UnityEngine;

public class ButtonMultiOptionsOnApplyChangeWindow : MonoBehaviour
{
    public class WindowOptionsSaveData
    {
        public string WindowMode;
        public float resolutionX, resolutionY;
    }

    enum WindowMode : int
    {
        WINDOWED = 0,
        BORDERLESS = 1,
        FULLSCREEN = 2
    }


    public void ApplyChangeWindow()
    {
        WindowOptionsSaveData newSaveData = new WindowOptionsSaveData();
        
        Vector2 currentResolution = _aspectRatioResolutions[_currentAspectRatioIndex].Resolutions[_resolutionButtonOptions.CurrentOptionIndex];
        newSaveData.resolutionX = currentResolution.x;
        newSaveData.resolutionY = currentResolution.y;

        switch (_windowModeButtonOptions.CurrentOptionIndex)
        {
            case (int)WindowMode.WINDOWED:
                newSaveData.WindowMode = "Windowed";
                Screen.SetResolution((int)currentResolution.x, (int)currentResolution.y, false);
                break;
            case (int)WindowMode.BORDERLESS:
#if !UNITY_STANDALONE_LINUX
                newSaveData.WindowMode = "Borderless";
                Screen.SetResolution((int)currentResolution.x, (int)currentResolution.y, FullScreenMode.MaximizedWindow);
#endif
                break;
            case (int)WindowMode.FULLSCREEN:
                newSaveData.WindowMode = "Fullscreen";
                Screen.SetResolution((int)currentResolution.x, (int)currentResolution.y, FullScreenMode.FullScreenWindow);
                break;
        }

        JSONFileManager.SaveJSON(JSONFileManager.Instance.WindowFileName, JsonUtility.ToJson(newSaveData));
    }



    [SerializeField]
    private List<WindowResolutions> _aspectRatioResolutions = new List<WindowResolutions>();

    [SerializeField]
    private ButtonOptions _windowModeButtonOptions;
    [SerializeField]
    private ButtonOptions _resolutionButtonOptions;

    private int _currentAspectRatioIndex;


    //localization updater
    private string _windowedOptionText;
    public void SetWindedOptionText(string value)
    {
        _windowedOptionText = value;
        UpdateWindowModeOptions();
    }
    //localization updater
    private string _borderLessOptionText;
    public void SetBorderlessOptionText(string value)
    {
        _borderLessOptionText = value;
        UpdateWindowModeOptions();
    }
    //localization updater
    private string _fullsreenOptionText;
    public void SetFullscreenOptionText(string value)
    {
        _fullsreenOptionText = value;
        UpdateWindowModeOptions();
    }
    //localization updater
    private string _defaultOptionText;
    public void SetDefaultOptionText(string value)
    {
        _defaultOptionText = value;
        UpdateResolutioOptions();
    }

    private void Awake()
    {
        InitializeCurrentAspectIndex();
        UpdateResolutioOptions();
        UpdateWindowModeOptions();
    }

    private void AspectRaioOptionsButton_OnOptionChanged(object sender, int e)
    {
        UpdateResolutioOptions();
    }

    private void InitializeCurrentAspectIndex()
    {
        for (int i = 0; i < _aspectRatioResolutions.Count; i++)
        {
            if ((float)Screen.currentResolution.width / Screen.currentResolution.height > _aspectRatioResolutions[i].AspectRaio.x / _aspectRatioResolutions[i].AspectRaio.y - 0.05f)
            {
                _currentAspectRatioIndex = i;
            }
        }
    }

    private void UpdateResolutioOptions()
    {
        _resolutionButtonOptions.Options.Clear();

        bool optionIndexUnsat = true;
        for (int i = 0; i < _aspectRatioResolutions[_currentAspectRatioIndex].Resolutions.Count; i++)
        {
            //check if real resolution equals to current option, then add (Default) prefix
            if (
                Math.Abs(_aspectRatioResolutions[_currentAspectRatioIndex].Resolutions[i].x - Screen.currentResolution.width) < 0.05f &&
                Math.Abs(_aspectRatioResolutions[_currentAspectRatioIndex].Resolutions[i].y - Screen.currentResolution.height) < 0.05f
                )
            {
                _resolutionButtonOptions.Options.Add(new ButtonOptions.ButtonOptionsOption(
                    $"{_aspectRatioResolutions[_currentAspectRatioIndex].GetResolutionString(i)}\n({_defaultOptionText})"
                    ));
                _resolutionButtonOptions.SetOptionIndex(i);
                optionIndexUnsat = false;
            }
            //checks if real resolution is less than current option, else doesn't add this option
            else if (
                _aspectRatioResolutions[_currentAspectRatioIndex].Resolutions[i].x < Screen.currentResolution.width &&
                _aspectRatioResolutions[_currentAspectRatioIndex].Resolutions[i].y < Screen.currentResolution.height
                )
            {
                _resolutionButtonOptions.Options.Add(new ButtonOptions.ButtonOptionsOption(
                    _aspectRatioResolutions[_currentAspectRatioIndex].GetResolutionString(i)
                    ));
            }
        }

        if (optionIndexUnsat )
        {
            _resolutionButtonOptions.SetOptionIndex(0);
        }
    }

    private void UpdateWindowModeOptions()
    {
        _windowModeButtonOptions.Options.Clear();
        _windowModeButtonOptions.Options.Add(new ButtonOptions.ButtonOptionsOption(_windowedOptionText));
        _windowModeButtonOptions.Options.Add(new ButtonOptions.ButtonOptionsOption(_borderLessOptionText));
        _windowModeButtonOptions.Options.Add(new ButtonOptions.ButtonOptionsOption(_fullsreenOptionText));

        _windowModeButtonOptions.SetOptionIndex(_windowModeButtonOptions.CurrentOptionIndex);
    }
}
