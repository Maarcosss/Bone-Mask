using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu_Manager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject MainMenuPanel;
    public GameObject OptionsMainMenuPanel;
    public GameObject ExtrasPanel;
    public GameObject GameOptionsPanel;
    public GameObject BrightnessGameOptionsPanel;
    public GameObject AudioPanel;
    public GameObject ControllerPanel;
    public GameObject QuitGamePanel;

    [Header("First Selection para Controlador")]
    [Tooltip("Primer botón seleccionado en el menú principal")]
    public Button firstSelectedMainMenu;
    [Tooltip("Primer botón seleccionado en opciones principales")]
    public Button firstSelectedOptionsMain;
    [Tooltip("Primer botón seleccionado en extras")]
    public Button firstSelectedExtras;
    [Tooltip("Primer botón seleccionado en opciones de juego")]
    public Button firstSelectedGameOptions;
    [Tooltip("Primer slider seleccionado en brillo")]
    public Slider firstSelectedBrightness;
    [Tooltip("Primer slider seleccionado en audio")]
    public Slider firstSelectedAudio;
    [Tooltip("Primer botón seleccionado en controlador")]
    public Button firstSelectedController;
    [Tooltip("Primer botón seleccionado en salir del juego")]
    public Button firstSelectedQuitGame;

    [Header("Graphics Settings")]
    [SerializeField] private Slider brightnessSlider = null;
    [SerializeField] private TMP_Text brightnessTextValue = null;
    [SerializeField] private float defaultBrightness = 1f;

    [Header("Brightness Preview")]
    [SerializeField] private Image brightnessPreviewImage = null;

    [Header("Brightness Render")]
    [SerializeField] private BrightnessManager brightnessManager = null;

    [Header("Resolutions Dropdown")]
    public TMP_Dropdown resolutionDropdown;
    private Resolution[] resolutions;

    [Header("Configuración de Input")]
    public InputActionAsset inputActions;

    [HideInInspector] public bool insideSubmenu = false;

    private bool _isFullScreen;
    private float _brightnessLevel;
    private EventSystem eventSystem;
    private bool isUsingController = false;

    // Sistema de memoria de botones por panel
    private Dictionary<GameObject, Selectable> ultimaSeleccionPorPanel = new Dictionary<GameObject, Selectable>();

    // Input System
    private InputAction navigateAction;
    private InputAction submitAction;
    private InputAction cancelAction;

    private void Start()
    {
        ValidarReferencias();

        eventSystem = EventSystem.current;

        // Configurar Input System
        ConfigurarInputSystem();

        // --- Resoluciones ---
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
                currentResolutionIndex = i;
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        // --- Brillo inicial ---
        float savedBrightness = PlayerPrefs.GetFloat("masterBrightness", defaultBrightness);
        brightnessSlider.value = savedBrightness;
        SetBrightness(savedBrightness);

        // Establecer primera selección en menú principal
        EstablecerPrimeraSeleccion(firstSelectedMainMenu);

        // Configurar cursor inicial
        MostrarCursorSegunInput();

        Debug.Log("🎮 Menu_Manager iniciado correctamente con Input System");
    }

    void ValidarReferencias()
    {
        Debug.Log("✅ Validación de referencias completada");
    }

    void GuardarSeleccionActual(GameObject panelOrigen)
    {
        if (eventSystem != null && eventSystem.currentSelectedGameObject != null && panelOrigen != null)
        {
            Selectable seleccionActual = eventSystem.currentSelectedGameObject.GetComponent<Selectable>();
            if (seleccionActual != null)
            {
                ultimaSeleccionPorPanel[panelOrigen] = seleccionActual;
                Debug.Log($"🔄 Guardada selección para {panelOrigen.name}: {seleccionActual.name}");
            }
        }
    }

    void EstablecerPrimeraSeleccion(Selectable elementoSeleccionado, GameObject panelActual = null, bool usarMemoria = false)
    {
        Selectable elementoASeleccionar = elementoSeleccionado;

        if (usarMemoria && panelActual != null && ultimaSeleccionPorPanel.ContainsKey(panelActual))
        {
            Selectable seleccionGuardada = ultimaSeleccionPorPanel[panelActual];
            if (seleccionGuardada != null && seleccionGuardada.gameObject.activeInHierarchy && seleccionGuardada.interactable)
            {
                elementoASeleccionar = seleccionGuardada;
                Debug.Log($"📍 Usando memoria para {panelActual.name}: {seleccionGuardada.name}");
            }
        }

        if (eventSystem != null && elementoASeleccionar != null)
        {
            StartCoroutine(SeleccionarDespuesDeFrame(elementoASeleccionar.gameObject));
        }
        else
        {
            GameObject panelActivo = ObtenerPanelActivo();
            if (panelActivo != null)
            {
                StartCoroutine(BuscarYSeleccionarAutomaticamente(panelActivo));
            }
        }
    }

    IEnumerator SeleccionarDespuesDeFrame(GameObject objetoASeleccionar)
    {
        yield return null;

        if (eventSystem != null && objetoASeleccionar != null)
        {
            eventSystem.SetSelectedGameObject(null);
            eventSystem.SetSelectedGameObject(objetoASeleccionar);
            Debug.Log($"✅ Seleccionado para controlador: {objetoASeleccionar.name}");
        }
    }

    IEnumerator BuscarYSeleccionarAutomaticamente(GameObject panel)
    {
        yield return null;

        if (panel != null && eventSystem != null)
        {
            Selectable primerElemento = panel.GetComponentInChildren<Selectable>();

            if (primerElemento != null && primerElemento.gameObject.activeInHierarchy && primerElemento.interactable)
            {
                eventSystem.SetSelectedGameObject(null);
                eventSystem.SetSelectedGameObject(primerElemento.gameObject);
                Debug.Log($"🔍 Selección automática: {primerElemento.name}");
            }
            else
            {
                Debug.LogWarning($"❌ No se encontró elemento seleccionable en: {panel.name}");
            }
        }
    }

    GameObject ObtenerPanelActivo()
    {
        if (MainMenuPanel != null && MainMenuPanel.activeInHierarchy) return MainMenuPanel;
        if (OptionsMainMenuPanel != null && OptionsMainMenuPanel.activeInHierarchy) return OptionsMainMenuPanel;
        if (ExtrasPanel != null && ExtrasPanel.activeInHierarchy) return ExtrasPanel;
        if (GameOptionsPanel != null && GameOptionsPanel.activeInHierarchy) return GameOptionsPanel;
        if (BrightnessGameOptionsPanel != null && BrightnessGameOptionsPanel.activeInHierarchy) return BrightnessGameOptionsPanel;
        if (AudioPanel != null && AudioPanel.activeInHierarchy) return AudioPanel;
        if (ControllerPanel != null && ControllerPanel.activeInHierarchy) return ControllerPanel;
        if (QuitGamePanel != null && QuitGamePanel.activeInHierarchy) return QuitGamePanel;

        return null;
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

        if (navigateAction == null)
        {
            navigateAction = new InputAction("Navigate", InputActionType.Value);
            navigateAction.AddBinding("<Gamepad>/leftStick");
            navigateAction.AddBinding("<Gamepad>/dpad");
            navigateAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");
        }

        if (submitAction == null)
        {
            submitAction = new InputAction("Submit", InputActionType.Button);
            submitAction.AddBinding("<Gamepad>/buttonSouth");
            submitAction.AddBinding("<Keyboard>/enter");
            submitAction.AddBinding("<Keyboard>/space");
        }

        if (cancelAction == null)
        {
            cancelAction = new InputAction("Cancel", InputActionType.Button);
            cancelAction.AddBinding("<Gamepad>/buttonEast");
            cancelAction.AddBinding("<Gamepad>/buttonNorth");
            cancelAction.AddBinding("<Keyboard>/escape");
        }

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
            if (context.control.device is Gamepad)
            {
                isUsingController = true;
            }
            else
            {
                isUsingController = false;
            }
            MostrarCursorSegunInput();
        }
    }

    void OnSubmit(InputAction.CallbackContext context)
    {
        if (context.control.device is Gamepad)
        {
            isUsingController = true;
        }
        else
        {
            isUsingController = false;
        }
        MostrarCursorSegunInput();
    }

    void OnCancel(InputAction.CallbackContext context)
    {
        if (context.control.device is Gamepad)
        {
            isUsingController = true;
            MostrarCursorSegunInput();
            ManejarCancelacion();
        }
        else
        {
            isUsingController = false;
            MostrarCursorSegunInput();
        }
    }

    void ManejarCancelacion()
    {
        if (OptionsMainMenuPanel != null && OptionsMainMenuPanel.activeInHierarchy)
        {
            BackOpciones();
        }
        else if (GameOptionsPanel != null && GameOptionsPanel.activeInHierarchy)
        {
            BackGameOptions();
        }
        else if (BrightnessGameOptionsPanel != null && BrightnessGameOptionsPanel.activeInHierarchy)
        {
            DoneBrightnessGameOptions();
        }
        else if (AudioPanel != null && AudioPanel.activeInHierarchy)
        {
            BackAudio();
        }
        else if (ControllerPanel != null && ControllerPanel.activeInHierarchy)
        {
            BackController();
        }
        else if (ExtrasPanel != null && ExtrasPanel.activeInHierarchy)
        {
            BackExtras();
        }
        else if (QuitGamePanel != null && QuitGamePanel.activeInHierarchy)
        {
            NoQuitGame();
        }
    }

    void Update()
    {
        if (Mouse.current != null && Mouse.current.delta.ReadValue().magnitude > 0.1f)
        {
            isUsingController = false;
            MostrarCursorSegunInput();
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            isUsingController = false;
            MostrarCursorSegunInput();
        }

        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            isUsingController = false;
            MostrarCursorSegunInput();
        }

        if (Gamepad.current != null)
        {
            if (Gamepad.current.leftStick.ReadValue().magnitude > 0.1f ||
                Gamepad.current.rightStick.ReadValue().magnitude > 0.1f)
            {
                isUsingController = true;
                MostrarCursorSegunInput();
            }

            Vector2 dpad = Gamepad.current.dpad.ReadValue();
            if (dpad.magnitude > 0.1f)
            {
                isUsingController = true;
                MostrarCursorSegunInput();
            }

            if (Gamepad.current.buttonSouth.wasPressedThisFrame ||
                Gamepad.current.buttonEast.wasPressedThisFrame ||
                Gamepad.current.buttonWest.wasPressedThisFrame ||
                Gamepad.current.buttonNorth.wasPressedThisFrame)
            {
                isUsingController = true;
                MostrarCursorSegunInput();
            }
        }
    }

    void MostrarCursorSegunInput()
    {
        if ((MainMenuPanel != null && MainMenuPanel.activeInHierarchy) || insideSubmenu)
        {
            if (isUsingController)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    // ===================== BRILLO =====================
    public void SetBrightness(float brightness)
    {
        _brightnessLevel = brightness;
        if (brightnessTextValue != null)
            brightnessTextValue.text = brightness.ToString("0.0");

        if (brightnessPreviewImage != null)
        {
            Color c = brightnessPreviewImage.color;
            c.r = brightness;
            c.g = brightness;
            c.b = brightness;
            c.a = 1f;
            brightnessPreviewImage.color = c;
        }

        if (brightnessManager != null)
        {
            brightnessManager.SetBrightness(brightness);
        }
    }

    public void ResetBrightness()
    {
        brightnessSlider.value = defaultBrightness;
        SetBrightness(defaultBrightness);
    }

    public void SetFullScreen(bool isFullScreen)
    {
        _isFullScreen = isFullScreen;
    }

    public void GraphicsApply()
    {
        PlayerPrefs.SetFloat("masterBrightness", _brightnessLevel);
        PlayerPrefs.SetFloat("masterFullScreen", (_isFullScreen ? 1 : 0));
        PlayerPrefs.Save();
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    // ===================== MENÚS =====================

    // ✅ MÉTODO START GAME LIMPIO - SIN TRANSICIONES
    public void StartGame()
    {
        Debug.Log("🎮 StartGame llamado - Cargando escena directamente");

        // Actualizar AudioManager si existe
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.RefreshSlidersAndTexts();
        }

        // Cargar escena del juego (índice 1)
        SceneManager.LoadScene(1);
    }

    public void Options()
    {
        Debug.Log("🔄 Cambiando a OPTIONS...");

        GuardarSeleccionActual(MainMenuPanel);

        if (MainMenuPanel != null) MainMenuPanel.SetActive(false);
        if (OptionsMainMenuPanel != null) OptionsMainMenuPanel.SetActive(true);

        EstablecerPrimeraSeleccion(firstSelectedOptionsMain);
    }

    public void GameOptions()
    {
        Debug.Log("🔄 Cambiando a GAME OPTIONS...");

        GuardarSeleccionActual(OptionsMainMenuPanel);

        if (OptionsMainMenuPanel != null) OptionsMainMenuPanel.SetActive(false);
        if (GameOptionsPanel != null) GameOptionsPanel.SetActive(true);

        EstablecerPrimeraSeleccion(firstSelectedGameOptions);
    }

    public void BrightnessGameOptions()
    {
        Debug.Log("🔄 Cambiando a BRIGHTNESS...");

        GuardarSeleccionActual(GameOptionsPanel);

        if (GameOptionsPanel != null) GameOptionsPanel.SetActive(false);
        if (BrightnessGameOptionsPanel != null) BrightnessGameOptionsPanel.SetActive(true);

        EstablecerPrimeraSeleccion(firstSelectedBrightness);
    }

    public void DoneBrightnessGameOptions()
    {
        GraphicsApply();
        if (BrightnessGameOptionsPanel != null) BrightnessGameOptionsPanel.SetActive(false);
        if (GameOptionsPanel != null) GameOptionsPanel.SetActive(true);

        EstablecerPrimeraSeleccion(firstSelectedGameOptions, GameOptionsPanel, usarMemoria: true);
    }

    public void BackGameOptions()
    {
        if (GameOptionsPanel != null) GameOptionsPanel.SetActive(false);
        if (OptionsMainMenuPanel != null) OptionsMainMenuPanel.SetActive(true);

        EstablecerPrimeraSeleccion(firstSelectedOptionsMain, OptionsMainMenuPanel, usarMemoria: true);
    }

    public void Audio()
    {
        Debug.Log("🔄 Cambiando a AUDIO...");

        GuardarSeleccionActual(OptionsMainMenuPanel);

        if (OptionsMainMenuPanel != null) OptionsMainMenuPanel.SetActive(false);
        if (AudioPanel != null) AudioPanel.SetActive(true);

        EstablecerPrimeraSeleccion(firstSelectedAudio);
    }

    public void BackAudio()
    {
        if (AudioPanel != null) AudioPanel.SetActive(false);
        if (OptionsMainMenuPanel != null) OptionsMainMenuPanel.SetActive(true);

        EstablecerPrimeraSeleccion(firstSelectedOptionsMain, OptionsMainMenuPanel, usarMemoria: true);
    }

    public void Controller()
    {
        Debug.Log("🔄 Cambiando a CONTROLLER...");

        GuardarSeleccionActual(OptionsMainMenuPanel);

        if (OptionsMainMenuPanel != null) OptionsMainMenuPanel.SetActive(false);
        if (ControllerPanel != null) ControllerPanel.SetActive(true);

        EstablecerPrimeraSeleccion(firstSelectedController);
    }

    public void BackController()
    {
        if (ControllerPanel != null) ControllerPanel.SetActive(false);
        if (OptionsMainMenuPanel != null) OptionsMainMenuPanel.SetActive(true);

        EstablecerPrimeraSeleccion(firstSelectedOptionsMain, OptionsMainMenuPanel, usarMemoria: true);
    }

    public void BackOpciones()
    {
        if (OptionsMainMenuPanel != null) OptionsMainMenuPanel.SetActive(false);
        if (MainMenuPanel != null) MainMenuPanel.SetActive(true);

        EstablecerPrimeraSeleccion(firstSelectedMainMenu, MainMenuPanel, usarMemoria: true);
    }

    public void Extras()
    {
        Debug.Log("🔄 Cambiando a EXTRAS...");

        GuardarSeleccionActual(MainMenuPanel);

        if (MainMenuPanel != null) MainMenuPanel.SetActive(false);
        if (ExtrasPanel != null) ExtrasPanel.SetActive(true);

        EstablecerPrimeraSeleccion(firstSelectedExtras);
    }

    public void BackExtras()
    {
        if (ExtrasPanel != null) ExtrasPanel.SetActive(false);
        if (MainMenuPanel != null) MainMenuPanel.SetActive(true);

        EstablecerPrimeraSeleccion(firstSelectedMainMenu, MainMenuPanel, usarMemoria: true);
    }

    public void QuitGame()
    {
        Debug.Log("🔄 Cambiando a QUIT GAME...");

        GuardarSeleccionActual(MainMenuPanel);

        if (MainMenuPanel != null) MainMenuPanel.SetActive(false);
        if (QuitGamePanel != null) QuitGamePanel.SetActive(true);

        EstablecerPrimeraSeleccion(firstSelectedQuitGame);
    }

    public void YesQuitGame()
    {
        Debug.Log("🚪 Salir del juego");
        Application.Quit();
    }

    public void NoQuitGame()
    {
        if (QuitGamePanel != null) QuitGamePanel.SetActive(false);
        if (MainMenuPanel != null) MainMenuPanel.SetActive(true);

        EstablecerPrimeraSeleccion(firstSelectedMainMenu, MainMenuPanel, usarMemoria: true);
    }
}
