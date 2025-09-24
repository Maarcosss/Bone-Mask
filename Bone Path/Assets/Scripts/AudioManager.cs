using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

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

    // Valores actuales de 0 a 1
    private float masterVolume = 1f;
    private float musicVolume = 1f;
    private float sfxVolume = 1f;

    void Awake()
    {
        // Singleton
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Cargar valores guardados
        masterVolume = PlayerPrefs.GetFloat(masterParam, 1f);
        musicVolume = PlayerPrefs.GetFloat(musicParam, 1f);
        sfxVolume = PlayerPrefs.GetFloat(sfxParam, 1f);

        // Aplicar al mixer
        ApplyVolumes();
    }

    void Start()
    {
        // Inicializar sliders y textos si están asignados
        RefreshSlidersAndTexts();
    }

    // Llamar cuando se cambia un slider
    public void SetMasterVolume(float value)
    {
        masterVolume = Clamp01(value);
        SaveVolume(masterParam, masterVolume);
        ApplyVolume(masterParam, masterVolume);
        UpdateTexts();
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Clamp01(value);
        SaveVolume(musicParam, musicVolume);
        ApplyVolume(musicParam, musicVolume);
        UpdateTexts();
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = Clamp01(value);
        SaveVolume(sfxParam, sfxVolume);
        ApplyVolume(sfxParam, sfxVolume);
        UpdateTexts();
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
            if (value < 0.0001f) value = 0.0001f; // evitar log(0)
            float dB = 20f * (float)System.Math.Log10((double)value);
            gameMixer.SetFloat(param, dB);
        }
    }

    void SaveVolume(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
    }

    float Clamp01(float value)
    {
        if (value < 0f) return 0f;
        if (value > 1f) return 1f;
        return value;
    }

    // Actualiza sliders y textos TMP (llamar al cargar nueva escena)
    public void RefreshSlidersAndTexts()
    {
        // Buscar sliders si no están asignados
        if (masterSlider == null) masterSlider = GameObject.Find("MasterSlider")?.GetComponent<Slider>();
        if (musicSlider == null) musicSlider = GameObject.Find("MusicSlider")?.GetComponent<Slider>();
        if (sfxSlider == null) sfxSlider = GameObject.Find("SFXSlider")?.GetComponent<Slider>();

        // Buscar textos TMP si no están asignados
        if (masterText == null) masterText = GameObject.Find("MasterText")?.GetComponent<TextMeshProUGUI>();
        if (musicText == null) musicText = GameObject.Find("MusicText")?.GetComponent<TextMeshProUGUI>();
        if (sfxText == null) sfxText = GameObject.Find("SFXText")?.GetComponent<TextMeshProUGUI>();

        // Actualizar sliders
        if (masterSlider != null) masterSlider.value = masterVolume;
        if (musicSlider != null) musicSlider.value = musicVolume;
        if (sfxSlider != null) sfxSlider.value = sfxVolume;

        // Aplicar al mixer
        ApplyVolumes();

        // Actualizar textos
        UpdateTexts();
    }

    void UpdateTexts()
    {
        if (masterText != null) masterText.text = FloatToPercent(masterVolume);
        if (musicText != null) musicText.text = FloatToPercent(musicVolume);
        if (sfxText != null) sfxText.text = FloatToPercent(sfxVolume);
    }

    // Convierte un valor 0-1 a porcentaje sin usar Mathf
    string FloatToPercent(float value)
    {
        int percent = (int)((value * 100f) + 0.5f); // redondeo manual
        return percent.ToString();
    }

    // Opcional: obtener valores actuales
    public float GetMasterVolume() { return masterVolume; }
    public float GetMusicVolume() { return musicVolume; }
    public float GetSFXVolume() { return sfxVolume; }

    // Dentro de AudioManager.cs

    public void ResetVolumesToHalf()
    {
        masterVolume = 0.5f;
        musicVolume = 0.5f;
        sfxVolume = 0.5f;

        // Guardar los valores
        SaveVolume(masterParam, masterVolume);
        SaveVolume(musicParam, musicVolume);
        SaveVolume(sfxParam, sfxVolume);

        // Aplicar al mixer
        ApplyVolumes();

        // Actualizar sliders y textos
        RefreshSlidersAndTexts();
    }

}
