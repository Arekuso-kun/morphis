using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SettingsManager : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown _resolutionDropdown;
    [SerializeField] private TMP_Dropdown _fullscreenDropdown;
    [SerializeField] private TMP_Dropdown _fpsDropdown;
    [SerializeField] private Toggle _vsyncToggle;


    private Resolution[] _resolutions;
    private List<Resolution> _availableResolutions = new List<Resolution>();

    void Start()
    {
        SetupResolutionOptions();
        SetupFullscreenOptions();
        SetupFPSOptions();
        SetupVsyncOptions();
    }

    void SetupResolutionOptions()
    {
        _resolutions = Screen.resolutions;
        _availableResolutions.Clear();
        _resolutionDropdown.ClearOptions();

        var options = new List<string>();
        int currentResolutionIndex = 0;

        foreach (var res in _resolutions)
        {
            string option = res.width + " x " + res.height;
            if (!options.Contains(option))
            {
                options.Add(option);
                _availableResolutions.Add(res);

                if (res.width == Screen.currentResolution.width && res.height == Screen.currentResolution.height)
                    currentResolutionIndex = _availableResolutions.Count - 1;
            }
        }

        _resolutionDropdown.AddOptions(options);
        _resolutionDropdown.value = currentResolutionIndex;
        _resolutionDropdown.RefreshShownValue();
    }

    void SetupFullscreenOptions()
    {
        _fullscreenDropdown.ClearOptions();
        _fullscreenDropdown.AddOptions(new List<string> {
            "Fullscreen",
            "Borderless",
            "Windowed"
        });

        switch (Screen.fullScreenMode)
        {
            case FullScreenMode.ExclusiveFullScreen:
                _fullscreenDropdown.value = 0;
                break;
            case FullScreenMode.FullScreenWindow:
                _fullscreenDropdown.value = 1;
                break;
            case FullScreenMode.Windowed:
                _fullscreenDropdown.value = 2;
                break;
        }

        _fullscreenDropdown.RefreshShownValue();
    }

    void SetupFPSOptions()
    {
        _fpsDropdown.ClearOptions();
        _fpsDropdown.AddOptions(new System.Collections.Generic.List<string> {
            "30", "60", "120", "144", "Unlimited"
        });

        int current = Application.targetFrameRate;
        switch (current)
        {
            case 30: _fpsDropdown.value = 0; break;
            case 60: _fpsDropdown.value = 1; break;
            case 120: _fpsDropdown.value = 2; break;
            case 144: _fpsDropdown.value = 3; break;
            default: _fpsDropdown.value = 4; break;
        }

        _fpsDropdown.RefreshShownValue();
    }

    void SetupVsyncOptions()
    {
        _vsyncToggle.isOn = QualitySettings.vSyncCount > 0;
    }


    public void OnResolutionChanged()
    {
        var selectedRes = _availableResolutions[_resolutionDropdown.value];
        var refreshRate = selectedRes.refreshRateRatio;

        Screen.SetResolution(selectedRes.width, selectedRes.height, Screen.fullScreenMode, refreshRate);
    }

    public void OnFullscreenChanged()
    {
        switch (_fullscreenDropdown.value)
        {
            case 0: Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen; break;
            case 1: Screen.fullScreenMode = FullScreenMode.FullScreenWindow; break;
            case 2: Screen.fullScreenMode = FullScreenMode.Windowed; break;
        }
    }

    public void OnFPSChanged()
    {
        switch (_fpsDropdown.value)
        {
            case 0: Application.targetFrameRate = 30; break;
            case 1: Application.targetFrameRate = 60; break;
            case 2: Application.targetFrameRate = 120; break;
            case 3: Application.targetFrameRate = 144; break;
            case 4: Application.targetFrameRate = -1; break; // Unlimited
        }
    }

    public void OnVSyncChanged()
    {
        QualitySettings.vSyncCount = _vsyncToggle.isOn ? 1 : 0;
    }
}
