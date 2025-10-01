using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerHealth : MonoBehaviour
{
    // UI corazones (asignar en Inspector)
    public Image heart1;
    public Image heart2;
    public Image heart3;

    public Sprite fullHeart;
    public Sprite emptyHeart;

    // Vida
    int currentHealth = 3;
    int maxHealth = 3;

    // Alma (recurso para curarse)
    public float maxSoul = 100f;      // alma máxima
    public float currentSoul = 0f;    // alma actual
    [Range(0.01f, 1f)]
    public float healCostPercent = 0.5f; // porcentaje del maxSoul que cuesta 1 curación (0.5 = 50%)

    // Curación por mantener tecla
    public float healTime = 1.0f; // tiempo que hay que mantener L1 para curar 1 corazón
    private float healTimer = 0f;
    private bool isHealing = false;

    // Invencibilidad/interrupción
    private bool isInvincible = false;
    private float invincibleTime = 1.0f;
    private float invincibleTimer = 0f;

    // Variables para knockback del jugador
    [Header("Knockback Settings")]
    [Tooltip("Fuerza del knockback cuando recibe daño")]
    public float knockbackForce = 3f;

    [Tooltip("Duración del knockback")]
    public float knockbackDuration = 0.2f;

    [Header("Input System")]
    public InputActionAsset inputActions;

    [Header("Debug")]
    [Tooltip("Mostrar logs de debug")]
    public bool showDebugLogs = false;

    // ✅ ELIMINADO: debugDamageAction - Ya no existe el atajo de H
    // Input actions
    private InputAction healAction;

    // Input values
    private bool healPressed;
    private bool healHeld;

    // Variables para knockback
    private Rigidbody playerRb;
    private Vector3 knockbackVelocity = Vector3.zero;
    private float knockbackTimer = 0f;

    public int GetCurrentHealth() => currentHealth;
    public float GetCurrentSoul() => currentSoul;

    void Start()
    {
        // Obtener Rigidbody del jugador
        playerRb = GetComponent<Rigidbody>();

        // ajustar por si currentSoul > maxSoul
        if (currentSoul > maxSoul) currentSoul = maxSoul;
        UpdateHeartsUI();
        UpdateSoulUI(); // opcional (implementa esta función para mostrar barra de alma)

        // Setup Input System
        SetupInputActions();

        if (showDebugLogs)
            Debug.Log($"💚 PlayerHealth iniciado | HP: {currentHealth}/{maxHealth} | Alma: {currentSoul}/{maxSoul}");
    }

    void SetupInputActions()
    {
        // If no input asset is assigned, try to find the one in your project
        if (inputActions == null)
        {
            inputActions = Resources.Load<InputActionAsset>("InputSystem_Actions");
        }

        if (inputActions != null)
        {
            // Get actions from the Player action map
            var playerActionMap = inputActions.FindActionMap("Player");

            if (playerActionMap != null)
            {
                // Try to use the existing Interact action for healing
                healAction = playerActionMap.FindAction("Interact");
            }
        }

        // Fallback: create heal action manually if not found
        if (healAction == null)
        {
            healAction = new InputAction("Heal", InputActionType.Button);
            healAction.AddBinding("<Keyboard>/e");
            healAction.AddBinding("<Gamepad>/leftShoulder"); // L1/LB button
        }

        // ✅ ELIMINADO: No más configuración de debugDamageAction

        // Setup input callbacks
        SetupInputCallbacks();

        // Enable actions
        EnableInputActions();
    }

    void SetupInputCallbacks()
    {
        healAction.started += OnHealStarted;
        healAction.canceled += OnHealCanceled;

        // ✅ ELIMINADO: debugDamageAction.started callback
    }

    void EnableInputActions()
    {
        healAction?.Enable();
        // ✅ ELIMINADO: debugDamageAction?.Enable();
    }

    void DisableInputActions()
    {
        healAction?.Disable();
        // ✅ ELIMINADO: debugDamageAction?.Disable();
    }

    void OnDestroy()
    {
        DisableInputActions();
    }

    void OnHealStarted(InputAction.CallbackContext context)
    {
        healHeld = true;
    }

    void OnHealCanceled(InputAction.CallbackContext context)
    {
        healHeld = false;
    }

    // ✅ ELIMINADO: OnDebugDamage method - Ya no existe el atajo

    void Update()
    {
        // Procesar knockback del jugador
        UpdateKnockback();

        // temporizador de invencibilidad
        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer <= 0f)
            {
                isInvincible = false;

                if (showDebugLogs)
                    Debug.Log("🛡️ Invencibilidad terminada");
            }
        }

        // Comenzar o mantener curación con Input System
        if (healHeld)
        {
            // Solo si no estás full health y tienes alma suficiente para una curación
            if (currentHealth < maxHealth && HasEnoughSoulForOneHeal())
            {
                if (!isHealing)
                {
                    // iniciar conteo
                    isHealing = true;
                    healTimer = healTime;
                    Debug.Log("Iniciando curación... mantén L1");
                }

                // descontar tiempo manteniéndola
                if (isHealing)
                {
                    healTimer -= Time.deltaTime;

                    // Si completó el hold-time
                    if (healTimer <= 0f)
                    {
                        DoHealOneHeart();

                        // Si todavía tienes alma y sigues manteniendo, reinicia para intentar otra curación
                        if (currentHealth < maxHealth && HasEnoughSoulForOneHeal())
                        {
                            healTimer = healTime; // volver a empezar para curar siguiente corazón
                            // isHealing queda true
                        }
                        else
                        {
                            isHealing = false; // no más curación
                        }
                    }
                }
            }
            else
            {
                // No se puede iniciar curación (salud completa o alma insuficiente)
                if (isHealing)
                {
                    isHealing = false;
                }
            }
        }
        else
        {
            // Si suelta la tecla, cancelar la curación en progreso (sin consumir alma)
            if (isHealing)
            {
                isHealing = false;
                Debug.Log("Curación interrumpida (soltaste L1)");
            }
        }
    }

    // Manejar knockback del jugador
    void UpdateKnockback()
    {
        if (knockbackTimer > 0f && playerRb != null)
        {
            knockbackTimer -= Time.deltaTime;

            if (!playerRb.isKinematic)
            {
                // Aplicar drag progresivo
                Vector3 currentVel = playerRb.velocity;
                currentVel.x = Mathf.Lerp(currentVel.x, 0f, 8f * Time.deltaTime);
                currentVel.z = Mathf.Lerp(currentVel.z, 0f, 8f * Time.deltaTime);
                playerRb.velocity = currentVel;
            }

            // Detener knockback
            if (knockbackTimer <= 0f)
            {
                knockbackVelocity = Vector3.zero;
                if (playerRb != null)
                {
                    Vector3 vel = playerRb.velocity;
                    vel.x = 0f;
                    vel.z = 0f;
                    playerRb.velocity = vel; // Mantener Y para gravedad
                }

                if (showDebugLogs)
                    Debug.Log("🛑 Knockback del jugador terminado");
            }
        }
    }

    public void SetCurrentHealth(int value)
    {
        currentHealth = Mathf.Clamp(value, 0, maxHealth);
        UpdateHeartsUI();
    }

    public void SetCurrentSoul(float value)
    {
        currentSoul = Mathf.Clamp(value, 0f, maxSoul);
        UpdateSoulUI();
    }

    // Realiza la curación: consume alma y suma 1 vida (si no está a tope)
    void DoHealOneHeart()
    {
        float cost = maxSoul * healCostPercent;

        if (currentSoul >= cost && currentHealth < maxHealth)
        {
            currentSoul -= cost;
            if (currentSoul < 0f) currentSoul = 0f;

            currentHealth++;
            UpdateHeartsUI();
            UpdateSoulUI();

            Debug.Log("Curé 1 corazón. Alma restante: " + currentSoul);
        }
        else
        {
            Debug.Log("No hay alma suficiente para curar.");
            // si por alguna razón no hay alma suficiente (ej. se gastó en otra parte), cancelar
            isHealing = false;
        }
    }

    // Comprueba si tienes suficiente alma para curar 1 corazón
    bool HasEnoughSoulForOneHeal()
    {
        float cost = maxSoul * healCostPercent;
        if (currentSoul >= cost) return true;
        return false;
    }

    // Añadir alma (llámalo desde el sistema de combate o al golpear enemigos)
    public void AddSoul(float amount)
    {
        currentSoul += amount;
        if (currentSoul > maxSoul) currentSoul = maxSoul;
        UpdateSoulUI();

        if (showDebugLogs)
            Debug.Log($"💙 +{amount} alma añadida. Total: {currentSoul}/{maxSoul}");
    }

    // TakeDamage con sobrecarga para ambos casos
    public void TakeDamage(int damage)
    {
        TakeDamage(damage, Vector3.zero); // Usar versión con hitDirection por defecto
    }

    // TakeDamage con dirección del golpe
    public void TakeDamage(int damage, Vector3 hitDirection)
    {
        if (isInvincible)
        {
            if (showDebugLogs)
                Debug.Log("🛡️ Daño bloqueado por invencibilidad");
            return;
        }

        // Si estaba curando, interrumpe sin consumir alma
        if (isHealing)
        {
            isHealing = false;
            Debug.Log("Curación interrumpida por daño.");
        }

        int previousHealth = currentHealth;
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        UpdateHeartsUI();

        // Aplicar knockback si hay dirección
        if (hitDirection != Vector3.zero && playerRb != null)
        {
            ApplyKnockback(hitDirection);
        }

        // activar invencibilidad breve para evitar perder múltiples corazones de golpe
        isInvincible = true;
        invincibleTimer = invincibleTime;

        if (showDebugLogs)
            Debug.Log($"💔 DAÑO RECIBIDO: {damage} | HP: {previousHealth} → {currentHealth} | Knockback: {hitDirection != Vector3.zero}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Aplicar knockback al jugador
    void ApplyKnockback(Vector3 hitDirection)
    {
        if (playerRb == null) return;

        // Aplicar knockback inmediato
        Vector3 knockback = hitDirection.normalized * knockbackForce;
        knockback.y = 0f; // No knockback vertical

        playerRb.velocity = new Vector3(knockback.x, playerRb.velocity.y, knockback.z);
        knockbackTimer = knockbackDuration;

        if (showDebugLogs)
            Debug.Log($"👊 Knockback aplicado al jugador: {knockback}");
    }

    // Método para verificar si está en knockback
    public bool IsInKnockback()
    {
        return knockbackTimer > 0f;
    }

    // Actualizar UI de corazones (sin arrays ni ternarios)
    void UpdateHeartsUI()
    {
        if (currentHealth >= 1)
        {
            heart1.sprite = fullHeart;
        }
        else
        {
            heart1.sprite = emptyHeart;
        }

        if (currentHealth >= 2)
        {
            heart2.sprite = fullHeart;
        }
        else
        {
            heart2.sprite = emptyHeart;
        }

        if (currentHealth >= 3)
        {
            heart3.sprite = fullHeart;
        }
        else
        {
            heart3.sprite = emptyHeart;
        }
    }

    // Placeholder para actualizar UI de alma; implementa tu barra aquí o conecta la UI que quieras
    void UpdateSoulUI()
    {
        // Ejemplo de debug, puedes reemplazar con actualización de Image.fillAmount, texto, etc.
        // Debug.Log("Alma: " + currentSoul + " / " + maxSoul);
    }

    void Die()
    {
        if (showDebugLogs)
            Debug.Log("💀 Jugador ha muerto - Iniciando respawn");

        // Usar el SaveSystem para respawn
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.RespawnPlayer();
        }
        else
        {
            Debug.LogError("❌ SaveSystem no encontrado para respawn");
        }
    }

}
