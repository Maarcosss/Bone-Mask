using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Dead Zone (Hollow Knight Style)")]
    [Tooltip("Horizontal dead zone - camera won't move until player exits this area")]
    public float deadZoneWidth = 4f;
    [Tooltip("Vertical dead zone - camera won't move until player exits this area")]
    public float deadZoneHeight = 2f;
    [Tooltip("Show dead zone in Scene view for debugging")]
    public bool showDeadZoneGizmo = true;

    [Header("Camera Follow Settings")]
    [Tooltip("How fast camera follows when player exits dead zone")]
    public float followSpeed = 2f;
    [Tooltip("Smoothing factor for camera movement (lower = smoother)")]
    public float smoothDamping = 0.3f;
    private Vector3 velocity = Vector3.zero;

    [Header("Look Ahead System")]
    [Tooltip("How far ahead to look when player is moving")]
    public float lookAheadDistance = 3f;
    [Tooltip("How fast to apply look ahead")]
    public float lookAheadSmooth = 2f;
    [Tooltip("Minimum player speed to trigger look ahead")]
    public float lookAheadThreshold = 0.1f;
    private float currentLookAhead = 0f;

    [Header("Vertical Look Controls")]
    [Tooltip("How far up the camera moves when looking up")]
    public float lookUpOffset = 3f;
    [Tooltip("How far down the camera moves when looking down")]
    public float lookDownOffset = -3f;
    [Tooltip("How fast vertical look interpolates")]
    public float verticalLookSpeed = 3f;
    private float currentVerticalLook = 0f;

    [Header("Camera Boundaries")]
    public bool useCameraBounds = false;
    [Tooltip("Camera won't go beyond these boundaries")]
    public Vector2 minBounds;
    public Vector2 maxBounds;

    [Header("Hollow Knight Features")]
    [Tooltip("Slight upward bias to focus on what's ahead")]
    public float verticalBias = 0.5f;
    [Tooltip("How fast camera snaps to new focus points")]
    public float focusSnapSpeed = 1f;

    [Header("Input System")]
    public InputActionAsset inputActions;

    // Input actions
    private InputAction lookAction;
    private InputAction cameraAction;

    // Input values
    private Vector2 lookInput;
    private float cameraInput;

    // Camera state
    private Vector3 targetPosition;
    private Vector3 deadZoneCenter;
    private Rigidbody playerRb;

    public bool validar_inputs_camara = true;

    void Start()
    {
        SetupInputActions();

        // Get player rigidbody for velocity calculations
        if (player != null)
        {
            playerRb = player.GetComponent<Rigidbody>();
            deadZoneCenter = player.position;
        }
    }

    void SetupInputActions()
    {
        if (inputActions == null)
        {
            inputActions = Resources.Load<InputActionAsset>("InputSystem_Actions");
        }

        if (inputActions != null)
        {
            var playerActionMap = inputActions.FindActionMap("Player");
            if (playerActionMap != null)
            {
                lookAction = playerActionMap.FindAction("Look");
            }
        }

        // Create separate camera action for keyboard W/S only
        cameraAction = new InputAction("CameraVertical", InputActionType.Value, expectedControlType: "Axis");
        cameraAction.AddCompositeBinding("1DAxis")
            .With("Positive", "<Keyboard>/w")
            .With("Negative", "<Keyboard>/s");

        // Fallback for look action
        if (lookAction == null)
        {
            lookAction = new InputAction("Look", InputActionType.Value, expectedControlType: "Vector2");
            lookAction.AddBinding("<Gamepad>/rightStick");
        }

        SetupInputCallbacks();
        EnableInputActions();
    }

    void SetupInputCallbacks()
    {
        lookAction.performed += OnLook;
        lookAction.canceled += OnLook;
        cameraAction.performed += OnCameraVertical;
        cameraAction.canceled += OnCameraVertical;
    }

    void EnableInputActions()
    {
        lookAction?.Enable();
        cameraAction?.Enable();
    }

    void DisableInputActions()
    {
        lookAction?.Disable();
        cameraAction?.Disable();
    }

    void OnDestroy()
    {
        DisableInputActions();
    }

    void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    void OnCameraVertical(InputAction.CallbackContext context)
    {
        cameraInput = context.ReadValue<float>();
    }

    void LateUpdate()
    {
        if (player == null) return;

        UpdateDeadZone();
        UpdateLookAhead();
        UpdateVerticalLook();
        UpdateCameraPosition();
        ApplyBounds();
    }

    void UpdateDeadZone()
    {
        Vector3 playerPos = player.position;
        Vector3 cameraPos = transform.position;

        // Calculate dead zone boundaries
        float leftBound = deadZoneCenter.x - deadZoneWidth * 0.5f;
        float rightBound = deadZoneCenter.x + deadZoneWidth * 0.5f;
        float bottomBound = deadZoneCenter.y - deadZoneHeight * 0.5f;
        float topBound = deadZoneCenter.y + deadZoneHeight * 0.5f;

        // Check if player is outside dead zone
        Vector3 newDeadZoneCenter = deadZoneCenter;

        // Horizontal dead zone check
        if (playerPos.x < leftBound)
        {
            newDeadZoneCenter.x = playerPos.x + deadZoneWidth * 0.5f;
        }
        else if (playerPos.x > rightBound)
        {
            newDeadZoneCenter.x = playerPos.x - deadZoneWidth * 0.5f;
        }

        // Vertical dead zone check
        if (playerPos.y < bottomBound)
        {
            newDeadZoneCenter.y = playerPos.y + deadZoneHeight * 0.5f;
        }
        else if (playerPos.y > topBound)
        {
            newDeadZoneCenter.y = playerPos.y - deadZoneHeight * 0.5f;
        }

        // Smoothly move dead zone center
        deadZoneCenter = Vector3.Lerp(deadZoneCenter, newDeadZoneCenter, Time.deltaTime * followSpeed);
    }

    void UpdateLookAhead()
    {
        float targetLookAhead = 0f;

        if (playerRb != null)
        {
            // Use velocity for more responsive look ahead
            float horizontalVelocity = playerRb.velocity.x;

            if (Mathf.Abs(horizontalVelocity) > lookAheadThreshold)
            {
                targetLookAhead = Mathf.Sign(horizontalVelocity) * lookAheadDistance;
            }
        }
        else
        {
            // Fallback: use player scale for direction
            if (player.localScale.x > 0)
            {
                targetLookAhead = lookAheadDistance;
            }
            else if (player.localScale.x < 0)
            {
                targetLookAhead = -lookAheadDistance;
            }
        }

        currentLookAhead = Mathf.Lerp(currentLookAhead, targetLookAhead, Time.deltaTime * lookAheadSmooth);
    }

    void UpdateVerticalLook()
    {
        float targetVerticalLook = 0f;

        if (validar_inputs_camara)
        {
            // Keyboard input takes priority
            if (Mathf.Abs(cameraInput) > 0.1f)
            {
                if (cameraInput > 0)
                {
                    targetVerticalLook = lookUpOffset;
                }
                else
                {
                    targetVerticalLook = lookDownOffset;
                }
            }
            // Controller right stick input
            else if (Mathf.Abs(lookInput.y) > 0.1f)
            {
                float stickInput = -lookInput.y; // Inverted for natural feel

                if (stickInput > 0)
                {
                    targetVerticalLook = stickInput * lookUpOffset;
                }
                else
                {
                    targetVerticalLook = stickInput * Mathf.Abs(lookDownOffset);
                }
            }
        }

        currentVerticalLook = Mathf.Lerp(currentVerticalLook, targetVerticalLook, Time.deltaTime * verticalLookSpeed);
    }

    void UpdateCameraPosition()
    {
        // Calculate target position based on dead zone center + look ahead + vertical look + bias
        targetPosition = new Vector3(
            deadZoneCenter.x + currentLookAhead,
            deadZoneCenter.y + currentVerticalLook + verticalBias,
            transform.position.z
        );

        // Smooth camera movement using SmoothDamp for Hollow Knight-like feel
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothDamping);
    }

    void ApplyBounds()
    {
        if (!useCameraBounds) return;

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minBounds.x, maxBounds.x);
        pos.y = Mathf.Clamp(pos.y, minBounds.y, maxBounds.y);
        transform.position = pos;
    }

    // Gizmos for visualizing dead zone in Scene view
    void OnDrawGizmosSelected()
    {
        if (!showDeadZoneGizmo) return;

        if (Application.isPlaying)
        {
            // Draw current dead zone
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(deadZoneCenter, new Vector3(deadZoneWidth, deadZoneHeight, 0f));

            // Draw target position
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(targetPosition, 0.2f);
        }
        else if (player != null)
        {
            // Draw initial dead zone in edit mode
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(player.position, new Vector3(deadZoneWidth, deadZoneHeight, 0f));
        }
    }

    // Public methods for external control (like room transitions)
    public void SetDeadZoneCenter(Vector3 newCenter)
    {
        deadZoneCenter = newCenter;
    }

    public void FocusOnPosition(Vector3 focusPoint, float focusTime = 1f)
    {
        StartCoroutine(FocusCoroutine(focusPoint, focusTime));
    }

    private System.Collections.IEnumerator FocusCoroutine(Vector3 focusPoint, float focusTime)
    {
        Vector3 originalTarget = targetPosition;
        float elapsed = 0f;

        while (elapsed < focusTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / focusTime;

            Vector3 focusTarget = new Vector3(focusPoint.x, focusPoint.y, transform.position.z);
            transform.position = Vector3.Lerp(originalTarget, focusTarget, t * focusSnapSpeed);

            yield return null;
        }

        // Reset dead zone to new focus point
        deadZoneCenter = focusPoint;
    }
}
