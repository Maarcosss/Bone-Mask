using System.Collections;
using UnityEngine;

/*Author: Lucas Vaquerizas
Date: 20 - Nov - 2025*/

public class SpikeTrap : MonoBehaviour
{
    [Header("Spike Movement Settings")]
    public float moveSpeed = 3f;         
    public float spikeHeight = 2f;       
    public float waitTimeAtTop = 0.5f;   
    public float waitTimeAtBottom = 0.5f; 

    [Header("Damage Settings")]
    public int damage = 1;               
    public string playerTag = "Player";

    private Vector3 startPos;
    private Vector3 topPos;
    private bool isMovingUp = true;
    private bool isWaiting = false;

    void Start()
    {
        startPos = transform.position;
        topPos = startPos + Vector3.up * spikeHeight;
        StartCoroutine(MoveSpike());
    }

    //Coroutine to handle spike movement
    IEnumerator MoveSpike()
    {
        while (true)
        {
            if (!isWaiting)
            {
                if (isMovingUp)
                {
                    transform.position = Vector3.MoveTowards(transform.position, topPos, moveSpeed * Time.deltaTime);
                    if (Vector3.Distance(transform.position, topPos) < 0.01f)
                    {
                        isWaiting = true;
                        StartCoroutine(WaitAtTop());
                    }
                }
                else
                {
                    transform.position = Vector3.MoveTowards(transform.position, startPos, moveSpeed * Time.deltaTime);
                    if (Vector3.Distance(transform.position, startPos) < 0.01f)
                    {
                        isWaiting = true;
                        StartCoroutine(WaitAtBottom());
                    }
                }
            }
            yield return null;
        }
    }

    //Wait at top before moving down
    IEnumerator WaitAtTop()
    {
        yield return new WaitForSeconds(waitTimeAtTop);
        isMovingUp = false;
        isWaiting = false;
    }

    //Wait at bottom before moving up
    IEnumerator WaitAtBottom()
    {
        yield return new WaitForSeconds(waitTimeAtBottom);
        isMovingUp = true;
        isWaiting = false;
    }

    //Detect player collision and deal damage
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            //Die
        }
    }

    //Draw gizmos for editor visualization
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * spikeHeight);
        Gizmos.DrawWireSphere(transform.position, 0.2f);
        Gizmos.DrawWireSphere(transform.position + Vector3.up * spikeHeight, 0.2f);
    }
}
