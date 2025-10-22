using UnityEngine;

public class ActiveEnemyAI : Enemy
{
    [Header("AI Behavior - Active Enemy")]
    public float detectionRange = 8f;
    public float attackRange = 2f;
    public float moveSpeed = 3f;
    public float patrolSpeed = 1.5f;
    public int attackDamage = 1;
    public float attackCooldown = 1.5f;
    public float attackDuration = 0.5f;

    [Header("Patrol Settings")]
    public float patrolRadius = 5f;
    public float patrolChangeInterval = 3f;

    [Header("AI States")]
    public bool isChasing = false;
    public bool isAttacking = false;
    public bool isPatrolling = true;

    private float lastAttackTime = 0f;
    private Vector3 initialPosition;
    private float attackTimer = 0f;
    private int patrolDirection = 1;
    private float patrolTimer = 0f;

    private enum AIState { Patrolling, Chasing, Attacking, Stunned }
    private AIState currentState = AIState.Patrolling;

    //Initializes enemy state and random patrol direction
    protected override void Start()
    {
        base.Start();
        initialPosition = transform.position;
        patrolTimer = patrolChangeInterval;
        patrolDirection = Random.Range(0, 2) == 0 ? -1 : 1;
    }

    //Handles AI updates each physics frame
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (isDead || rb == null)
        {
            return;
        }

        if (IsInKnockback())
        {
            return;
        }

        UpdateAI();
    }

    //Controls AI logic and state transitions
    void UpdateAI()
    {
        if (playerTransform == null)
        {
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        switch (currentState)
        {
            case AIState.Patrolling:
                {
                    if (distanceToPlayer <= detectionRange)
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
                }

            case AIState.Chasing:
                {
                    if (distanceToPlayer > detectionRange * 1.5f)
                    {
                        currentState = AIState.Patrolling;
                        isChasing = false;
                        isPatrolling = true;
                    }
                    else if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
                    {
                        StartAttack();
                    }
                    else
                    {
                        ChasePlayer();
                    }
                    break;
                }

            case AIState.Attacking:
                {
                    attackTimer -= Time.fixedDeltaTime;
                    if (attackTimer <= 0f)
                    {
                        EndAttack();
                    }
                    break;
                }
        }
    }

    //Handles random patrol movement within a radius
    void Patrol()
    {
        if (rb.isKinematic)
        {
            return;
        }

        patrolTimer -= Time.fixedDeltaTime;

        if (patrolTimer <= 0f || ShouldChangePatrolDirection())
        {
            patrolDirection *= -1;
            patrolTimer = patrolChangeInterval + Random.Range(-1f, 1f);
        }

        Vector3 targetVelocity = new Vector3(patrolDirection * patrolSpeed, rb.velocity.y, 0f);
        rb.velocity = targetVelocity;
    }

    //Checks if enemy should change patrol direction based on distance
    bool ShouldChangePatrolDirection()
    {
        float distanceFromStart = transform.position.x - initialPosition.x;

        if (Mathf.Abs(distanceFromStart) >= patrolRadius)
        {
            if (distanceFromStart > 0 && patrolDirection > 0)
            {
                return true;
            }

            if (distanceFromStart < 0 && patrolDirection < 0)
            {
                return true;
            }
        }

        return false;
    }

    //Moves enemy toward player position on X axis
    void ChasePlayer()
    {
        if (playerTransform == null || rb.isKinematic)
        {
            return;
        }

        float directionX = Mathf.Sign(playerTransform.position.x - transform.position.x);
        Vector3 targetVelocity = new Vector3(directionX * moveSpeed, rb.velocity.y, 0f);
        rb.velocity = targetVelocity;
    }

    //Starts enemy attack sequence
    void StartAttack()
    {
        currentState = AIState.Attacking;
        isAttacking = true;
        attackTimer = attackDuration;
        lastAttackTime = Time.time;

        if (rb != null && !rb.isKinematic)
        {
            Vector3 vel = rb.velocity;
            vel.x = 0f;
            vel.z = 0f;
            rb.velocity = vel;
        }

        if (Vector3.Distance(transform.position, playerTransform.position) <= attackRange)
        {
            DamagePlayer();
        }
    }

    //Ends attack and resumes chase or patrol
    void EndAttack()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= detectionRange)
        {
            currentState = AIState.Chasing;
            isChasing = true;
        }
        else
        {
            currentState = AIState.Patrolling;
            isPatrolling = true;
            isChasing = false;
        }

        isAttacking = false;
    }

    //Applies damage to the player if in range
    void DamagePlayer()
    {
        Player playerHealth = playerTransform.GetComponent<Player>();
        if (playerHealth != null)
        {
            Vector3 hitDirection = (playerTransform.position - transform.position).normalized;
            //playerHealth.TakeDamage(attackDamage, hitDirection);
        }
    }

    //Handles enemy damage and potential state changes
    public override void TakeDamage(int damage, Vector3 hitDirection)
    {
        base.TakeDamage(damage, hitDirection);

        if (currentState == AIState.Attacking)
        {
            EndAttack();
        }

        if (!isDead && playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= detectionRange * 1.5f)
            {
                currentState = AIState.Chasing;
                isChasing = true;
                isPatrolling = false;
            }
        }
    }

    //Draws debug gizmos for detection, attack and patrol areas
    void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying ? initialPosition : transform.position;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(center, new Vector3(patrolRadius * 2f, 1f, 1f));

        if (Application.isPlaying && isChasing && playerTransform != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, new Vector3(playerTransform.position.x, transform.position.y, transform.position.z));
        }

        if (Application.isPlaying && isPatrolling)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, new Vector3(patrolDirection, 0f, 0f) * 2f);
        }
    }
}
