using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    public int damage = 1;
    public float soulPerHit = 10f; // Cuánto alma ganas por golpe
    private bool isActive = false;
    private PlayerHealth playerHealth;

    void Start()
    {
        playerHealth = FindObjectOfType<PlayerHealth>();
        gameObject.SetActive(false);
    }

    public void Activate()
    {
        isActive = true;
        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        isActive = false;
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;

        // Enemigo
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null && !enemy.isDead)
        {
            // Dirección desde el jugador hacia el enemigo
            Vector3 hitDirection = (enemy.transform.position - transform.position).normalized;

            // Daño + knockback
            enemy.TakeDamage(damage, hitDirection);

            // Dar alma al jugador
            if (playerHealth != null)
            {
                playerHealth.AddSoul(soulPerHit);
            }
        }

        // Otros objetos destructibles
        BreakableObject breakable = other.GetComponent<BreakableObject>();
        if (breakable != null)
        {
            breakable.TakeDamage(damage);
        }
    }
}
