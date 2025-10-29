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
        if(Screen.fullScreen)
        {
            toggle.isOn = true;
        }
        else
        {
            toggle.isOn = false;
        }

        ReviewResolutions();

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

    public void ActiveFullScreen(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
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

            if (Screen.fullScreen && resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolution = i;
            }
        }
        resolutionsDropdown.AddOptions(options);
        resolutionsDropdown.value = currentResolution;
        resolutionsDropdown.RefreshShownValue();

        resolutionsDropdown.value = PlayerPrefs.GetInt("resolutionnumber", 0);
    }

    public void ChangeResolution(int indexResolution)
    {
        PlayerPrefs.SetInt("resolutionnumber", resolutionsDropdown.value);

        Resolution resolution = resolutions[indexResolution];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void ChangeSlider(float valor)
    {
        sliderValue = valor;
        PlayerPrefs.SetFloat("brillo", sliderValue);
        if (panelBrightnessInstance != null)
        {
            panelBrightnessInstance.color = new Color(panelBrightnessInstance.color.r, panelBrightnessInstance.color.g, panelBrightnessInstance.color.b, 1 - sliderValue);
        }
        UpdateBrilloText(sliderValue);
    }

    private void UpdateBrilloText(float valor)
    {
        if (brightnessText != null)
        {
            brightnessText.text = (valor * 10).ToString("0");
        }
    }

    public void ResetValue()
    {
        sliderValue = 1f;
        if (slider != null)
        {
            slider.value = sliderValue;
        }

        if (panelBrightnessInstance != null)
        {
            panelBrightnessInstance.color = new Color(panelBrightnessInstance.color.r, panelBrightnessInstance.color.g, panelBrightnessInstance.color.b, 1 - sliderValue);
        }
        UpdateBrilloText(sliderValue);
        PlayerPrefs.SetFloat("brillo", sliderValue);
    }
}
