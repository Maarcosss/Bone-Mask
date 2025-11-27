using System.Collections;
using UnityEngine;

/*Author: Lucas Vaquerizas & Marcos Isar
Date: 20 - Nov - 2025*/

public class BarrierTrap : MonoBehaviour
{
    [SerializeField] private GameObject block;
    [SerializeField] private Transform finalPosition;
    [SerializeField] private float speed = 2f;

    private Vector3 initialPosition;
    private bool isActivated = false;

    private void Start()
    {
        if (block != null)
        {
            initialPosition = block.transform.position;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActivated && other.CompareTag("Player"))
        {
            isActivated = true;
            StartCoroutine(MoveBlock());
        }
    }

    private IEnumerator MoveBlock()
    {
        float t = 0f;
        Vector3 startPos = block.transform.position;
        Vector3 endPos = finalPosition.position;

        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            block.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        block.transform.position = endPos;
    }
}
