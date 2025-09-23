using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Dead Zone")]
    public Vector2 deadZoneSize = new Vector2(3f, 2f); // ancho/alto de la zona muerta

    [Header("Smoothness")]
    public float smoothTime = 0.2f;
    private Vector3 velocity = Vector3.zero;

    [Header("Offset dinámico")]
    public float lookAheadDistance = 2f;
    public float verticalOffset = 1f;

    [Header("Mirar arriba/abajo")]
    public float lookUpOffset = 2f;
    public float lookDownOffset = -2f;
    public float lookSmooth = 5f;
    private float currentVerticalLook = 0f;

    [Header("Límites del mapa")]
    public bool useLimits = false;
    public Vector2 minLimits;
    public Vector2 maxLimits;

    private Vector3 targetPosition;

    public bool validar_inputs_camara = true;

    void LateUpdate()
    {
        if (player == null) return;

        // Diferencia entre cámara y jugador
        Vector3 diff = transform.position - player.position;

        float offsetX = 0f;
        float offsetY = 0f;

        // Dead zone en X
        if (diff.x > deadZoneSize.x)
        {
            offsetX = -deadZoneSize.x;
        }
        else if (diff.x < -deadZoneSize.x)
        {
            offsetX = deadZoneSize.x;
        }

        // Dead zone en Y
        if (diff.y > deadZoneSize.y)
        {
            offsetY = -deadZoneSize.y;
        }
        else if (diff.y < -deadZoneSize.y)
        {
            offsetY = deadZoneSize.y;
        }

        // Look ahead en X
        float lookAhead = 0f;
        if (player.localScale.x > 0)
        {
            lookAhead = lookAheadDistance;
        }
        else
        {
            lookAhead = -lookAheadDistance;
        }

        // --- Mirar arriba y abajo (con W y S) ---
        float targetVerticalLook = 0f;

        if (validar_inputs_camara)
        {

            if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.S))
            {
                return;
            }
            if (Input.GetKey(KeyCode.W))
            {
                targetVerticalLook = lookUpOffset;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                targetVerticalLook = lookDownOffset;
            }

        }
        

        currentVerticalLook = currentVerticalLook + (targetVerticalLook - currentVerticalLook) * Time.deltaTime * lookSmooth;

        // Posición objetivo
        targetPosition = new Vector3(
            player.position.x + lookAhead,
            player.position.y + verticalOffset + currentVerticalLook,
            transform.position.z
        );

        // Movimiento suavizado manual (sin Mathf.SmoothDamp)
        velocity = (targetPosition - transform.position) * (Time.deltaTime / smoothTime);
        transform.position += velocity;

        // Aplicar límites si están activos
        if (useLimits)
        {
            float posX = transform.position.x;
            float posY = transform.position.y;

            if (posX < minLimits.x) posX = minLimits.x;
            if (posX > maxLimits.x) posX = maxLimits.x;
            if (posY < minLimits.y) posY = minLimits.y;
            if (posY > maxLimits.y) posY = maxLimits.y;

            transform.position = new Vector3(posX, posY, transform.position.z);
        }
    }
}
