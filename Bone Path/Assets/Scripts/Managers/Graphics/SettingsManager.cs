using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

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

        sliderValue = PlayerPrefs.GetFloat("brillo", 0.5f);
        if (slider != null)
        {
            slider.value = sliderValue;
            slider.onValueChanged.AddListener(ChangeSlider);
        }
        if (brightnessText != null)
        {
            UpdateBrilloText(sliderValue);
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("No se encontró un Canvas en la escena. El brillo no se mostrará.");
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

    public void ChangeSlider(float value)
    {
        sliderValue = value;
        PlayerPrefs.SetFloat("brillo", sliderValue);
        PlayerPrefs.Save();

        if (panelBrightnessInstance != null)
            panelBrightnessInstance.color = new Color(panelBrightnessInstance.color.r, panelBrightnessInstance.color.g, panelBrightnessInstance.color.b, 1 - sliderValue);

        UpdateBrilloText(sliderValue);
    }

    private void UpdateBrilloText(float value)
    {
        if (brightnessText != null)
            brightnessText.text = (value * 10).ToString("0");
    }

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

        UpdateBrilloText(sliderValue);
        PlayerPrefs.SetFloat("brillo", sliderValue);

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


    public void ActiveFullScreen(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
        PlayerPrefs.SetInt("fullscreen", fullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

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

    public void ChangeResolution(int indexResolution)
    {
        PlayerPrefs.SetInt("resolutionnumber", indexResolution);
        PlayerPrefs.Save();

        Resolution resolution = resolutions[indexResolution];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
}
