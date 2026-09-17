using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SimpleDialogueUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Root object to show/hide when dialogue opens/closes.")]
    [SerializeField] private GameObject dialogueRoot;
    [Tooltip("Primary speaker text (TextMeshPro).")]
    [SerializeField] private TMP_Text speakerTMPText;
    [Tooltip("Primary dialogue line text (TextMeshPro).")]
    [SerializeField] private TMP_Text lineTMPText;
    [Tooltip("Fallback speaker text (Legacy UI Text).")]
    [SerializeField] private Text speakerLegacyText;
    [Tooltip("Fallback line text (Legacy UI Text).")]
    [SerializeField] private Text lineLegacyText;

    [Header("Typing")]
    [Tooltip("Characters displayed per second during typewriter effect.")]
    [Min(1f)]
    [SerializeField] private float charactersPerSecond = 45f;
    [Tooltip("Use TMP fields first. Disable only if using legacy UI Text fields.")]
    [SerializeField] private bool preferTMP = true;

    private string[] currentLines;
    private int currentIndex;
    private Coroutine typingRoutine;
    private bool isTyping;
    private string fullCurrentLine;

    public bool IsOpen => dialogueRoot != null && dialogueRoot.activeSelf;
    public GameObject DialogueRoot => dialogueRoot;

    private void Start()
    {
        ConfigureTextTargets();
        CloseDialogue();
    }

    private void ConfigureTextTargets()
    {
        if (preferTMP)
        {
            if (speakerTMPText != null && speakerLegacyText != null)
                speakerLegacyText.gameObject.SetActive(false);
            if (lineTMPText != null && lineLegacyText != null)
                lineLegacyText.gameObject.SetActive(false);
        }
        else
        {
            if (speakerLegacyText != null && speakerTMPText != null)
                speakerTMPText.gameObject.SetActive(false);
            if (lineLegacyText != null && lineTMPText != null)
                lineTMPText.gameObject.SetActive(false);
        }

        if (lineTMPText != null)
        {
            lineTMPText.textWrappingMode = TextWrappingModes.Normal;
            lineTMPText.overflowMode = TextOverflowModes.Truncate;
        }

        if (lineLegacyText != null)
        {
            lineLegacyText.horizontalOverflow = HorizontalWrapMode.Wrap;
            lineLegacyText.verticalOverflow = VerticalWrapMode.Truncate;
        }
    }

    public void OpenDialogue(string speaker, string[] lines)
    {
        if (dialogueRoot == null || lines == null || lines.Length == 0)
            return;

        if (!HasAnyLineTarget())
        {
            Debug.LogWarning("No line text target assigned on " + gameObject.name + ". Assign Line TMPText or Line Legacy Text.");
            return;
        }

        StopTypingIfNeeded();

        currentLines = lines;
        currentIndex = 0;

        SetSpeakerText(speaker);

        dialogueRoot.SetActive(true);
        StartTypingCurrentLine();
    }

    public bool NextLine()
    {
        if (!IsOpen || currentLines == null)
            return false;

        if (isTyping)
        {
            CompleteCurrentLineInstantly();
            return true;
        }

        currentIndex++;
        if (currentIndex >= currentLines.Length)
        {
            CloseDialogue();
            return false;
        }

        StartTypingCurrentLine();
        return true;
    }

    public void CloseDialogue()
    {
        StopTypingIfNeeded();

        if (dialogueRoot != null)
            dialogueRoot.SetActive(false);

        SetSpeakerText(string.Empty);
        SetLineText(string.Empty);
        currentLines = null;
        currentIndex = 0;
        fullCurrentLine = string.Empty;
    }

    private void StartTypingCurrentLine()
    {
        if (currentLines == null || currentIndex < 0 || currentIndex >= currentLines.Length)
            return;

        fullCurrentLine = currentLines[currentIndex];
        StopTypingIfNeeded();
        typingRoutine = StartCoroutine(TypeLine(fullCurrentLine));
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        SetLineText(string.Empty);

        float safeSpeed = Mathf.Max(charactersPerSecond, 1f);
        float delay = 1f / safeSpeed;

        for (int i = 1; i <= line.Length; i++)
        {
            SetLineText(line.Substring(0, i));
            yield return new WaitForSeconds(delay);
        }

        isTyping = false;
        typingRoutine = null;
    }

    private void CompleteCurrentLineInstantly()
    {
        StopTypingIfNeeded();
        SetLineText(fullCurrentLine);
    }

    private void StopTypingIfNeeded()
    {
        if (typingRoutine == null)
            return;

        StopCoroutine(typingRoutine);
        typingRoutine = null;
        isTyping = false;
    }

    private void SetSpeakerText(string value)
    {
        if (preferTMP)
        {
            if (speakerTMPText != null)
            {
                speakerTMPText.text = value;
                return;
            }

            if (speakerLegacyText != null)
            {
                speakerLegacyText.text = value;
                return;
            }

            return;
        }

        if (speakerLegacyText != null)
        {
            speakerLegacyText.text = value;
            return;
        }

        if (speakerTMPText != null)
        {
            speakerTMPText.text = value;
            return;
        }

        Debug.LogWarning("No speaker text target assigned on " + gameObject.name + ".");
    }

    private void SetLineText(string value)
    {
        if (preferTMP)
        {
            if (lineTMPText != null)
            {
                lineTMPText.text = value;
                return;
            }

            if (lineLegacyText != null)
            {
                lineLegacyText.text = value;
                return;
            }

            return;
        }

        if (lineLegacyText != null)
        {
            lineLegacyText.text = value;
            return;
        }

        if (lineTMPText != null)
        {
            lineTMPText.text = value;
            return;
        }
    }

    private bool HasAnyLineTarget()
    {
        return lineTMPText != null || lineLegacyText != null;
    }
}
