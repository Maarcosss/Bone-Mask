using UnityEngine;
using TMPro;

public class HUD : MonoBehaviour
{
    [Header("References")]
    public PlayerHealth playerHealthRef;
    public TMP_Text soulText;

    [Header("Settings")]
    [Tooltip("Formato para mostrar el alma (0 = sin decimales, 1 = un decimal)")]
    public int decimalPlaces = 0;
    [Tooltip("Mostrar logs de debug")]
    public bool showDebugLogs = false;

    // Cache para evitar conversiones innecesarias
    private float lastSoulValue = -1f;
    private string cachedSoulString = "";

    void Start()
    {
        // Validar referencias
        if (playerHealthRef == null)
        {
            Debug.LogError("❌ playerHealthRef no está asignado en HUD");
            enabled = false; // Deshabilitar este componente
            return;
        }

        if (soulText == null)
        {
            Debug.LogError("❌ soulText no está asignado en HUD");
            enabled = false;
            return;
        }

        if (showDebugLogs)
            Debug.Log("✅ HUD inicializado correctamente");

        // Actualización inicial
        UpdateSoulDisplay();
    }

    void Update()
    {
        // Solo actualizar si hay cambios
        UpdateSoulDisplay();
    }

    void UpdateSoulDisplay()
    {
        if (playerHealthRef == null || soulText == null) return;

        float currentSoul = playerHealthRef.GetCurrentSoul();

        // Solo actualizar si el valor cambió
        if (Mathf.Approximately(currentSoul, lastSoulValue)) return;

        lastSoulValue = currentSoul;

        // Formatear según la configuración
        if (decimalPlaces == 0)
        {
            cachedSoulString = ((int)currentSoul).ToString();
        }
        else
        {
            cachedSoulString = currentSoul.ToString($"F{decimalPlaces}");
        }

        soulText.text = cachedSoulString;

        if (showDebugLogs)
            Debug.Log($"🔄 HUD actualizado: Alma = {cachedSoulString}");
    }

    // Método público para forzar actualización
    public void ForceUpdate()
    {
        lastSoulValue = -1f; // Forzar actualización
        UpdateSoulDisplay();
    }

    // Método público para obtener el valor actual mostrado
    public string GetDisplayedSoulValue()
    {
        return cachedSoulString;
    }
}
