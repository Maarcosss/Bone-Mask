using UnityEngine;

public class PassiveEnemyAI : Enemy
{
    [Header("AI Behavior - Passive Enemy")]
    [Tooltip("Velocidad de movimiento normal")]
    public float normalMoveSpeed = 2f;

    [Tooltip("Velocidad cuando huye")]
    public float fleeSpeed = 5f;

    [Tooltip("Tiempo que huye antes de volver a comportamiento normal")]
    public float fleeDuration = 4f;

    [Tooltip("Radio de movimiento aleatorio")]
    public float wanderRadius = 8f;

    [Tooltip("Tiempo entre cambios de dirección")]
    public float wanderInterval = 3f;

    [Header("Flee Settings")]
    [Tooltip("Radio de detección del jugador para huir")]
    public float playerDetectionRadius = 6f;

    [Tooltip("Tiempo sin ser atacado para dejar de huir")]
    public float fleeTimeout = 5f;

    [Header("Movement Pattern")]
    [Tooltip("Patrón de movimiento: Random, Circle, BackAndForth")]
    public MovementPattern movementPattern = MovementPattern.Random;

    public enum MovementPattern { Random, Circle, BackAndForth }

    // Variables privadas
    private Vector3 initialPosition;
    private Vector3 currentTarget;
    private float fleeTimer = 0f;
    private float wanderTimer = 0f;
    private bool isFleeing = false;
    private int backAndForthDirection = 1;
    private int wanderDirection = 1;
    private float lastDamageTime = 0f;
    private bool isFleeingFromPlayer = false;
    private bool wasAttacked = false;

    // ✅ NUEVO: Variables para asegurar movimiento inicial
    private bool hasStartedMoving = false;
    private bool isMoving = false;

    protected override void Start()
    {
        base.Start();
        initialPosition = transform.position;

        // ✅ CONFIGURAR MOVIMIENTO INICIAL
        InitializeMovement();

        if (showDebugLogs)
            Debug.Log($"🟡 ENEMIGO PASIVO INICIADO: {gameObject.name} | Detección: {playerDetectionRadius}m | Patrón: {movementPattern} | Movimiento inicial: ✅");
    }

    // ✅ NUEVO: Inicializar movimiento desde el start
    void InitializeMovement()
    {
        // Dirección inicial aleatoria
        wanderDirection = Random.Range(0, 2) == 0 ? -1 : 1;

        // Configurar timer inicial
        wanderTimer = Random.Range(0.5f, 1.5f); // Tiempo pequeño para empezar rápido

        // Establecer primer objetivo inmediatamente
        SetNewWanderTarget();

        // Marcar que el movimiento ha comenzado
        hasStartedMoving = true;
        isMoving = true;

        if (showDebugLogs)
            Debug.Log($"🎯 {gameObject.name} - Movimiento inicial configurado. Dirección: {(wanderDirection > 0 ? "DERECHA" : "IZQUIERDA")} | Objetivo: {currentTarget}");
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate(); // Knockback handling

        if (isDead || rb == null) return;

        // No hacer AI si está en knockback
        if (IsInKnockback()) return;

        // ✅ ASEGURAR QUE SIEMPRE HAY MOVIMIENTO
        UpdateAI();
    }

    void UpdateAI()
    {
        // Verificar proximidad del jugador
        bool playerInRange = IsPlayerInRange();

        // Lógica de huida
        if (isFleeing)
        {
            fleeTimer -= Time.fixedDeltaTime;

            // Condiciones para parar de huir
            bool timeoutReached = (Time.time - lastDamageTime) >= fleeTimeout;
            bool playerTooFar = !playerInRange && !wasAttacked;

            if (fleeTimer <= 0f || timeoutReached || playerTooFar)
            {
                StopFleeing();

                if (timeoutReached && showDebugLogs)
                    Debug.Log($"⏰ {gameObject.name} - Timeout de huida alcanzado");
                else if (playerTooFar && showDebugLogs)
                    Debug.Log($"🏃 {gameObject.name} - Jugador lejos, dejando de huir");
            }
            else
            {
                FleeFromPlayer();
                return;
            }
        }

        // Iniciar huida si el jugador está cerca (sin haber sido atacado)
        if (!isFleeing && playerInRange && !wasAttacked)
        {
            StartFleeingFromPlayer();
            return;
        }

        // ✅ COMPORTAMIENTO NORMAL DE MOVIMIENTO (MEJORADO)
        UpdateNormalMovement();
    }

