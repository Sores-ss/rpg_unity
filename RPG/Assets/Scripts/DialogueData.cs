using UnityEngine;

[CreateAssetMenu(fileName = "DialogueData", menuName = "RPG/Dialogue Data", order = 1)]
public class DialogueData : ScriptableObject
{
    [Tooltip("Name displayed as speaker in dialogue UI.")]
    [SerializeField] private string npcName = "PNJ";
    [TextArea(2, 4)]
    [Tooltip("Dialogue lines shown sequentially when interacting with NPC.")]
    [SerializeField] private string[] lines;

    public string NpcName => npcName;
    public string[] Lines => lines;
}
