using UnityEngine;

/*Author: Marcos Isar
Date: 20 - Nov - 2025*/

[RequireComponent(typeof(Rigidbody))]
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
    public bool isPatrolling = true;

    private Vector3 initialPosition;
    private int patrolDirection = 1;
    private float patrolTimer = 0f;

    private Rigidbody rigibodyActiveEnemy;
    private CoinDropper coinDropper;

    private enum AIState { Patrolling, Chasing }
    private AIState currentState = AIState.Patrolling;

    void Start()
    {
        currentHealth = maxHealth;
        initialPosition = transform.position;
        patrolTimer = patrolChangeInterval;
        patrolDirection = Random.Range(0, 2) == 0 ? -1 : 1;

        rigibodyActiveEnemy = GetComponent<Rigidbody>();
        coinDropper = GetComponent<CoinDropper>();
    }

    //Handles AI update every fixed frame, skips if dead or Rigidbody missing
    void FixedUpdate()
    {
        if (isDead || rigibodyActiveEnemy == null) return;
        UpdateAI();
    }

    //Updates AI state and decides whether to patrol or chase player
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

    //Handles patrolling behavior, changes direction if needed
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

    //Checks if the enemy should reverse patrol direction based on patrol radius
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

    //Chases the player by moving towards their horizontal position
    void ChasePlayer(Transform playerTransform)
    {
        if (rigibodyActiveEnemy.isKinematic) return;

        float directionX = Mathf.Sign(playerTransform.position.x - transform.position.x);
        Vector3 targetVelocity = new Vector3(directionX * moveSpeed, rigibodyActiveEnemy.velocity.y, 0f);
        rigibodyActiveEnemy.velocity = targetVelocity;
    }

    //Reduces health by specified amount and checks for death
    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    //Handles enemy death, stops movement, drops coins and destroys the object
    private void Die()
    {
        isDead = true;
        rigibodyActiveEnemy.velocity = Vector3.zero;

        if (coinDropper != null)
        {
            coinDropper.DropCoins();
        }

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
