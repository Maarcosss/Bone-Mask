using UnityEngine;
using UnityEngine.InputSystem;

public class CursorManager : MonoBehaviour
{
    [Header("Configuración del Cursor")]
    [Tooltip("Sensibilidad para detectar movimiento del mouse")]
    public float mouseSensitivity = 0.1f;

    [Tooltip("Mostrar logs de debugging")]
    public bool showDebugLogs = false;

    [Header("Estados del Cursor")]
    [Tooltip("Estado actual del input")]
    public bool isUsingController = false;

    // Singleton para acceso global
    public static CursorManager Instance { get; private set; }

    // Estado anterior para detectar cambios
    private bool wasUsingController = false;

    private void Awake()
    {
        // Implementar Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Configuración inicial del cursor
        ActualizarCursor();

        if (showDebugLogs)
            Debug.Log("🖱️ CursorManager iniciado correctamente");
    }

    private void Update()
    {
        DetectarTipoDeInput();

        // Solo actualizar si cambió el estado
        if (wasUsingController != isUsingController)
        {
            ActualizarCursor();
            wasUsingController = isUsingController;
        }
    }

    void DetectarTipoDeInput()
    {
        // Detectar movimiento del mouse
        if (Mouse.current != null && Mouse.current.delta.ReadValue().magnitude > mouseSensitivity)
        {
            CambiarAMouse("Movimiento del mouse detectado");
            return;
        }

        // Detectar clicks del mouse
        if (Mouse.current != null &&
            (Mouse.current.leftButton.wasPressedThisFrame ||
             Mouse.current.rightButton.wasPressedThisFrame ||
             Mouse.current.middleButton.wasPressedThisFrame))
        {
            CambiarAMouse("Click del mouse detectado");
            return;
        }

        // Detectar scroll del mouse
        if (Mouse.current != null && Mouse.current.scroll.ReadValue().magnitude > 0.1f)
        {
            CambiarAMouse("Scroll del mouse detectado");
            return;
        }

        // Detectar input del controlador
        if (Gamepad.current != null)
        {
            // Detectar movimiento de sticks
            if (Gamepad.current.leftStick.ReadValue().magnitude > 0.1f ||
                Gamepad.current.rightStick.ReadValue().magnitude > 0.1f)
            {
                CambiarAControlador("Movimiento de stick detectado");
                return;
            }

            // Detectar D-pad
            Vector2 dpad = Gamepad.current.dpad.ReadValue();
            if (dpad.magnitude > 0.1f)
            {
                CambiarAControlador("D-pad detectado");
                return;
            }

            // Detectar botones del controlador
            if (Gamepad.current.buttonSouth.wasPressedThisFrame ||
                Gamepad.current.buttonEast.wasPressedThisFrame ||
                Gamepad.current.buttonWest.wasPressedThisFrame ||
                Gamepad.current.buttonNorth.wasPressedThisFrame)
            {
                CambiarAControlador("Botón de controlador detectado");
                return;
            }

            // Detectar triggers y shoulders
            if (Gamepad.current.leftTrigger.ReadValue() > 0.1f ||
                Gamepad.current.rightTrigger.ReadValue() > 0.1f ||
                Gamepad.current.leftShoulder.wasPressedThisFrame ||
                Gamepad.current.rightShoulder.wasPressedThisFrame)
            {
                CambiarAControlador("Trigger/Shoulder detectado");
                return;
            }
        }

        // Detectar teclado (cambiar a mouse ya que va junto)
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            CambiarAMouse("Tecla presionada");
            return;
        }
    }

    void CambiarAMouse(string razon)
    {
        if (isUsingController)
        {
            isUsingController = false;
            if (showDebugLogs)
                Debug.Log($"🖱️ Cambiando a MOUSE: {razon}");
        }
    }

    void CambiarAControlador(string razon)
    {
        if (!isUsingController)
        {
            isUsingController = true;
            if (showDebugLogs)
                Debug.Log($"🎮 Cambiando a CONTROLADOR: {razon}");
        }
    }

    void ActualizarCursor()
    {
        if (isUsingController)
        {
            // Ocultar cursor cuando se usa controlador
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            if (showDebugLogs)
                Debug.Log("🎮 Cursor OCULTO - Modo controlador");
        }
        else
        {
            // Mostrar cursor cuando se usa mouse/teclado
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            if (showDebugLogs)
                Debug.Log("🖱️ Cursor VISIBLE - Modo mouse");
        }
    }

    // Métodos públicos para control manual
    public void ForzarModoMouse()
    {
        isUsingController = false;
        ActualizarCursor();
        if (showDebugLogs)
            Debug.Log("🖱️ Modo mouse FORZADO");
    }

    public void ForzarModoControlador()
    {
        isUsingController = true;
        ActualizarCursor();
        if (showDebugLogs)
            Debug.Log("🎮 Modo controlador FORZADO");
    }

    public bool EstaUsandoControlador()
    {
        return isUsingController;
    }
}
