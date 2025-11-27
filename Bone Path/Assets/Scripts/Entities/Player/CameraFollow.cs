using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollowInputC : MonoBehaviour
{
    //Autoría o Encargado del Script completo: David Gomez Gines

    [SerializeField] private Transform player;
    [SerializeField] private BoxCollider2D camBox;
    //Array of objects for finding the boundaries.
    [SerializeField] private GameObject[] boundaries;
    //Array of box colliders bounds for all of objects to find.
    [SerializeField] private Bounds[] allBounds;
    //This is the One where the players is in.
    [SerializeField] private Bounds targetBounds;

    [SerializeField] private float speed;
    [SerializeField] private float verticalMoveSpeed;
    [SerializeField] private float waitForSeconds = 0.5f;

    [SerializeField] private Vector2 input;
    [SerializeField] private PlayerInput playerInput;

    //On the start we get the Player and the Camera collider. And call FindLimits.
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

    //Executes once per frame, but after the updates.
    private void LateUpdate()
    {
        //Wait he half of a second. In orde to be smooth.
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
    //Finds all limits of the stage enviroment.
    void FindLimits() 
    {
        // We find all GO with the tag "Boundary".
        boundaries = GameObject.FindGameObjectsWithTag("Boundary");
        // We give the array the same length that the above one.
        allBounds = new Bounds[boundaries.Length];
        // Loop to get allof the box colliders bounds from each of the found objects.
        for (int i = 0; i < boundaries.Length; i++)
        {
            allBounds[i] = boundaries[i].gameObject.GetComponent<BoxCollider2D>().bounds;
        }

    }
    //Locates in witch boundary the player is.
    void SetOneLimit()
    {
        //We go through all of the bounds.
        for (int i = 0; i < allBounds.Length; i++)
        {
            //Checks if the player is inside any of the boundaries.
            if (player.position.x > allBounds[i].min.x && player.position.x < allBounds[i].max.x && player.position.y > allBounds[i].min.y && player.position.y < allBounds[i].max.y)
            {
                //The we make targetBound taht specific bound.
                targetBounds = allBounds[i];
                //we stop the method, so it do not chek the rest.
                return;
            }
        }
    }

    /*void FollowPlayer() //This the previous code of FolloPlayer, in this version the camera can go up neider down.
    {
        //We use Mathf.Clamp to target the player and set the minimun and maximun position of the camera.
        //camBox.size.x < targetBounds.size.x Checks if the width of the camera is less than all the area where it can move.
        // ? (value if the condition it's true) / : (value if the condition it's false);
            // if the condition it's true: Mathf.Clamp( player.position.x, targetBounds.min.x + camBox.size.x / 2, targetBounds.max.x - camBox.size.x / 2
                //if the condition it's false: (targetBounds.min.x + targetBounds.max.x) / 2 The camera keeps in the center.
        float xTarget = camBox.size.x < targetBounds.size.x ? Mathf.Clamp(player.position.x, targetBounds.min.x + camBox.size.x / 2, targetBounds.max.x - camBox.size.x/2) : (targetBounds.min.x + targetBounds.max.x) / 2;
        //The same as above but for the vertical position. But this one just happend if the camera is smaller than the boundary.
        float yTarget = camBox.size.y < targetBounds.size.y ? Mathf.Clamp(player.position.y, targetBounds.min.y + camBox.size.y / 2, targetBounds.max.y - camBox.size.y/2) : (targetBounds.min.y + targetBounds.max.y) / 2;
        //And target to limit the camera. The z keeps so it doesn't move.
        Vector3 target = new Vector3(xTarget, yTarget, transform.position.z);
        //We uses Lerp to move the camera to the new target vector.
        transform.position = Vector3.Lerp(transform.position, target, speed * Time.deltaTime);

    }*/

    void FollowPlayer()
    {
        //We use Mathf.Clamp to target the player and set the minimun and maximun position of the camera.
        float xTarget = camBox.size.x < targetBounds.size.x
            ? Mathf.Clamp(player.position.x,
                targetBounds.min.x + camBox.size.x / 2,
                targetBounds.max.x - camBox.size.x / 2)
            : (targetBounds.min.x + targetBounds.max.x) / 2;

        //Vertical movement with the manual change.

        // //The same as above but for the vertical position. But this one just happend if the camera is smaller than the boundary.
        float playerYTarget = camBox.size.y < targetBounds.size.y
            ? Mathf.Clamp(player.position.y,
                targetBounds.min.y + camBox.size.y / 2,
                targetBounds.max.y - camBox.size.y / 2)
            : (targetBounds.min.y + targetBounds.max.y) / 2;

        // If input it's vertical, (W/S).
        if (Mathf.Abs(input.y) > 0.1f)
        {
            // Desplaza la cámara un poco en esa dirección
            float manualOffset = input.y * 2f; // puedes ajustar este valor
            playerYTarget += manualOffset;
        }

        // Clamp final dentro del boundary
        float yTarget = Mathf.Clamp(playerYTarget,
            targetBounds.min.y + camBox.size.y / 2,
            targetBounds.max.y - camBox.size.y / 2);

        // --- MOVIMIENTO FINAL SUAVE ---
        Vector3 target = new Vector3(xTarget, yTarget, transform.position.z);
        transform.position = target;

    }
    public void MoveCam()
    {
        input = playerInput.actions["Camera"].ReadValue<Vector2>();
    }
}
