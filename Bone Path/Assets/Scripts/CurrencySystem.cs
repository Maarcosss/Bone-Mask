using UnityEngine;
using TMPro;

public class CurrencySystem : MonoBehaviour
{
    [Header("Currency Settings")]
    [Tooltip("Cantidad actual de monedas del jugador")]
    public int currentCoins = 0;

    [Tooltip("Cantidad máxima de monedas que puede tener el jugador")]
    public int maxCoins = 9999;

    [Header("UI References")]
    [Tooltip("Texto que muestra la cantidad de monedas en el HUD")]
    public TMP_Text coinText;

    [Tooltip("Formato del texto de monedas (ej: 'Coins: {0}' o solo '{0}')")]
    public string coinFormat = "{0}";

    [Header("Audio")]
    [Tooltip("Sonido que se reproduce al recoger una moneda")]
    public AudioClip coinCollectSound;

    [Header("Animation Settings")]
    [Tooltip("Duración de la animación al cambiar el contador")]
    public float animationDuration = 0.3f;

    [Tooltip("Escala de animación cuando se actualiza el contador")]
    public float animationScale = 1.2f;

    [Tooltip("Activar/desactivar animación de escala")]
    public bool enableScaleAnimation = true;

    [Header("Debug")]
    [Tooltip("Mostrar mensajes de debug del sistema de monedas")]
    public bool showDebugLogs = true;

    // Variables privadas
    private int lastDisplayedCoins = 0;
    private AudioSource audioSource;
    private Coroutine currentAnimation; // ✅ PARA CONTROLAR ANIMACIONES
    private Vector3 originalScale;       // ✅ GUARDAR ESCALA ORIGINAL

    // Singleton para acceso global
    public static CurrencySystem Instance { get; private set; }

    // Eventos para notificar a otros sistemas
    public static System.Action<int> OnCoinsChanged;
    public static System.Action<int> OnCoinsAdded;
    public static System.Action<int> OnCoinsSpent;

    void Awake()
    {
        // Implementar Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Buscar referencias si no están asignadas
        if (coinText == null)
        {
            coinText = GameObject.Find("CoinText")?.GetComponent<TMP_Text>();
        }

        // ✅ GUARDAR ESCALA ORIGINAL AL INICIO
        if (coinText != null)
        {
            originalScale = coinText.transform.localScale;
            if (showDebugLogs)
                Debug.Log($"💰 Escala original del texto guardada: {originalScale}");
        }

        // Obtener AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Cargar monedas guardadas
        LoadCoins();

        // Actualizar UI inicial
        UpdateCoinUI();

        if (showDebugLogs)
            Debug.Log($"💰 CurrencySystem iniciado con {currentCoins} monedas");
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0) return;

        int previousCoins = currentCoins;
        currentCoins = Mathf.Min(currentCoins + amount, maxCoins);
        int actualAdded = currentCoins - previousCoins;

