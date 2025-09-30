using UnityEngine;

public class ActiveEnemyAI : Enemy
{
    [Header("AI Behavior - Active Enemy")]
    [Tooltip("Distancia a la que detecta al jugador")]
    public float detectionRange = 8f;

    [Tooltip("Distancia mínima para atacar")]
    public float attackRange = 2f;

    [Tooltip("Velocidad de movimiento")]
    public float moveSpeed = 3f;

    [Tooltip("Velocidad cuando patrulla")]
    public float patrolSpeed = 1.5f;

    [Tooltip("Daño que hace al atacar")]
    public int attackDamage = 1;

    [Tooltip("Cooldown entre ataques")]
    public float attackCooldown = 1.5f;

    [Tooltip("Tiempo que dura el ataque")]
    public float attackDuration = 0.5f;

    [Header("Patrol Settings")]
    [Tooltip("Radio de patrullaje en X")]
    public float patrolRadius = 5f;

    [Tooltip("Tiempo entre cambios de dirección")]
    public float patrolChangeInterval = 3f;

    [Header("AI States")]
    public bool isChasing = false;
    public bool isAttacking = false;
    public bool isPatrolling = true;

    // Variables privadas
    private float lastAttackTime = 0f;
    private Vector3 initialPosition;
    private float attackTimer = 0f;
    private int patrolDirection = 1; // 1 = derecha, -1 = izquierda
    private float patrolTimer = 0f;
    private float lastDirectionChange = 0f;

    // Estados de AI
    private enum AIState { Patrolling, Chasing, Attacking, Stunned }
    private AIState currentState = AIState.Patrolling;

    protected override void Start()
    {
        base.Start();
        initialPosition = transform.position;
        patrolTimer = patrolChangeInterval;

        // ✅ ELEGIR DIRECCIÓN INICIAL ALEATORIA
        patrolDirection = Random.Range(0, 2) == 0 ? -1 : 1;

        if (showDebugLogs)
            Debug.Log($"🔴 ENEMIGO ACTIVO: {gameObject.name} | Detección: {detectionRange}m | Patrulla: {patrolRadius}m");
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate(); // Knockback handling

        if (isDead || rb == null) return;

        // No hacer AI si está en knockback
        if (IsInKnockback()) return;

        // Lógica de AI
        UpdateAI();
    }

    void UpdateAI()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Actualizar estado basado en distancia
        switch (currentState)
        {
            case AIState.Patrolling:
                if (distanceToPlayer <= detectionRange)
                {
                    currentState = AIState.Chasing;
                    isChasing = true;
                    isPatrolling = false;

                    if (showDebugLogs)
                        Debug.Log($"👁️ {gameObject.name} - ¡JUGADOR DETECTADO! Iniciando persecución");
                }
                else
                {
                    Patrol();
                }
                break;

            case AIState.Chasing:
                if (distanceToPlayer > detectionRange * 1.5f) // Histeresis
                {
                    currentState = AIState.Patrolling;
                    isChasing = false;
                    isPatrolling = true;

                    if (showDebugLogs)
                        Debug.Log($"😴 {gameObject.name} - Jugador perdido, volviendo a patrullar");
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

            case AIState.Attacking:
                attackTimer -= Time.fixedDeltaTime;
                if (attackTimer <= 0f)
                {
                    EndAttack();
                }
                break;
        }
    }

    // ✅ NUEVO: Sistema de patrullaje sin rotación
    void Patrol()
    {
        if (rb.isKinematic) return;

        // Actualizar timer de cambio de dirección
        patrolTimer -= Time.fixedDeltaTime;

        // Cambiar dirección si es necesario
        if (patrolTimer <= 0f || ShouldChangePatrolDirection())
        {
            patrolDirection *= -1;
            patrolTimer = patrolChangeInterval + Random.Range(-1f, 1f);
            lastDirectionChange = Time.time;

            if (showDebugLogs)
                Debug.Log($"🔄 {gameObject.name} - Cambiando dirección de patrulla a: {(patrolDirection > 0 ? "DERECHA" : "IZQUIERDA")}");
        }

        // ✅ MOVIMIENTO SOLO EN X - SIN ROTACIÓN
        Vector3 targetVelocity = new Vector3(patrolDirection * patrolSpeed, rb.velocity.y, 0f);
        rb.velocity = targetVelocity;
    }

