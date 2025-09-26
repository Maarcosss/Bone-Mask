using UnityEngine;
using System.Collections;

public class NPC : MonoBehaviour
{
    [Header("Dialogo")]
    [TextArea]
    public string[] dialogueLines;

    [Header("Interacción")]
    public KeyCode interactKey = KeyCode.T;

    [Header("Referencias")]
    public CameraFollow CameraFollowRef; // asignar en inspector o buscar en Start

    private bool playerInRange = false;
    private bool inDialogue = false;
    private PlayerMovement playerMovement;

    void Start()
    {
        if (CameraFollowRef == null)
            CameraFollowRef = FindObjectOfType<CameraFollow>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerMovement = other.GetComponent<PlayerMovement>();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    void Update()
    {
        if (playerInRange && !inDialogue && Input.GetKeyDown(interactKey))
        {
            StartDialogue();
        }

        if (inDialogue && Input.GetKeyDown(KeyCode.Space))
        {
            DialogueManager.Instance.ShowNextLine();
        }
    }

    void StartDialogue()
    {
        inDialogue = true;
        if (playerMovement != null) playerMovement.validar_inputs = false;
        if (playerMovement != null) playerMovement.validar_inputs_esc = false;
        if (CameraFollowRef != null) CameraFollowRef.validar_inputs_camara = false;

        DialogueManager.Instance.StartDialogue(dialogueLines, EndDialogue);
    }

    void EndDialogue()
    {
        inDialogue = false;
        StartCoroutine(EnableInputsNextFrame());
    }

    private IEnumerator EnableInputsNextFrame()
    {
        yield return null;
        if (playerMovement != null)
        {
            playerMovement.validar_inputs = true;
            playerMovement.validar_inputs_esc = true;
        }

        if (CameraFollowRef != null)
        {
            CameraFollowRef.validar_inputs_camara = true;
        }
    }
}