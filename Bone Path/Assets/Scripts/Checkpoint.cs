using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    [Tooltip("Tipo de checkpoint")]
    public CheckpointType type = CheckpointType.Respawn;

    [Header("Checkpoint ID")]
    [Tooltip("ID único del checkpoint (se genera automáticamente si está vacío)")]
    public string checkpointId = "";

    [Header("Visual Feedback")]
    public GameObject savePromptUI;
    public GameObject savedFeedbackUI;
    public Light checkpointLight;
    public ParticleSystem saveEffect;

    [Header("Audio")]
    public AudioClip enterSound;
    public AudioClip saveSound;

    [Header("Settings")]
    public bool showDebugLogs = false;

    public enum CheckpointType
    {
        Respawn,    // Checkpoint automático (se destruye después de usar)
        SavePoint   // Save point manual (no se destruye)
    }

    private bool playerInRange = false;
    private bool hasBeenUsed = false;
    private AudioSource audioSource;

    void Start()
    {
        if (string.IsNullOrEmpty(checkpointId))
        {
            checkpointId = GenerateCheckpointId();
        }

        if (SaveSystem.Instance != null && SaveSystem.Instance.IsCheckpointDestroyed(checkpointId))
        {
            Debug.Log($"🗑️ Checkpoint {checkpointId} ya fue destruido - Eliminando");
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();

        if (type == CheckpointType.Respawn)
        {
            if (savePromptUI != null)
                savePromptUI.SetActive(false);
        }

        if (savedFeedbackUI != null)
            savedFeedbackUI.SetActive(false);

        if (showDebugLogs)
            Debug.Log($"✅ Checkpoint inicializado: {checkpointId} | Tipo: {type}");
    }

    string GenerateCheckpointId()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        Vector3 pos = transform.position;
        return $"{sceneName}_{gameObject.name}_{pos.x:F1}_{pos.y:F1}_{pos.z:F1}";
    }

    void Update()
    {
        if (type == CheckpointType.SavePoint && playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            SaveGame();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasBeenUsed)
        {
            playerInRange = true;

            if (type == CheckpointType.Respawn)
            {
                if (SaveSystem.Instance != null)
                {
                    Vector3 adjustedPosition = GetAdjustedRespawnPosition();
                    SaveSystem.Instance.UpdateRespawnPoint(adjustedPosition);
                }

                hasBeenUsed = true;

                if (showDebugLogs)
                    Debug.Log($"🔄 Respawn point usado: {checkpointId} - Será destruido");

                if (SaveSystem.Instance != null)
                {
                    SaveSystem.Instance.RegisterDestroyedCheckpoint(checkpointId);
                }

                if (saveEffect != null)
                    saveEffect.Play();

                if (audioSource != null && enterSound != null)
                    audioSource.PlayOneShot(enterSound);

                Invoke(nameof(DestroyCheckpoint), 0.5f);
            }
            else if (type == CheckpointType.SavePoint)
            {
                if (SaveSystem.Instance != null)
                {
                    SaveSystem.Instance.EnterSavePoint();
                }

                if (savePromptUI != null)
                    savePromptUI.SetActive(true);

                if (audioSource != null && enterSound != null)
                    audioSource.PlayOneShot(enterSound);

                if (checkpointLight != null)
                    checkpointLight.enabled = true;

                if (showDebugLogs)
                    Debug.Log($"💾 Save Point alcanzado: {checkpointId} - Presiona F para guardar");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (type == CheckpointType.SavePoint)
            {
                if (SaveSystem.Instance != null)
                {
                    SaveSystem.Instance.ExitSavePoint();
                }

                if (savePromptUI != null)
                    savePromptUI.SetActive(false);

                if (checkpointLight != null)
                    checkpointLight.enabled = false;
            }
        }
    }

    Vector3 GetAdjustedRespawnPosition()
    {
        Vector3 basePosition = transform.position;

        RaycastHit hit;
        Vector3 rayStart = basePosition + Vector3.up * 5f;

        if (Physics.Raycast(rayStart, Vector3.down, out hit, 10f))
        {
            Vector3 adjustedPosition = hit.point + Vector3.up * 1.5f;

            if (showDebugLogs)
                Debug.Log($"🎯 Checkpoint {checkpointId}: Suelo detectado en {hit.point}, respawn ajustado a {adjustedPosition}");

            return adjustedPosition;
        }
        else
        {
            Vector3 adjustedPosition = basePosition + Vector3.up * 1f;

            if (showDebugLogs)
                Debug.LogWarning($"⚠️ Checkpoint {checkpointId}: No se detectó suelo, usando posición con offset: {adjustedPosition}");

            return adjustedPosition;
        }
    }

    void DestroyCheckpoint()
    {
        if (showDebugLogs)
            Debug.Log($"🗑️ Destruyendo checkpoint: {checkpointId}");

        Destroy(gameObject);
    }

    void SaveGame()
    {
        if (SaveSystem.Instance != null)
        {
            Vector3 adjustedPosition = GetAdjustedRespawnPosition();
            SaveSystem.Instance.UpdateRespawnPoint(adjustedPosition);
            SaveSystem.Instance.ForceSave();

            if (saveEffect != null)
                saveEffect.Play();

            if (audioSource != null && saveSound != null)
                audioSource.PlayOneShot(saveSound);

            ShowSavedFeedback();

            if (showDebugLogs)
                Debug.Log($"✅ Juego guardado en Save Point: {checkpointId} | Posición ajustada: {adjustedPosition}");
        }
    }

    void ShowSavedFeedback()
    {
        if (savedFeedbackUI != null)
        {
            savedFeedbackUI.SetActive(true);
            Invoke(nameof(HideSavedFeedback), 2f);
        }
    }

    void HideSavedFeedback()
    {
        if (savedFeedbackUI != null)
            savedFeedbackUI.SetActive(false);
    }

    public string GetCheckpointId()
    {
        if (string.IsNullOrEmpty(checkpointId))
        {
            checkpointId = GenerateCheckpointId();
        }
        return checkpointId;
    }

    [ContextMenu("Destruir este Checkpoint")]
    void ForceDestroy()
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.RegisterDestroyedCheckpoint(GetCheckpointId());
        }
        Destroy(gameObject);
    }
}
