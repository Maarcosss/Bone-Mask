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

    [HideInInspector] public bool insideSubmenu = false; // NUEVO: indica si estamos dentro de un submen�

    // Start is called before the first frame update
    void Start()
    {
        // Asegurarse de que todos los paneles secundarios est�n cerrados al inicio
        OptionsPausePanel.SetActive(false);
        QuitToMenuPausePanel.SetActive(false);
    }

    // Abrir men� de pausa
    public void Pause()
    {
        PausePanel.SetActive(true);
        PlayerMovementRef.validar_inputs = false;
        Time.timeScale = 0f;
    }

    // Continuar juego desde pausa
    public void Continue()
    {
        PausePanel.SetActive(false);
        PlayerMovementRef.validar_inputs = true;
        Time.timeScale = 1f;
    }

    // Abrir submen� de opciones desde pausa
    public void OptionsPause()
    {
        PausePanel.SetActive(false);
        OptionsPausePanel.SetActive(true);
        insideSubmenu = true;
    }

    // Volver del submen� de opciones al panel de pausa
    public void BackOptionsPause()
    {
        OptionsPausePanel.SetActive(false);
        PausePanel.SetActive(true);
        insideSubmenu = false;
    }

    // Abrir submen� de "Salir al men�" desde pausa
    public void QuitToMenuPause()
    {
        PausePanel.SetActive(false);
        QuitToMenuPausePanel.SetActive(true);
        insideSubmenu = true;
    }

    // Confirmar salida al men� principal
    public void YesQuitToMenu()
    {
        SceneManager.LoadScene(0);
    }

    // Cancelar salida y volver al panel de pausa
    public void NoQuitToMenu()
    {
        QuitToMenuPausePanel.SetActive(false);
        PausePanel.SetActive(true);
        insideSubmenu = false;
    }
}
