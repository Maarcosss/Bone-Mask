    using UnityEngine;

    public class PlayerDamageOnContact : MonoBehaviour
    {
        [Header("Damage Settings")]
        public PlayerHealth playerHealth;
        public float damageCooldown = 1f;

        [Header("Debug")]
        [Tooltip("Mostrar logs de debug para depuración")]
        public bool showDebugLogs = false;

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
            TryDamage(collision.gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            TryDamage(other.gameObject);
        }

        void TryDamage(GameObject target)
        {
            if (damageTimer > 0f) return;

            // ✅ FILTRO 1: Ignorar AttackHitbox propio
            if (target.name.Contains("AttackHitbox") || target.name.Contains("Hitbox"))
            {
                if (showDebugLogs)
                    Debug.Log($"🛡️ Ignorando AttackHitbox: {target.name}");
                return;
            }

            // ✅ FILTRO 2: Solo objetos con tag "Enemy" pueden hacer daño
            if (!target.CompareTag("Enemy"))
            {
                if (showDebugLogs)
                    Debug.Log($"🛡️ Ignorando objeto sin tag Enemy: {target.name} (Tag: {target.tag})");
                return;
            }

            // ✅ FILTRO 3: Verificar que es realmente un enemigo
            Enemy enemy = target.GetComponent<Enemy>();
            if (enemy != null && !enemy.isDead)
            {
                if (showDebugLogs)
                    Debug.Log($"💔 Jugador recibe daño de: {target.name}");

                playerHealth.TakeDamage(1);
                damageTimer = damageCooldown;
            }
            else
            {
                if (showDebugLogs)
                    Debug.Log($"🛡️ Ignorando: {target.name} (No es enemigo válido)");
            }
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
