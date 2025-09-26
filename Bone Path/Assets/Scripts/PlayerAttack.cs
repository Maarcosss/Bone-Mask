using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Configuración de Ataque")]
    public int damage = 1;
    public GameObject attackHitbox; // Asignar el hijo AttackHitbox
    public float attackDuration = 0.2f; // Tiempo que dura activo

    [Header("Sistema de Input")]
    public InputActionAsset inputActions;

    // Estados de ataque
    private bool isAttacking = false;
    private float attackTimer = 0f;

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

            // Gatillo derecho del controlador (input principal)
            attackAction.AddBinding("<Gamepad>/rightTrigger");

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
        // ✅ VÁLIDO: Ya usa Time.deltaTime correctamente
        if (attackPressed && !isAttacking)
        {
            EjecutarAtaque();
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

        Debug.Log("¡Ataque ejecutado!");
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

        Debug.Log("Ataque finalizado");
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
}
