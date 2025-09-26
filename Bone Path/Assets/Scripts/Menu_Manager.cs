using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Menu_Manager : MonoBehaviour
{
    public GameObject MainMenuPanel;
    public GameObject OptionsMainMenuPanel;
    public GameObject ExtrasPanel;
    public GameObject GameOptionsPanel;
    public GameObject BrightnessGameOptionsPanel;
    public GameObject AudioPanel;
    public GameObject ControllerPanel;
    public GameObject QuitGamePanel;

    [Header("Graphics Settings")]
    [SerializeField] private Slider brightnessSlider = null;
    [SerializeField] private TMP_Text brightnessTextValue = null;
    [SerializeField] private float defaultBrightness = 1f;

    [Header("Brightness Preview")]
    [SerializeField] private Image brightnessPreviewImage = null; // Imagen para previsualización del brillo

    [Header("Brightness Render")]
    [SerializeField] private BrightnessManager brightnessManager = null; // Script que controla el Volume

    [Header("Resolutions Dropdown")]
    public TMP_Dropdown resolutionDropdown;
    private Resolution[] resolutions;

    private bool _isFullScreen;
    private float _brightnessLevel;

    private void Start()
    {
        // --- Resoluciones ---
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
                currentResolutionIndex = i;
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        // --- Brillo inicial ---
        float savedBrightness = PlayerPrefs.GetFloat("masterBrightness", defaultBrightness);
        brightnessSlider.value = savedBrightness;
        SetBrightness(savedBrightness);
    }

    // ===================== BRILLO =====================
    public void SetBrightness(float brightness)
    {
        _brightnessLevel = brightness;
        if (brightnessTextValue != null)
            brightnessTextValue.text = brightness.ToString("0.0");

        // Previsualización en la imagen
        if (brightnessPreviewImage != null)
        {
            Color c = brightnessPreviewImage.color;
            c.r = brightness;
            c.g = brightness;
            c.b = brightness;
            c.a = 1f;
            brightnessPreviewImage.color = c;
        }

        // Aplicar al render real si hay BrightnessManager
        if (brightnessManager != null)
        {
            brightnessManager.SetBrightness(brightness);
        }
    }

    public void ResetBrightness()
    {
        brightnessSlider.value = defaultBrightness;
        SetBrightness(defaultBrightness);
    }

    public void SetFullScreen(bool isFullScreen)
    {
        _isFullScreen = isFullScreen;
    }

    public void GraphicsApply()
    {
        PlayerPrefs.SetFloat("masterBrightness", _brightnessLevel);
        PlayerPrefs.SetFloat("masterFullScreen", (_isFullScreen ? 1 : 0));
        PlayerPrefs.Save();
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    // ===================== MENÚS =====================
    public void StartGame()
    {
        AudioManager.instance.RefreshSlidersAndTexts();
        SceneManager.LoadScene(1);
    }

    public void Options()
    {
        MainMenuPanel.SetActive(false);
        OptionsMainMenuPanel.SetActive(true);
    }

    public void GameOptions()
    {
        OptionsMainMenuPanel.SetActive(false);
        GameOptionsPanel.SetActive(true);
    }

    public void BrightnessGameOptions()
    {
        GameOptionsPanel.SetActive(false);
        BrightnessGameOptionsPanel.SetActive(true);
    }

    public void DoneBrightnessGameOptions()
    {
        GraphicsApply();
        BrightnessGameOptionsPanel.SetActive(false);
        GameOptionsPanel.SetActive(true);
    }

    public void BackGameOptions()
    {
        GameOptionsPanel.SetActive(false);
        OptionsMainMenuPanel.SetActive(true);
    }

    public void Audio()
    {
        OptionsMainMenuPanel.SetActive(false);
        AudioPanel.SetActive(true);
    }

    public void BackAudio()
    {
        AudioPanel.SetActive(false);
        OptionsMainMenuPanel.SetActive(true);
    }

    public void Controller()
    {
        OptionsMainMenuPanel.SetActive(false);
        ControllerPanel.SetActive(true);
    }

    public void BackController()
    {
        ControllerPanel.SetActive(false);
        OptionsMainMenuPanel.SetActive(true);
    }

    public void BackOpciones()
    {
        OptionsMainMenuPanel.SetActive(false);
        MainMenuPanel.SetActive(true);
    }

    public void Extras()
    {
        MainMenuPanel.SetActive(false);
        ExtrasPanel.SetActive(true);
    }

    public void BackExtras()
    {
        ExtrasPanel.SetActive(false);
        MainMenuPanel.SetActive(true);
    }

    public void QuitGame()
    {
        MainMenuPanel.SetActive(false);
        QuitGamePanel.SetActive(true);
    }

    public void YesQuitGame()
    {
        Debug.Log("Salir del juego");
        Application.Quit();
    }

    public void NoQuitGame()
    {
        QuitGamePanel.SetActive(false);
        MainMenuPanel.SetActive(true);
    }
}
