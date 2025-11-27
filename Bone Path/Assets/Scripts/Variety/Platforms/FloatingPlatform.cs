using UnityEngine;

/*Author: Alejandro Cruz
Date: 20 - Nov - 2025*/

public class FloatingPlatform : MonoBehaviour
{
    [Header("Floating movement")]
    [SerializeField] private float floatAmplitude = 0.05f;
    [SerializeField] private float floatFrequency = 2f;

    [Header("Sinking on contact")]
    [SerializeField] private float sinkAmount = 0.2f;
    [SerializeField] private float sinkSpeed = 2f;

    private Vector3 initialPosition;
    private Vector3 targetOffset;
    private bool playerOnPlatform = false;
    private Transform playerTransform;      
    private Vector3 lastPlatformPosition;

    void Start()
    {
        initialPosition = transform.position;
        targetOffset = Vector3.zero;
        lastPlatformPosition = transform.position;
    }

    void Update()
    {
        float floatOffset = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;

        Vector3 desiredOffset;

        if (playerOnPlatform)
        {
            desiredOffset = new Vector3(0, -sinkAmount, 0);
        }
        else
        {
            desiredOffset = Vector3.zero;
        }

        targetOffset = Vector3.Lerp(targetOffset, desiredOffset, Time.deltaTime * sinkSpeed);

        Vector3 newPosition = initialPosition + new Vector3(0, floatOffset, 0) + targetOffset;

        if (playerOnPlatform && playerTransform != null)
        {
            Vector3 platformDelta = newPosition - lastPlatformPosition;
            playerTransform.position += platformDelta;
        }

        transform.position = newPosition;
        lastPlatformPosition = newPosition;
    }

    //Detects when the player steps on the platform
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerOnPlatform = true;
            playerTransform = collision.transform;
        }
    }

    //Detects when the player leaves the platform
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerOnPlatform = false;
            playerTransform = null;
        }
    }

    //Draws a gizmo showing the sinking limit in the editor
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position - new Vector3(0, sinkAmount, 0));
        }
    }
}
