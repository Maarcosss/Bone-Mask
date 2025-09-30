using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Configuración de Ataque")]
    public int damage = 1;
    public GameObject attackHitbox; // Asignar el hijo AttackHitbox
    public float attackDuration = 0.2f; // Tiempo que dura activo

    [Header("Cooldown de Ataque")]
    [Tooltip("Tiempo mínimo entre ataques (en segundos)")]
    public float attackCooldown = 0.5f; // ← NUEVO: Cooldown entre ataques

    [Header("Sistema de Input")]
    public InputActionAsset inputActions;

    [Header("Debug")]
    [Tooltip("Mostrar logs de debug")]
    public bool showDebugLogs = false;

    // Estados de ataque
    private bool isAttacking = false;
    private float attackTimer = 0f;
    private float cooldownTimer = 0f; // ← NUEVO: Timer del cooldown

    // Input System
    private InputAction attackAction;
    private bool attackPressed = false;

    void Start()
    {
        ConfigurarAccionesInput();
    }

    void ConfigurarAccionesInput()
    {
        // Si no hay asset asignado, intentar encontrar el del proyecto
        if (inputActions == null)
        {
            inputActions = Resources.Load<InputActionAsset>("InputSystem_Actions");
        }

        if (inputActions != null)
        {
            // Obtener acciones del mapa de acciones Player
            var playerActionMap = inputActions.FindActionMap("Player");

            if (playerActionMap != null)
            {
                attackAction = playerActionMap.FindAction("Attack");
            }
        }

        // Crear acción de ataque manualmente si no se encuentra en el asset
        if (attackAction == null)
        {
            attackAction = new InputAction("Attack", InputActionType.Button);

            // Cuadrado/X en lugar de gatillo derecho
            attackAction.AddBinding("<Gamepad>/buttonWest"); // Cuadrado en PlayStation / X en Xbox

            // Fallback: click izquierdo del mouse
            attackAction.AddBinding("<Mouse>/leftButton");

            // Fallback adicional: tecla Z
            attackAction.AddBinding("<Keyboard>/z");
        }

        // Configurar callbacks de input
        ConfigurarCallbacksInput();

        // Habilitar acciones
        HabilitarAccionesInput();
    }

    void ConfigurarCallbacksInput()
    {
        attackAction.started += AlPresionarAtaque;
    }

    void HabilitarAccionesInput()
    {
        attackAction?.Enable();
    }

    void DeshabilitarAccionesInput()
    {
        attackAction?.Disable();
    }

    void OnDestroy()
    {
        DeshabilitarAccionesInput();
    }

    void AlPresionarAtaque(InputAction.CallbackContext context)
    {
        attackPressed = true;
    }

    void Update()
    {
        // ✅ NUEVO: Actualizar cooldown timer
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }

        // ✅ MODIFICADO: Verificar cooldown antes de atacar
        if (attackPressed && !isAttacking && cooldownTimer <= 0f)
        {
            EjecutarAtaque();
        }
        else if (attackPressed && cooldownTimer > 0f)
        {
            // ✅ NUEVO: Feedback cuando está en cooldown
            if (showDebugLogs)
                Debug.Log($"⏱️ Ataque en cooldown: {cooldownTimer:F2}s restantes");
        }

        attackPressed = false;

        if (isAttacking)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                FinalizarAtaque();
            }
        }
    }

    void EjecutarAtaque()
    {
        isAttacking = true;
        attackHitbox.SetActive(true);

        // Activar el hitbox
        AttackHitbox hitboxComponent = attackHitbox.GetComponent<AttackHitbox>();
        if (hitboxComponent != null)
        {
            hitboxComponent.Activate();
        }

        attackTimer = attackDuration;

        // ✅ NUEVO: Activar cooldown
        cooldownTimer = attackCooldown;

        if (showDebugLogs)
            Debug.Log($"⚔️ Ataque ejecutado | Duración: {attackDuration}s | Cooldown: {attackCooldown}s");
    }

    void FinalizarAtaque()
    {
        attackHitbox.SetActive(false);
        isAttacking = false;

        // Desactivar el hitbox
        AttackHitbox hitboxComponent = attackHitbox.GetComponent<AttackHitbox>();
        if (hitboxComponent != null)
        {
            hitboxComponent.Deactivate();
        }

        if (showDebugLogs)
            Debug.Log("⚔️ Ataque finalizado");
    }

    // ✅ NUEVO: Método público para verificar si está en cooldown
    public bool IsInCooldown()
    {
        return cooldownTimer > 0f;
    }

    // ✅ NUEVO: Método público para obtener tiempo restante de cooldown
    public float GetCooldownRemaining()
    {
        return Mathf.Max(0f, cooldownTimer);
    }

    // ✅ NUEVO: Método público para obtener progreso del cooldown (0-1)
    public float GetCooldownProgress()
    {
        if (attackCooldown <= 0f) return 1f;
        return 1f - (cooldownTimer / attackCooldown);
    }

    // Métodos públicos para control externo
    public bool EstaAtacando()
    {
        return isAttacking;
    }

    public void CancelarAtaque()
    {
        if (isAttacking)
        {
            FinalizarAtaque();
        }
    }

    // ✅ NUEVO: Método para resetear cooldown (para testing o power-ups)
    public void ResetCooldown()
    {
        cooldownTimer = 0f;
        if (showDebugLogs)
            Debug.Log("⚔️ Cooldown reseteado");
    }

    // ✅ NUEVO: Método para forzar un ataque inmediato (ignora cooldown)
    public void ForceAttack()
    {
        if (!isAttacking)
        {
            EjecutarAtaque();
            if (showDebugLogs)
                Debug.Log("⚔️ Ataque forzado (ignorando cooldown)");
        }
    }

    // ✅ NUEVO: Información del sistema de ataque
    public string GetAttackInfo()
    {
        return $"Ataque | Atacando: {isAttacking} | Cooldown: {cooldownTimer:F2}s/{attackCooldown}s | Progreso: {GetCooldownProgress():P}";
    }
}