        if (actualAdded > 0)
        {
            UpdateCoinUI();
            PlayCoinSound();
            SaveCoins();

            // Disparar eventos
            OnCoinsChanged?.Invoke(currentCoins);
            OnCoinsAdded?.Invoke(actualAdded);

            if (showDebugLogs)
                Debug.Log($"💰 +{actualAdded} monedas añadidas. Total: {currentCoins}");
        }
    }

    public bool SpendCoins(int amount)
    {
        if (amount <= 0 || currentCoins < amount)
        {
            if (showDebugLogs)
                Debug.Log($"💸 No se pueden gastar {amount} monedas. Disponibles: {currentCoins}");
            return false;
        }

        currentCoins -= amount;
        UpdateCoinUI();
        SaveCoins();

        // Disparar eventos
        OnCoinsChanged?.Invoke(currentCoins);
        OnCoinsSpent?.Invoke(amount);

        if (showDebugLogs)
            Debug.Log($"💸 -{amount} monedas gastadas. Total: {currentCoins}");

        return true;
    }

    public bool HasEnoughCoins(int amount)
    {
        return currentCoins >= amount;
    }

    public int GetCurrentCoins()
    {
        return currentCoins;
    }

    public void SetCoins(int amount)
    {
        currentCoins = Mathf.Clamp(amount, 0, maxCoins);
        UpdateCoinUI();
        SaveCoins();
        OnCoinsChanged?.Invoke(currentCoins);

        if (showDebugLogs)
            Debug.Log($"💰 Monedas establecidas a: {currentCoins}");
    }

    void UpdateCoinUI()
    {
        if (coinText == null) return;

        coinText.text = string.Format(coinFormat, currentCoins);

        // ✅ ANIMACIÓN MEJORADA CON CONTROL
        if (enableScaleAnimation && currentCoins != lastDisplayedCoins)
        {
            // ✅ DETENER ANIMACIÓN ANTERIOR SI EXISTE
            if (currentAnimation != null)
            {
                StopCoroutine(currentAnimation);
                // ✅ RESTAURAR ESCALA INMEDIATAMENTE
                coinText.transform.localScale = originalScale;
            }

            // ✅ INICIAR NUEVA ANIMACIÓN
            currentAnimation = StartCoroutine(AnimateCoinText());
            lastDisplayedCoins = currentCoins;
        }
    }

    // ✅ ANIMACIÓN MEJORADA Y CONTROLADA
    System.Collections.IEnumerator AnimateCoinText()
    {
        if (coinText == null) yield break;

        Vector3 targetScale = originalScale * animationScale;

        if (showDebugLogs)
            Debug.Log($"🎬 Iniciando animación: {originalScale} → {targetScale} → {originalScale}");

        // ✅ FASE 1: Escalar hacia arriba
        float elapsed = 0f;
        float halfDuration = animationDuration * 0.5f;

        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;

            // ✅ USAR CURVA SUAVE
            t = Mathf.SmoothStep(0f, 1f, t);

            coinText.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        // ✅ ASEGURAR QUE LLEGUE AL TAMAÑO MÁXIMO
        coinText.transform.localScale = targetScale;

        // ✅ FASE 2: Escalar hacia abajo
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;

            // ✅ USAR CURVA SUAVE
            t = Mathf.SmoothStep(0f, 1f, t);

            coinText.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        // ✅ GARANTIZAR ESCALA ORIGINAL AL FINAL
        coinText.transform.localScale = originalScale;
        currentAnimation = null;

        if (showDebugLogs)
            Debug.Log($"🎬 Animación completada. Escala final: {coinText.transform.localScale}");
    }

    void PlayCoinSound()
    {
        if (audioSource != null && coinCollectSound != null)
        {
            audioSource.PlayOneShot(coinCollectSound);
        }
    }

    void SaveCoins()
    {
        PlayerPrefs.SetInt("PlayerCoins", currentCoins);
        PlayerPrefs.Save();
    }

    void LoadCoins()
    {
        currentCoins = PlayerPrefs.GetInt("PlayerCoins", 0);
        currentCoins = Mathf.Clamp(currentCoins, 0, maxCoins);
    }

    // ✅ MÉTODO PARA RESETEAR ESCALA MANUALMENTE
    public void ResetTextScale()
    {
        if (coinText != null && originalScale != Vector3.zero)
        {
            if (currentAnimation != null)
            {
                StopCoroutine(currentAnimation);
                currentAnimation = null;
            }

            coinText.transform.localScale = originalScale;

            if (showDebugLogs)
                Debug.Log($"🔧 Escala del texto reseteada a: {originalScale}");
        }
    }

    // ✅ MÉTODO PARA DESACTIVAR ANIMACIÓN
    public void SetAnimationEnabled(bool enabled)
    {
        enableScaleAnimation = enabled;

        if (!enabled && currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            coinText.transform.localScale = originalScale;
            currentAnimation = null;
        }

        if (showDebugLogs)
            Debug.Log($"🎬 Animación de escala: {(enabled ? "ACTIVADA" : "DESACTIVADA")}");
    }

    // Métodos públicos para debugging
    [System.Obsolete("Solo para testing - Usar AddCoins() en producción")]
    public void AddCoins_Debug(int amount)
    {
        AddCoins(amount);
    }

    [System.Obsolete("Solo para testing - Usar SpendCoins() en producción")]
    public void SpendCoins_Debug(int amount)
    {
        SpendCoins(amount);
    }

    public void ResetCoins()
    {
        SetCoins(0);
        if (showDebugLogs)
            Debug.Log("🗑️ Monedas reseteadas a 0");
    }

    // Información del sistema para debugging
    public string GetSystemInfo()
    {
        return $"Monedas: {currentCoins}/{maxCoins} | UI: {(coinText != null ? "✅" : "❌")} | Audio: {(audioSource != null ? "✅" : "❌")} | Animación: {(enableScaleAnimation ? "✅" : "❌")}";
    }

    // ✅ CLEANUP AL DESTRUIR
    void OnDestroy()
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }
    }
}
