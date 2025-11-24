using UnityEngine;

/*Author: Marcos Isar
Date: 20 - Nov - 2025*/

public class BreakableObject : MonoBehaviour
{
    public int maxHits = 3;
    private int currentHits;

    void Start()
    {
        currentHits = maxHits;
    }

    //Reduce hits by damage and destroy object if depleted
    public void TakeDamage(int damage)
    {
        currentHits -= damage;
        if (currentHits <= 0)
        {
            Destroy(gameObject);
        }
    }
}
