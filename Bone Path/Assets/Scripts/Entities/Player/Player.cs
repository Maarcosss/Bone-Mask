using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class Player : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private Image heart1;
    [SerializeField] private Image heart2;
    [SerializeField] private Image heart3;
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite emptyHeart;
    public int health = 3;
    public int maxHealth = 3;
    public float soul = 0f;
    public float maxSoul = 100f;
    [Range(0.01f, 1f)][SerializeField] private float healCostPercent = 0.5f;
    [SerializeField] private float healTime = 1.0f;

    [Header("Coin Settings")]
    public int coins = 0;
    public TMP_Text coinText;

    [Header("UI")]
    public TMP_Text soulText;

    private float healTimer = 0f;

    private Rigidbody rigidBody;
    private PlayerInput playerInput;
    private Vector2 input;

    [Header("Movement Settings")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float airControlMultiplier = 0.5f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float gravityMultiplier = 2f;

    [Header("Input System")]
    public InputActionReference healAction;
    public InputActionReference interactAction;
    public InputActionReference pauseAction;

    [Header("Damage Settings")]
    [SerializeField] private float damageCooldown = 1f;
    private float damageTimer = 0f;

    private bool isGrounded;

    // ---------------------------
    //       ANIMATION
    // ---------------------------
    [Header("Animations")]
    [SerializeField] private Animator animator;

    public ManagerOptions ManagerOptionsRef;

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        rigidBody.freezeRotation = true;

        // Animator reference
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (damageTimer > 0f)
            damageTimer -= Time.deltaTime;

        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);

        Move();
        HandleHealing();
        UpdateHeartsUI();
        UpdateSoulUI();
        Pause();

        HandleAnimations();
    }

    private void FixedUpdate()
    {
        float control = isGrounded ? 1f : airControlMultiplier;
        Vector3 targetVelocity = new Vector3(input.x * speed * control, rigidBody.velocity.y, 0f);
        rigidBody.velocity = targetVelocity;

        if (!isGrounded)
            rigidBody.AddForce(Physics.gravity * (gravityMultiplier - 1f) * rigidBody.mass);

        if (Mathf.Abs(input.x) > 0.01f)
            transform.rotation = input.x < 0f ? Quaternion.Euler(0f, 180f, 0f) : Quaternion.Euler(0f, 0f, 0f);
    }

    // Animation handling
    private void HandleAnimations()
    {
        animator.SetBool("IsRunning", Mathf.Abs(input.x) > 0.01f && isGrounded);
        animator.SetBool("isGrounded", isGrounded);

        // Si el personaje vuelve al suelo, desactiva el bool jump.
        if (isGrounded)
            animator.SetBool("jump", false);
    }

    void OnEnable()
    {
        healAction?.action.Enable();
        interactAction?.action.Enable();
    }

    void OnDisable()
    {
        healAction?.action.Disable();
        interactAction?.action.Disable();
    }

    private void Pause()
    {
        if (pauseAction != null && pauseAction.action.ReadValue<float>() > 0.1f)
            ManagerOptionsRef.Pause();
    }

    public void Move()
    {
        input = playerInput.actions["Move"].ReadValue<Vector2>();
    }

    public void Jump(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.performed && isGrounded)
        {
            rigidBody.velocity = new Vector3(rigidBody.velocity.x, 0f, rigidBody.velocity.z);
            rigidBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            animator.SetBool("Jump", true); // Activamos el bool
        }
    }


    private void HandleHealing()
    {
        float requiredSoul = maxSoul * healCostPercent;

        if (healAction != null && healAction.action.ReadValue<float>() > 0.1f &&
            soul >= requiredSoul && health < maxHealth)
        {
            healTimer += Time.deltaTime;

            if (healTimer >= healTime)
            {
                HealPlayer();
                healTimer = 0f;
            }
        }
        else
        {
            healTimer = 0f;
        }
    }

    private void HealPlayer()
    {
        // Healing animation
        animator.SetTrigger("heal");

        health = Mathf.Min(health + 1, maxHealth);
        soul -= maxSoul * healCostPercent;
        soul = Mathf.Max(soul, 0f);
        UpdateSoulUI();
    }

    private void UpdateHeartsUI()
    {
        heart1.sprite = health >= 1 ? fullHeart : emptyHeart;
        heart2.sprite = health >= 2 ? fullHeart : emptyHeart;
        heart3.sprite = health >= 3 ? fullHeart : emptyHeart;
    }

    public void UpdateSoulUI()
    {
        if (soulText != null)
            soulText.text = Mathf.FloorToInt(soul).ToString();
    }

    private void UpdateCoinUI()
    {
        if (coinText != null)
            coinText.text = coins.ToString();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Checkpoint") && interactAction != null &&
            interactAction.action.ReadValue<float>() > 0.1f)
        {
            SaveData();
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Coin"))
        {
            Destroy(collision.gameObject);
            coins++;
            UpdateCoinUI();
        }

        if (collision.gameObject.CompareTag("Enemy") && damageTimer <= 0f)
        {
            health = Mathf.Max(health - 1, 0);
            damageTimer = damageCooldown;

            // Hurt animation
            animator.SetTrigger("hurt");

            UpdateHeartsUI();
        }
    }

    public void SaveData()
    {
        SaveManager.SavePlayerData(this);
        Debug.Log("DATOS GUARDADOS");
    }

    public void LoadData()
    {
        PlayerData playerData = SaveManager.LoadPlayerData();
        soul = playerData.soul;
        health = playerData.health;
        coins = playerData.coins;
        transform.position = new Vector3(playerData.position[0], playerData.position[1], playerData.position[2]);
        UpdateSoulUI();
        UpdateHeartsUI();
        Debug.Log("Datos cargados");
    }
}
