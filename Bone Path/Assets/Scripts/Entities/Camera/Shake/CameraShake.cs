using System.Collections;
using UnityEngine;

/*Author: Alejandro Cruz
Date: 20 - Nov - 2025*/

public class CameraShake : MonoBehaviour
{
    [SerializeField] private float duration = 1.5f;
    [SerializeField] private float magnitude = 1.5f;

    public IEnumerator Shake(CameraFollowInputC cameraFollow)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            cameraFollow.shakeOffset = new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        cameraFollow.shakeOffset = Vector3.zero;
    }
}