    // ✅ NUEVO: Manejo mejorado del movimiento normal
    void UpdateNormalMovement()
    {
        // Actualizar timer de wander
        wanderTimer -= Time.fixedDeltaTime;

        // ✅ CAMBIAR OBJETIVO SI ES NECESARIO
        bool shouldChangeTarget = wanderTimer <= 0f;
        bool reachedTarget = Vector3.Distance(transform.position, currentTarget) < 1f;

        if (shouldChangeTarget || reachedTarget)
        {
            SetNewWanderTarget();
            wanderTimer = wanderInterval + Random.Range(-0.5f, 0.5f);

            if (showDebugLogs && reachedTarget)
                Debug.Log($"🎯 {gameObject.name} - Objetivo alcanzado, estableciendo nuevo objetivo");
        }

        // ✅ SIEMPRE MOVERSE HACIA EL OBJETIVO
        MoveToTarget();
        isMoving = true;
    }

    bool IsPlayerInRange()
    {
        if (playerTransform == null) return false;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        return distanceToPlayer <= playerDetectionRadius;
    }

    void StartFleeingFromPlayer()
    {
        isFleeing = true;
        isFleeingFromPlayer = true;
        fleeTimer = fleeDuration;
        isMoving = true; // ✅ Asegurar que sigue moviéndose

        // Calcular dirección de huida basada en la posición del jugador
        if (playerTransform != null)
        {
            float fleeDirectionX = Mathf.Sign(transform.position.x - playerTransform.position.x);

            // Si están en la misma X, elegir dirección aleatoria
            if (Mathf.Abs(fleeDirectionX) < 0.1f)
            {
                fleeDirectionX = Random.Range(0, 2) == 0 ? -1f : 1f;
            }

            wanderDirection = (int)fleeDirectionX;

            if (showDebugLogs)
                Debug.Log($"😰 {gameObject.name} - ¡HUYENDO DEL JUGADOR! Dirección: {(fleeDirectionX > 0 ? "DERECHA" : "IZQUIERDA")}");
        }
    }

