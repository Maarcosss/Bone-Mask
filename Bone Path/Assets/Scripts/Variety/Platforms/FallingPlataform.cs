using UnityEngine;


/*Author: Lucas Vaquerizas
Date: 20 - Nov - 2025*/

public class FallingPlatform : MonoBehaviour
{
    public float fallSpeed = 20f;
    public float destroyY = -10f;

    private bool shouldFall = false;
    private Collider platformCollider;
    private Rigidbody platformRigidbody;

    void Start()
    {
        platformCollider = GetComponent<Collider>();
        platformRigidbody = gameObject.AddComponent<Rigidbody>();
        platformRigidbody.useGravity = false;
        platformRigidbody.isKinematic = true;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!shouldFall && collision.gameObject.CompareTag("Player"))
        {
            shouldFall = true;
            platformCollider.isTrigger = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (shouldFall)
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (shouldFall)
        {
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;

            if (transform.position.y <= destroyY)
            {
                Destroy(gameObject);
            }
        }
    }
}
