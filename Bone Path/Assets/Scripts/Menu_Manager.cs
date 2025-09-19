using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu_Manager : MonoBehaviour
{

    public GameObject MainMenuPanel;
    public GameObject OptionsMainMenuPanel;
    public GameObject ExtrasPanel;
    public GameObject AudioPanel;
    public GameObject ControllerPanel;
    public GameObject QuitGamePanel;

    // Start is called before the first frame update
    void Start()
    {

        

    }

    // Update is called once per frame
    void Update()
    {

        

    }

    public void StartGame()
    {

        SceneManager.LoadScene(1);

    }

    public void Options()
    {

        MainMenuPanel.SetActive(false);
        OptionsMainMenuPanel.SetActive(true);
    }

    public void Audio()
    {

        OptionsMainMenuPanel.SetActive(false);
        AudioPanel.SetActive(true);
        
    }

    public void VolverAudio()
    {

        AudioPanel.SetActive(false);
        OptionsMainMenuPanel.SetActive(true);

    }

    public void Controller()
    {

        OptionsMainMenuPanel.SetActive(false);
        ControllerPanel.SetActive(false);

    }

    public void VolverController()
    {

        ControllerPanel.SetActive(false);
        OptionsMainMenuPanel.SetActive(false);

    }

    public void VolverOpciones()
    {

        OptionsMainMenuPanel.SetActive(false);
        MainMenuPanel.SetActive(true);
    }

    public void Extras()
    {

        MainMenuPanel.SetActive(false);
        ExtrasPanel.SetActive(true);

    }

    public void VolverExtras()
    {

        ExtrasPanel.SetActive(false);
        MainMenuPanel.SetActive(true);

    }

    public void QuitGame()
    {

        MainMenuPanel.SetActive(false);
        QuitGamePanel.SetActive(true);

    }

    public void YesQuitGame()
    {

        Debug.Log("Salir del juego");
        Application.Quit();

    }

    public void NoQuitGame()
    {

        QuitGamePanel.SetActive(false);
        MainMenuPanel.SetActive(true);

    }

}
