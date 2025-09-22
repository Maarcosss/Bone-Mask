using UnityEngine;

public class BreakableObject : MonoBehaviour
{
    public int maxHits = 3;
    private int currentHits;

    void Start()
    {
        currentHits = maxHits;
    }

    public void TakeDamage(int damage)
    {
        currentHits -= damage;
        if (currentHits <= 0)
        {
            Destroy(gameObject);
        }
    }
}
