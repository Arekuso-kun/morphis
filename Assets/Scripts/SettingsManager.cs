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
    private List<Resolution> _availableResolutions = new();
    private bool _isLoadingSettings = true;

    void Awake()
    {
        SetupResolutionOptions();
        SetupFullscreenOptions();
        SetupFPSOptions();
        SetupVsyncOptions();
        LoadSettings();
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
    }

    void SetupFPSOptions()
    {
        _fpsDropdown.ClearOptions();
        _fpsDropdown.AddOptions(new List<string> {
            "30", "60", "120", "144", "Unlimited"
        });
    }

    void SetupVsyncOptions()
    {
        _vsyncToggle.isOn = QualitySettings.vSyncCount > 0;
    }

    public void OnResolutionChanged()
    {
        if (_isLoadingSettings) return;
        ApplyResolution(_resolutionDropdown.value);
        SaveSettings();
    }

    public void OnFullscreenChanged()
    {
        if (_isLoadingSettings) return;
        ApplyFullscreen(_fullscreenDropdown.value);
        SaveSettings();
    }

    public void OnFPSChanged()
    {
        if (_isLoadingSettings) return;
        ApplyFPS(_fpsDropdown.value);
        SaveSettings();
    }

    public void OnVSyncChanged()
    {
        if (_isLoadingSettings) return;
        ApplyVSync(_vsyncToggle.isOn);
        SaveSettings();
    }

    private void ApplyResolution(int resolutionIndex)
    {
        var selectedRes = _availableResolutions[resolutionIndex];
        var refreshRate = selectedRes.refreshRateRatio;
        Screen.SetResolution(selectedRes.width, selectedRes.height, Screen.fullScreenMode, refreshRate);
    }

    private void ApplyFullscreen(int fullscreenMode)
    {
        switch (fullscreenMode)
        {
            case 0: Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen; break;
            case 1: Screen.fullScreenMode = FullScreenMode.FullScreenWindow; break;
            case 2: Screen.fullScreenMode = FullScreenMode.Windowed; break;
        }
    }

    private void ApplyFPS(int fpsLimit)
    {
        switch (fpsLimit)
        {
            case 0: Application.targetFrameRate = 30; break;
            case 1: Application.targetFrameRate = 60; break;
            case 2: Application.targetFrameRate = 120; break;
            case 3: Application.targetFrameRate = 144; break;
            case 4: Application.targetFrameRate = -1; break;
        }
    }

    private void ApplyVSync(bool isOn)
    {
        QualitySettings.vSyncCount = isOn ? 1 : 0;

        _fpsDropdown.interactable = !isOn;
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetInt("ResolutionIndex", _resolutionDropdown.value);
        PlayerPrefs.SetInt("FullscreenMode", _fullscreenDropdown.value);
        PlayerPrefs.SetInt("FPSLimit", _fpsDropdown.value);
        PlayerPrefs.SetInt("VSync", _vsyncToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void LoadSettings()
    {
        _isLoadingSettings = true;

        if (PlayerPrefs.HasKey("ResolutionIndex"))
        {
            int resolutionIndex = PlayerPrefs.GetInt("ResolutionIndex");
            _resolutionDropdown.value = resolutionIndex;
            ApplyResolution(resolutionIndex);
        }

        if (PlayerPrefs.HasKey("FullscreenMode"))
        {
            int fullscreenMode = PlayerPrefs.GetInt("FullscreenMode");
            _fullscreenDropdown.value = fullscreenMode;
            ApplyFullscreen(fullscreenMode);
        }

        if (PlayerPrefs.HasKey("FPSLimit"))
        {
            int fpsLimit = PlayerPrefs.GetInt("FPSLimit");
            _fpsDropdown.value = fpsLimit;
            ApplyFPS(fpsLimit);
        }

        if (PlayerPrefs.HasKey("VSync"))
        {
            bool vsyncOn = PlayerPrefs.GetInt("VSync") == 1;
            _vsyncToggle.isOn = vsyncOn;
            ApplyVSync(vsyncOn);
        }

        _resolutionDropdown.RefreshShownValue();
        _fullscreenDropdown.RefreshShownValue();
        _fpsDropdown.RefreshShownValue();

        _isLoadingSettings = false;
    }
}
