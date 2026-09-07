using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OptionDisplayController : MonoBehaviour
{
    [Header("Dropdown")]
    [SerializeField] private TMP_Dropdown displayModeDropdown;
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    private readonly List<Vector2Int> resolutions = new();

    private void Start()
    {
        InitializeDisplayModeDropdown();
        InitializeResolutionDropdown();
    }

    private void InitializeDisplayModeDropdown()
    {
        if (displayModeDropdown == null)
            return;

        displayModeDropdown.ClearOptions();
        displayModeDropdown.AddOptions(new List<string>
        {
            "전체화면",
            "창모드"
        });

        displayModeDropdown.SetValueWithoutNotify(Screen.fullScreen ? 0 : 1);
        displayModeDropdown.RefreshShownValue();

        displayModeDropdown.onValueChanged.RemoveAllListeners();
        displayModeDropdown.onValueChanged.AddListener(OnDisplayModeChanged);
    }

    private void InitializeResolutionDropdown()
    {
        if (resolutionDropdown == null)
            return;

        resolutionDropdown.ClearOptions();
        resolutions.Clear();

        List<string> options = new List<string>();
        HashSet<string> added = new HashSet<string>();

        Resolution[] screenResolutions = Screen.resolutions;

        foreach (Resolution r in screenResolutions)
        {
            string key = $"{r.width}x{r.height}";

            if (added.Contains(key))
                continue;

            added.Add(key);
            resolutions.Add(new Vector2Int(r.width, r.height));
            options.Add($"{r.width} x {r.height}");
        }

        if (options.Count == 0)
        {
            resolutions.Add(new Vector2Int(Screen.width, Screen.height));
            options.Add($"{Screen.width} x {Screen.height}");
        }

        int currentIndex = 0;

        for (int i = 0; i < resolutions.Count; i++)
        {
            if (resolutions[i].x == Screen.width &&
                resolutions[i].y == Screen.height)
            {
                currentIndex = i;
                break;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.SetValueWithoutNotify(currentIndex);
        resolutionDropdown.RefreshShownValue();

        resolutionDropdown.onValueChanged.RemoveAllListeners();
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
    }

    public void OnDisplayModeChanged(int index)
    {
        bool isFullscreen = index == 0;
        Screen.fullScreen = isFullscreen;

        ApplyCurrentResolution();
    }

    public void OnResolutionChanged(int index)
    {
        ApplyResolution(index);
    }

    private void ApplyCurrentResolution()
    {
        if (resolutionDropdown == null)
            return;

        ApplyResolution(resolutionDropdown.value);
    }

    private void ApplyResolution(int index)
    {
        if (index < 0 || index >= resolutions.Count)
            return;

        Vector2Int resolution = resolutions[index];
        Screen.SetResolution(resolution.x, resolution.y, Screen.fullScreen);
    }
}