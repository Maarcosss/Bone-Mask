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
    public GameObject mainMenuPanel;
    public GameObject optionsMainMenuPanel;
    public GameObject extrasPanel;
    public GameObject gameOptionsPanel;
    public GameObject brightnessGameOptionsPanel;
    public GameObject audioPanel;
    public GameObject controllerPanel;
    public GameObject quitGamePanel;

    [Header("First Selected UI Elements")]
    public Button firstSelectedMainMenu;
    public Button firstSelectedOptionsMain;
    public Button firstSelectedExtras;
    public Button firstSelectedGameOptions;
    public Slider firstSelectedBrightness;
    public Slider firstSelectedAudio;
    public Button firstSelectedController;
    public Button firstSelectedQuitGame;

    Resolution[] allResolutions;

    [Header("Input Settings")]
    public InputActionAsset inputActions;

    [HideInInspector] public bool insideSubmenu = false;

    private EventSystem eventSystem;

    private bool isUsingController = false;

    //Button memory system per panel
    private Dictionary<GameObject, Selectable> lastSelectionPerPanel = new Dictionary<GameObject, Selectable>();

    //Input System actions
    private InputAction navigateAction;
    private InputAction submitAction;
    private InputAction cancelAction;

    private string url = "https://www.youtube.com/channel/UCd3P90PkgE53ogQkl72LNvQ";

    private void Start()
    {
        eventSystem = EventSystem.current;
        ConfigureInputSystem();
        SetFirstSelection(firstSelectedMainMenu);
    }

    //Save UI selection
    void SaveSelected(GameObject sourcePanel)
    {
        if (eventSystem != null && eventSystem.currentSelectedGameObject != null && sourcePanel != null)
        {
            Selectable seleccionActual = eventSystem.currentSelectedGameObject.GetComponent<Selectable>();
            if (seleccionActual != null)
            {
                lastSelectionPerPanel[sourcePanel] = seleccionActual;
            }
        }
    }

    //First selection menu command
    void SetFirstSelection(Selectable selectedElement, GameObject currentPanel = null, bool useMemory = false)
    {
        if (!isUsingController)
        {
            return;
        }

        Selectable elementToSelect = selectedElement;

        if (useMemory && currentPanel != null && lastSelectionPerPanel.ContainsKey(currentPanel))
        {
            Selectable savedSelection = lastSelectionPerPanel[currentPanel];
            if (savedSelection != null && savedSelection.gameObject.activeInHierarchy && savedSelection.interactable)
            {
                elementToSelect = savedSelection;
            }
        }

        if (eventSystem != null && elementToSelect != null)
        {
            StartCoroutine(SelectAfterFrame(elementToSelect.gameObject));
        }
        else
        {
            GameObject activePanel = GetActivePanel();
            if (activePanel != null)
            {
                StartCoroutine(SearchAndSelectAutomatically(activePanel));
            }
        }
    }

    //Wait for menu frame
    IEnumerator SelectAfterFrame(GameObject objetoASeleccionar)
    {
        yield return null;

        if (eventSystem != null && objetoASeleccionar != null)
        {
            eventSystem.SetSelectedGameObject(null);
            eventSystem.SetSelectedGameObject(objetoASeleccionar);
        }
    }

    //Button selected after the frame
    IEnumerator SearchAndSelectAutomatically(GameObject panel)
    {
        yield return null;

        if (panel != null && eventSystem != null)
        {
            Selectable primerElemento = panel.GetComponentInChildren<Selectable>();

            if (primerElemento != null && primerElemento.gameObject.activeInHierarchy && primerElemento.interactable)
            {
                eventSystem.SetSelectedGameObject(null);
                eventSystem.SetSelectedGameObject(primerElemento.gameObject);
            }
        }
    }

    //Current active panel in the hierarchy
    GameObject GetActivePanel()
    {
        if (mainMenuPanel != null && mainMenuPanel.activeInHierarchy)
        {
            return mainMenuPanel;
        }
        if (optionsMainMenuPanel != null && optionsMainMenuPanel.activeInHierarchy)
        {
            return optionsMainMenuPanel;
        }
        if (extrasPanel != null && extrasPanel.activeInHierarchy)
        {
            return extrasPanel;
        }
        if (gameOptionsPanel != null && gameOptionsPanel.activeInHierarchy)
        {
            return gameOptionsPanel;
        }
        if (brightnessGameOptionsPanel != null && brightnessGameOptionsPanel.activeInHierarchy)
        {
            return brightnessGameOptionsPanel;
        }
        if (audioPanel != null && audioPanel.activeInHierarchy)
        {
            return audioPanel;
        }
        if (controllerPanel != null && controllerPanel.activeInHierarchy)
        {
            return controllerPanel;
        }
        if (quitGamePanel != null && quitGamePanel.activeInHierarchy)
        {
            return quitGamePanel;
        }

        return null;
    }

    //Read player inputs
    void ConfigureInputSystem()
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

        ConfigureCallbacks();
        EnableActions();
    }

    //Connection Inputs to logic menu
    void ConfigureCallbacks()
    {
        navigateAction.performed += OnNavigate;
        submitAction.performed += OnSubmit;
        cancelAction.performed += OnCancel;
    }

    //Enables Input System actions
    void EnableActions()
    {
        navigateAction?.Enable();
        submitAction?.Enable();
        cancelAction?.Enable();
    }

    //Disables Input System actions
    void DisableActions()
    {
        navigateAction?.Disable();
        submitAction?.Disable();
        cancelAction?.Disable();
    }

    //Detection of navigation between menus
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
            
        }
    }

    //Selection confirmation detection
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

    }

    //Detection of back button between menus
    void OnCancel(InputAction.CallbackContext context)
    {
        if (context.control.device is Gamepad)
        {
            isUsingController = true;
            
            ManageCancellation();
        }
        else
        {
            isUsingController = false;
            
        }
    }

    //Cancel or go back between menus
    void ManageCancellation()
    {
        if (optionsMainMenuPanel != null && optionsMainMenuPanel.activeInHierarchy)
        {
            BackOptions();
        }
        else if (gameOptionsPanel != null && gameOptionsPanel.activeInHierarchy)
        {
            BackGameSettings();
        }
        else if (brightnessGameOptionsPanel != null && brightnessGameOptionsPanel.activeInHierarchy)
        {
            AcceptBrightnessSettings();
        }
        else if (audioPanel != null && audioPanel.activeInHierarchy)
        {
            BackAudioSettings();
        }
        else if (controllerPanel != null && controllerPanel.activeInHierarchy)
        {
            BackControllerSettings();
        }
        else if (extrasPanel != null && extrasPanel.activeInHierarchy)
        {
            BackExtrasMenu();
        }
        else if (quitGamePanel != null && quitGamePanel.activeInHierarchy)
        {
            NoQuitGame();
        }
    }

    //Determines whether mouse/keyboard or controller is being used
    void Update()
    {
        if (Mouse.current != null && Mouse.current.delta.ReadValue().magnitude > 0.1f)
        {
            isUsingController = false;
            
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            isUsingController = false;
            
        }

        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            isUsingController = false;
            
        }

        if (Gamepad.current != null)
        {
            if (Gamepad.current.leftStick.ReadValue().magnitude > 0.1f || Gamepad.current.rightStick.ReadValue().magnitude > 0.1f)
            {
                isUsingController = true;
                
            }

            Vector2 dpad = Gamepad.current.dpad.ReadValue();
            if (dpad.magnitude > 0.1f)
            {
                isUsingController = true;
                
            }

            if (Gamepad.current.buttonSouth.wasPressedThisFrame ||
                Gamepad.current.buttonEast.wasPressedThisFrame ||
                Gamepad.current.buttonWest.wasPressedThisFrame ||
                Gamepad.current.buttonNorth.wasPressedThisFrame)
            {
                isUsingController = true;
                
            }
        }
    }

    //Start the game
    public void StartGame()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.RefreshSlidersAndTexts();
        }

        SceneManager.LoadScene(1);
    }

    //Options menu
    public void Options()
    {
        SaveSelected(mainMenuPanel);

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }
        if (optionsMainMenuPanel != null)
        {
            optionsMainMenuPanel.SetActive(true);
        }

        SetFirstSelection(firstSelectedOptionsMain);
    }

    //Game settings
    public void GameOptions()
    {
        SaveSelected(optionsMainMenuPanel);

        if (optionsMainMenuPanel != null)
        {
            optionsMainMenuPanel.SetActive(false);
        }
        if (gameOptionsPanel != null)
        {
            gameOptionsPanel.SetActive(true);
        }

        SetFirstSelection(firstSelectedGameOptions);
    }

    //Brightness settings
    public void BrightnessGameOptions()
    {
        SaveSelected(gameOptionsPanel);

        if (gameOptionsPanel != null)
        {
            gameOptionsPanel.SetActive(false);
        }
        if (brightnessGameOptionsPanel != null)
        {
            brightnessGameOptionsPanel.SetActive(true);
        }

        SetFirstSelection(firstSelectedBrightness);
    }

    //Accept brightness settings
    public void AcceptBrightnessSettings()
    {

        if (brightnessGameOptionsPanel != null)
        {
            brightnessGameOptionsPanel.SetActive(false);
        }
        if (gameOptionsPanel != null)
        {
            gameOptionsPanel.SetActive(true);
        }

        SetFirstSelection(firstSelectedGameOptions, gameOptionsPanel, useMemory: true);
    }

    //Return from brightness settings
    public void BackGameSettings()
    {
        if (gameOptionsPanel != null)
        {
            gameOptionsPanel.SetActive(false);
        }
        if (optionsMainMenuPanel != null)
        {
            optionsMainMenuPanel.SetActive(true);
        }

        SetFirstSelection(firstSelectedOptionsMain, optionsMainMenuPanel, useMemory: true);
    }

    //Audio settings
    public void Audio()
    {
        SaveSelected(optionsMainMenuPanel);

        if (optionsMainMenuPanel != null)
        {
            optionsMainMenuPanel.SetActive(false);
        }
        if (audioPanel != null)
        {
            audioPanel.SetActive(true);
        }

        SetFirstSelection(firstSelectedAudio);
    }

    //Back from audio
    public void BackAudioSettings()
    {
        if (audioPanel != null)
        {
            audioPanel.SetActive(false);
        }
        if (optionsMainMenuPanel != null)
        {
            optionsMainMenuPanel.SetActive(true);
        }

        SetFirstSelection(firstSelectedOptionsMain, optionsMainMenuPanel, useMemory: true);
    }

    //Control menu
    public void Controller()
    {
        SaveSelected(optionsMainMenuPanel);

        if (optionsMainMenuPanel != null)
        {
            optionsMainMenuPanel.SetActive(false);
        }
        if (controllerPanel != null)
        {
            controllerPanel.SetActive(true);
        }

        SetFirstSelection(firstSelectedController);
    }

    //Return from controls
    public void BackControllerSettings()
    {
        if (controllerPanel != null)
        {
            controllerPanel.SetActive(false);
        }
        if (optionsMainMenuPanel != null)
        {
            optionsMainMenuPanel.SetActive(true);
        }

        SetFirstSelection(firstSelectedOptionsMain, optionsMainMenuPanel, useMemory: true);
    }

    //Back from options
    public void BackOptions()
    {
        if (optionsMainMenuPanel != null)
        {
            optionsMainMenuPanel.SetActive(false);
        }
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }

        SetFirstSelection(firstSelectedMainMenu, mainMenuPanel, useMemory: true);
    }

    //Extras menu
    public void Extras()
    {
        SaveSelected(mainMenuPanel);

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }
        if (extrasPanel != null)
        {
            extrasPanel.SetActive(true);
        }

        SetFirstSelection(firstSelectedExtras);
    }

    //Back from extras
    public void BackExtrasMenu()
    {
        if (extrasPanel != null)
        {
            extrasPanel.SetActive(false);
        }
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }

        SetFirstSelection(firstSelectedMainMenu, mainMenuPanel, useMemory: true);
    }

    //Close the game
    public void QuitGame()
    {
        SaveSelected(mainMenuPanel);

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }
        if (quitGamePanel != null)
        {
            quitGamePanel.SetActive(true);
        }

        SetFirstSelection(firstSelectedQuitGame);
    }

    //If close game
    public void YesQuitGame()
    {
        Debug.Log("Saliendo del juego");
        Application.Quit();
    }

    //Do not close game
    public void NoQuitGame()
    {
        if (quitGamePanel != null)
        {
            quitGamePanel.SetActive(false);
        }
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }

        SetFirstSelection(firstSelectedMainMenu, mainMenuPanel, useMemory: true);
    }

    //Image URL
    public void OpenURL()
    {
        Application.OpenURL(url);
    }

}
