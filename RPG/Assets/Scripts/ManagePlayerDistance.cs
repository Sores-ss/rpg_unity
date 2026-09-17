using UnityEngine;
using UnityEngine.InputSystem;

public class ManagePlayerDistance : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Player transform used to compute interaction distance.")]
    [SerializeField] private Transform player;
    [Tooltip("Shown only when player is close enough.")]
    [SerializeField] private GameObject redIndicator;
    [Tooltip("Dialogue UI controller used for this NPC.")]
    [SerializeField] private SimpleDialogueUI dialogueUI;

    [Header("Dialogue")]
    [Tooltip("Optional. Reusable dialogue asset. If assigned, it overrides inline fields below.")]
    [SerializeField] private DialogueData dialogueData;
    [Tooltip("Fallback NPC name when no DialogueData is assigned.")]
    [SerializeField] private string npcName = "PNJ";
    [TextArea(2, 4)]
    [Tooltip("Fallback dialogue lines when no DialogueData is assigned.")]
    [SerializeField] private string[] dialogueLines;

    [Header("Settings")]
    [Tooltip("Distance required to show indicator and allow interaction.")]
    [Min(0.1f)]
    [SerializeField] private float interactionDistance = 2f;
    private bool isPlayerNear;

    private void Awake()
    {
        AutoAssignMissingReferences();
    }

    private void Start()
    {
        AutoAssignMissingReferences();

        if (dialogueUI != null)
            dialogueUI.CloseDialogue();

        if (redIndicator != null)
            redIndicator.SetActive(false);

        if (player == null)
            Debug.LogError("Player is not assigned on " + gameObject.name);

        if (redIndicator == null)
            Debug.LogWarning("RedIndicator is not assigned on " + gameObject.name);

        if (dialogueUI == null)
            Debug.LogWarning("DialogueUI is not assigned on " + gameObject.name);

        if (redIndicator != null && dialogueUI != null && dialogueUI.DialogueRoot != null)
        {
            if (redIndicator == dialogueUI.DialogueRoot || redIndicator.transform.IsChildOf(dialogueUI.DialogueRoot.transform))
                Debug.LogWarning("RedIndicator is linked inside DialogueRoot on " + gameObject.name + ". Use a separate indicator object.");
        }
    }

    private void Update()
    {
        if (player == null)
            return;

        float distance = Vector2.Distance(transform.position, player.position);
        isPlayerNear = distance <= interactionDistance;

        if (redIndicator != null)
            redIndicator.SetActive(isPlayerNear);

        if (!isPlayerNear)
        {
            if (dialogueUI != null && dialogueUI.IsOpen)
                dialogueUI.CloseDialogue();
            return;
        }

        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            Interact();
    }

    private void Interact()
    {
        string speaker = GetSpeakerName();
        string[] lines = GetDialogueLines();

        if (dialogueUI == null || lines == null || lines.Length == 0)
            return;

        if (!dialogueUI.IsOpen)
        {
            dialogueUI.OpenDialogue(speaker, lines);
            return;
        }

        dialogueUI.NextLine();
    }

    private string GetSpeakerName()
    {
        if (dialogueData != null && !string.IsNullOrWhiteSpace(dialogueData.NpcName))
            return dialogueData.NpcName;
        return npcName;
    }

    private string[] GetDialogueLines()
    {
        if (dialogueData != null && dialogueData.Lines != null && dialogueData.Lines.Length > 0)
            return dialogueData.Lines;
        return dialogueLines;
    }

    private void AutoAssignMissingReferences()
    {
        if (dialogueUI == null)
            dialogueUI = GetComponentInChildren<SimpleDialogueUI>(true);

        if (redIndicator == null)
        {
            Transform indicator = transform.Find("RedIndicator");
            if (indicator != null)
                redIndicator = indicator.gameObject;
        }
    }
}
