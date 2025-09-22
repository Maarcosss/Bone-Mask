using UnityEngine;

public class PlayerDamageOnContact : MonoBehaviour
{
    public PlayerHealth playerHealth; // Arrastra aquí tu PlayerHealth
    public float damageCooldown = 1f; // Tiempo entre daños consecutivos
    private float damageTimer = 0f;

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

        // Comprueba si el objeto tiene el script Enemy y no está muerto
        Enemy enemy = target.GetComponent<Enemy>();
        if (enemy != null && !enemy.isDead)
        {
            playerHealth.TakeDamage(1);   // Quita 1 corazón
            damageTimer = damageCooldown; // Evita quitar más de un corazón de golpe
        }
    }
}
