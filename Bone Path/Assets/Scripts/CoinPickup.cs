using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    [Header("Coin Settings")]
    [Tooltip("Valor de esta moneda")]
    public int coinValue = 1;

    [Tooltip("Auto-destruir después de ser recogida")]
    public bool autoDestroy = true;

    [Header("Physics Settings")]
    [Tooltip("Fuerza mínima para recoger la moneda")]
    public float minimumCollisionForce = 0.1f;

    [Header("Audio")]
    [Tooltip("Sonido específico para esta moneda (opcional)")]
    public AudioClip customPickupSound;

    [Header("Debug")]
    [Tooltip("Mostrar logs de debug para esta moneda")]
    public bool showDebugLogs = false;

    // Variables privadas
    private bool isCollected = false;
    private AudioSource audioSource;

    void Start()
    {
        // ✅ CONFIGURAR AUDIO
        if (customPickupSound != null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.spatialBlend = 1f; // 3D Audio
            audioSource.volume = 0.7f;
            audioSource.playOnAwake = false;
        }

        if (showDebugLogs)
            Debug.Log($"💰 Moneda creada | Valor: {coinValue}");
    }

    // ✅ DETECTAR COLISIÓN CON JUGADOR
    void OnCollisionEnter(Collision collision)
    {
        if (isCollected) return;

        if (showDebugLogs)
            Debug.Log($"💰 Colisión detectada: {collision.gameObject.name} (Tag: {collision.gameObject.tag}) | Fuerza: {collision.relativeVelocity.magnitude:F2}");

        // ✅ VERIFICAR SI ES EL JUGADOR
        if (collision.gameObject.CompareTag("Player"))
        {
            // ✅ VERIFICAR FUERZA MÍNIMA (opcional)
            if (collision.relativeVelocity.magnitude >= minimumCollisionForce)
            {
                CollectCoin();
            }
            else if (showDebugLogs)
            {
                Debug.Log($"💰 Colisión muy suave, no se recoge | Fuerza: {collision.relativeVelocity.magnitude:F2} < {minimumCollisionForce}");
            }
        }
    }

    void CollectCoin()
    {
        if (isCollected) return;
        isCollected = true;

        if (showDebugLogs)
            Debug.Log($"💰 ¡MONEDA RECOGIDA! Valor: +{coinValue}");

        // ✅ DETENER FÍSICA INMEDIATAMENTE
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true; // Detener toda física
        }

        // ✅ AÑADIR MONEDAS AL SISTEMA
        if (CurrencySystem.Instance != null)
        {
            CurrencySystem.Instance.AddCoins(coinValue);
        }
        else
        {
            Debug.LogError("❌ CurrencySystem.Instance no encontrado al recoger moneda");
        }

        // ✅ REPRODUCIR SONIDO
        if (customPickupSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(customPickupSound);
        }

        // ✅ OCULTAR INMEDIATAMENTE
        if (autoDestroy)
        {
            // Ocultar visual inmediatamente
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = false;

            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;

            // Destruir después de un pequeño delay para el sonido
            float delay = customPickupSound != null ? Mathf.Min(customPickupSound.length, 0.5f) : 0.1f;
            Destroy(gameObject, delay);
        }
        else
        {
            // Solo desactivar visualmente
            GetComponent<Renderer>().enabled = false;
            GetComponent<Collider>().enabled = false;
        }
    }

    // ✅ MÉTODO VACÍO PARA COMPATIBILIDAD CON ENEMY.CS
    public void SetupPhysics(bool restrictZ = true, float zPos = 0f)
    {
        // Método vacío - toda configuración se hace manualmente en Inspector
        if (showDebugLogs)
            Debug.Log($"💰 SetupPhysics llamado - configuración manual requerida");
    }

    // Método para establecer el valor dinámicamente
    public void SetCoinValue(int newValue)
    {
        coinValue = Mathf.Max(1, newValue);

        if (showDebugLogs)
            Debug.Log($"💰 Valor de moneda cambiado a: {coinValue}");
    }

    // Método para reactivar la moneda (útil para respawn)
    public void ResetCoin()
    {
        isCollected = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        GetComponent<Renderer>().enabled = true;
        GetComponent<Collider>().enabled = true;

        if (showDebugLogs)
            Debug.Log($"🔄 Moneda reseteada");
    }
}
