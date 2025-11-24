using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*Author: David Gomez
Date: 20 - Nov - 2025*/

public class ShooterController : MonoBehaviour
{
    [Header("Detection Settings")]
    public float detectionRange = 10f;
    public string playerTag = "Player";
    public string obstacleTag = "Obstacle";

    [Header("Shooting Settings")]
    public GameObject arrowPrefab;
    public Transform firePoint;
    public float shootForce = 15f;
    public float fireRate = 2f;

    [Header("Rotation")]
    public float rotationSpeed = 5f;
    public Transform rotatingPart;

    private Transform player;
    private float lastShotTime;
    private bool playerDetected;

    //Initialize shooter and find the player
    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning($"No object with tag '{playerTag}' found in the scene");
        }

        lastShotTime = -fireRate; // Allow immediate shooting
    }

    //Update shooter behavior each frame
    void Update()
    {
        if (player == null) return;

        CheckPlayerDetection();

        if (playerDetected)
        {
            RotateTowardsPlayer();
            Shoot();
        }
    }

    //Check if the player is in detection range and visible
    void CheckPlayerDetection()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            RaycastHit hit;

            if (Physics.Raycast(transform.position, directionToPlayer, out hit, detectionRange))
            {
                if (hit.collider.CompareTag(playerTag))
                {
                    playerDetected = true;
                }
                else if (hit.collider.CompareTag(obstacleTag))
                {
                    playerDetected = false;
                }
                else
                {
                    RaycastHit[] hits = Physics.RaycastAll(transform.position, directionToPlayer, detectionRange);
                    bool visible = true;

                    foreach (RaycastHit h in hits)
                    {
                        if (h.collider.CompareTag(obstacleTag))
                        {
                            visible = false;
                            break;
                        }
                        if (h.collider.CompareTag(playerTag))
                        {
                            break;
                        }
                    }

                    playerDetected = visible;
                }
            }
        }
        else
        {
            playerDetected = false;
        }
    }

    //Rotate the turret smoothly towards the player
    void RotateTowardsPlayer()
    {
        if (rotatingPart == null) return;

        Vector3 direction = player.position - rotatingPart.position;
        direction.y = 0;

        Quaternion desiredRotation = Quaternion.LookRotation(direction);
        rotatingPart.rotation = Quaternion.Slerp(rotatingPart.rotation, desiredRotation, rotationSpeed * Time.deltaTime);
    }

    //Shoot an arrow towards the player if fire rate allows
    void Shoot()
    {
        if (Time.time - lastShotTime >= fireRate)
        {
            if (arrowPrefab != null && firePoint != null)
            {
                GameObject arrow = Instantiate(arrowPrefab, firePoint.position, firePoint.rotation);

                Rigidbody rb = arrow.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddForce(firePoint.forward * shootForce, ForceMode.Impulse);
                }

                lastShotTime = Time.time;

                Debug.Log("Turret fired an arrow!");
            }
        }
    }

    //Draw gizmos for detection range and shooting line
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        if (Application.isPlaying && playerDetected && player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(firePoint != null ? firePoint.position : transform.position, player.position);
        }
    }
}
