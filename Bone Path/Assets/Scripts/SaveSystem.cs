using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;
using System.Collections;

public class SaveSystem : MonoBehaviour
{
    [Header("Debug")]
    public bool showDebugMessages = true;

    public static SaveSystem Instance { get; private set; }

    private string saveFile;
    private string checkpointsFile;
    private bool nearSavePoint = false;
    private Transform player;
    private Vector3 lastRespawnPoint = Vector3.zero;
    private HashSet<string> destroyedCheckpoints = new HashSet<string>();

    [System.Serializable]
    public class GameData
    {
        public float playerX, playerY, playerZ;
        public int health;
        public float soul;
        public int coins;
        public string currentScene;
        public string timestamp;
        public float respawnX, respawnY, respawnZ;
        public List<string> destroyedCheckpointIds = new List<string>();

        public GameData()
        {
            health = 3;
            soul = 0f;
            coins = 0;
            currentScene = "";
            timestamp = "";
            respawnX = respawnY = respawnZ = 0f;
            destroyedCheckpointIds = new List<string>();
        }
    }

    [System.Serializable]
    public class CheckpointData
    {
        public List<string> destroyedIds = new List<string>();
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("💾 SaveSystem creado");
        }
        else
        {
            Debug.Log("💾 SaveSystem duplicado destruido");
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        saveFile = Application.persistentDataPath + "/gamedata.json";
        checkpointsFile = Application.persistentDataPath + "/checkpoints.json";

        Debug.Log("🔍 Buscando jugador...");
        FindPlayer();

        Debug.Log("📁 Configurando save file: " + saveFile);
        LoadGameData();

        LoadDestroyedCheckpoints();
        ApplyDestroyedCheckpoints();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"📋 Escena cargada: {scene.name}");

        FindPlayer();
        ApplyDestroyedCheckpoints();

