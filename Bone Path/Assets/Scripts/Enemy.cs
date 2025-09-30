using UnityEngine;

[System.Serializable]
public class CoinDropSettings
{
    [Header("Coin Drop Configuration")]
    [Tooltip("¿Este enemigo suelta monedas al morir?")]
    public bool dropCoins = true;

    [Tooltip("Cantidad mínima de monedas que suelta")]
    public int minCoins = 1;

    [Tooltip("Cantidad máxima de monedas que suelta")]
    public int maxCoins = 3;

    [Tooltip("Probabilidad de soltar monedas (0-100%)")]
    [Range(0f, 100f)]
    public float dropChance = 80f;

    [Tooltip("Prefab de la moneda a instanciar")]
    public GameObject coinPrefab;

    [Tooltip("Fuerza del lanzamiento de las monedas")]
    public float dropForce = 5f;

    [Tooltip("Altura desde donde salen las monedas")]
    public float dropHeight = 1f;

    [Tooltip("Radio de dispersión de las monedas")]
    public float scatterRadius = 1.5f;
}

public class Enemy : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    protected int currentHealth;
    public bool isDead = false;

    [Header("Knockback Settings")]
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.3f;
    public float knockbackDrag = 8f; // ✅ NUEVA: Para detener el knockback

    [Header("Currency System")]
    public CoinDropSettings coinDropSettings = new CoinDropSettings();

    [Header("Debug")]
    [Tooltip("Mostrar logs de debug para este enemigo")]
    public bool showDebugLogs = false; // ✅ CAMBIADO A FALSE por defecto

    // Variables protegidas para herencia
    protected Rigidbody rb;
    protected Transform playerTransform;
    protected Vector3 knockbackVelocity = Vector3.zero;
    protected float knockbackTimer = 0f;
    protected bool originalKinematic;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();

        // Guardar estado original
        if (rb != null)
        {
            originalKinematic = rb.isKinematic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        // Cache del jugador al inicio
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        if (showDebugLogs)
            Debug.Log($"👹 ENEMIGO INICIADO: '{gameObject.name}' | HP: {maxHealth} | Tipo: {GetType().Name}");
    }

    protected virtual void FixedUpdate()
    {
        // ✅ KNOCKBACK CORREGIDO
        if (knockbackTimer > 0f && rb != null)
        {
            knockbackTimer -= Time.fixedDeltaTime;

            if (!rb.isKinematic)
            {
                // ✅ APLICAR DRAG PROGRESIVO EN LUGAR DE LERP CONFUSO
                Vector3 currentVel = rb.velocity;
                currentVel.x = Mathf.Lerp(currentVel.x, 0f, knockbackDrag * Time.fixedDeltaTime);
                currentVel.z = Mathf.Lerp(currentVel.z, 0f, knockbackDrag * Time.fixedDeltaTime);
                rb.velocity = currentVel;
            }

            // ✅ DETENER KNOCKBACK CORRECTAMENTE
            if (knockbackTimer <= 0f)
            {
                knockbackVelocity = Vector3.zero;
                if (rb != null && !isDead)
                {
                    Vector3 vel = rb.velocity;
                    vel.x = 0f;
                    vel.z = 0f;
                    rb.velocity = vel; // Mantener Y para gravedad
                }

                if (showDebugLogs)
                    Debug.Log($"🛑 {gameObject.name} - Knockback terminado");
            }
        }
    }

    public virtual void TakeDamage(int damage, Vector3 hitDirection)
    {
        if (isDead)
        {
            if (showDebugLogs)
                Debug.Log($"⚠️ {gameObject.name} ya está muerto - ignorando daño");
            return;
        }

        int previousHealth = currentHealth;
        currentHealth -= damage;

        if (showDebugLogs)
            Debug.Log($"💥 DAÑO: {gameObject.name} | {damage} dmg | HP: {previousHealth} → {currentHealth}");

        // ✅ KNOCKBACK MEJORADO
        if (!isDead && rb != null)
        {
            rb.isKinematic = false;

            // ✅ APLICAR KNOCKBACK INMEDIATO
            Vector3 knockback = hitDirection.normalized * knockbackForce;
            knockback.y = 0f; // No knockback vertical

            rb.velocity = new Vector3(knockback.x, rb.velocity.y, knockback.z);
            knockbackTimer = knockbackDuration;

            if (showDebugLogs)
                Debug.Log($"👊 {gameObject.name} - Knockback aplicado: {knockback}");
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        if (isDead) return;

        isDead = true;

        if (showDebugLogs)
            Debug.Log($"💀 MUERTE: {gameObject.name}");

        // Optimizar colliders
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            col.isTrigger = true;
        }

        // Detener física
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
        }

        knockbackVelocity = Vector3.zero;
        knockbackTimer = 0f;

        // Soltar monedas
        DropCoins();

        // ✅ DESTRUIR DESPUÉS DE UN TIEMPO
        Destroy(gameObject, 2f);
    }

    protected virtual void DropCoins()
    {
        if (!coinDropSettings.dropCoins) return;

        // Verificar probabilidad
        if (Random.Range(0f, 100f) > coinDropSettings.dropChance) return;

        // Obtener Z del jugador
        float playerZPosition = transform.position.z;
        if (playerTransform != null)
        {
            playerZPosition = playerTransform.position.z;
        }

        // Verificar CurrencySystem
        if (CurrencySystem.Instance == null)
        {
            Debug.LogError($"❌ CurrencySystem.Instance es NULL");
            return;
        }

        int coinsToDropCount = Random.Range(coinDropSettings.minCoins, coinDropSettings.maxCoins + 1);

        for (int i = 0; i < coinsToDropCount; i++)
        {
            if (coinDropSettings.coinPrefab == null)
            {
                CurrencySystem.Instance.AddCoins(1);
            }
            else
            {
                Vector3 dropPosition = new Vector3(
                    transform.position.x,
                    transform.position.y + coinDropSettings.dropHeight,
                    playerZPosition
                );

                Vector3 randomOffset = new Vector3(
                    Random.Range(-coinDropSettings.scatterRadius, coinDropSettings.scatterRadius),
                    Random.Range(0f, coinDropSettings.dropHeight * 0.3f),
                    0f
                );
                dropPosition += randomOffset;
                dropPosition.z = playerZPosition;

                GameObject droppedCoin = Instantiate(coinDropSettings.coinPrefab, dropPosition, Quaternion.identity);

                CoinPickup coinPickup = droppedCoin.GetComponent<CoinPickup>();
                if (coinPickup != null)
                {
                    coinPickup.SetupPhysics(true, playerZPosition);
                }

                Rigidbody coinRb = droppedCoin.GetComponent<Rigidbody>();
                if (coinRb != null)
                {
                    coinRb.constraints = RigidbodyConstraints.FreezePositionZ |
                                       RigidbodyConstraints.FreezeRotationX |
                                       RigidbodyConstraints.FreezeRotationZ;

                    Vector3 forceDirection;
                    int direction = Random.Range(0, 3);
                    switch (direction)
                    {
                        case 0: forceDirection = new Vector3(-1f, 0.5f, 0f); break;
                        case 1: forceDirection = new Vector3(1f, 0.5f, 0f); break;
                        default: forceDirection = new Vector3(Random.Range(-0.3f, 0.3f), 1f, 0f); break;
                    }

                    Vector3 finalForce = forceDirection.normalized * coinDropSettings.dropForce;
                    coinRb.AddForce(finalForce, ForceMode.Impulse);
                }
            }
        }
    }

    // Métodos públicos
    public bool IsInKnockback() => knockbackTimer > 0f;
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public float GetHealthPercentage() => maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
}
