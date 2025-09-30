using UnityEngine;

public class PlayerDamageOnContact : MonoBehaviour
{
    [Header("Damage Settings")]
    public PlayerHealth playerHealth;
    public float damageCooldown = 1f;

    [Header("Debug")]
    [Tooltip("Mostrar logs de debug para depuración")]
    public bool showDebugLogs = true; // ← CAMBIADO A TRUE PARA DEBUG

    private float damageTimer = 0f;

    void Start()
    {
        if (playerHealth == null)
        {
            playerHealth = GetComponent<PlayerHealth>();
            if (playerHealth == null)
            {
                Debug.LogError("❌ PlayerHealth no encontrado en PlayerDamageOnContact");
            }
        }

        if (showDebugLogs)
            Debug.Log($"🛡️ PlayerDamageOnContact iniciado en: {gameObject.name}");
    }

    void Update()
    {
        if (damageTimer > 0f)
        {
            damageTimer -= Time.deltaTime;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (showDebugLogs)
            Debug.Log($"🔍 COLLISION detectada: {collision.gameObject.name} (Tag: {collision.gameObject.tag})");

        TryDamage(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (showDebugLogs)
            Debug.Log($"🔍 TRIGGER detectado: {other.gameObject.name} (Tag: {other.tag})");

        TryDamage(other.gameObject);
    }

    void TryDamage(GameObject target)
    {
        if (damageTimer > 0f)
        {
            if (showDebugLogs)
                Debug.Log($"🛡️ COOLDOWN activo - ignorando: {target.name}");
            return;
        }

        // ✅ FILTRO 1: Ignorar AttackHitbox propio y objetos del jugador
        if (target.name.Contains("AttackHitbox") ||
            target.name.Contains("Hitbox") ||
            target.transform.IsChildOf(this.transform) ||
            target.CompareTag("Player"))
        {
            if (showDebugLogs)
                Debug.Log($"🛡️ FILTRO 1: Ignorando objeto propio/hitbox: {target.name}");
            return;
        }

        // ✅ FILTRO 2: Solo objetos con tag "Enemy" pueden hacer daño
        if (!target.CompareTag("Enemy"))
        {
            if (showDebugLogs)
                Debug.Log($"🛡️ FILTRO 2: Ignorando - no es Enemy: {target.name} (Tag: {target.tag})");
            return;
        }

        // ✅ FILTRO 3: Verificar que es realmente un enemigo vivo
        Enemy enemy = target.GetComponent<Enemy>();
        if (enemy == null)
        {
            if (showDebugLogs)
                Debug.Log($"🛡️ FILTRO 3A: No tiene componente Enemy: {target.name}");
            return;
        }

        if (enemy.isDead)
        {
            if (showDebugLogs)
                Debug.Log($"🛡️ FILTRO 3B: Enemigo ya está muerto: {target.name}");
            return;
        }

        // ✅ FILTRO 4: Verificar que no estamos atacando (para evitar confusión)
        PlayerAttack playerAttack = GetComponent<PlayerAttack>();
        if (playerAttack != null && playerAttack.EstaAtacando())
        {
            if (showDebugLogs)
                Debug.Log($"🛡️ FILTRO 4: Estamos atacando - posible confusión con hitbox: {target.name}");
            return;
        }

        // ✅ TODO PASÓ - RECIBIR DAÑO
        if (showDebugLogs)
            Debug.Log($"💔 ¡DAÑO VÁLIDO! Recibiendo daño de: {target.name}");

        playerHealth.TakeDamage(1);
        damageTimer = damageCooldown;
    }

    // Método público para testing
    public void ResetCooldown()
    {
        damageTimer = 0f;
    }

    // Método para verificar si está en cooldown
    public bool IsInCooldown()
    {
        return damageTimer > 0f;
    }

    // Método para obtener tiempo restante de cooldown
    public float GetCooldownRemaining()
    {
        return Mathf.Max(0f, damageTimer);
    }
}
