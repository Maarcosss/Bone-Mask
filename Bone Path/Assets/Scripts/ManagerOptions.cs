using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;


public class ManagerOptions : MonoBehaviour
{
    [Header("Referencias de Jugador")]
    public PlayerMovement PlayerMovementRef;

    [Header("Paneles de UI")]
    public GameObject PausePanel;
    public GameObject OptionsPausePanel;
    public GameObject QuitToMenuPausePanel;

    [Header("Primeras Selecciones para Controlador")]
    [Tooltip("Primer botón seleccionado cuando se abre el menú de pausa")]
    public Button firstSelectedPauseButton;
    [Tooltip("Primer botón seleccionado cuando se abre el menú de opciones")]
    public Button firstSelectedOptionsButton;
    [Tooltip("Primer botón seleccionado cuando se abre el menú de salir")]
    public Button firstSelectedQuitButton;

    [Header("Configuración de Input")]
    public InputActionAsset inputActions;

    [HideInInspector] public bool insideSubmenu = false;

    // Input System
    private InputAction navigateAction;
    private InputAction submitAction;
    private InputAction cancelAction;

    // Control de navegación
    private bool isUsingController = false;
    private EventSystem eventSystem;

    void Start()
    {
        // Obtener referencia al EventSystem
        eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            Debug.LogError("No se encontró EventSystem en la escena. Agrega un EventSystem para navegación con controlador.");
        }

        // Configurar Input System
        ConfigurarInputSystem();

        // Cerrar paneles secundarios al inicio
        OptionsPausePanel.SetActive(false);
        QuitToMenuPausePanel.SetActive(false);

        // Ocultar cursor al inicio
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void ConfigurarInputSystem()
    {
        if (inputActions == null)
        {
            inputActions = Resources.Load<InputActionAsset>("InputSystem_Actions");
        }

        if (inputActions != null)
        {
            var uiActionMap = inputActions.FindActionMap("UI");
            if (uiActionMap != null)
            {
                navigateAction = uiActionMap.FindAction("Navigate");
                submitAction = uiActionMap.FindAction("Submit");
                cancelAction = uiActionMap.FindAction("Cancel");
            }
        }

        // Crear acciones manualmente si no se encuentran
        if (navigateAction == null)
        {
            navigateAction = new InputAction("Navigate", InputActionType.Value, expectedControlType: "Vector2");
            navigateAction.AddBinding("<Gamepad>/leftStick");
            navigateAction.AddBinding("<Gamepad>/dpad");
        }

        if (submitAction == null)
        {
            submitAction = new InputAction("Submit", InputActionType.Button);
            submitAction.AddBinding("<Gamepad>/buttonSouth"); // A button
            submitAction.AddBinding("<Keyboard>/enter");
        }

        if (cancelAction == null)
        {
            cancelAction = new InputAction("Cancel", InputActionType.Button);
            cancelAction.AddBinding("<Gamepad>/buttonEast"); // B button
            cancelAction.AddBinding("<Keyboard>/escape");
        }

        // Configurar callbacks
        ConfigurarCallbacks();
        HabilitarAcciones();
    }

    void ConfigurarCallbacks()
    {
        navigateAction.performed += OnNavigate;
        submitAction.performed += OnSubmit;
        cancelAction.performed += OnCancel;
    }

    void HabilitarAcciones()
    {
        navigateAction?.Enable();
        submitAction?.Enable();
        cancelAction?.Enable();
    }

    void DeshabilitarAcciones()
    {
        navigateAction?.Disable();
        submitAction?.Disable();
        cancelAction?.Disable();
    }

    void OnDestroy()
    {
        DeshabilitarAcciones();
    }

    void OnNavigate(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        if (input.magnitude > 0.1f)
        {
            isUsingController = true;
            MostrarCursorSegunInput();
        }
    }

    void OnSubmit(InputAction.CallbackContext context)
    {
        // Detectar si el input viene del controlador
        if (context.control.device is Gamepad)
        {
            isUsingController = true;
            MostrarCursorSegunInput();
        }
    }

    void OnCancel(InputAction.CallbackContext context)
    {
        // Detectar si el input viene del controlador
        if (context.control.device is Gamepad)
        {
            isUsingController = true;
            MostrarCursorSegunInput();
        }
    }

    void Update()
    {
        // Detectar movimiento del mouse con Input System
        if (Mouse.current != null && Mouse.current.delta.ReadValue().magnitude > 0.1f)
        {
            isUsingController = false;
            MostrarCursorSegunInput();
        }

        // Detectar clicks del mouse
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            isUsingController = false;
            MostrarCursorSegunInput();
        }

        // Detectar input del controlador para cambiar a modo controlador
        if (Gamepad.current != null)
        {
            // Detectar movimiento de sticks
            if (Gamepad.current.leftStick.ReadValue().magnitude > 0.1f ||
                Gamepad.current.rightStick.ReadValue().magnitude > 0.1f)
            {
                isUsingController = true;
                MostrarCursorSegunInput();
            }

            // Detectar botones del controlador
            if (Gamepad.current.buttonSouth.wasPressedThisFrame ||
                Gamepad.current.buttonEast.wasPressedThisFrame ||
                Gamepad.current.buttonWest.wasPressedThisFrame ||
                Gamepad.current.buttonNorth.wasPressedThisFrame)
            {
                isUsingController = true;
                MostrarCursorSegunInput();
            }
        }

