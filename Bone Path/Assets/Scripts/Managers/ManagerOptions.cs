using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ManagerOptions : MonoBehaviour
{
    [Header("Player References")]
    public Player playerRef;

    [Header("UI Panels")]
    public GameObject pausePanel;
    public GameObject optionsPanel;
    public GameObject quitPanel;

    [Header("First Selection Buttons")]
    public Button firstPauseButton;
    public Button firstOptionsButton;
    public Button firstQuitButton;

    [Header("Input Settings")]
    public InputActionAsset inputActions;

    [HideInInspector] public bool insideSubmenu = false;

    private Button lastSelectedButton = null;
    private InputAction navigateAction;
    private InputAction submitAction;
    private InputAction cancelAction;

    private bool isUsingController = false;
    private EventSystem eventSystem;

    void Start()
    {
        eventSystem = EventSystem.current;

        ConfigureInputSystem();

        optionsPanel.SetActive(false);
        quitPanel.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
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
            isUsingController = true;
            ShowCursorBasedOnInput();
        }
    }

    //Selection confirmation detection
    void OnSubmit(InputAction.CallbackContext context)
    {
        if (context.control.device is Gamepad)
        {
            isUsingController = true;
            ShowCursorBasedOnInput();
        }
    }

    //Detection of back button between menus
    void OnCancel(InputAction.CallbackContext context)
    {
        if (context.control.device is Gamepad)
        {
            isUsingController = true;
            ShowCursorBasedOnInput();
            HandleCancel();
        }
    }

    //Go back through the menus
    void HandleCancel()
    {
        if (optionsPanel.activeInHierarchy)
        {
            BackOptionsPanel();
        }
        else if (quitPanel.activeInHierarchy)
        {
            NoQuitPanel();
        }
    }

    //Last saved button UI
    void SaveCurrentButton()
    {
        if (eventSystem != null && eventSystem.currentSelectedGameObject != null)
        {
            Button currentButton = eventSystem.currentSelectedGameObject.GetComponent<Button>();
            if (currentButton != null)
            {
                lastSelectedButton = currentButton;
            }
        }
    }

    //First selection menu command
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

    void Update()
    {
        if (Mouse.current != null && Mouse.current.delta.ReadValue().magnitude > 0.1f)
        {
            isUsingController = false;
            ShowCursorBasedOnInput();
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            isUsingController = false;
            ShowCursorBasedOnInput();
        }

        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            isUsingController = false;
            ShowCursorBasedOnInput();
        }

        if (Gamepad.current != null)
        {
            if (Gamepad.current.leftStick.ReadValue().magnitude > 0.1f || Gamepad.current.rightStick.ReadValue().magnitude > 0.1f)
            {
                isUsingController = true;
                ShowCursorBasedOnInput();
            }

            if (Gamepad.current.buttonSouth.wasPressedThisFrame ||
                Gamepad.current.buttonEast.wasPressedThisFrame ||
                Gamepad.current.buttonWest.wasPressedThisFrame ||
                Gamepad.current.buttonNorth.wasPressedThisFrame)
            {
                isUsingController = true;
                ShowCursorBasedOnInput();
            }
        }
    }

    //Visibility and cursor blocking
    void ShowCursorBasedOnInput()
    {
        if (pausePanel.activeInHierarchy || insideSubmenu)
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
    }

    //Wait for menu frame
    IEnumerator SelectAfterFrame(GameObject objToSelect)
    {
        yield return null;
        if (eventSystem != null && objToSelect != null)
        {
            eventSystem.SetSelectedGameObject(objToSelect);
        }
    }

    //Pause game
    public void Pause()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;

        if (isUsingController)
        {
            SetFirstSelection(firstPauseButton);
        }

        ShowCursorBasedOnInput();
    }

    //Continue playing
    public void Continue()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;

        if (eventSystem != null)
        {
            eventSystem.SetSelectedGameObject(null);
        }

        lastSelectedButton = null;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    //Options menu
    public void OptionsPause()
    {
        SaveCurrentButton();
        pausePanel.SetActive(false);
        optionsPanel.SetActive(true);
        insideSubmenu = true;

        if (isUsingController)
        {
            SetFirstSelection(firstOptionsButton);
        }

        ShowCursorBasedOnInput();
    }

    //Back from options
    public void BackOptionsPanel()
    {
        optionsPanel.SetActive(false);
        pausePanel.SetActive(true);
        insideSubmenu = false;

        if (isUsingController)
        {
            SetFirstSelection(firstPauseButton, true);
        }

        ShowCursorBasedOnInput();
    }

    //Close the game
    public void QuitPause()
    {
        SaveCurrentButton();
        pausePanel.SetActive(false);
        quitPanel.SetActive(true);
        insideSubmenu = true;

        if (isUsingController)
        {
            SetFirstSelection(firstQuitButton);
        }

        ShowCursorBasedOnInput();
    }

    //If close game
    public void YesQuit()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.RefreshSlidersAndTexts();
        }
        SceneManager.LoadScene(0);
    }

    //Do not close game
    public void NoQuitPanel()
    {
        quitPanel.SetActive(false);
        pausePanel.SetActive(true);
        insideSubmenu = false;

        if (isUsingController)
        {
            SetFirstSelection(firstPauseButton, true);
        }

        ShowCursorBasedOnInput();
    }
}
