using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Referencias")]
    public ManagerOptions ManagerOptionsRef;

    Rigidbody mask_child;

    [Header("Movimiento")]
    public float Speed = 1f;
    public float JumpForce = 7f;

    [Header("Gravedad")]
    public float extraGravity = 20f;        // fuerza extra hacia abajo
    public float shortJumpMultiplier = 2f;  // salto corto si sueltas espacio
    public float maxJumpTime = 0.25f;       // tiempo máximo manteniendo salto
    float jumpTimeCounter;

    [Header("Doble salto")]
    public int maxJumps = 2;
    int currentJumps;

    [Header("Estados")]
    public bool validar_inputs = true;
    public bool validar_inputs_esc = true;
    bool Contacto_Suelo = false;
    bool isJumping = false;

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
        Time.timeScale = 1.0f;
        mask_child = GetComponent<Rigidbody>();
        currentJumps = maxJumps;

        // Setup Input System
        SetupInputActions();

        // Ocultar cursor al iniciar el juego
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
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
                moveAction = playerActionMap.FindAction("Move");
                jumpAction = playerActionMap.FindAction("Jump");

                // Create a pause action (you might need to add this to your Input Actions)
                pauseAction = new InputAction("Pause", InputActionType.Button);
                pauseAction.AddBinding("<Keyboard>/escape");
                pauseAction.AddBinding("<Gamepad>/start");
            }
        }

        // Fallback: create actions manually if asset is not found
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

        // Setup input callbacks
        SetupInputCallbacks();

        // Enable actions
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
        // Solo si se permiten inputs normales
        if (validar_inputs)
        {
            DetectarMovimientoYSaltos();
        }

        // Reset jump pressed flag
        jumpPressed = false;

        // Aplicar gravedad extra todo el tiempo
        mask_child.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);

        // Salto corto si sueltas el botón de salto antes de tiempo
        if (mask_child.velocity.y > 0 && !jumpHeld)
        {
            mask_child.velocity += Vector3.up * Physics.gravity.y * (shortJumpMultiplier - 1) * Time.deltaTime;
        }
    }

    void DetectarEscape()
    {
        // Si no estamos dentro de un submenú
        if (!ManagerOptionsRef.insideSubmenu)
        {
            bool isPanelCurrentlyActive = ManagerOptionsRef.PausePanel.activeInHierarchy;

            Debug.Log($"ESC Pressed - Panel was: {isPanelCurrentlyActive}");

            if (!isPanelCurrentlyActive) // Panel va a activarse (pausar)
            {
                // Use the ManagerOptions.Pause() method
                ManagerOptionsRef.Pause();
            }
            else // Panel va a desactivarse (despausar)
            {
                // Use the SAME method as the Continue button
                ManagerOptionsRef.Continue();
            }
        }
    }



    // Add this new coroutine to your PlayerMovement class
    private IEnumerator ForceHideCursor()
    {
        // Wait a frame to ensure all other scripts have finished their updates
        yield return null;

        // Force hide cursor multiple times to ensure it sticks
        for (int i = 0; i < 3; i++)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            Debug.Log($"Force hiding cursor attempt {i + 1}");
            yield return new WaitForEndOfFrame();
        }

        Debug.Log($"Final cursor state - Visible: {Cursor.visible}, LockState: {Cursor.lockState}");
    }


    void DetectarMovimientoYSaltos()
    {
        // Salto (normal + doble salto)
        if (jumpPressed && currentJumps > 0)
        {
            isJumping = true;
            jumpTimeCounter = maxJumpTime;
            mask_child.velocity = new Vector3(mask_child.velocity.x, JumpForce, mask_child.velocity.z);

            currentJumps--; // gasta un salto
            Contacto_Suelo = false;
        }

        // Mantener salto más tiempo
        if (jumpHeld && isJumping)
        {
            if (jumpTimeCounter > 0)
            {
                mask_child.velocity = new Vector3(mask_child.velocity.x, JumpForce, mask_child.velocity.z);
                jumpTimeCounter -= Time.deltaTime;
            }
        }

        // Soltar botón de salto interrumpe salto
        if (!jumpHeld)
        {
            isJumping = false;
        }

        // Movimiento lateral usando Input System
        float horizontalInput = moveInput.x;

        // Si ambos lados están presionados (o no hay input), detener
        if (Mathf.Approximately(horizontalInput, 0f))
        {
            mask_child.velocity = new Vector3(0f, mask_child.velocity.y, mask_child.velocity.z);
            return;
        }

        // Movimiento hacia la izquierda
        if (horizontalInput < 0)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
            mask_child.velocity = new Vector3(-Speed, mask_child.velocity.y, 0f);
            return;
        }

        // Movimiento hacia la derecha
        if (horizontalInput > 0)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            mask_child.velocity = new Vector3(Speed, mask_child.velocity.y, 0f);
            return;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Suelo"))
        {
            Contacto_Suelo = true;
            isJumping = false;
            currentJumps = maxJumps; // resetear saltos al tocar suelo
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Suelo"))
        {
            Contacto_Suelo = false;
        }
    }
}
