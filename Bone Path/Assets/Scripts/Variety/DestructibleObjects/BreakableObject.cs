using UnityEngine;

public class BreakableObject : MonoBehaviour
{
    public int maxHits = 3;
    private int currentHits;

    void Start()
    {
        currentHits = maxHits;
    }

    //Apply damage to breakable object
    public void TakeDamage(int damage)
    {
        currentHits -= damage;
        if (currentHits <= 0)
        {
            Destroy(gameObject);
        }
    }
}
