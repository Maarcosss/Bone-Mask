using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollowInputCooldown : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Dead Zone")]
    public float deadZoneWidth = 4f;
    public float deadZoneHeight = 2f;

    [Header("Camera Settings")]
    public float followSpeed = 5f;
    public float smoothDamp = 0.3f;
    public float verticalOffset = 2f;

    [Header("Vertical Look Cooldown")]
    public float verticalCooldown = 0.5f;
    public float holdTimeToActivate = 0.1f;

    [Header("Input System")]
    public InputActionReference verticalLookAction;

    private Vector3 deadZoneCenter;
    private Vector3 velocity = Vector3.zero;
    private float currentVerticalOffset = 0f;
    private float verticalCooldownTimer = 0f;
    private float holdTimer = 0f;
    private bool isHoldingInput = false;
    private float currentDirection = 0f;

    void OnEnable()
    {
        verticalLookAction.action.Enable();
    }

    void OnDisable()
    {
        verticalLookAction.action.Disable();
    }

    void Start()
    {
        if (player != null)
        {
            deadZoneCenter = player.position;
        }
    }

    void Update()
    {
        float input = verticalLookAction.action.ReadValue<float>();
        float inputDirection = 0f;
        bool hasInput = false;

        if (Mathf.Abs(input) > 0.1f)
        {
            hasInput = true;
            inputDirection = Mathf.Sign(input);
        }

        if (hasInput)
        {
            if (!isHoldingInput)
            {
                holdTimer = 0f;
                currentDirection = inputDirection;
                isHoldingInput = true;
            }

            if (inputDirection != currentDirection)
            {
                holdTimer = 0f;
                currentDirection = inputDirection;
            }

            holdTimer += Time.deltaTime;

            if (holdTimer >= holdTimeToActivate && verticalCooldownTimer <= 0f)
            {
                currentVerticalOffset = currentDirection * verticalOffset;
                verticalCooldownTimer = verticalCooldown;
            }
        }
        else
        {
            if (isHoldingInput)
            {
                isHoldingInput = false;
                holdTimer = 0f;
            }
        }

        if (verticalCooldownTimer > 0f)
        {
            verticalCooldownTimer -= Time.deltaTime;
        }

        // Suavizado para que vuelva al eje del jugador
        if (!hasInput && currentVerticalOffset != 0f)
        {
            currentVerticalOffset = Mathf.Lerp(currentVerticalOffset, 0f, Time.deltaTime * followSpeed);
        }
    }

    void LateUpdate()
    {
        if (player == null)
        {
            return;
        }

        Vector3 playerPos = player.position;
        Vector3 newCenter = deadZoneCenter;

        if (playerPos.x < deadZoneCenter.x - deadZoneWidth / 2f)
        {
            newCenter.x = playerPos.x + deadZoneWidth / 2f;
        }
        else
        {
            if (playerPos.x > deadZoneCenter.x + deadZoneWidth / 2f)
            {
                newCenter.x = playerPos.x - deadZoneWidth / 2f;
            }
        }

        if (playerPos.y < deadZoneCenter.y - deadZoneHeight / 2f)
        {
            newCenter.y = playerPos.y + deadZoneHeight / 2f;
        }
        else
        {
            if (playerPos.y > deadZoneCenter.y + deadZoneHeight / 2f)
            {
                newCenter.y = playerPos.y - deadZoneHeight / 2f;
            }
        }

        deadZoneCenter = Vector3.Lerp(deadZoneCenter, newCenter, Time.deltaTime * followSpeed);

        Vector3 targetPos = new Vector3(deadZoneCenter.x, deadZoneCenter.y + currentVerticalOffset, transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothDamp);
    }

    void OnDrawGizmosSelected()
    {
        if (player == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(deadZoneCenter, new Vector3(deadZoneWidth, deadZoneHeight, 0f));
    }
}
