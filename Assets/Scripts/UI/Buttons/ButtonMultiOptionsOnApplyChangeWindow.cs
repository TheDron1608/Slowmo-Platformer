using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

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
        
        Vector2 currentResolution = _aspectRatioResolutions[_aspectRatioButtonOptions.CurrentOptionIndex].Resolutions[_resolutionButtonOptions.CurrentOptionIndex];
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
    [SerializeField]
    private ButtonOptions _aspectRatioButtonOptions;



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
        UpdateAspectRaioOptions();
        UpdateResolutioOptions();
    }

    private void Awake()
    {
        UpdateAspectRaioOptions();
        UpdateResolutioOptions();
        UpdateWindowModeOptions();

        _aspectRatioButtonOptions.ButtonOptions_OnOptionChanged += AspectRaioOptionsButton_OnOptionChanged;
    }

    private void AspectRaioOptionsButton_OnOptionChanged(object sender, int e)
    {
        UpdateResolutioOptions();
    }

    private void UpdateAspectRaioOptions()
    {
        _aspectRatioButtonOptions.Options.Clear();

        bool optionIndexUnsat = true;
        for (int i = 0; i < _aspectRatioResolutions.Count; i++)
        {
            if (
                _aspectRatioResolutions[i].Resolutions[0].x <= Screen.currentResolution.width &&
                _aspectRatioResolutions[i].Resolutions[0].y <= Screen.currentResolution.height
                )
            {
                if (Math.Abs((float)Screen.currentResolution.width / Screen.currentResolution.height - _aspectRatioResolutions[i].AspectRaio.x / _aspectRatioResolutions[i].AspectRaio.y) < 0.05f)
                {
                    _aspectRatioButtonOptions.Options.Add(new ButtonOptions.ButtonOptionsOption(
                        $"{_aspectRatioResolutions[i].GetAspectRatioSting()}\n({_defaultOptionText})"
                        ));
                    _aspectRatioButtonOptions.SetOptionIndex(i);
                    optionIndexUnsat = false;
                }
                else
                {
                    _aspectRatioButtonOptions.Options.Add(new ButtonOptions.ButtonOptionsOption(
                        _aspectRatioResolutions[i].GetAspectRatioSting()
                        ));
                }
            }

        }

        if (optionIndexUnsat)
        {
            _aspectRatioButtonOptions.SetOptionIndex(0);
        }
    }

    private void UpdateResolutioOptions()
    {
        _resolutionButtonOptions.Options.Clear();

        bool optionIndexUnsat = true;
        for (int i = 0; i < _aspectRatioResolutions[_aspectRatioButtonOptions.CurrentOptionIndex].Resolutions.Count; i++)
        {
            //check if real resolution equals to current option, then add (Default) prefix
            if (
                _aspectRatioResolutions[_aspectRatioButtonOptions.CurrentOptionIndex].Resolutions[i].x == Screen.currentResolution.width &&
                _aspectRatioResolutions[_aspectRatioButtonOptions.CurrentOptionIndex].Resolutions[i].y == Screen.currentResolution.height
                )
            {
                _resolutionButtonOptions.Options.Add(new ButtonOptions.ButtonOptionsOption(
                    $"{_aspectRatioResolutions[_aspectRatioButtonOptions.CurrentOptionIndex].GetResolutionString(i)}\n({_defaultOptionText})"
                    ));
                _resolutionButtonOptions.SetOptionIndex(i);
                optionIndexUnsat = false;
            }
            //checks if real resolution is less than current option, else doesn't add this option
            else if (
                _aspectRatioResolutions[_aspectRatioButtonOptions.CurrentOptionIndex].Resolutions[i].x < Screen.currentResolution.width &&
                _aspectRatioResolutions[_aspectRatioButtonOptions.CurrentOptionIndex].Resolutions[i].y < Screen.currentResolution.height
                )
            {
                _resolutionButtonOptions.Options.Add(new ButtonOptions.ButtonOptionsOption(
                    _aspectRatioResolutions[_aspectRatioButtonOptions.CurrentOptionIndex].GetResolutionString(i)
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
