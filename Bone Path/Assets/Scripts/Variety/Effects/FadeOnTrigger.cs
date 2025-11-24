using UnityEngine;

/*Author: Marcos Isar
Date: 20 - Nov - 2025*/

public class FadeOnTrigger : MonoBehaviour
{
    public string playerTag = "Player";
    public float fadeDuration = 1f;
    public bool destroyAfterFade = true;

    private Renderer objectRenderer;
    private Material materialInstance;
    private bool hasFaded = false;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            materialInstance = objectRenderer.material;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) && !hasFaded)
        {
            StartCoroutine(FadeToTransparent());
        }
    }

    //Make an object gradually become transparent (fade out) when called.
    System.Collections.IEnumerator FadeToTransparent()
    {
        hasFaded = true;
        float elapsedTime = 0f;
        Color originalColor = materialInstance.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(originalColor.a, 0f, elapsedTime / fadeDuration);

            Color newColor = originalColor;
            newColor.a = alpha;
            materialInstance.color = newColor;

            yield return null;
        }

        Color finalColor = originalColor;
        finalColor.a = 0f;
        materialInstance.color = finalColor;

        if (destroyAfterFade)
        {
            Destroy(gameObject);
        }
    }
}
