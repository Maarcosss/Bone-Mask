using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/*Author: Marcos Isar
Date: 20 - Nov - 2025*/

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance;

    [Header("UI Menu")]
    public Slider slider;
    public TextMeshProUGUI brightnessText;

    public Toggle toggle;
    public TMP_Dropdown resolutionsDropdown;
    Resolution[] resolutions;

    [Header("Panel Prefab")]
    public Image panelBrightnessPrefab;

    private Image panelBrightnessInstance;
    public float sliderValue;

    //Ensure singleton instance and persist across scenes
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {

        sliderValue = PlayerPrefs.GetFloat("brightness", 0.5f);
        if (slider != null)
        {
            slider.value = sliderValue;
            slider.onValueChanged.AddListener(ChangeSlider);
        }
        if (brightnessText != null)
        {
            UpdateBrightnessText(sliderValue);
        }

        bool fullscreenSaved = PlayerPrefs.GetInt("fullscreen", Screen.fullScreen ? 1 : 0) == 1;
        if (toggle != null)
        {
            toggle.isOn = fullscreenSaved;
            toggle.onValueChanged.AddListener(ActiveFullScreen);
        }
        Screen.fullScreen = fullscreenSaved;

        ReviewResolutions();
    }

    //Instantiate brightness panel and update its transparency
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("No se encontró un Canvas en la escena. El brightness no se mostrará.");
            return;
        }

        if (panelBrightnessInstance == null && panelBrightnessPrefab != null)
        {
            panelBrightnessInstance = Instantiate(panelBrightnessPrefab, canvas.transform, false);
        }

        if (panelBrightnessInstance != null)
        {
            panelBrightnessInstance.color = new Color(
                panelBrightnessInstance.color.r,
                panelBrightnessInstance.color.g,
                panelBrightnessInstance.color.b,
                1 - sliderValue
            );
        }
    }

    //Update slider value, brightness panel, and text
    public void ChangeSlider(float value)
    {
        sliderValue = value;
        PlayerPrefs.SetFloat("brightness", sliderValue);
        PlayerPrefs.Save();

        if (panelBrightnessInstance != null)
            panelBrightnessInstance.color = new Color(panelBrightnessInstance.color.r, panelBrightnessInstance.color.g, panelBrightnessInstance.color.b, 1 - sliderValue);

        UpdateBrightnessText(sliderValue);
    }

    //Update brightness text display
    private void UpdateBrightnessText(float value)
    {
        if (brightnessText != null)
            brightnessText.text = (value * 10).ToString("0");
    }

    //Reset all settings to default values
    public void ResetValue()
    {
        sliderValue = 1f;
        if (slider != null)
            slider.value = sliderValue;

        if (panelBrightnessInstance != null)
            panelBrightnessInstance.color = new Color(
                panelBrightnessInstance.color.r,
                panelBrightnessInstance.color.g,
                panelBrightnessInstance.color.b,
                1 - sliderValue
            );

        UpdateBrightnessText(sliderValue);
        PlayerPrefs.SetFloat("brightness", sliderValue);

        Screen.SetResolution(1920, 1080, true);
        PlayerPrefs.SetInt("fullscreen", 1);

        if (toggle != null)
        {
            toggle.isOn = true;
            ActiveFullScreen(true);
        }

        if (resolutionsDropdown != null)
        {
            for (int i = 0; i < resolutions.Length; i++)
            {
                if (resolutions[i].width == 1920 && resolutions[i].height == 1080)
                {
                    resolutionsDropdown.value = i;
                    resolutionsDropdown.RefreshShownValue();
                    ChangeResolution(i);
                    break;
                }
            }
        }

        PlayerPrefs.Save();
    }

    //Enable or disable fullscreen and save preference
    public void ActiveFullScreen(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
        PlayerPrefs.SetInt("fullscreen", fullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    //Populate resolutions dropdown and set current or saved resolution
    public void ReviewResolutions()
    {
        resolutions = Screen.resolutions;
        resolutionsDropdown.ClearOptions();
        List<string> options = new List<string>();
        int currentResolution = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolution = i;
            }
        }

        resolutionsDropdown.AddOptions(options);

        // Cargar resolución guardada o usar actual
        int savedResolution = PlayerPrefs.GetInt("resolutionnumber", currentResolution);
        resolutionsDropdown.value = Mathf.Clamp(savedResolution, 0, resolutions.Length - 1);
        resolutionsDropdown.RefreshShownValue();

        resolutionsDropdown.onValueChanged.AddListener(ChangeResolution);
    }

    //Apply selected screen resolution and save preference
    public void ChangeResolution(int indexResolution)
    {
        PlayerPrefs.SetInt("resolutionnumber", indexResolution);
        PlayerPrefs.Save();

        Resolution resolution = resolutions[indexResolution];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
}
