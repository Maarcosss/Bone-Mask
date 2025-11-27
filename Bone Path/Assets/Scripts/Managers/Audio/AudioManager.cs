using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class AudioManager : MonoBehaviour
{
    [Header("Mixer")]
    public AudioMixer gameMixer;

    [Header("Exposed Parameters")]
    public string masterParam = "MasterVol";
    public string musicParam = "MusicVol";
    public string sfxParam = "SFXVol";

    [Header("Sliders")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Texts TMP")]
    public TextMeshProUGUI masterText;
    public TextMeshProUGUI musicText;
    public TextMeshProUGUI sfxText;

    public static AudioManager Instance { get; private set; }

    private float masterVolume = 1f;
    private float musicVolume = 1f;
    private float sfxVolume = 1f;

    private bool isUpdatingSliders = false;

    public System.Action<float, float, float> OnVolumeChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadSavedVolumes();
            ApplyVolumes();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        RefreshSlidersAndTexts();
    }

    void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    //Update the sliders and texts of the audio UI after the scene has finished loading
    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        StartCoroutine(RefreshUIAfterFrame());
    }

    //Ensures that the UI updates correctly after the scene loads
    System.Collections.IEnumerator RefreshUIAfterFrame()
    {
        yield return null;
        RefreshSlidersAndTexts();
    }

    //Loads the volume values previously saved in PlayerPrefs for Master, Music, and SFX
    void LoadSavedVolumes()
    {
        masterVolume = PlayerPrefs.GetFloat(masterParam, 1f);
        musicVolume = PlayerPrefs.GetFloat(musicParam, 1f);
        sfxVolume = PlayerPrefs.GetFloat(sfxParam, 1f);
    }

    //Sets volume values directly from saved or loaded parameters.
    public void SetVolumeFromSave(float master, float music, float sfx)
    {
        masterVolume = Clamp01(master);
        musicVolume = Clamp01(music);
        sfxVolume = Clamp01(sfx);

        PlayerPrefs.SetFloat(masterParam, masterVolume);
        PlayerPrefs.SetFloat(musicParam, musicVolume);
        PlayerPrefs.SetFloat(sfxParam, sfxVolume);

        ApplyVolumes();
        RefreshSlidersAndTexts();
    }

    //Individually adjust the volume of Master
    public void SetMasterVolume(float value)
    {
        if (isUpdatingSliders)
        {
            return;
        }

        masterVolume = Clamp01(value);
        SaveVolume(masterParam, masterVolume);
        ApplyVolume(masterParam, masterVolume);
        UpdateMasterText();
        OnVolumeChanged?.Invoke(masterVolume, musicVolume, sfxVolume);
    }

    //Individually adjust the volume of Music
    public void SetMusicVolume(float value)
    {
        if (isUpdatingSliders)
        {
            return;
        }

        musicVolume = Clamp01(value);
        SaveVolume(musicParam, musicVolume);
        ApplyVolume(musicParam, musicVolume);
        UpdateMusicText();
        OnVolumeChanged?.Invoke(masterVolume, musicVolume, sfxVolume);
    }

    //Individually adjust the volume of SFX
    public void SetSFXVolume(float value)
    {
        if (isUpdatingSliders)
        {
            return;
        }

        sfxVolume = Clamp01(value);
        SaveVolume(sfxParam, sfxVolume);
        ApplyVolume(sfxParam, sfxVolume);
        UpdateSFXText();
        OnVolumeChanged?.Invoke(masterVolume, musicVolume, sfxVolume);
    }

    //Apply the three current volumes (master, music, and effects) to the AudioMixer
    void ApplyVolumes()
    {
        ApplyVolume(masterParam, masterVolume);
        ApplyVolume(musicParam, musicVolume);
        ApplyVolume(sfxParam, sfxVolume);
    }

    //Assigns the converted value to the corresponding parameter of the AudioMixer
    void ApplyVolume(string param, float value)
    {
        if (gameMixer != null)
        {
            if (value < 0.0001f)
            {
                value = 0.0001f;
            }
            float dB = 20f * (float)System.Math.Log10((double)value);
            gameMixer.SetFloat(param, dB);
        }
    }

    //Save the volume value in PlayerPrefs
    void SaveVolume(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
        PlayerPrefs.Save();
    }

    //Normalize volume values or any input that should be in that range.
    float Clamp01(float value)
    {
        if (value < 0f)
        {
            return 0f;
        }
        else if (value > 1f)
        {
            return 1f;
        }
        else
        {
            return value;
        }
    }

    //Synchronize the sliders and UI texts with the current volume values
    public void RefreshSlidersAndTexts()
    {
        LoadSavedVolumes();

        FindSliders();
        FindTexts();

        isUpdatingSliders = true;

        UpdateSliders();
        ConfigureSliderCallbacks();

        isUpdatingSliders = false;

        ApplyVolumes();
        UpdateAllTexts();
    }

    //Search the scene for Sliders that control master volume, music, and SFX if they are not manually assigned
    void FindSliders()
    {
        if (masterSlider == null)
        {
            masterSlider = GameObject.Find("SliderMaster")?.GetComponent<Slider>();
        }
        if (musicSlider == null)
        {
            musicSlider = GameObject.Find("SliderMusic")?.GetComponent<Slider>();
        }
        if (sfxSlider == null)
        {
            sfxSlider = GameObject.Find("SliderSound")?.GetComponent<Slider>();
        }
    }

    //Search the scene for UI texts (TextMeshProUGUI) that display volume values if they are not assigned
    void FindTexts()
    {
        if (masterText == null)
        {
            masterText = GameObject.Find("MasterText")?.GetComponent<TextMeshProUGUI>();
        }
        if (musicText == null)
        {
            musicText = GameObject.Find("MusicText")?.GetComponent<TextMeshProUGUI>();
        }
        if (sfxText == null)
        {
            sfxText = GameObject.Find("SFXText")?.GetComponent<TextMeshProUGUI>();
        }
    }

    //Configure the Sliders to call the corresponding methods (SetMasterVolume, SetMusicVolume, SetSFXVolume) each time their value changes
    void ConfigureSliderCallbacks()
    {
        if (masterSlider != null)
        {
            masterSlider.onValueChanged.RemoveAllListeners();
            masterSlider.onValueChanged.AddListener(SetMasterVolume);
        }

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }

    //Update the Slider values to reflect the current volume values
    void UpdateSliders()
    {
        if (masterSlider != null)
        {
            masterSlider.value = masterVolume;
        }

        if (musicSlider != null)
        {
            musicSlider.value = musicVolume;
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = sfxVolume;
        }
    }

    //Updates the text of the master volume (masterText)
    void UpdateMasterText()
    {
        if (masterText != null)
        {
            masterText.text = FloatToPercent(masterVolume);
        }
    }

    //Updates the text of the music volume (musicText)
    void UpdateMusicText()
    {
        if (musicText != null)
        {
            musicText.text = FloatToPercent(musicVolume);
        }
    }

    //Updates the text of the sound effects volume (sfxText)
    void UpdateSFXText()
    {
        if (sfxText != null)
        {
            sfxText.text = FloatToPercent(sfxVolume);
        }
    }

    //Call the three previous functions to update all audio UI texts at once
    void UpdateAllTexts()
    {
        UpdateMasterText();
        UpdateMusicText();
        UpdateSFXText();
    }

    //Converts a float volume value (range 0 to 1) to an integer percentage as a string
    string FloatToPercent(float value)
    {
        int percent = (int)((value * 100f) + 0.5f);
        return percent.ToString();
    }

    //Returns the current value of the master volume
    public float GetMasterVolume()
    {
        return masterVolume;
    }

    //Returns the current music volume value
    public float GetMusicVolume()
    {
        return musicVolume;
    }

    //Returns the current value of the sound effects volume
    public float GetSFXVolume()
    {
        return sfxVolume;
    }

    //Reset all volumes to an intermediate value of 50%
    public void ResetVolumesToHalf()
    {
        masterVolume = 0.5f;
        musicVolume = 0.5f;
        sfxVolume = 0.5f;

        SaveVolume(masterParam, masterVolume);
        SaveVolume(musicParam, musicVolume);
        SaveVolume(sfxParam, sfxVolume);

        ApplyVolumes();
        RefreshSlidersAndTexts();
        OnVolumeChanged?.Invoke(masterVolume, musicVolume, sfxVolume);
    }

    //Generates and returns a summary of the current status of the audio system
    public string GetAudioSystemInfo()
    {
        string sliderInfo = "Sliders: M:";
        if (masterSlider != null)
        {
            sliderInfo += "Yes";
        }
        else
        {
            sliderInfo += "No";
        }

        sliderInfo += " | Mu:";
        if (musicSlider != null)
        {
            sliderInfo += "Yes";
        }
        else
        {
            sliderInfo += "No";
        }

        sliderInfo += " | S:";
        if (sfxSlider != null)
        {
            sliderInfo += "Yes";
        }
        else
        {
            sliderInfo += "No";
        }

        string textInfo = "Texts: M:";
        if (masterText != null)
        {
            textInfo += "Yes";
        }
        else
        {
            textInfo += "No";
        }

        textInfo += " | Mu:";
        if (musicText != null)
        {
            textInfo += "Yes";
        }
        else
        {
            textInfo += "No";
        }

        textInfo += " | S:";
        if (sfxText != null)
        {
            textInfo += "Yes";
        }
        else
        {
            textInfo += "No";
        }

        string valueInfo = "Values: M:" + FloatToPercent(masterVolume) +
                           "% | Mu:" + FloatToPercent(musicVolume) +
                           "% | S:" + FloatToPercent(sfxVolume) + "%";

        return "AudioManager | " + sliderInfo + " | " + textInfo + " | " + valueInfo;
    }
}
