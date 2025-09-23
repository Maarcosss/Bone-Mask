using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;

    private string[] lines;
    private int index;
    private System.Action onDialogueEnd; // Callback al terminar

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        dialoguePanel.SetActive(false);
    }

    public void StartDialogue(string[] dialogueLines, System.Action onEnd)
    {
        lines = dialogueLines;
        index = 0;
        onDialogueEnd = onEnd;
        dialoguePanel.SetActive(true);
        ShowNextLine();
    }

    public void ShowNextLine()
    {
        if (index < lines.Length)
        {
            dialogueText.text = lines[index];
            index++;
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        onDialogueEnd?.Invoke();
    }
}
