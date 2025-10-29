using UnityEngine;

public class ActiveEnemyAI : MonoBehaviour
{
    [Header("AI Behavior - Active Enemy")]
    public float detectionRange = 8f;
    public float attackRange = 2f;
    public float moveSpeed = 3f;
    public float patrolSpeed = 1.5f;

    [Header("Patrol Settings")]
    public float patrolRadius = 5f;
    public float patrolChangeInterval = 3f;

    [Header("Enemy Stats")]
    public int maxHealth = 3;
    private int currentHealth;
    private bool isDead = false;

    [Header("AI States")]
    public bool isChasing = false;
    public bool isAttacking = false;
    public bool isPatrolling = true;

    [Header("Coin Settings")]
    public GameObject coinPrefab;
    public int coinsToDrop;
    public float spreadRadius;

    private Vector3 initialPosition;
    private int patrolDirection = 1;
    private float patrolTimer = 0f;

    private Rigidbody rigibodyActiveEnemy;

    private enum AIState { Patrolling, Chasing, Attacking, Stunned }
    private AIState currentState = AIState.Patrolling;

    void Start()
    {
        currentHealth = maxHealth;
        initialPosition = transform.position;
        patrolTimer = patrolChangeInterval;
        patrolDirection = Random.Range(0, 2) == 0 ? -1 : 1;

        rigibodyActiveEnemy = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (isDead || rigibodyActiveEnemy == null) return;
        UpdateAI();
    }

    void UpdateAI()
    {
        float distanceToPlayer = 0f;
        Transform playerTransform = GameObject.FindWithTag("Player")?.transform;

        if (playerTransform != null)
            distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        switch (currentState)
        {
            case AIState.Patrolling:
                if (playerTransform != null && distanceToPlayer <= detectionRange)
                {
                    currentState = AIState.Chasing;
                    isChasing = true;
                    isPatrolling = false;
                }
                else
                {
                    Patrol();
                }
                break;

            case AIState.Chasing:
                if (playerTransform == null || distanceToPlayer > detectionRange * 1.5f)
                {
                    currentState = AIState.Patrolling;
                    isChasing = false;
                    isPatrolling = true;
                }
                else
                {
                    ChasePlayer(playerTransform);
                }
                break;
        }
    }

    void Patrol()
    {
        if (rigibodyActiveEnemy.isKinematic) return;

        patrolTimer -= Time.fixedDeltaTime;

        if (patrolTimer <= 0f || ShouldChangePatrolDirection())
        {
            patrolDirection *= -1;
            patrolTimer = patrolChangeInterval + Random.Range(-1f, 1f);
        }

        Vector3 targetVelocity = new Vector3(patrolDirection * patrolSpeed, rigibodyActiveEnemy.velocity.y, 0f);
        rigibodyActiveEnemy.velocity = targetVelocity;
    }

    bool ShouldChangePatrolDirection()
    {
        float distanceFromStart = transform.position.x - initialPosition.x;
        if (Mathf.Abs(distanceFromStart) >= patrolRadius)
        {
            if ((distanceFromStart > 0 && patrolDirection > 0) || (distanceFromStart < 0 && patrolDirection < 0))
                return true;
        }
        return false;
    }

    void ChasePlayer(Transform playerTransform)
    {
        if (rigibodyActiveEnemy.isKinematic) return;

        float directionX = Mathf.Sign(playerTransform.position.x - transform.position.x);
        Vector3 targetVelocity = new Vector3(directionX * moveSpeed, rigibodyActiveEnemy.velocity.y, 0f);
        rigibodyActiveEnemy.velocity = targetVelocity;
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Drop()
    {
        if (coinPrefab == null || coinsToDrop <= 0)
        {
            return;
        }

        for (int i = 0; i < coinsToDrop; i++)
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(-spreadRadius, spreadRadius),
                0.5f,
                Random.Range(-spreadRadius, spreadRadius)
            );

            Instantiate(coinPrefab, transform.position + randomOffset, Quaternion.identity);
        }
    }

    private void Die()
    {
        isDead = true;
        rigibodyActiveEnemy.velocity = Vector3.zero;
        Drop();
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying ? initialPosition : transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(center, new Vector3(patrolRadius * 2f, 1f, 1f));
    }
}
