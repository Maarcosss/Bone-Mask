using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    [Header("Coin Settings")]
    [Tooltip("Valor de esta moneda")]
    public int coinValue = 1;

    [Tooltip("Auto-destruir después de ser recogida")]
    public bool autoDestroy = true;

    [Header("Visual Effects")]
    [Tooltip("Animación de rotación de la moneda")]
    public float rotationSpeed = 90f;

    [Header("Physics Settings")]
    [Tooltip("Restringir movimiento en Z (para falso 2D)")]
    public bool restrictZMovement = true;

    [Tooltip("Posición Z fija cuando está restringido")]
    public float fixedZPosition = 0f;

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
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Configurar Z fijo si está habilitado
        if (restrictZMovement)
        {
            fixedZPosition = transform.position.z;
        }

        // ✅ CONFIGURAR RIGIDBODY PARA FÍSICA REAL CON RESTRICCIÓN Z
        if (rb != null && restrictZMovement)
        {
            // Solo restringir rotación en X e Y, y posición en Z
            rb.constraints = RigidbodyConstraints.FreezePositionZ |
                           RigidbodyConstraints.FreezeRotationX |
                           RigidbodyConstraints.FreezeRotationZ;

            // Configurar física para que se comporte bien
            rb.drag = 0.5f;          // Un poco de resistencia al aire
            rb.angularDrag = 1f;     // Resistencia rotacional
            rb.mass = 0.1f;          // Ligeras para que sean fáciles de mover
        }

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
            Debug.Log($"💰 Moneda física creada | Valor: {coinValue} | Restrict Z: {restrictZMovement}");
    }

    void Update()
    {
        if (isCollected) return;

        // ✅ MANTENER Z FIJO SI ESTÁ RESTRINGIDO
        if (restrictZMovement && rb != null)
        {
            Vector3 pos = transform.position;
            if (Mathf.Abs(pos.z - fixedZPosition) > 0.01f)
            {
                pos.z = fixedZPosition;
                transform.position = pos;
            }
        }

        // ✅ ROTACIÓN SUAVE
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }

    void FixedUpdate()
    {
        // ✅ ASEGURAR QUE Z SE MANTENGA FIJO EN FÍSICA
        if (restrictZMovement && rb != null)
        {
            Vector3 velocity = rb.velocity;
            velocity.z = 0f; // Eliminar cualquier velocidad en Z
            rb.velocity = velocity;
        }
    }

    // ✅ COLISIÓN FÍSICA EN LUGAR DE TRIGGER
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

    // ✅ MÉTODO PARA CONFIGURAR FÍSICA ESPECÍFICA
    public void SetupPhysics(bool restrictZ = true, float zPos = 0f)
    {
        restrictZMovement = restrictZ;
        fixedZPosition = zPos;

        if (rb == null) rb = GetComponent<Rigidbody>();

        if (rb != null && restrictZMovement)
        {
            rb.constraints = RigidbodyConstraints.FreezePositionZ |
                           RigidbodyConstraints.FreezeRotationX |
                           RigidbodyConstraints.FreezeRotationZ;
        }

        if (showDebugLogs)
            Debug.Log($"💰 Física configurada | Restrict Z: {restrictZ} | Z Position: {zPos}");
    }

    // Método para reactivar la moneda (útil para respawn)
    public void ResetCoin()
    {
        isCollected = false;

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

    // Método para establecer el valor dinámicamente
    public void SetCoinValue(int newValue)
    {
        coinValue = Mathf.Max(1, newValue);

        if (showDebugLogs)
            Debug.Log($"💰 Valor de moneda cambiado a: {coinValue}");
    }
}
