using System.Collections;
using UnityEngine;

/*Author: Alejandro Cruz
Date: 20 - Nov - 2025*/

public class SpawnExplosion : MonoBehaviour
{
    public CameraShake cameraShake;
    public CameraFollowInputC cameraFollow;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(cameraShake.Shake(cameraFollow));
        }
    }
}
