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

    [HideInInspector] public bool insideSubmenu = false;

    void Start()
    {
        // Cerrar paneles secundarios al inicio
        OptionsPausePanel.SetActive(false);
        QuitToMenuPausePanel.SetActive(false);

        // Ocultar cursor al inicio
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void Pause()
    {
        PausePanel.SetActive(true);
        PlayerMovementRef.validar_inputs = false;
        Time.timeScale = 0f;

        // Show cursor when pausing
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Continue()
    {
        PausePanel.SetActive(false);
        PlayerMovementRef.validar_inputs = true;
        Time.timeScale = 1f;

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

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void BackOptionsPause()
    {
        OptionsPausePanel.SetActive(false);
        PausePanel.SetActive(true);
        insideSubmenu = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void QuitToMenuPause()
    {
        PausePanel.SetActive(false);
        QuitToMenuPausePanel.SetActive(true);
        insideSubmenu = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
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

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
