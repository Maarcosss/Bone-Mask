using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;       // Jugador
    public Vector3 offset = new Vector3(0, 0, -10f); // Z fijo
    public Vector2 deadZoneSize = new Vector2(2f, 2f); // Tamaño de la zona muerta
    public float smoothTime = 0.15f; // Suavizado

    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (!target) return;

        Vector3 targetPos = target.position + offset;
        Vector3 camPos = transform.position;

        // Calcular diferencia
        float deltaX = targetPos.x - camPos.x;
        float deltaY = targetPos.y - camPos.y;

        Vector3 desiredPosition = camPos;

        // Solo mover la cámara si el jugador sale de la dead zone
        if (Mathf.Abs(deltaX) > deadZoneSize.x)
        {
            desiredPosition.x = targetPos.x - Mathf.Sign(deltaX) * deadZoneSize.x;
        }

        if (Mathf.Abs(deltaY) > deadZoneSize.y)
        {
            desiredPosition.y = targetPos.y - Mathf.Sign(deltaY) * deadZoneSize.y;
        }

        // Suavizado tipo Hollow Knight
        transform.position = Vector3.SmoothDamp(camPos, desiredPosition, ref velocity, smoothTime);
    }
}
