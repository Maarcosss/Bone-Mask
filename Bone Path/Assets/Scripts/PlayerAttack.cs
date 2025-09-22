using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public int damage = 1;
    public GameObject attackHitbox; // Asignar el hijo AttackHitbox
    public float attackDuration = 0.2f; // Tiempo que dura activo

    private bool isAttacking = false;
    private float attackTimer = 0f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            isAttacking = true;
            attackHitbox.SetActive(true);

            // activar el hitbox
            attackHitbox.GetComponent<AttackHitbox>().Activate();

            attackTimer = attackDuration;
        }

        if (isAttacking)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                attackHitbox.SetActive(false);
                isAttacking = false;

                // desactivar el hitbox
                attackHitbox.GetComponent<AttackHitbox>().Deactivate();
            }
        }
    }



}
