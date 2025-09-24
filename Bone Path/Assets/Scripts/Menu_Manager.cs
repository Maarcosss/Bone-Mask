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
    [SerializeField] private float defaultbrightness = 1;

    [Header("Resolutions Dropdown")]
    public TMP_Dropdown resolutionDropdown;
    private Resolution[] resolutions;

    private bool _isFullScreen;
    private float _brightnessLevel;



    public void SetBrightness(float brightness)
    {

        _brightnessLevel = brightness;
        brightnessTextValue.text = brightness.ToString("0.0");
        Screen.fullScreen = _isFullScreen;

    }

    public void SetFullScreen(bool isFullScreen)
    {

        _isFullScreen = isFullScreen;

    }

    public void GraphicsApply()
    {

        PlayerPrefs.SetFloat("masterBrightness", _brightnessLevel);

        PlayerPrefs.SetFloat("masterFullScreen", (_isFullScreen ? 1 : 0));

    }


    public void SetResolution(int resolutionIndex)
    {

        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);

    }

    // Start is called before the first frame update
    void Start()
    {

        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();

        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {

            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
            {

                currentResolutionIndex = i;

            }

        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

    }

    // Update is called once per frame
    void Update()
    {

        

    }

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

    public void BackBrightnessGameOptions()
    {

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
