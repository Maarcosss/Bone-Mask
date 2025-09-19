using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class ManagerOptions : MonoBehaviour
{

    public PlayerMovement PlayerMovementRef; 

    public GameObject OptionsPausePanel;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

       

    }

    public void OptionsPause()
    {

        OptionsPausePanel.SetActive(true);

    }

    public void VolverOptionsPause()
    {
        Time.timeScale = 1.0f;
        PlayerMovementRef.validar_inputs = true;
        OptionsPausePanel.SetActive(false);

    }

    public void MainMenuOption()
    {

        SceneManager.LoadScene(0);

    }


}
