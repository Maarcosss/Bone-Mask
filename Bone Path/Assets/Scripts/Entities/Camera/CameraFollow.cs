using UnityEngine;
using UnityEngine.InputSystem;

/*Author: David Gomez
Date: 20 - Nov - 2025*/

public class CameraFollowInputC : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private BoxCollider2D camBox;
    [SerializeField] private GameObject[] boundaries;
    [SerializeField] private Bounds[] allBounds;
    [SerializeField] private Bounds targetBounds;

    [SerializeField] private float speed = 5f;
    [SerializeField] private float verticalMoveSpeed = 2f;
    [SerializeField] private float waitForSeconds = 0.5f;

    [SerializeField] private Vector2 input;
    [SerializeField] private PlayerInput playerInput;

    [HideInInspector] public Vector3 shakeOffset = Vector3.zero;

    private void Start()
    {
        player = GameObject.Find("Player").GetComponent<Transform>();
        camBox = GetComponent<BoxCollider2D>();
        playerInput = GameObject.Find("Player").GetComponent<PlayerInput>();
        FindLimits();
    }

    private void Update()
    {
        MoveCam();
    }

    private void LateUpdate()
    {
        if (waitForSeconds > 0)
        {
            waitForSeconds -= Time.deltaTime;
        }
        else
        {
            SetOneLimit();
            FollowPlayer();
        }
    }

    void FindLimits()
    {
        boundaries = GameObject.FindGameObjectsWithTag("Boundary");
        allBounds = new Bounds[boundaries.Length];

        for (int i = 0; i < boundaries.Length; i++)
        {
            allBounds[i] = boundaries[i].GetComponent<BoxCollider2D>().bounds;
        }
    }

    void SetOneLimit()
    {
        for (int i = 0; i < allBounds.Length; i++)
        {
            if (player.position.x > allBounds[i].min.x && player.position.x < allBounds[i].max.x &&
                player.position.y > allBounds[i].min.y && player.position.y < allBounds[i].max.y)
            {
                targetBounds = allBounds[i];
                return;
            }
        }
    }

    void FollowPlayer()
    {
        float xTarget = camBox.size.x < targetBounds.size.x
            ? Mathf.Clamp(player.position.x,
                targetBounds.min.x + camBox.size.x / 2,
                targetBounds.max.x - camBox.size.x / 2)
            : (targetBounds.min.x + targetBounds.max.x) / 2;

        float playerYTarget = camBox.size.y < targetBounds.size.y
            ? Mathf.Clamp(player.position.y,
                targetBounds.min.y + camBox.size.y / 2,
                targetBounds.max.y - camBox.size.y / 2)
            : (targetBounds.min.y + targetBounds.max.y) / 2;

        if (Mathf.Abs(input.y) > 0.1f)
        {
            float manualOffset = input.y * 2f;
            playerYTarget += manualOffset;
        }

        float yTarget = Mathf.Clamp(playerYTarget,
            targetBounds.min.y + camBox.size.y / 2,
            targetBounds.max.y - camBox.size.y / 2);

        Vector3 target = new Vector3(xTarget, yTarget, transform.position.z) + shakeOffset;
        transform.position = Vector3.Lerp(transform.position, target, speed * Time.deltaTime);

    }

    public void MoveCam()
    {
        input = playerInput.actions["Camera"].ReadValue<Vector2>();
    }
}
