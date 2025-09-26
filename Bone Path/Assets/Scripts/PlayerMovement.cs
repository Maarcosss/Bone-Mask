using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Referencias")]
    public ManagerOptions ManagerOptionsRef;

    Rigidbody rb;

    [Header("Movimiento")]
    public float Speed = 6f;
    public float JumpForce = 12f;
    public float airControlMultiplier = 0.8f; // Control en el aire

    [Header("Gravedad y Física")]
    public float extraGravity = 25f;
    public float shortJumpMultiplier = 2.5f;
    public float maxJumpTime = 0.2f;
    private float jumpTimeCounter;

    [Header("Doble salto")]
    public int maxJumps = 2;
    private int currentJumps;

    [Header("Estados")]
    public bool validar_inputs = true;
    public bool validar_inputs_esc = true;
    private bool isGrounded = false;
    private bool isJumping = false;
    private bool jumpBuffered = false;
    private float jumpBufferTime = 0.1f;
    private float jumpBufferTimer = 0f;

    [Header("Input System")]
    public InputActionAsset inputActions;

    // Input actions
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction pauseAction;

    // Input values
    private Vector2 moveInput;
    private bool jumpPressed;
    private bool jumpHeld;

    void Start()
    {
        // 🚀 QUITAR LÍMITE DE FPS - USAR FÍSICA APROPIADA
        // Application.targetFrameRate = -1; // Sin límite
        // QualitySettings.vSyncCount = 0;   // Deshabilitar VSync si es necesario

        Time.timeScale = 1.0f;
        rb = GetComponent<Rigidbody>();
        currentJumps = maxJumps;

        // 🔧 OPTIMIZAR RIGIDBODY PARA MOVIMIENTO CONSISTENTE
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.freezeRotation = true; // Evitar rotación no deseada

        SetupInputActions();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void SetupInputActions()
    {
        if (inputActions == null)
        {
            inputActions = Resources.Load<InputActionAsset>("InputSystem_Actions");
        }

        if (inputActions != null)
        {
            var playerActionMap = inputActions.FindActionMap("Player");

            if (playerActionMap != null)
            {
                moveAction = playerActionMap.FindAction("Move");
                jumpAction = playerActionMap.FindAction("Jump");
            }
        }

        // Fallback: crear acciones manualmente
        if (moveAction == null)
        {
            moveAction = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            moveAction.AddBinding("<Gamepad>/leftStick");
        }

        if (jumpAction == null)
        {
            jumpAction = new InputAction("Jump", InputActionType.Button);
            jumpAction.AddBinding("<Keyboard>/space");
            jumpAction.AddBinding("<Gamepad>/buttonSouth");
        }

        if (pauseAction == null)
        {
            pauseAction = new InputAction("Pause", InputActionType.Button);
            pauseAction.AddBinding("<Keyboard>/escape");
            pauseAction.AddBinding("<Gamepad>/start");
        }

        SetupInputCallbacks();
        EnableInputActions();
    }

    void SetupInputCallbacks()
    {
        moveAction.performed += OnMove;
        moveAction.canceled += OnMove;

        jumpAction.started += OnJumpStarted;
        jumpAction.canceled += OnJumpCanceled;

        pauseAction.started += OnPause;
    }

    void EnableInputActions()
    {
        moveAction?.Enable();
        jumpAction?.Enable();
        pauseAction?.Enable();
    }

    void DisableInputActions()
    {
        moveAction?.Disable();
        jumpAction?.Disable();
        pauseAction?.Disable();
    }

    void OnDestroy()
    {
        DisableInputActions();
    }

    void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    void OnJumpStarted(InputAction.CallbackContext context)
    {
        jumpPressed = true;
        jumpHeld = true;
        jumpBuffered = true;
        jumpBufferTimer = jumpBufferTime;
    }

    void OnJumpCanceled(InputAction.CallbackContext context)
    {
        jumpHeld = false;
    }

    void OnPause(InputAction.CallbackContext context)
    {
        DetectarEscape();
    }

    void Update()
    {
        // 🚀 SOLO LÓGICA DE INPUT, NO FÍSICA
        if (Time.timeScale == 0f) return;

        // Jump buffer timer
        if (jumpBufferTimer > 0f)
        {
            jumpBufferTimer -= Time.deltaTime;
            if (jumpBufferTimer <= 0f)
            {
                jumpBuffered = false;
            }
        }

        // Reset jump pressed flag
        jumpPressed = false;
    }

    void FixedUpdate()
    {
        // 🚀 TODA LA FÍSICA EN FIXEDUPDATE PARA CONSISTENCIA
        if (Time.timeScale == 0f) return;

        if (validar_inputs)
        {
            HandleMovement();
            HandleJumping();
        }

        ApplyExtraGravity();
    }

    void HandleMovement()
    {
        // 🔧 MOVIMIENTO FRAME-RATE INDEPENDENT
        float horizontalInput = moveInput.x;
        float targetSpeed = horizontalInput * Speed;

        // Aplicar control de aire
        if (!isGrounded)
        {
            targetSpeed *= airControlMultiplier;
        }

        // ✅ CAMBIAR DIRECCIÓN DEL PERSONAJE CON ROTACIÓN (NO ESCALADO)
        if (Mathf.Abs(horizontalInput) > 0.1f)
        {
            if (horizontalInput < 0)
            {
                transform.rotation = Quaternion.Euler(0, 180, 0); // Izquierda
            }
            else if (horizontalInput > 0)
            {
                transform.rotation = Quaternion.Euler(0, 0, 0);   // Derecha
            }
        }

        // Aplicar movimiento suave
        Vector3 targetVelocity = new Vector3(targetSpeed, rb.velocity.y, rb.velocity.z);
        rb.velocity = Vector3.Lerp(rb.velocity, targetVelocity, Time.fixedDeltaTime * 15f);
    }

    void HandleJumping()
    {
        // Jump buffering - permite saltar justo antes de tocar el suelo
        bool shouldJump = (jumpPressed || jumpBuffered) && currentJumps > 0;

        if (shouldJump)
        {
            // Primer salto solo si está en el suelo (o muy cerca)
            if (currentJumps == maxJumps && !isGrounded)
            {
                return;
            }

            PerformJump();
        }

        // Salto variable (mantener espacio para saltar más alto)
        if (jumpHeld && isJumping && jumpTimeCounter > 0f)
        {
            jumpTimeCounter -= Time.fixedDeltaTime;
            rb.velocity = new Vector3(rb.velocity.x, JumpForce, rb.velocity.z);
        }
        else
        {
            isJumping = false;
        }
    }

    void PerformJump()
    {
        isJumping = true;
        jumpTimeCounter = maxJumpTime;
        jumpBuffered = false;
        jumpBufferTimer = 0f;

        rb.velocity = new Vector3(rb.velocity.x, JumpForce, rb.velocity.z);
        currentJumps--;

        if (currentJumps == maxJumps - 1)
        {
            isGrounded = false;
        }
    }

    void ApplyExtraGravity()
    {
        // 🔧 GRAVEDAD CONSISTENTE
        rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);

        // Caída más rápida si se suelta el salto
        if (rb.velocity.y > 0 && !jumpHeld)
        {
            rb.AddForce(Vector3.down * extraGravity * (shortJumpMultiplier - 1), ForceMode.Acceleration);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Suelo"))
        {
            isGrounded = true;
            isJumping = false;
            currentJumps = maxJumps;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Suelo"))
        {
            isGrounded = false;
        }
    }

    void DetectarEscape()
    {
        if (!validar_inputs_esc)
        {
            Debug.Log("🚫 ESC bloqueado - En diálogo o estado especial");
            return;
        }

        if (!ManagerOptionsRef.insideSubmenu)
        {
            bool isPanelCurrentlyActive = ManagerOptionsRef.PausePanel.activeInHierarchy;

            if (!isPanelCurrentlyActive)
            {
                ManagerOptionsRef.Pause();
            }
            else
            {
                ManagerOptionsRef.Continue();
            }
        }
    }

    // ✅ MÉTODOS PÚBLICOS PARA CONTROL EXTERNO DE ESC
    public void DeshabilitarEsc()
    {
        pauseAction?.Disable();
        Debug.Log("🔒 Acción de ESC deshabilitada");
    }

    public void HabilitarEsc()
    {
        pauseAction?.Enable();
        Debug.Log("🔓 Acción de ESC habilitada");
    }

    public bool EstaEscHabilitado()
    {
        return pauseAction != null && pauseAction.enabled;
    }
}