    void SetNewWanderTarget()
    {
        Vector3 newTarget;

        switch (movementPattern)
        {
            case MovementPattern.Random:
                // ✅ MOVIMIENTO ALEATORIO MEJORADO EN X
                // Cambiar dirección ocasionalmente
                if (Random.Range(0f, 1f) < 0.3f) // 30% chance de cambiar dirección
                {
                    wanderDirection *= -1;
                }

                float randomDistance = Random.Range(3f, wanderRadius);
                newTarget = transform.position + new Vector3(wanderDirection * randomDistance, 0f, 0f);

                // ✅ ASEGURAR QUE EL OBJETIVO NO ESTÉ DEMASIADO LEJOS DEL INICIO
                float distanceFromStart = Vector3.Distance(newTarget, initialPosition);
                if (distanceFromStart > wanderRadius)
                {
                    // Forzar dirección hacia el centro
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

        if (showDebugLogs)
            Debug.Log($"🎯 {gameObject.name} - Nuevo objetivo establecido: {currentTarget} | Dirección: {(wanderDirection > 0 ? "DERECHA" : "IZQUIERDA")}");
    }

    void MoveToTarget()
    {
        if (rb.isKinematic) return;

        // ✅ ASEGURAR MOVIMIENTO CONTINUO
        float directionX = 0f;
        Vector3 targetVelocity = Vector3.zero;

        if (movementPattern == MovementPattern.Random || movementPattern == MovementPattern.BackAndForth)
        {
            // ✅ MOVIMIENTO DIRECTO EN X
            directionX = Mathf.Sign(currentTarget.x - transform.position.x);

            // ✅ FORZAR MOVIMIENTO AUNQUE EL OBJETIVO ESTÉ CERCA
            if (Mathf.Abs(directionX) < 0.1f)
            {
                directionX = wanderDirection; // Usar dirección guardada
            }

            targetVelocity = new Vector3(directionX * normalMoveSpeed, rb.velocity.y, 0f);
        }
        else
        {
            // Para circle pattern
            Vector3 direction = (currentTarget - transform.position).normalized;
            direction.y = 0f;

            targetVelocity = direction * normalMoveSpeed;
            targetVelocity.y = rb.velocity.y;
        }

        // ✅ APLICAR VELOCIDAD
        rb.velocity = targetVelocity;

        // ✅ DEBUG MOVIMIENTO
        if (showDebugLogs && Time.frameCount % 60 == 0) // Log cada 60 frames (1 segundo aprox)
        {
            Debug.Log($"🚶 {gameObject.name} - Moviéndose hacia {currentTarget} | Velocidad actual: {rb.velocity} | Distancia: {Vector3.Distance(transform.position, currentTarget):F1}m");
        }
    }

    void StartFleeing(Vector3 hitDirection)
    {
        isFleeing = true;
        isFleeingFromPlayer = false;
        wasAttacked = true;
        fleeTimer = fleeDuration;
        lastDamageTime = Time.time;
        isMoving = true; // ✅ Asegurar movimiento

        // Priorizar huir del jugador sobre la dirección del golpe
        float fleeDirectionX;

        if (playerTransform != null)
        {
            fleeDirectionX = Mathf.Sign(transform.position.x - playerTransform.position.x);
        }
        else
        {
            fleeDirectionX = -Mathf.Sign(hitDirection.x);
        }

        if (Mathf.Abs(fleeDirectionX) < 0.1f)
        {
            fleeDirectionX = Random.Range(0, 2) == 0 ? -1f : 1f;
        }

        wanderDirection = (int)fleeDirectionX;

        if (showDebugLogs)
            Debug.Log($"😰 {gameObject.name} - ¡HUYENDO POR ATAQUE! Dirección: {(fleeDirectionX > 0 ? "DERECHA" : "IZQUIERDA")}");
    }

    void FleeFromPlayer()
    {
        if (rb.isKinematic) return;

        // Actualizar dirección de huida en tiempo real si está huyendo del jugador
        if (isFleeingFromPlayer && playerTransform != null)
        {
            float currentFleeDirection = Mathf.Sign(transform.position.x - playerTransform.position.x);

            if (Mathf.Abs(currentFleeDirection - wanderDirection) > 0.5f)
            {
                wanderDirection = (int)currentFleeDirection;

                if (showDebugLogs)
                    Debug.Log($"🔄 {gameObject.name} - Actualizando dirección de huida: {(wanderDirection > 0 ? "DERECHA" : "IZQUIERDA")}");
            }
        }

        // ✅ HUIDA CON VELOCIDAD GARANTIZADA
        Vector3 targetVelocity = new Vector3(wanderDirection * fleeSpeed, rb.velocity.y, 0f);
        rb.velocity = targetVelocity;
        isMoving = true;
    }

    void StopFleeing()
    {
        isFleeing = false;
        isFleeingFromPlayer = false;
        fleeTimer = 0f;

        if (wasAttacked && (Time.time - lastDamageTime) >= fleeTimeout)
        {
            wasAttacked = false;
        }

        // ✅ CONTINUAR MOVIMIENTO INMEDIATAMENTE
        SetNewWanderTarget();
        wanderTimer = Random.Range(0.5f, 1.5f); // Tiempo corto para reanudar movimiento rápido
        isMoving = true;

        if (showDebugLogs)
            Debug.Log($"😌 {gameObject.name} - Dejando de huir, reanudando movimiento normal hacia: {currentTarget}");
    }

    public override void TakeDamage(int damage, Vector3 hitDirection)
    {
        base.TakeDamage(damage, hitDirection);

        lastDamageTime = Time.time;

        if (!isDead)
        {
            StartFleeing(hitDirection);
        }
    }

    // ✅ NUEVO: Método público para verificar si se está moviendo
    public bool IsMoving()
    {
        return isMoving && rb != null && rb.velocity.magnitude > 0.1f;
    }

    // Visualización en el editor
    void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying ? initialPosition : transform.position;

        // Radio de detección del jugador
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, playerDetectionRadius);

        // Radio de movimiento
        Gizmos.color = Color.blue;

        if (movementPattern == MovementPattern.Random || movementPattern == MovementPattern.BackAndForth)
        {
            Gizmos.DrawWireCube(center, new Vector3(wanderRadius * 2f, 1f, 1f));
        }
        else
        {
            Gizmos.DrawWireSphere(center, wanderRadius);
        }

        // Objetivo actual
        if (Application.isPlaying && currentTarget != Vector3.zero)
        {
            Gizmos.color = isFleeing ? Color.red : Color.green;
            Gizmos.DrawWireSphere(currentTarget, 0.5f);

            // Línea al objetivo
            Gizmos.color = isMoving ? Color.green : Color.gray;
            Gizmos.DrawLine(transform.position, currentTarget);
        }

        // Dirección actual
        if (Application.isPlaying)
        {
            Gizmos.color = isFleeing ? Color.red : Color.cyan;
            Gizmos.DrawRay(transform.position, new Vector3(wanderDirection, 0f, 0f) * 2f);
        }

        // Línea al jugador si está en rango
        if (Application.isPlaying && playerTransform != null && IsPlayerInRange())
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, playerTransform.position);
        }
    }
}
