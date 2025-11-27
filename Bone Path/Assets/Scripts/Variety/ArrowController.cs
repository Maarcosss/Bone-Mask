using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*Author: David Gomez
Date: 20 - Nov - 2025*/

public class ArrowController : MonoBehaviour
{
    [Header("Arrow Settings")]
    public float damage = 10f;
    public float lifeTime = 5f;
    public GameObject impactEffect;
    public string playerTag = "Player";
    public string obstacleTag = "Obstacle";

    private Rigidbody rb;
    private bool hasHit = false;

    //Initialize the arrow and schedule its destruction
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Destroy(gameObject, lifeTime);
    }

    // Handles collision with player or obstacles
    void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;

        hasHit = true;

        if (collision.gameObject.CompareTag(playerTag))
        {
            //Apply damage to the player if needed
            //collision.gameObject.GetComponent<PlayerHealth>()?.TakeDamage(damage);
            Debug.Log("Arrow hit the player!");
        }

        //Spawn impact effect if assigned
        if (impactEffect != null)
        {
            Instantiate(impactEffect, transform.position, transform.rotation);
        }

        //Stop the arrow movement
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;
        }

        //Stick arrow to the object if it's not the player
        if (!collision.gameObject.CompareTag(playerTag))
        {
            transform.SetParent(collision.transform);
        }

        //Destroy the arrow after a short delay
        Destroy(gameObject, 2f);
    }
}