        if (scene.name != "MainMenu")
        {
            LoadGameData();
        }
    }

    void FindPlayer()
    {
        PlayerHealth[] allPlayerHealths = FindObjectsOfType<PlayerHealth>();

        if (allPlayerHealths.Length > 0)
        {
            PlayerHealth playerHealth = allPlayerHealths[0];
            player = playerHealth.transform;

            Debug.Log($"✅ Jugador encontrado por PlayerHealth: {player.name} en {player.position}");
            Debug.Log($"   HP: {playerHealth.GetCurrentHealth()} | Soul: {playerHealth.GetCurrentSoul()}");
        }
        else
        {
            Debug.LogError("❌ No se encontró ningún objeto con PlayerHealth");

            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
                Debug.Log($"✅ Jugador encontrado por tag: {player.name}");
            }
        }
    }

    void Update()
    {
        if (nearSavePoint && Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("🎮 Tecla F presionada - Intentando guardar");
            SaveGameData();
        }

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.H))
        {
            DebugRemoveHealth();
        }
    }

    void DebugRemoveHealth()
    {
        if (player == null)
        {
            Debug.LogWarning("⚠️ No se puede quitar vida: jugador no encontrado");
            return;
        }

        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if (ph == null)
        {
            Debug.LogWarning("⚠️ No se puede quitar vida: PlayerHealth no encontrado");
            return;
        }

        ph.TakeDamage(1);
        Debug.Log($"🔧 DEBUG: Vida quitada | HP actual: {ph.GetCurrentHealth()}");
    }

    public void SaveGameData()
    {
        Debug.Log("💾 === INICIANDO GUARDADO ===");

        if (player == null)
        {
            Debug.LogError("❌ No se puede guardar: player es null");
            FindPlayer();
            if (player == null)
            {
                Debug.LogError("❌ Aún no se puede encontrar el jugador");
                return;
            }
        }

        Debug.Log($"✅ Jugador encontrado: {player.name}");

        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if (ph == null)
        {
            Debug.LogError($"❌ PlayerHealth no encontrado en {player.name}");
            return;
        }

        Debug.Log($"✅ PlayerHealth encontrado");

        try
        {
            GameData data = new GameData();

            data.playerX = player.position.x;
            data.playerY = player.position.y;
            data.playerZ = player.position.z;
            data.health = ph.GetCurrentHealth();
            data.soul = ph.GetCurrentSoul();
            data.currentScene = SceneManager.GetActiveScene().name;
            data.timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            data.respawnX = lastRespawnPoint.x;
            data.respawnY = lastRespawnPoint.y;
            data.respawnZ = lastRespawnPoint.z;

            if (CurrencySystem.Instance != null)
            {
                data.coins = CurrencySystem.Instance.GetCurrentCoins();
            }

            data.destroyedCheckpointIds = new List<string>(destroyedCheckpoints);

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(saveFile, json);

            Debug.Log($"✅ GUARDADO EXITOSO! HP: {data.health} | Soul: {data.soul:F1} | Pos: ({data.playerX:F1}, {data.playerY:F1}, {data.playerZ:F1})");
            Debug.Log($"📄 Checkpoints destruidos: {data.destroyedCheckpointIds.Count}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ ERROR EN GUARDADO: {e.Message}");
        }
    }

    public void LoadGameData()
    {
        if (!File.Exists(saveFile))
        {
            Debug.Log("📁 No hay archivo de guardado - Creando datos por defecto");
            CreateDefaultSave();
            return;
        }

        try
        {
            string json = File.ReadAllText(saveFile);
            GameData data = JsonUtility.FromJson<GameData>(json);

            if (SceneManager.GetActiveScene().name == data.currentScene && player != null)
            {
                Debug.Log($"📍 Teleportando jugador a posición guardada: ({data.playerX:F1}, {data.playerY:F1}, {data.playerZ:F1})");
                player.position = new Vector3(data.playerX, data.playerY, data.playerZ);

                PlayerHealth ph = player.GetComponent<PlayerHealth>();
                if (ph != null)
                {
                    ph.SetCurrentHealth(data.health);
                    ph.SetCurrentSoul(data.soul);
                    Debug.Log($"💚 Estado cargado: HP: {data.health} | Soul: {data.soul:F1}");
                }
            }

            lastRespawnPoint = new Vector3(data.respawnX, data.respawnY, data.respawnZ);
            Debug.Log($"🔄 Respawn point cargado: {lastRespawnPoint}");

            if (CurrencySystem.Instance != null)
            {
                CurrencySystem.Instance.SetCoins(data.coins);
            }

            if (data.destroyedCheckpointIds != null)
            {
                destroyedCheckpoints = new HashSet<string>(data.destroyedCheckpointIds);
                Debug.Log($"📄 Checkpoints destruidos cargados: {destroyedCheckpoints.Count}");
            }

            Debug.Log($"📂 Partida cargada completamente");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error cargando: {e.Message}");
            CreateDefaultSave();
        }
    }

    void CreateDefaultSave()
    {
        GameData defaultData = new GameData();

        if (player != null)
        {
            defaultData.playerX = player.position.x;
            defaultData.playerY = player.position.y;
            defaultData.playerZ = player.position.z;
            defaultData.respawnX = player.position.x;
            defaultData.respawnY = player.position.y;
            defaultData.respawnZ = player.position.z;
            lastRespawnPoint = player.position;
        }

        defaultData.currentScene = SceneManager.GetActiveScene().name;
        defaultData.timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        try
        {
            string json = JsonUtility.ToJson(defaultData, true);
            File.WriteAllText(saveFile, json);
            Debug.Log("✅ Datos por defecto creados");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error creando defaults: {e.Message}");
        }
    }

    public void RegisterDestroyedCheckpoint(string checkpointId)
    {
        destroyedCheckpoints.Add(checkpointId);
        SaveDestroyedCheckpoints();
        Debug.Log($"🗑️ Checkpoint destruido registrado: {checkpointId}");
    }

    void SaveDestroyedCheckpoints()
    {
        try
        {
            CheckpointData checkpointData = new CheckpointData();
            checkpointData.destroyedIds = new List<string>(destroyedCheckpoints);

            string json = JsonUtility.ToJson(checkpointData, true);
            File.WriteAllText(checkpointsFile, json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error guardando checkpoints: {e.Message}");
        }
    }

    void LoadDestroyedCheckpoints()
    {
        if (!File.Exists(checkpointsFile)) return;

        try
        {
            string json = File.ReadAllText(checkpointsFile);
            CheckpointData checkpointData = JsonUtility.FromJson<CheckpointData>(json);

            if (checkpointData.destroyedIds != null)
            {
                destroyedCheckpoints = new HashSet<string>(checkpointData.destroyedIds);
                Debug.Log($"📄 Checkpoints destruidos cargados desde archivo: {destroyedCheckpoints.Count}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error cargando checkpoints: {e.Message}");
        }
    }

    void ApplyDestroyedCheckpoints()
    {
        Checkpoint[] allCheckpoints = FindObjectsOfType<Checkpoint>();

        foreach (Checkpoint checkpoint in allCheckpoints)
        {
            string checkpointId = checkpoint.GetCheckpointId();
            if (destroyedCheckpoints.Contains(checkpointId))
            {
                Debug.Log($"🗑️ Destruyendo checkpoint persistente: {checkpointId}");
                Destroy(checkpoint.gameObject);
            }
        }
    }

    public bool IsCheckpointDestroyed(string checkpointId)
    {
        return destroyedCheckpoints.Contains(checkpointId);
    }

    public void UpdateRespawnPoint(Vector3 newRespawnPoint)
    {
        lastRespawnPoint = newRespawnPoint;
        Debug.Log($"🔄 Respawn point actualizado: {newRespawnPoint}");
    }

    public void RespawnPlayer()
    {
        if (player == null)
        {
            FindPlayer();
            if (player == null)
            {
                Debug.LogError("❌ No se puede respawnear: jugador no encontrado");
                return;
            }
        }

        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if (ph == null)
        {
            Debug.LogError("❌ No se puede respawnear: PlayerHealth no encontrado");
            return;
        }

        Vector3 safeRespawnPosition = GetSafeRespawnPosition(lastRespawnPoint);

        Collider playerCollider = player.GetComponent<Collider>();
        bool colliderWasEnabled = false;
        if (playerCollider != null)
        {
            colliderWasEnabled = playerCollider.enabled;
            playerCollider.enabled = false;
        }

        player.position = safeRespawnPosition;

        ph.SetCurrentHealth(3);
        ph.SetCurrentSoul(0f);

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (playerCollider != null)
        {
            StartCoroutine(EnableColliderAfterFrame(playerCollider, colliderWasEnabled));
        }

        Debug.Log($"✅ Respawn seguro completado en: {safeRespawnPosition} | HP: {ph.GetCurrentHealth()} | Soul: {ph.GetCurrentSoul()}");
    }

    Vector3 GetSafeRespawnPosition(Vector3 targetPosition)
    {
        if (targetPosition == Vector3.zero)
        {
            Debug.LogWarning("⚠️ No hay respawn point válido, usando posición actual");
            return player.position;
        }

        Vector3 safePosition = targetPosition;

        RaycastHit hit;
        Vector3 rayStartPosition = new Vector3(targetPosition.x, targetPosition.y + 10f, targetPosition.z);

        if (Physics.Raycast(rayStartPosition, Vector3.down, out hit, 20f, GetGroundLayerMask()))
        {
            safePosition = hit.point + Vector3.up * 1.5f;
            Debug.Log($"🎯 Suelo detectado en: {hit.point} | Respawn ajustado a: {safePosition}");
        }
        else
        {
            safePosition = targetPosition + Vector3.up * 2f;
            Debug.LogWarning($"⚠️ No se detectó suelo, usando posición con offset: {safePosition}");
        }

        Collider playerCollider = player.GetComponent<Collider>();
        if (playerCollider != null)
        {
            Bounds playerBounds = playerCollider.bounds;
            Vector3 checkPosition = safePosition;

            if (Physics.CheckBox(checkPosition, playerBounds.extents, Quaternion.identity, GetObstacleLayerMask()))
            {
                for (int i = 1; i <= 10; i++)
                {
                    Vector3 testPosition = safePosition + Vector3.up * i;
                    if (!Physics.CheckBox(testPosition, playerBounds.extents, Quaternion.identity, GetObstacleLayerMask()))
                    {
                        safePosition = testPosition;
                        Debug.Log($"🔧 Obstáculo detectado, ajustando altura: {safePosition}");
                        break;
                    }
                }
            }
        }

        return safePosition;
    }

    LayerMask GetGroundLayerMask()
    {
        LayerMask groundMask = 0;
        groundMask |= (1 << LayerMask.NameToLayer("Default"));
        return groundMask;
    }

    LayerMask GetObstacleLayerMask()
    {
        LayerMask obstacleMask = ~0;
        return obstacleMask;
    }

    IEnumerator EnableColliderAfterFrame(Collider playerCollider, bool wasEnabled)
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        if (playerCollider != null)
        {
            playerCollider.enabled = wasEnabled;
            Debug.Log("🔄 Colisiones del jugador reactivadas");
        }
    }

    public void EnterSavePoint()
    {
        nearSavePoint = true;
        Debug.Log("💾 ENTRÓ EN SAVE POINT - Presiona F para guardar");
    }

    public void ExitSavePoint()
    {
        nearSavePoint = false;
        Debug.Log("📤 SALIÓ DEL SAVE POINT");
    }

    public void ForceSave()
    {
        Debug.Log("🔨 GUARDADO FORZADO");
        SaveGameData();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
