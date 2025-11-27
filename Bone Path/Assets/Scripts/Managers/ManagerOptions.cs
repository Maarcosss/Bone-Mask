using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ManagerOptions : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject pausePanel;
    public GameObject optionsPanel;
    public GameObject quitPanel;
    public GameObject quitGamePanel;

    [Header("First Selection Buttons")]
    public Button firstPauseButton;
    public Button firstOptionsButton;
    public Button firstQuitButton;
    public Button firstQuitGameButton;

    [Header("Input Settings")]
    public InputActionAsset inputActions;

    [HideInInspector] public bool insideSubmenu = false;

    private Button lastSelectedButton = null;
    private InputAction navigateAction;
    private InputAction submitAction;
    private InputAction cancelAction;

    private bool isUsingController = false;
    private EventSystem eventSystem;

    public Player playerRef;

    void Start()
    {
        playerRef.LoadData();
        eventSystem = EventSystem.current;

        ConfigureInputSystem();

        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (quitPanel != null) quitPanel.SetActive(false);

    }

    void OnEnable()
    {
        EnableActions();
    }

    void OnDisable()
    {
        DisableActions();
    }

    void ConfigureInputSystem()
    {
        if (inputActions == null)
        {
            inputActions = Resources.Load<InputActionAsset>("InputSystem_Actions");
        }

        if (inputActions != null)
        {
            var uiMap = inputActions.FindActionMap("UI");
            if (uiMap != null)
            {
                navigateAction = uiMap.FindAction("Navigate");
                submitAction = uiMap.FindAction("Submit");
                cancelAction = uiMap.FindAction("Cancel");
            }
        }

        if (navigateAction == null)
        {
            navigateAction = new InputAction("Navigate", InputActionType.Value, expectedControlType: "Vector2");
            navigateAction.AddBinding("<Gamepad>/leftStick");
            navigateAction.AddBinding("<Gamepad>/dpad");
        }

        if (submitAction == null)
        {
            submitAction = new InputAction("Submit", InputActionType.Button);
            submitAction.AddBinding("<Gamepad>/buttonSouth");
            submitAction.AddBinding("<Keyboard>/enter");
        }

        if (cancelAction == null)
        {
            cancelAction = new InputAction("Cancel", InputActionType.Button);
            cancelAction.AddBinding("<Gamepad>/buttonEast");
            cancelAction.AddBinding("<Gamepad>/buttonNorth");
            cancelAction.AddBinding("<Keyboard>/escape");
        }

        ConfigureCallbacks();
    }

    void ConfigureCallbacks()
    {
        if (navigateAction != null) navigateAction.performed += OnNavigate;
        if (submitAction != null) submitAction.performed += OnSubmit;
        if (cancelAction != null) cancelAction.performed += OnCancel;
    }

    void EnableActions()
    {
        navigateAction?.Enable();
        submitAction?.Enable();
        cancelAction?.Enable();
    }

    void DisableActions()
    {
        if (navigateAction != null) navigateAction.performed -= OnNavigate;
        if (submitAction != null) submitAction.performed -= OnSubmit;
        if (cancelAction != null) cancelAction.performed -= OnCancel;

        navigateAction?.Disable();
        submitAction?.Disable();
        cancelAction?.Disable();
    }

    void OnNavigate(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        if (input.magnitude > 0.1f) isUsingController = true;
    }

    void OnSubmit(InputAction.CallbackContext context)
    {
        if (context.control.device is Gamepad) isUsingController = true;
    }

    void OnCancel(InputAction.CallbackContext context)
    {
        if (context.control.device is Gamepad || context.control.device is Keyboard)
        {
            isUsingController = true;
            HandleCancel();
        }
    }

    void HandleCancel()
    {
        if (optionsPanel != null && optionsPanel.activeInHierarchy)
        {
            BackOptionsPanel();
        }
        else if (quitPanel != null && quitPanel.activeInHierarchy)
        {
            NoQuitPanel();
        }
    }

    void SaveCurrentButton()
    {
        if (eventSystem != null && eventSystem.currentSelectedGameObject != null)
        {
            lastSelectedButton = eventSystem.currentSelectedGameObject.GetComponent<Button>();
        }
    }

    void SetFirstSelection(Button buttonToSelect, bool useMemory = false)
    {
        Button button = buttonToSelect;

        if (useMemory && lastSelectedButton != null && lastSelectedButton.gameObject.activeInHierarchy)
        {
            button = lastSelectedButton;
        }

        if (eventSystem != null && button != null)
        {
            eventSystem.SetSelectedGameObject(null);
            StartCoroutine(SelectAfterFrame(button.gameObject));
        }
    }

    IEnumerator SelectAfterFrame(GameObject objToSelect)
    {
        yield return null;
        if (eventSystem != null && objToSelect != null)
        {
            eventSystem.SetSelectedGameObject(objToSelect);
        }
    }

    public void Pause()
    {
        if (pausePanel != null) pausePanel.SetActive(true);
        Time.timeScale = 0f;

        if (isUsingController) SetFirstSelection(firstPauseButton);
    }

    public void Continue()
    {
        Debug.Log("Se ha pulsado Continue");

        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;

        if (eventSystem != null) eventSystem.SetSelectedGameObject(null);
        lastSelectedButton = null;
    }

    public void OptionsPause()
    {
        SaveCurrentButton();
        if (pausePanel != null) pausePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
        insideSubmenu = true;

        if (isUsingController) SetFirstSelection(firstOptionsButton);
    }

    public void BackOptionsPanel()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true);
        insideSubmenu = false;

        if (isUsingController) SetFirstSelection(firstPauseButton, true);
    }

    public void QuitPause()
    {
        Debug.Log("Se ha pulsado QuitPause");

        SaveCurrentButton();
        if (pausePanel != null) pausePanel.SetActive(false);
        if (quitPanel != null) quitPanel.SetActive(true);
        insideSubmenu = true;

        if (isUsingController) SetFirstSelection(firstQuitButton);
    }

    public void YesQuit()
    {
        playerRef.SaveData();
        if (AudioManager.Instance != null) AudioManager.Instance.RefreshSlidersAndTexts();
        SceneManager.LoadScene(0);
    }

    public void NoQuitPanel()
    {
        if (quitPanel != null) quitPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true);
        insideSubmenu = false;

        if (isUsingController) SetFirstSelection(firstPauseButton, true);
    }

    public void QuitGame()
    {
        Debug.Log("Se ha pulsado Quit Game");
        SaveCurrentButton();

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        if (quitGamePanel != null)
        {
            quitGamePanel.SetActive(true);
        }

         if (isUsingController) SetFirstSelection(firstQuitGameButton);
    }

    public void YesQuitGame()
    {
        playerRef.SaveData();
        if (AudioManager.Instance != null) AudioManager.Instance.RefreshSlidersAndTexts();
        Debug.Log("Saliendo del juego");
        Application.Quit();
    }

    public void NoQuitGame()
    {
        if (quitGamePanel != null)
        {
            quitGamePanel.SetActive(false);
        }
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        if (isUsingController) SetFirstSelection(firstQuitGameButton);
    }

    void Update()
    {
        if (Mouse.current != null && (Mouse.current.delta.ReadValue().magnitude > 0.1f || Mouse.current.leftButton.wasPressedThisFrame))
        {
            isUsingController = false;
        }

        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) isUsingController = false;

        if (Gamepad.current != null)
        {
            if (Gamepad.current.leftStick.ReadValue().magnitude > 0.1f || Gamepad.current.rightStick.ReadValue().magnitude > 0.1f)
                isUsingController = true;

            if (Gamepad.current.buttonSouth.wasPressedThisFrame ||
                Gamepad.current.buttonEast.wasPressedThisFrame ||
                Gamepad.current.buttonWest.wasPressedThisFrame ||
                Gamepad.current.buttonNorth.wasPressedThisFrame)
                isUsingController = true;
        }
    }
}
