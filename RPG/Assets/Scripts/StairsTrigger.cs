using UnityEngine;

// Place on a trigger zone at the top or bottom of a staircase.
// Requires a Collider2D set as trigger on the same GameObject.
[RequireComponent(typeof(Collider2D))]
public class StairsTrigger : MonoBehaviour
{
    [Tooltip("The player must be on this level for the trigger to activate.")]
    [SerializeField] private int fromLevel = 0;
    [Tooltip("The level the player transitions to.")]
    [SerializeField] private int toLevel = 1;
    [Tooltip("Optional offset applied to the player's position after the transition (useful to nudge them onto the new floor).")]
    [SerializeField] private Vector2 exitOffset = Vector2.zero;

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (!col.isTrigger)
        {
            col.isTrigger = true;
            Debug.LogWarning("StairsTrigger: Collider2D was not set as trigger. Fixed automatically.", this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerLevel playerLevel = other.GetComponent<PlayerLevel>();
        if (playerLevel == null || playerLevel.CurrentLevel != fromLevel)
            return;

        playerLevel.SetLevel(toLevel);

        if (exitOffset.sqrMagnitude > 0.01f)
            other.transform.position += (Vector3)exitOffset;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.8f, 0f, 0.5f);
        Collider2D col = GetComponent<Collider2D>();
        if (col is BoxCollider2D box)
        {
            Gizmos.DrawCube(transform.position + (Vector3)box.offset, box.size);
        }
        else
        {
            Gizmos.DrawSphere(transform.position, 0.3f);
        }

#if UNITY_EDITOR
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 0.5f,
            $"Level {fromLevel} → {toLevel}");
#endif
    }
}
