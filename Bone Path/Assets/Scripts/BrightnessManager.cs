using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BrightnessManager : MonoBehaviour
{
    [Header("Volume Settings")]
    [SerializeField] private Volume globalVolume = null;

    [Header("Brightness Settings")]
    [Tooltip("Rango mínimo de brillo (más oscuro)")]
    public float minBrightness = -1f;
    [Tooltip("Rango máximo de brillo (normal)")]
    public float maxBrightness = 0f;
    [Tooltip("Mostrar logs de debug")]
    public bool showDebugLogs = false;

    private ColorAdjustments colorAdjustments;
    private bool isInitialized = false;

    private void Awake()
    {
        InitializeBrightnessSystem();
    }

    void InitializeBrightnessSystem()
    {
        // Validar Volume
        if (globalVolume == null)
        {
            globalVolume = FindObjectOfType<Volume>();
            if (globalVolume == null)
            {
                Debug.LogError("❌ No se encontró Global Volume en la escena. Agrega un Volume con perfil URP.");
                return;
            }
        }

        // Validar perfil del Volume
        if (globalVolume.profile == null)
        {
            Debug.LogError("❌ Global Volume no tiene perfil asignado.");
            return;
        }

        // Obtener ColorAdjustments del Volume
        if (!globalVolume.profile.TryGet<ColorAdjustments>(out colorAdjustments))
        {
            Debug.LogError("❌ No se encontró ColorAdjustments en el perfil del Volume. Agrega ColorAdjustments al perfil URP.");
            return;
        }

        // Habilitar ColorAdjustments si no está activo
        if (!colorAdjustments.active)
        {
            colorAdjustments.active = true;
            if (showDebugLogs)
                Debug.Log("✅ ColorAdjustments habilitado");
        }

        isInitialized = true;

        // Aplicar brillo guardado al iniciar
        float savedBrightness = PlayerPrefs.GetFloat("masterBrightness", 1f);
        SetBrightness(savedBrightness);

        if (showDebugLogs)
            Debug.Log("✅ BrightnessManager inicializado correctamente");
    }

    public void SetBrightness(float value)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("⚠️ BrightnessManager no está inicializado");
            return;
        }

        if (colorAdjustments == null)
        {
            Debug.LogError("❌ ColorAdjustments es null");
            return;
        }

        // Validar rango de entrada
        value = Mathf.Clamp01(value);

        try
        {
            // Mapear valor de 0-1 al rango configurado
            float mappedValue = Mathf.Lerp(minBrightness, maxBrightness, value);

            // Aplicar el valor
            colorAdjustments.postExposure.value = mappedValue;

            // Guardar el valor
            PlayerPrefs.SetFloat("masterBrightness", value);

            if (showDebugLogs)
                Debug.Log($"🔆 Brillo establecido: {value:F2} (PostExposure: {mappedValue:F2})");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error estableciendo brillo: {e.Message}");
        }
    }

    public float GetBrightness()
    {
        if (!isInitialized || colorAdjustments == null)
            return 1f;

        // Convertir de vuelta al rango 0-1
        float currentExposure = colorAdjustments.postExposure.value;
        return Mathf.InverseLerp(minBrightness, maxBrightness, currentExposure);
    }

    public void ResetBrightness()
    {
        SetBrightness(1f); // Valor por defecto

        if (showDebugLogs)
            Debug.Log("🔄 Brillo reiniciado a valor por defecto");
    }

    // Método para validar el sistema
    public bool IsSystemValid()
    {
        return isInitialized && globalVolume != null && colorAdjustments != null;
    }

    // Método para obtener información del sistema
    public string GetSystemInfo()
    {
        if (!isInitialized)
            return "Sistema no inicializado";

        return $"Volume: {(globalVolume != null ? "✅" : "❌")}, " +
               $"Profile: {(globalVolume?.profile != null ? "✅" : "❌")}, " +
               $"ColorAdjustments: {(colorAdjustments != null ? "✅" : "❌")}";
    }

    void OnValidate()
    {
        // Validar rangos en el editor
        if (minBrightness > maxBrightness)
        {
            maxBrightness = minBrightness;
        }
    }
}
