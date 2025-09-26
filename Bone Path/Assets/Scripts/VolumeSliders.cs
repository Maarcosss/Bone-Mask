using UnityEngine;
using UnityEngine.UI;

public class VolumeSliders : MonoBehaviour
{
    [Header("Slider References")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Settings")]
    [Tooltip("Mostrar logs de debug")]
    public bool showDebugLogs = false;

    void Start()
    {
        // Validar AudioManager
        if (AudioManager.instance == null)
        {
            Debug.LogError("❌ AudioManager.instance es null. Asegúrate de que AudioManager esté en la escena.");
            return;
        }

        // Validar sliders
        if (!ValidateSliders())
        {
            return;
        }

        InitializeSliders();
        SetupSliderEvents();

        if (showDebugLogs)
            Debug.Log("✅ VolumeSliders inicializado correctamente");
    }

    bool ValidateSliders()
    {
        bool isValid = true;

        if (masterSlider == null)
        {
            Debug.LogError("❌ masterSlider no está asignado en VolumeSliders");
            isValid = false;
        }

        if (musicSlider == null)
        {
            Debug.LogError("❌ musicSlider no está asignado en VolumeSliders");
            isValid = false;
        }

        if (sfxSlider == null)
        {
            Debug.LogError("❌ sfxSlider no está asignado en VolumeSliders");
            isValid = false;
        }

        return isValid;
    }

    void InitializeSliders()
    {
        try
        {
            // Inicializar sliders con valores guardados
            if (masterSlider != null)
                masterSlider.value = AudioManager.instance.GetMasterVolume();

            if (musicSlider != null)
                musicSlider.value = AudioManager.instance.GetMusicVolume();

            if (sfxSlider != null)
                sfxSlider.value = AudioManager.instance.GetSFXVolume();

            if (showDebugLogs)
                Debug.Log("🔊 Sliders inicializados con valores guardados");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error inicializando sliders: {e.Message}");
        }
    }

    void SetupSliderEvents()
    {
        try
        {
            // Asignar eventos con validación
            if (masterSlider != null)
            {
                masterSlider.onValueChanged.RemoveAllListeners(); // Limpiar eventos previos
                masterSlider.onValueChanged.AddListener(AudioManager.instance.SetMasterVolume);
            }

            if (musicSlider != null)
            {
                musicSlider.onValueChanged.RemoveAllListeners();
                musicSlider.onValueChanged.AddListener(AudioManager.instance.SetMusicVolume);
            }

            if (sfxSlider != null)
            {
                sfxSlider.onValueChanged.RemoveAllListeners();
                sfxSlider.onValueChanged.AddListener(AudioManager.instance.SetSFXVolume);
            }

            if (showDebugLogs)
                Debug.Log("🔗 Eventos de sliders configurados");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error configurando eventos: {e.Message}");
        }
    }

    // Método público para refrescar valores
    public void RefreshSliderValues()
    {
        if (AudioManager.instance == null) return;

        if (masterSlider != null)
            masterSlider.value = AudioManager.instance.GetMasterVolume();

        if (musicSlider != null)
            musicSlider.value = AudioManager.instance.GetMusicVolume();

        if (sfxSlider != null)
            sfxSlider.value = AudioManager.instance.GetSFXVolume();

        if (showDebugLogs)
            Debug.Log("🔄 Valores de sliders actualizados");
    }

    // Método para resetear a valores por defecto
    public void ResetToDefaults()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.ResetVolumesToHalf();
            RefreshSliderValues();
        }
    }

    void OnDestroy()
    {
        // Limpiar eventos para evitar errores
        if (masterSlider != null)
            masterSlider.onValueChanged.RemoveAllListeners();

        if (musicSlider != null)
            musicSlider.onValueChanged.RemoveAllListeners();

        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveAllListeners();
    }
}
