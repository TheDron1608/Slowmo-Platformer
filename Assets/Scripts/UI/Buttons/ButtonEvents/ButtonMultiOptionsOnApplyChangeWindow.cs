using System;
using System.Linq;
using UnityEngine;

public class ButtonMultiOptionsOnApplyChangeWindow : MonoBehaviour
{
    enum WindowMode : int
    {
        WINDOWED = 0,
        BORDERLESS = 1,
        FULLSCREEN = 2
    }



    [SerializeField]
    private ButtonOptions _windowModeButtonOptions;
    [SerializeField]
    private ButtonOptions _resolutionButtonOptions;

    private int _currentAspectRatioIndex;



    //called when press a submit button
    public void ApplyChangeWindow()
    {

        JSONFileManager.WindowOptionsSaveData newSaveData =
            JsonUtility.FromJson<JSONFileManager.WindowOptionsSaveData>(JSONFileManager.ReadJSON(JSONFileManager.Instance.WindowFileName));

        switch (_windowModeButtonOptions.CurrentOptionIndex)
        {
            case (int)WindowMode.WINDOWED:
                newSaveData.WindowMode = "Windowed";
                break;
            case (int)WindowMode.BORDERLESS:
#if !UNITY_STANDALONE_LINUX
                newSaveData.WindowMode = "Borderless";
#endif
                break;
            case (int)WindowMode.FULLSCREEN:
                newSaveData.WindowMode = "Fullscreen";
                break;
        }

        if (newSaveData.WindowMode != "Borderless")
        {
            string currentResolutionOption = _resolutionButtonOptions.GetCurrentOption().Title;
            if (currentResolutionOption.IndexOf('(') != -1)
            {
                currentResolutionOption = currentResolutionOption.Substring(0, currentResolutionOption.IndexOf('(')); //converting 1920x1080 (default) into 1920x1080
            }
            newSaveData.resolutionX = Int32.Parse(currentResolutionOption.Substring(0, currentResolutionOption.IndexOf('x'))); //converting 1920x1080 into 1920
            newSaveData.resolutionY = Int32.Parse(currentResolutionOption.Substring(currentResolutionOption.IndexOf('x') + 1)); //converting 1920x1080 into 1080
        }

        JSONFileManager.SaveJSON(JSONFileManager.Instance.WindowFileName, JsonUtility.ToJson(newSaveData));
        newSaveData.ApplyOptions();
    }

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
        UpdateWindowModeOptions();
        UpdateResolutioOptions();
    }

    private void Start()
    {
        _windowModeButtonOptions.OnOptionChanged += WindowModeButtonOptions_OnOptionChanged;
    }

    private void WindowModeButtonOptions_OnOptionChanged(object sender, int e)
    {
        UpdateResolutioOptions();
    }

    private void UpdateResolutioOptions()
    {
        _resolutionButtonOptions.gameObject.SetActive(true);
        _resolutionButtonOptions.Options.Clear();

        if (_windowModeButtonOptions.CurrentOptionIndex == (int)WindowMode.BORDERLESS)
        {
            _resolutionButtonOptions.gameObject.SetActive(false);
            return;
        }

        bool isResolutionDefaultIndexSat = false;
        for (int i = 0; i < Screen.resolutions.Length; i++)
        {
            ButtonOptions.ButtonOptionsOption newButtonOption;

            newButtonOption = new ButtonOptions.ButtonOptionsOption(
                $"{Screen.resolutions[i].width}x{Screen.resolutions[i].height}"
            );

            //do not add option if it is a dupe of current last option
            if (_resolutionButtonOptions.Options.Count == 0 || _resolutionButtonOptions.Options.Last().Title != newButtonOption.Title)
            {
                _resolutionButtonOptions.Options.Add(newButtonOption);

                //check if real resolution equals to current option, then set as default option
                if (
                    Screen.resolutions[i].width == Screen.currentResolution.width &&
                    Screen.resolutions[i].height == Screen.currentResolution.height
                    )
                {
                    isResolutionDefaultIndexSat = true;
                    _resolutionButtonOptions.SetOptionIndex(_resolutionButtonOptions.Options.Count - 1);
                }
            }
        }

        //sets default option to max resolution if was not set before
        if (!isResolutionDefaultIndexSat)
        {
            _resolutionButtonOptions.SetOptionIndex(_resolutionButtonOptions.Options.Count - 1);
        }
    }

    private void UpdateWindowModeOptions()
    {
        _windowModeButtonOptions.Options.Clear();
        _windowModeButtonOptions.Options.Add(new ButtonOptions.ButtonOptionsOption(_windowedOptionText));
        _windowModeButtonOptions.Options.Add(new ButtonOptions.ButtonOptionsOption(_borderLessOptionText));
        _windowModeButtonOptions.Options.Add(new ButtonOptions.ButtonOptionsOption(_fullsreenOptionText));

        switch (Screen.fullScreenMode)
        {
            case (FullScreenMode.Windowed):
                _windowModeButtonOptions.SetOptionIndex((int)WindowMode.WINDOWED);
                break;
            case (FullScreenMode.MaximizedWindow):
                _windowModeButtonOptions.SetOptionIndex((int)WindowMode.BORDERLESS);
                break;
            case (FullScreenMode.FullScreenWindow):
                _windowModeButtonOptions.SetOptionIndex((int)WindowMode.FULLSCREEN);
                break;
        }

        _windowModeButtonOptions.SetOptionIndex(_windowModeButtonOptions.CurrentOptionIndex);
    }
}
