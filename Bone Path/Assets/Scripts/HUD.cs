using UnityEngine;
using TMPro;

public class HUD : MonoBehaviour
{
    [Header("References")]
    public PlayerHealth playerHealthRef;
    public TMP_Text soulText;
    public TMP_Text coinText;

    [Header("Soul Settings")]
    [Tooltip("Formato para mostrar el alma (0 = sin decimales, 1 = un decimal)")]
    public int soulDecimalPlaces = 0;

    [Header("Coin Settings")]
    [Tooltip("Formato del texto de monedas (ej: 'Coins: {0}' o solo '{0}')")]
    public string coinFormat = "{0}";

    [Tooltip("Mostrar animación cuando cambian las monedas")]
    public bool animateCoins = true;

    [Tooltip("Duración de la animación de monedas")]
    public float coinAnimationDuration = 0.3f;

    [Tooltip("Escala de animación para las monedas")]
    public float coinAnimationScale = 1.2f;

    [Header("Debug")]
    [Tooltip("Mostrar logs de debug")]
    public bool showDebugLogs = false;

    // Cache para evitar conversiones innecesarias
    private float lastSoulValue = -1f;
    private int lastCoinValue = -1;
    private string cachedSoulString = "";
    private string cachedCoinString = "";

    // Variables para animación de monedas
    private bool coinAnimationInProgress = false;

    void Start()
    {
        ValidateReferences();
        SubscribeToEvents();

        if (enabled)
        {
            // Actualización inicial
            UpdateSoulDisplay();
            UpdateCoinDisplay();
        }
    }

    void ValidateReferences()
    {
        bool hasErrors = false;

        if (playerHealthRef == null)
        {
            Debug.LogError("❌ playerHealthRef no está asignado en HUD");
            hasErrors = true;
        }

        if (soulText == null)
        {
            Debug.LogError("❌ soulText no está asignado en HUD");
            hasErrors = true;
        }

        if (coinText == null)
        {
            Debug.LogWarning("⚠️ coinText no está asignado en HUD - Las monedas no se mostrarán");
        }

        if (hasErrors)
        {
            enabled = false;
            return;
        }

        if (showDebugLogs)
            Debug.Log("✅ HUD inicializado correctamente");
    }

    void SubscribeToEvents()
    {
        // Suscribirse a eventos del sistema de monedas
        CurrencySystem.OnCoinsChanged += OnCoinsChanged;
    }

    void OnDestroy()
    {
        // Desuscribirse de eventos
        CurrencySystem.OnCoinsChanged -= OnCoinsChanged;
    }

    void Update()
    {
        // Solo actualizar si hay cambios
        UpdateSoulDisplay();
        UpdateCoinDisplay();
    }

    void UpdateSoulDisplay()
    {
        if (playerHealthRef == null || soulText == null) return;

        float currentSoul = playerHealthRef.GetCurrentSoul();

        // Solo actualizar si el valor cambió
        if (Mathf.Approximately(currentSoul, lastSoulValue)) return;

        lastSoulValue = currentSoul;

        // Formatear según la configuración
        if (soulDecimalPlaces == 0)
        {
            cachedSoulString = ((int)currentSoul).ToString();
        }
        else
        {
            cachedSoulString = currentSoul.ToString($"F{soulDecimalPlaces}");
        }

        soulText.text = cachedSoulString;

        if (showDebugLogs)
            Debug.Log($"🔄 HUD actualizado: Alma = {cachedSoulString}");
    }

    void UpdateCoinDisplay()
    {
        if (coinText == null || CurrencySystem.Instance == null) return;

        int currentCoins = CurrencySystem.Instance.GetCurrentCoins();

        // Solo actualizar si el valor cambió
        if (currentCoins == lastCoinValue) return;

        lastCoinValue = currentCoins;
        cachedCoinString = string.Format(coinFormat, currentCoins);
        coinText.text = cachedCoinString;

        // Animar si está habilitado y no hay animación en progreso
        if (animateCoins && !coinAnimationInProgress)
        {
            StartCoroutine(AnimateCoinText());
        }

        if (showDebugLogs)
            Debug.Log($"💰 HUD actualizado: Monedas = {cachedCoinString}");
    }

    System.Collections.IEnumerator AnimateCoinText()
    {
        if (coinText == null) yield break;

        coinAnimationInProgress = true;
        Vector3 originalScale = coinText.transform.localScale;
        Vector3 targetScale = originalScale * coinAnimationScale;

        // Escalar hacia arriba
        float elapsed = 0f;
        float halfDuration = coinAnimationDuration * 0.5f;

        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            coinText.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        // Escalar hacia abajo
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            coinText.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        coinText.transform.localScale = originalScale;
        coinAnimationInProgress = false;
    }

    // Callback para eventos del sistema de monedas
    void OnCoinsChanged(int newCoinAmount)
    {
        // La actualización se hará en UpdateCoinDisplay()
        if (showDebugLogs)
            Debug.Log($"💰 Evento de monedas recibido: {newCoinAmount}");
    }

    // Métodos públicos para forzar actualizaciones
    public void ForceUpdateSoul()
    {
        lastSoulValue = -1f;
        UpdateSoulDisplay();
    }

    public void ForceUpdateCoins()
    {
        lastCoinValue = -1;
        UpdateCoinDisplay();
    }

    public void ForceUpdateAll()
    {
        ForceUpdateSoul();
        ForceUpdateCoins();
    }

    // Métodos públicos para obtener valores mostrados
    public string GetDisplayedSoulValue()
    {
        return cachedSoulString;
    }

    public string GetDisplayedCoinValue()
    {
        return cachedCoinString;
    }

    // Método para configurar el formato de monedas dinámicamente
    public void SetCoinFormat(string newFormat)
    {
        coinFormat = newFormat;
        ForceUpdateCoins();

        if (showDebugLogs)
            Debug.Log($"💰 Formato de monedas cambiado a: '{newFormat}'");
    }

    // Información del HUD para debugging
    public string GetHUDInfo()
    {
        string coinStatus = coinText != null ? "✅" : "❌";
        string soulStatus = soulText != null ? "✅" : "❌";
        string currencySystemStatus = CurrencySystem.Instance != null ? "✅" : "❌";

        return $"HUD | Soul: {soulStatus} | Coins: {coinStatus} | CurrencySystem: {currencySystemStatus} | Animation: {coinAnimationInProgress}";
    }

    // Método para configurar referencias automáticamente
    [ContextMenu("Auto Setup References")]
    public void AutoSetupReferences()
    {
        if (playerHealthRef == null)
        {
            playerHealthRef = FindObjectOfType<PlayerHealth>();
        }

        if (soulText == null)
        {
            GameObject soulObject = GameObject.Find("SoulText");
            if (soulObject != null)
                soulText = soulObject.GetComponent<TMP_Text>();
        }

        if (coinText == null)
        {
            GameObject coinObject = GameObject.Find("CoinText");
            if (coinObject != null)
                coinText = coinObject.GetComponent<TMP_Text>();
        }

        if (showDebugLogs)
            Debug.Log("🔧 Auto setup de referencias completado");
    }
}
