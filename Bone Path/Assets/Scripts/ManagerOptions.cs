using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ManagerOptions : MonoBehaviour
{
    public PlayerMovement PlayerMovementRef;

    public GameObject PausePanel;
    public GameObject OptionsPausePanel;
    public GameObject QuitToMenuPausePanel;

    [HideInInspector] public bool insideSubmenu = false; // indica si estamos dentro de un submenú

    void Start()
    {
        // Cerrar paneles secundarios al inicio
        OptionsPausePanel.SetActive(false);
        QuitToMenuPausePanel.SetActive(false);

        // Ocultar cursor al inicio
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Mientras estemos en pausa o en submenú, mostrar cursor
        if (PausePanel.activeInHierarchy || insideSubmenu)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            // Juego en marcha: ocultar cursor
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void Pause()
    {
        PausePanel.SetActive(true);
        PlayerMovementRef.validar_inputs = false;
        Time.timeScale = 0f;
    }

    public void Continue()
    {
        PausePanel.SetActive(false);
        PlayerMovementRef.validar_inputs = true;
        Time.timeScale = 1f;
    }

    public void OptionsPause()
    {
        PausePanel.SetActive(false);
        OptionsPausePanel.SetActive(true);
        insideSubmenu = true;
    }

    public void BackOptionsPause()
    {
        OptionsPausePanel.SetActive(false);
        PausePanel.SetActive(true);
        insideSubmenu = false;
    }

    public void QuitToMenuPause()
    {
        PausePanel.SetActive(false);
        QuitToMenuPausePanel.SetActive(true);
        insideSubmenu = true;
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
    }
}
