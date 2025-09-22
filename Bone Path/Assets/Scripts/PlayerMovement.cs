using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerMovement : MonoBehaviour
{
    [Header("Referencias")]
    public ManagerOptions ManagerOptionsRef;

    Rigidbody mask_child;

    [Header("Movimiento")]
    public float Speed = 1f;
    public float JumpForce = 7f;

    [Header("Gravedad")]
    public float extraGravity = 20f;        // fuerza extra hacia abajo
    public float shortJumpMultiplier = 2f;  // salto corto si sueltas espacio
    public float maxJumpTime = 0.25f;       // tiempo máximo manteniendo salto
    float jumpTimeCounter;

    [Header("Doble salto")]
    public int maxJumps = 2;
    int currentJumps;

    [Header("Estados")]
    public bool validar_inputs = true;
    bool Contacto_Suelo = false;
    bool isJumping = false;

    void Start()
    {
        Time.timeScale = 1.0f;
        mask_child = GetComponent<Rigidbody>();
        currentJumps = maxJumps;
    }

    void Update()
    {
        // Escape siempre se detecta
        DetectarEscape();

        // Solo si se permiten inputs normales
        if (validar_inputs)
        {
            DetectarMovimientoYSaltos();
        }

        // Aplicar gravedad extra todo el tiempo
        mask_child.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);

        // Salto corto si sueltas espacio antes de tiempo
        if (mask_child.velocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            mask_child.velocity += Vector3.up * Physics.gravity.y * (shortJumpMultiplier - 1) * Time.deltaTime;
        }
    }

    void DetectarEscape()
    {
        // Escape: abre/cierra pausa solo si NO estamos dentro de un submenú
        if (Input.GetKeyDown(KeyCode.Escape) && !ManagerOptionsRef.insideSubmenu)
        {
            bool isActive = ManagerOptionsRef.PausePanel.activeInHierarchy;

            ManagerOptionsRef.PausePanel.SetActive(!isActive);
            validar_inputs = isActive; // si estaba activo, desbloquea; si estaba cerrado, bloquea
            Time.timeScale = isActive ? 1f : 0f;
        }

    }

    void DetectarMovimientoYSaltos()
    {
        // Salto (normal + doble salto)
        if (Input.GetKeyDown(KeyCode.Space) && currentJumps > 0)
        {
            isJumping = true;
            jumpTimeCounter = maxJumpTime;
            mask_child.velocity = new Vector3(mask_child.velocity.x, JumpForce, mask_child.velocity.z);

            currentJumps--; // gasta un salto
            Contacto_Suelo = false;
        }

        // Mantener salto más tiempo
        if (Input.GetKey(KeyCode.Space) && isJumping)
        {
            if (jumpTimeCounter > 0)
            {
                mask_child.velocity = new Vector3(mask_child.velocity.x, JumpForce, mask_child.velocity.z);
                jumpTimeCounter -= Time.deltaTime;
            }
        }

        // Soltar espacio interrumpe salto
        if (Input.GetKeyUp(KeyCode.Space))
        {
            isJumping = false;
        }

        // Movimiento lateral
        if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D))
        {
            mask_child.velocity = new Vector3(0f, mask_child.velocity.y, 0f);
            return;
        }

        if (Input.GetKey(KeyCode.A))
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
            mask_child.velocity = new Vector3(-Speed, mask_child.velocity.y, 0f);
            return;
        }

        if (Input.GetKey(KeyCode.D))
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            mask_child.velocity = new Vector3(Speed, mask_child.velocity.y, 0f);
            return;
        }

        // Evitar deslizamiento
        mask_child.velocity = new Vector3(0f, mask_child.velocity.y, mask_child.velocity.z);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Suelo"))
        {
            Contacto_Suelo = true;
            isJumping = false;
            currentJumps = maxJumps; // resetear saltos al tocar suelo
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
