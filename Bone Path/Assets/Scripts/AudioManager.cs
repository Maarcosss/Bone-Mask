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

    [Header("Debug")]
    [Tooltip("Mostrar logs de debug del AudioManager")]
    public bool showDebugLogs = true;

    // ✅ SINGLETON (único static permitido)
    public static AudioManager Instance { get; private set; }

    // Valores actuales de 0 a 1
    private float masterVolume = 1f;
    private float musicVolume = 1f;
    private float sfxVolume = 1f;

    // Prevenir loops infinitos
    private bool isUpdatingSliders = false;

    // ✅ EVENTOS CONVERTIDOS A NO-STATIC
    [System.NonSerialized] public System.Action<float, float, float> OnVolumeChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadSavedVolumes();
            ApplyVolumes();

            if (showDebugLogs)
                Debug.Log($"🔊 AudioManager Singleton iniciado | Master: {masterVolume:F2} | Music: {musicVolume:F2} | SFX: {sfxVolume:F2}");
        }
        else
        {
            if (showDebugLogs)
                Debug.Log("🔊 AudioManager duplicado destruido");
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

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (showDebugLogs)
            Debug.Log($"📋 Escena cargada: {scene.name} - Actualizando UI de audio");

        StartCoroutine(RefreshUIAfterFrame());
    }

    System.Collections.IEnumerator RefreshUIAfterFrame()
    {
        yield return null;
        RefreshSlidersAndTexts();
    }

    void LoadSavedVolumes()
    {
        masterVolume = PlayerPrefs.GetFloat(masterParam, 1f);
        musicVolume = PlayerPrefs.GetFloat(musicParam, 1f);
        sfxVolume = PlayerPrefs.GetFloat(sfxParam, 1f);

        if (showDebugLogs)
            Debug.Log($"📁 Volúmenes cargados desde PlayerPrefs | Master: {masterVolume:F2} | Music: {musicVolume:F2} | SFX: {sfxVolume:F2}");
    }

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

        if (showDebugLogs)
            Debug.Log($"🔊 Volúmenes cargados desde SaveSystem | M:{FloatToPercent(masterVolume)}% | Mu:{FloatToPercent(musicVolume)}% | S:{FloatToPercent(sfxVolume)}%");
    }

    public void SetMasterVolume(float value)
    {
        if (isUpdatingSliders) return;

        masterVolume = Clamp01(value);
        SaveVolume(masterParam, masterVolume);
        ApplyVolume(masterParam, masterVolume);
        UpdateMasterText();

        OnVolumeChanged?.Invoke(masterVolume, musicVolume, sfxVolume);

        if (showDebugLogs)
            Debug.Log($"🔊 Master Volume: {masterVolume:F2} ({FloatToPercent(masterVolume)}%)");
    }

    public void SetMusicVolume(float value)
    {
        if (isUpdatingSliders) return;

        musicVolume = Clamp01(value);
        SaveVolume(musicParam, musicVolume);
        ApplyVolume(musicParam, musicVolume);
        UpdateMusicText();

        OnVolumeChanged?.Invoke(masterVolume, musicVolume, sfxVolume);

        if (showDebugLogs)
            Debug.Log($"🎵 Music Volume: {musicVolume:F2} ({FloatToPercent(musicVolume)}%)");
    }

    public void SetSFXVolume(float value)
    {
        if (isUpdatingSliders) return;

        sfxVolume = Clamp01(value);
        SaveVolume(sfxParam, sfxVolume);
        ApplyVolume(sfxParam, sfxVolume);
        UpdateSFXText();

        OnVolumeChanged?.Invoke(masterVolume, musicVolume, sfxVolume);

        if (showDebugLogs)
            Debug.Log($"🔥 SFX Volume: {sfxVolume:F2} ({FloatToPercent(sfxVolume)}%)");
    }

    void ApplyVolumes()
    {
        ApplyVolume(masterParam, masterVolume);
        ApplyVolume(musicParam, musicVolume);
        ApplyVolume(sfxParam, sfxVolume);
    }

    void ApplyVolume(string param, float value)
    {
        if (gameMixer != null)
        {
            if (value < 0.0001f) value = 0.0001f;
            float dB = 20f * (float)System.Math.Log10((double)value);
            gameMixer.SetFloat(param, dB);
        }
    }

    void SaveVolume(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
        PlayerPrefs.Save();

        if (showDebugLogs)
            Debug.Log($"💾 Guardado {key}: {value:F2}");
    }

    float Clamp01(float value)
    {
        if (value < 0f) return 0f;
        if (value > 1f) return 1f;
        return value;
    }

    public void RefreshSlidersAndTexts()
    {
        LoadSavedVolumes();

        bool foundSliders = FindSliders();
        bool foundTexts = FindTexts();

        isUpdatingSliders = true;

        UpdateSliders();
        ConfigureSliderCallbacks();

        isUpdatingSliders = false;

        ApplyVolumes();
        UpdateAllTexts();

        if (showDebugLogs)
        {
            Debug.Log($"🔄 RefreshSlidersAndTexts completado:");
            Debug.Log($"   📊 Sliders encontrados: {foundSliders}");
            Debug.Log($"   📝 Textos encontrados: {foundTexts}");
            Debug.Log($"   🔊 Valores actuales: M:{FloatToPercent(masterVolume)}% | Mu:{FloatToPercent(musicVolume)}% | S:{FloatToPercent(sfxVolume)}%");
        }
    }

    bool FindSliders()
    {
        bool allFound = true;

        if (masterSlider == null)
        {
            masterSlider = GameObject.Find("SliderMaster")?.GetComponent<Slider>();
            if (masterSlider == null) allFound = false;
        }

        if (musicSlider == null)
        {
            musicSlider = GameObject.Find("SliderMusic")?.GetComponent<Slider>();
            if (musicSlider == null) allFound = false;
        }

        if (sfxSlider == null)
        {
            sfxSlider = GameObject.Find("SliderSound")?.GetComponent<Slider>();
            if (sfxSlider == null) allFound = false;
        }

        return allFound;
    }

    bool FindTexts()
    {
        bool allFound = true;

        if (masterText == null)
        {
            masterText = GameObject.Find("MasterText")?.GetComponent<TextMeshProUGUI>();
            if (masterText == null) allFound = false;
        }

        if (musicText == null)
        {
            musicText = GameObject.Find("MusicText")?.GetComponent<TextMeshProUGUI>();
            if (musicText == null) allFound = false;
        }

        if (sfxText == null)
        {
            sfxText = GameObject.Find("SFXText")?.GetComponent<TextMeshProUGUI>();
            if (sfxText == null) allFound = false;
        }

        return allFound;
    }

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

    void UpdateSliders()
    {
        if (masterSlider != null)
        {
            masterSlider.value = masterVolume;
            if (showDebugLogs)
                Debug.Log($"📊 Master Slider actualizado: {masterVolume:F2}");
        }

        if (musicSlider != null)
        {
            musicSlider.value = musicVolume;
            if (showDebugLogs)
                Debug.Log($"📊 Music Slider actualizado: {musicVolume:F2}");
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = sfxVolume;
            if (showDebugLogs)
                Debug.Log($"📊 SFX Slider actualizado: {sfxVolume:F2}");
        }
    }

    void UpdateMasterText()
    {
        if (masterText != null)
        {
            string newText = FloatToPercent(masterVolume);
            masterText.text = newText;
            if (showDebugLogs)
                Debug.Log($"📝 Master Text actualizado: '{newText}'");
        }
    }

    void UpdateMusicText()
    {
        if (musicText != null)
        {
            string newText = FloatToPercent(musicVolume);
            musicText.text = newText;
            if (showDebugLogs)
                Debug.Log($"📝 Music Text actualizado: '{newText}'");
        }
    }

    void UpdateSFXText()
    {
        if (sfxText != null)
        {
            string newText = FloatToPercent(sfxVolume);
            sfxText.text = newText;
            if (showDebugLogs)
                Debug.Log($"📝 SFX Text actualizado: '{newText}'");
        }
    }

    void UpdateAllTexts()
    {
        UpdateMasterText();
        UpdateMusicText();
        UpdateSFXText();
    }

    string FloatToPercent(float value)
    {
        int percent = (int)((value * 100f) + 0.5f);
        return percent.ToString();
    }

    public float GetMasterVolume() { return masterVolume; }
    public float GetMusicVolume() { return musicVolume; }
    public float GetSFXVolume() { return sfxVolume; }

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

        if (showDebugLogs)
            Debug.Log("🔄 Volúmenes reseteados al 50%");
    }

    public string GetAudioSystemInfo()
    {
        string sliderInfo = $"Sliders: M:{(masterSlider != null ? "✅" : "❌")} | Mu:{(musicSlider != null ? "✅" : "❌")} | S:{(sfxSlider != null ? "✅" : "❌")}";
        string textInfo = $"Texts: M:{(masterText != null ? "✅" : "❌")} | Mu:{(musicText != null ? "✅" : "❌")} | S:{(sfxText != null ? "✅" : "❌")}";
        string valueInfo = $"Values: M:{FloatToPercent(masterVolume)}% | Mu:{FloatToPercent(musicVolume)}% | S:{FloatToPercent(sfxVolume)}%";

        return $"AudioManager | {sliderInfo} | {textInfo} | {valueInfo}";
    }
}