        // Detectar teclado (cambiar a mouse ya que va junto)
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            isUsingController = false;
            MostrarCursorSegunInput();
        }
    }


    void MostrarCursorSegunInput()
    {
        if (PausePanel.activeInHierarchy || insideSubmenu)
        {
            if (isUsingController)
            {
                // Ocultar cursor cuando se usa controlador
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                // Mostrar cursor cuando se usa mouse
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }

    void EstablecerPrimeraSeleccion(Button botonSeleccionado)
    {
        if (eventSystem != null && botonSeleccionado != null)
        {
            // Limpiar selección actual
            eventSystem.SetSelectedGameObject(null);

            // Establecer nueva selección después de un frame
            StartCoroutine(SeleccionarDespuesDeFrame(botonSeleccionado.gameObject));
        }
    }

    IEnumerator SeleccionarDespuesDeFrame(GameObject objetoASeleccionar)
    {
        yield return null; // Esperar un frame

        if (eventSystem != null && objetoASeleccionar != null)
        {
            eventSystem.SetSelectedGameObject(objetoASeleccionar);
            Debug.Log($"Seleccionado para controlador: {objetoASeleccionar.name}");
        }
    }

    public void Pause()
    {
        PausePanel.SetActive(true);
        PlayerMovementRef.validar_inputs = false;
        Time.timeScale = 0f;

        // Establecer primera selección para controlador
        EstablecerPrimeraSeleccion(firstSelectedPauseButton);

        // Mostrar cursor según tipo de input
        MostrarCursorSegunInput();
    }

    public void Continue()
    {
        PausePanel.SetActive(false);
        PlayerMovementRef.validar_inputs = true;
        Time.timeScale = 1f;

        // Limpiar selección
        if (eventSystem != null)
        {
            eventSystem.SetSelectedGameObject(null);
        }

        // Aggressively hide cursor - multiple attempts
        StartCoroutine(AggressiveHideCursor());
    }

    private IEnumerator AggressiveHideCursor()
    {
        Debug.Log("Starting aggressive cursor hide");

        // Immediate hide
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Wait and hide again multiple times
        for (int i = 0; i < 10; i++)
        {
            yield return null; // Wait one frame
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            Debug.Log($"Aggressive hide attempt {i + 1}");
        }

        // Final attempts with different timing
        yield return new WaitForEndOfFrame();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        yield return new WaitForSeconds(0.1f);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        Debug.Log($"Final cursor state - Visible: {Cursor.visible}, LockState: {Cursor.lockState}");
    }

    public void OptionsPause()
    {
        PausePanel.SetActive(false);
        OptionsPausePanel.SetActive(true);
        insideSubmenu = true;

        // Establecer primera selección para controlador
        EstablecerPrimeraSeleccion(firstSelectedOptionsButton);

        // Mostrar cursor según tipo de input
        MostrarCursorSegunInput();
    }

    public void BackOptionsPause()
    {
        OptionsPausePanel.SetActive(false);
        PausePanel.SetActive(true);
        insideSubmenu = false;

        // Volver a establecer selección del menú principal
        EstablecerPrimeraSeleccion(firstSelectedPauseButton);

        // Mostrar cursor según tipo de input
        MostrarCursorSegunInput();
    }

    public void QuitToMenuPause()
    {
        PausePanel.SetActive(false);
        QuitToMenuPausePanel.SetActive(true);
        insideSubmenu = true;

        // Establecer primera selección para controlador
        EstablecerPrimeraSeleccion(firstSelectedQuitButton);

        // Mostrar cursor según tipo de input
        MostrarCursorSegunInput();
    }

    public void YesQuitToMenu()
    {
        AudioManager.instance.RefreshSlidersAndTexts();
        SceneManager.LoadScene(0);
    }

    public void NoQuitToMenu()
    {
        QuitToMenuPausePanel.SetActive(false);
        PausePanel.SetActive(true);
        insideSubmenu = false;

        // Volver a establecer selección del menú principal
        EstablecerPrimeraSeleccion(firstSelectedPauseButton);

        // Mostrar cursor según tipo de input
        MostrarCursorSegunInput();
    }

    // Métodos públicos para configuración externa
    public void CambiarPrimeraSeleccionPausa(Button nuevoBoton)
    {
        firstSelectedPauseButton = nuevoBoton;
    }

    public void CambiarPrimeraSeleccionOpciones(Button nuevoBoton)
    {
        firstSelectedOptionsButton = nuevoBoton;
    }

    public void CambiarPrimeraSeleccionSalir(Button nuevoBoton)
    {
        firstSelectedQuitButton = nuevoBoton;
    }
}