    bool ShouldChangePatrolDirection()
    {
        // Cambiar si se aleja mucho del punto inicial
        float distanceFromStart = transform.position.x - initialPosition.x;

        if (Mathf.Abs(distanceFromStart) >= patrolRadius)
        {
            // Si está muy lejos a la derecha y va hacia la derecha, cambiar
            if (distanceFromStart > 0 && patrolDirection > 0) return true;
            // Si está muy lejos a la izquierda y va hacia la izquierda, cambiar
            if (distanceFromStart < 0 && patrolDirection < 0) return true;
        }

        return false;
    }

    // ✅ PERSECUCIÓN SIN ROTACIÓN
    void ChasePlayer()
    {
        if (playerTransform == null || rb.isKinematic) return;

        // ✅ CALCULAR DIRECCIÓN SOLO EN X
        float directionX = Mathf.Sign(playerTransform.position.x - transform.position.x);

        // ✅ MOVIMIENTO SOLO EN X - SIN ROTACIÓN
        Vector3 targetVelocity = new Vector3(directionX * moveSpeed, rb.velocity.y, 0f);
        rb.velocity = targetVelocity;

        if (showDebugLogs && Time.frameCount % 30 == 0) // Log cada 30 frames
            Debug.Log($"🏃 {gameObject.name} - Persiguiendo jugador en X. Distancia: {Vector3.Distance(transform.position, playerTransform.position):F1}m");
    }

    void StartAttack()
    {
        currentState = AIState.Attacking;
        isAttacking = true;
        attackTimer = attackDuration;
        lastAttackTime = Time.time;

        // Detener movimiento durante ataque
        if (rb != null && !rb.isKinematic)
        {
            Vector3 vel = rb.velocity;
            vel.x = 0f;
            vel.z = 0f;
            rb.velocity = vel;
        }

        // Atacar al jugador si está en rango
        if (Vector3.Distance(transform.position, playerTransform.position) <= attackRange)
        {
            DamagePlayer();
        }

        if (showDebugLogs)
            Debug.Log($"⚔️ {gameObject.name} - ¡ATACANDO AL JUGADOR!");
    }

    void EndAttack()
    {
        // ✅ VOLVER A PERSEGUIR SOLO SI EL JUGADOR SIGUE EN RANGO
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
        attackTimer = 0f;

        if (showDebugLogs)
            Debug.Log($"⚔️ {gameObject.name} - Ataque terminado");
    }

    void DamagePlayer()
    {
        // Buscar PlayerHealth en el jugador
        PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            Vector3 hitDirection = (playerTransform.position - transform.position).normalized;
            playerHealth.TakeDamage(attackDamage, hitDirection);

            if (showDebugLogs)
                Debug.Log($"💀 {gameObject.name} - Daño aplicado al jugador: {attackDamage}");
        }
    }

    public override void TakeDamage(int damage, Vector3 hitDirection)
    {
        base.TakeDamage(damage, hitDirection);

        // Interrumpir ataque si está atacando
        if (currentState == AIState.Attacking)
        {
            EndAttack();
        }

        // Forzar persecución después del daño si el jugador está en rango
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

    // Visualización en el editor
    void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying ? initialPosition : transform.position;

        // Rango de detección
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Rango de ataque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Radio de patrullaje
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(center, new Vector3(patrolRadius * 2f, 1f, 1f));

        // Línea al jugador si está persiguiendo
        if (Application.isPlaying && isChasing && playerTransform != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, new Vector3(playerTransform.position.x, transform.position.y, transform.position.z));
        }

        // Dirección de patrullaje
        if (Application.isPlaying && isPatrolling)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, new Vector3(patrolDirection, 0f, 0f) * 2f);
        }
    }
}
