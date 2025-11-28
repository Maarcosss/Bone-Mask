using UnityEngine;

/*Author: Marcos Isar
Date: 20 - Nov - 2025*/

public class PassiveEnemyAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float normalMoveSpeed = 2f;
    public float wanderRadius = 8f;
    public float wanderInterval = 3f;
    public float playerDetectionRadius = 6f;
    public MovementPattern movementPattern = MovementPattern.Random;

    public enum MovementPattern { Random, Circle, BackAndForth }

    [Header("Enemy Stats")]
    public int maxHealth = 3;
    private int currentHealth;
    private bool isDead = false;

    private Vector3 initialPosition;
    private Vector3 currentTarget;
    private float wanderTimer = 0f;
    private int backAndForthDirection = 1;
    private int wanderDirection = 1;
    private bool isMoving = false;

    public Transform playerTransform;
    private CoinDropper coinDropper;
    private Rigidbody rb;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();
        coinDropper = GetComponent<CoinDropper>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.freezeRotation = true;

        initialPosition = transform.position;
        InitializeMovement();
    }

    //Sets initial wander direction, timer and target
    void InitializeMovement()
    {
        wanderDirection = Random.Range(0, 2) == 0 ? -1 : 1;
        wanderTimer = Random.Range(0.5f, 1.5f);
        SetNewWanderTarget();
        isMoving = true;
    }

    //Updates enemy movement every fixed frame, decides whether to chase player or wander
    void FixedUpdate()
    {
        if (rb == null || isDead) return;

        bool playerInRange = IsPlayerInRange();

        if (playerInRange)
            ChasePlayer();
        else
            UpdateNormalMovement();
    }

    //Handles normal wandering movement and changes target if needed
    void UpdateNormalMovement()
    {
        wanderTimer -= Time.fixedDeltaTime;
        bool shouldChangeTarget = wanderTimer <= 0f;
        bool reachedTarget = Vector3.Distance(transform.position, currentTarget) < 1f;

        if (shouldChangeTarget || reachedTarget)
        {
            SetNewWanderTarget();
            wanderTimer = wanderInterval + Random.Range(-0.5f, 0.5f);
        }

        MoveToTarget();
        isMoving = true;
    }

    //Checks if the player is within detection range
    bool IsPlayerInRange()
    {
        if (playerTransform == null) return false;
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        return distanceToPlayer <= playerDetectionRadius;
    }

    //Sets a new wander target depending on the movement pattern
    void SetNewWanderTarget()
    {
        Vector3 newTarget;

        switch (movementPattern)
        {
            case MovementPattern.Random:
                if (Random.Range(0f, 1f) < 0.3f) wanderDirection *= -1;
                float randomDistance = Random.Range(3f, wanderRadius);
                newTarget = transform.position + new Vector3(wanderDirection * randomDistance, 0f, 0f);
                float distanceFromStart = Vector3.Distance(newTarget, initialPosition);
                if (distanceFromStart > wanderRadius)
                {
                    wanderDirection = (int)Mathf.Sign(initialPosition.x - transform.position.x);
                    newTarget = transform.position + new Vector3(wanderDirection * randomDistance * 0.5f, 0f, 0f);
                }
                break;

            case MovementPattern.Circle:
                float angle = Time.time * 0.5f;
                newTarget = initialPosition + new Vector3(
                    Mathf.Cos(angle) * wanderRadius * 0.5f,
                    0f,
                    Mathf.Sin(angle) * wanderRadius * 0.5f
                );
                break;

            case MovementPattern.BackAndForth:
                newTarget = initialPosition + new Vector3(backAndForthDirection * wanderRadius * 0.5f, 0f, 0f);
                if (Vector3.Distance(transform.position, newTarget) < 1f)
                {
                    backAndForthDirection *= -1;
                    newTarget = initialPosition + new Vector3(backAndForthDirection * wanderRadius * 0.5f, 0f, 0f);
                }
                break;

            default:
                newTarget = initialPosition;
                break;
        }

        currentTarget = newTarget;
    }

    //Moves enemy toward the current target depending on movement pattern
    void MoveToTarget()
    {
        if (rb.isKinematic) return;

        Vector3 targetVelocity = Vector3.zero;
        if (movementPattern == MovementPattern.Random || movementPattern == MovementPattern.BackAndForth)
        {
            float directionX = Mathf.Sign(currentTarget.x - transform.position.x);
            if (Mathf.Abs(directionX) < 0.1f) directionX = wanderDirection;
            targetVelocity = new Vector3(directionX * normalMoveSpeed, rb.velocity.y, 0f);
        }
        else
        {
            Vector3 direction = (currentTarget - transform.position).normalized;
            direction.y = 0;
            targetVelocity = direction * normalMoveSpeed;
            targetVelocity.y = rb.velocity.y;
        }

        rb.velocity = targetVelocity;
    }

    //Chases the player horizontally if in range
    void ChasePlayer()
    {
        if (playerTransform == null || rb.isKinematic) return;

        float directionX = Mathf.Sign(playerTransform.position.x - transform.position.x);
        rb.velocity = new Vector3(directionX * normalMoveSpeed, rb.velocity.y, 0f);
    }

    //Reduces health by specified amount and checks for death
    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        if (currentHealth <= 0) Die();
    }

    //Handles enemy death, stops movement, drops coins and destroys object
    private void Die()
    {
        isDead = true;
        rb.velocity = Vector3.zero;
        if (coinDropper != null)
        {
            coinDropper.DropCoins();
        }
        Destroy(gameObject);
    }

    //Returns whether the enemy is moving
    public bool IsMoving()
    {
        return isMoving && rb != null && rb.velocity.magnitude > 0.1f;
    }

    //Draws gizmos in editor for detection range, wander radius and current target
    void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying ? initialPosition : transform.position;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, playerDetectionRadius);

        Gizmos.color = Color.blue;
        if (movementPattern == MovementPattern.Random || movementPattern == MovementPattern.BackAndForth)
            Gizmos.DrawWireCube(center, new Vector3(wanderRadius * 2f, 1f, 1f));
        else
            Gizmos.DrawWireSphere(center, wanderRadius);

        if (Application.isPlaying && currentTarget != Vector3.zero)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(currentTarget, 0.5f);
            Gizmos.DrawLine(transform.position, currentTarget);
        }
    }
}
