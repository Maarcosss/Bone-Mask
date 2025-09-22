using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class SaveSystem : MonoBehaviour
{
    private string saveFile;
    private bool nearCheckpoint = false;
    private Transform player;

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
        player = GameObject.FindGameObjectWithTag("Player").transform;

        LoadGame(); // intentar cargar partida al inicio
    }

    void Update()
    {
        if (nearCheckpoint && Input.GetKeyDown(KeyCode.F))
        {
            SaveGame();
        }
    }

    // ===== GUARDAR =====
    void SaveGame()
    {
        PlayerHealth ph = player.GetComponent<PlayerHealth>();

        SaveData data = new SaveData();
        data.x = player.position.x;
        data.y = player.position.y;
        data.z = player.position.z;
        data.health = ph.GetCurrentHealth();
        data.soul = ph.GetCurrentSoul();
        data.scene = SceneManager.GetActiveScene().name;

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFile, json);

        Debug.Log("✅ Juego guardado en checkpoint");
    }

    // ===== CARGAR =====
    void LoadGame()
    {
        if (File.Exists(saveFile))
        {
            string json = File.ReadAllText(saveFile);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            if (SceneManager.GetActiveScene().name == data.scene)
            {
                player.position = new Vector3(data.x, data.y, data.z);

                PlayerHealth ph = player.GetComponent<PlayerHealth>();
                ph.SetCurrentHealth(data.health);
                ph.SetCurrentSoul(data.soul);

                Debug.Log("📂 Partida cargada");
            }
            else
            {
                Debug.Log("El guardado es de otra escena: " + data.scene);
            }
        }
    }

    // ===== CHECKPOINTS =====
    private void OnTriggerEnter(Collider other)
    {   
        if (other.CompareTag("Checkpoint"))
        {
            nearCheckpoint = true;
            Debug.Log("ℹ️ Cerca de un checkpoint: pulsa F para guardar");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            nearCheckpoint = false;
            Debug.Log("ℹ️ Saliste del checkpoint");
        }
    }
}
