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
    private Coroutine currentAnimation;
    private Vector3 originalScale; // ✅ SIN VALOR POR DEFECTO - SE TOMA DEL INSPECTOR

    // ✅ SINGLETON (único static permitido)
    public static CurrencySystem Instance { get; private set; }

    // ✅ EVENTOS CONVERTIDOS A NO-STATIC
    [System.NonSerialized] public System.Action<int> OnCoinsChanged;
    [System.NonSerialized] public System.Action<int> OnCoinsAdded;
    [System.NonSerialized] public System.Action<int> OnCoinsSpent;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (showDebugLogs)
                Debug.Log("💰 CurrencySystem Singleton inicializado");
        }
        else
        {
            if (showDebugLogs)
                Debug.Log("💰 CurrencySystem duplicado destruido");
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        Invoke(nameof(InitializeCurrencySystem), 0.1f);
    }

    void InitializeCurrencySystem()
    {
        FindCoinText();
        SetupAudioSource();

        // ✅ GUARDAR LA ESCALA ORIGINAL DEL INSPECTOR (SIN MODIFICAR)
        if (coinText != null)
        {
            originalScale = coinText.transform.localScale;
            if (showDebugLogs)
                Debug.Log($"💰 Escala original del Inspector guardada: {originalScale}");
        }

        LoadCoins();
        UpdateCoinUI();

        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;

        if (showDebugLogs)
            Debug.Log($"💰 CurrencySystem iniciado con {currentCoins} monedas");
    }

    void SetupAudioSource()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (showDebugLogs)
            Debug.Log($"📋 Escena cargada: {scene.name} - Rebuscar CoinText");

        StartCoroutine(DelayedUISetup());
    }

    System.Collections.IEnumerator DelayedUISetup()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        FindCoinText();

        // ✅ RESTAURAR ESCALA ORIGINAL SI ES NECESARIO
        if (coinText != null && originalScale != Vector3.zero)
        {
            coinText.transform.localScale = originalScale;
            if (showDebugLogs)
                Debug.Log($"🔧 Escala restaurada después de cambio de escena: {originalScale}");
        }

        UpdateCoinUI();
    }

    void FindCoinText()
    {
        if (coinText == null)
        {
            coinText = GameObject.Find("CoinText")?.GetComponent<TMP_Text>();

            if (coinText == null)
            {
                coinText = GameObject.Find("CoinTXT")?.GetComponent<TMP_Text>();
            }

            if (coinText == null)
            {
                Canvas canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    TMP_Text[] allTexts = canvas.GetComponentsInChildren<TMP_Text>();
                    foreach (TMP_Text text in allTexts)
                    {
                        if (text.name.ToLower().Contains("coin"))
                        {
                            coinText = text;
                            break;
                        }
                    }
                }
            }

        }
    }

    void OnDestroy()
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }

        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
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
        if (coinText == null)
        {
            FindCoinText();
            if (coinText == null) return;
        }

        coinText.text = string.Format(coinFormat, currentCoins);

        if (enableScaleAnimation && currentCoins != lastDisplayedCoins)
        {
            if (currentAnimation != null)
            {
                StopCoroutine(currentAnimation);
                coinText.transform.localScale = originalScale;
            }

            currentAnimation = StartCoroutine(AnimateCoinText());
            lastDisplayedCoins = currentCoins;
        }
    }

    System.Collections.IEnumerator AnimateCoinText()
    {
        if (coinText == null || originalScale == Vector3.zero) yield break;

        Vector3 targetScale = originalScale * animationScale;

        if (showDebugLogs)
            Debug.Log($"🎬 Iniciando animación: {originalScale} → {targetScale} → {originalScale}");

        float elapsed = 0f;
        float halfDuration = animationDuration * 0.5f;

        // Escalar hacia arriba
        while (elapsed < halfDuration)
        {
            if (coinText == null) yield break;

            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            coinText.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        if (coinText != null)
            coinText.transform.localScale = targetScale;

        // Escalar hacia abajo
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            if (coinText == null) yield break;

            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            coinText.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        if (coinText != null)
        {
            coinText.transform.localScale = originalScale;
            if (showDebugLogs)
                Debug.Log($"🎬 Animación completada. Escala final: {coinText.transform.localScale}");
        }

        currentAnimation = null;
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

    public void SetAnimationEnabled(bool enabled)
    {
        enableScaleAnimation = enabled;

        if (!enabled && currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            if (coinText != null && originalScale != Vector3.zero)
                coinText.transform.localScale = originalScale;
            currentAnimation = null;
        }

        if (showDebugLogs)
            Debug.Log($"🎬 Animación de escala: {(enabled ? "ACTIVADA" : "DESACTIVADA")}");
    }

    public void ResetCoins()
    {
        SetCoins(0);
        if (showDebugLogs)
            Debug.Log("🗑️ Monedas reseteadas a 0");
    }

    [ContextMenu("Force Reset UI")]
    public void ForceResetUI()
    {
        coinText = null;
        FindCoinText();

        if (coinText != null)
        {
            originalScale = coinText.transform.localScale;
            UpdateCoinUI();

            if (showDebugLogs)
                Debug.Log($"🔧 UI forzada a resetear. Nueva escala: {originalScale}");
        }
    }

    public string GetSystemInfo()
    {
        return $"Monedas: {currentCoins}/{maxCoins} | UI: {(coinText != null ? "✅" : "❌")} | Audio: {(audioSource != null ? "✅" : "❌")} | Animación: {(enableScaleAnimation ? "✅" : "❌")} | Escala: {originalScale}";
    }
}
