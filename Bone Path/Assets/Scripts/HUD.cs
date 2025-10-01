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
        // ✅ SUSCRIBIRSE A EVENTOS NO-STATIC
        if (CurrencySystem.Instance != null)
        {
            CurrencySystem.Instance.OnCoinsChanged += OnCoinsChanged;
        }
    }

    void OnDestroy()
    {
        // ✅ DESUSCRIBIRSE DE EVENTOS NO-STATIC
        if (CurrencySystem.Instance != null)
        {
            CurrencySystem.Instance.OnCoinsChanged -= OnCoinsChanged;
        }
    }

    void Update()
    {
        UpdateSoulDisplay();
        UpdateCoinDisplay();
    }

    void UpdateSoulDisplay()
    {
        if (playerHealthRef == null || soulText == null) return;

        float currentSoul = playerHealthRef.GetCurrentSoul();

        if (Mathf.Approximately(currentSoul, lastSoulValue)) return;

        lastSoulValue = currentSoul;

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

        if (currentCoins == lastCoinValue) return;

        lastCoinValue = currentCoins;
        cachedCoinString = string.Format(coinFormat, currentCoins);
        coinText.text = cachedCoinString;

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

        float elapsed = 0f;
        float halfDuration = coinAnimationDuration * 0.5f;

        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            coinText.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

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

    void OnCoinsChanged(int newCoinAmount)
    {
        if (showDebugLogs)
            Debug.Log($"💰 Evento de monedas recibido: {newCoinAmount}");
    }

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

    public string GetDisplayedSoulValue()
    {
        return cachedSoulString;
    }

    public string GetDisplayedCoinValue()
    {
        return cachedCoinString;
    }

    public void SetCoinFormat(string newFormat)
    {
        coinFormat = newFormat;
        ForceUpdateCoins();

        if (showDebugLogs)
            Debug.Log($"💰 Formato de monedas cambiado a: '{newFormat}'");
    }

    public string GetHUDInfo()
    {
        string coinStatus = coinText != null ? "✅" : "❌";
        string soulStatus = soulText != null ? "✅" : "❌";
        string currencySystemStatus = CurrencySystem.Instance != null ? "✅" : "❌";

        return $"HUD | Soul: {soulStatus} | Coins: {coinStatus} | CurrencySystem: {currencySystemStatus} | Animation: {coinAnimationInProgress}";
    }

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
