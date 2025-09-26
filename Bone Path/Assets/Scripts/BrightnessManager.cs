using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BrightnessManager : MonoBehaviour
{
    [Header("Volume Settings")]
    [SerializeField] private Volume globalVolume = null;  // Asigna el Volume global en el inspector

    private UnityEngine.Rendering.Universal.ColorAdjustments colorAdjustments;

    private void Awake()
    {
        if (globalVolume == null)
        {
            Debug.LogError("Global Volume no asignado en BrightnessManager.");
            return;
        }

        // Obtener ColorAdjustments del Volume
        if (!globalVolume.profile.TryGet<ColorAdjustments>(out colorAdjustments))
        {
            Debug.LogError("No se encontró ColorAdjustments en el Volume.");
        }

        // Aplicar brillo guardado al iniciar
        float savedBrightness = PlayerPrefs.GetFloat("masterBrightness", 1f);
        SetBrightness(savedBrightness);
    }

    /// <summary>
    /// Ajusta el brillo del juego.
    /// </summary>
    /// <param name="value">0 = oscuro, 1 = normal</param>
    public void SetBrightness(float value)
    {
        if (colorAdjustments == null)
            return;

        // La propiedad post-exposure controla la exposición del juego
        // 0 = sin cambio, -1 = más oscuro, +1 = más brillante
        colorAdjustments.postExposure.value = Mathf.Lerp(-1f, 0f, value);
    }
}
