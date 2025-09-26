using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.IO;

public class SaveSystem : MonoBehaviour
{
    [Header("Input System")]
    public InputActionAsset inputActions;

    [Header("Save Settings")]
    [Tooltip("Mostrar mensajes de debug del sistema de guardado")]
    public bool showDebugMessages = true;

    // Sistema de guardado
    private string saveFile;
    private bool nearCheckpoint = false;
    private Transform player;

    // Input System
    private InputAction saveAction;

    [System.Serializable]
    class SaveData
    {
        public float x, y, z;
        public int health;
        public float soul;
        public string scene;
    }

    void Start()
    {
        saveFile = Application.persistentDataPath + "/save.json";

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError("❌ No se encontró objeto con tag 'Player'");
        }

        SetupInputActions();
        LoadGame();
    }

    void SetupInputActions()
    {
        if (inputActions == null)
        {
            inputActions = Resources.Load<InputActionAsset>("InputSystem_Actions");
        }

        if (inputActions != null)
        {
            var playerActionMap = inputActions.FindActionMap("Player");
            if (playerActionMap != null)
            {
                saveAction = playerActionMap.FindAction("Save");
            }
        }

        // Crear acción manualmente si no se encuentra
        if (saveAction == null)
        {
            saveAction = new InputAction("Save", InputActionType.Button);
            saveAction.AddBinding("<Keyboard>/f");
            saveAction.AddBinding("<Gamepad>/buttonNorth"); // X/Square
        }

        // Configurar callback
        saveAction.started += OnSave;

        // Habilitar acción
        saveAction?.Enable();
    }

    void OnDestroy()
    {
        saveAction?.Disable();
    }

    void OnSave(InputAction.CallbackContext context)
    {
        if (nearCheckpoint)
        {
            SaveGame();
        }
    }

    void SaveGame()
    {
        if (player == null)
        {
            Debug.LogError("❌ No se puede guardar: referencia al jugador es null");
            return;
        }

        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if (ph == null)
        {
            Debug.LogError("❌ No se puede guardar: PlayerHealth no encontrado");
            return;
        }

        SaveData data = new SaveData();
        data.x = player.position.x;
        data.y = player.position.y;
        data.z = player.position.z;
        data.health = ph.GetCurrentHealth();
        data.soul = ph.GetCurrentSoul();
        data.scene = SceneManager.GetActiveScene().name;

        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(saveFile, json);

            if (showDebugMessages)
                Debug.Log("✅ Juego guardado en checkpoint");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error al guardar: {e.Message}");
        }
    }

    void LoadGame()
    {
        if (!File.Exists(saveFile))
        {
            if (showDebugMessages)
                Debug.Log("📁 No hay partida guardada");
            return;
        }

        if (player == null)
        {
            Debug.LogError("❌ No se puede cargar: referencia al jugador es null");
            return;
        }

        try
        {
            string json = File.ReadAllText(saveFile);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            if (SceneManager.GetActiveScene().name == data.scene)
            {
                player.position = new Vector3(data.x, data.y, data.z);

                PlayerHealth ph = player.GetComponent<PlayerHealth>();
                if (ph != null)
                {
                    ph.SetCurrentHealth(data.health);
                    ph.SetCurrentSoul(data.soul);
                }

                if (showDebugMessages)
                    Debug.Log("📂 Partida cargada correctamente");
            }
            else
            {
                if (showDebugMessages)
                    Debug.Log($"📁 El guardado es de otra escena: {data.scene}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error al cargar partida: {e.Message}");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            nearCheckpoint = true;
            if (showDebugMessages)
                Debug.Log("💾 Cerca de un checkpoint: pulsa F para guardar");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            nearCheckpoint = false;
            if (showDebugMessages)
                Debug.Log("📤 Saliste del checkpoint");
        }
    }

    // Métodos públicos para guardado manual
    public void ForceSave()
    {
        SaveGame();
    }

    public void ForceLoad()
    {
        LoadGame();
    }

    public bool HasSaveFile()
    {
        return File.Exists(saveFile);
    }

    public void DeleteSave()
    {
        if (File.Exists(saveFile))
        {
            File.Delete(saveFile);
            if (showDebugMessages)
                Debug.Log("🗑️ Partida eliminada");
        }
    }
}
