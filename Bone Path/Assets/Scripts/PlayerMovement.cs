using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerMovement : MonoBehaviour
{

    //Conexiones Scripts
    public ManagerOptions ManagerOptionsRef;

    Rigidbody mask_child;

    //Floats
    public float Speed = 1f;
    public float Jump = 1f;

    //Bools
    public bool validar_inputs = true;
    bool Contacto_Suelo = false;

    // Start is called before the first frame update
    void Start()
    {

        //Tiempo
        Time.timeScale = 1.0f;

        //Rigidbody principal
        mask_child = GetComponent<Rigidbody>();

    }

    // Update is called once per frame
    void Update()
    {

        //Validación Inputs
        if (validar_inputs)
        {

            DetectarInputs();

        }

    }


    void DetectarInputs()
    {

        //Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {

            if (!ManagerOptionsRef.OptionsPausePanel.activeInHierarchy)
            {
                ManagerOptionsRef.OptionsPausePanel.SetActive(true);
                validar_inputs = false;
                Time.timeScale = 0.0f;
            }
            else
            {
                ManagerOptionsRef.OptionsPausePanel.SetActive(false);
                validar_inputs = true;
                Time.timeScale = 1.0f;
            }

        }

        //Salto
        if (Input.GetKeyDown(KeyCode.Space) && Contacto_Suelo)
        {

            mask_child.velocity = new Vector3(mask_child.velocity.x, Jump, mask_child.velocity.z);
            Contacto_Suelo = false;

        }

        //Bloqueo doble dirección
        if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D))
        {

            mask_child.velocity = new Vector3(0f, mask_child.velocity.y, 0f);
            return;

        }

        //Izquierda
        if (Input.GetKey(KeyCode.A))
        {

            transform.rotation = Quaternion.Euler(0, 180, 0);
            mask_child.velocity = new Vector3(-Speed, mask_child.velocity.y, 0f);
            return;

        }

        //Derecha
        if (Input.GetKey(KeyCode.D))
        {

            transform.rotation = Quaternion.Euler(0, 0, 0);
            mask_child.velocity = new Vector3(Speed, mask_child.velocity.y, 0f);
            return;

        }

        //Evitar deslizamiento
        else
        {

            mask_child.velocity = new Vector3(0f, mask_child.velocity.y, mask_child.velocity.z);

        }


    }

    private void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.CompareTag("Suelo"))
        {

            Contacto_Suelo = true;

        }

    }

    private void OnCollisionExit(Collision collision)
    {

        if (collision.gameObject.CompareTag("Suelo"))
        {

            Contacto_Suelo = false;

        }
    }


}
