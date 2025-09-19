using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class ManagerOptions : MonoBehaviour
{

    public PlayerMovement PlayerMovementRef;

    public GameObject PausePanel;
    public GameObject OptionsPausePanel;
    public GameObject QuitToMenuPausePanel;

    // Start is called before the first frame update
    void Start()
    {

        

    }

    // Update is called once per frame
    void Update()
    {

       

    }

    public void Pause()
    {

        PausePanel.SetActive(true);

    }

    public void Continue()
    {
        Time.timeScale = 1.0f;
        PlayerMovementRef.validar_inputs = true;
        PausePanel.SetActive(false);

    }

    public void OptionsPause()
    {

        PausePanel.SetActive(false);
        OptionsPausePanel.SetActive(true);

    }

    public void BackOptionsPause()
    {

        OptionsPausePanel.SetActive(false);
        PausePanel.SetActive(true);

    }

    public void QuitToMenuPause()
    {

        QuitToMenuPausePanel.SetActive(true);
        PausePanel.SetActive(false);

    }

    public void YesQuitToMenu()
    {

        SceneManager.LoadScene(0);

    }

    public void NoQuitToMenu()
    {

        QuitToMenuPausePanel.SetActive(false);
        PausePanel.SetActive(true);

    }


}
