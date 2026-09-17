using UnityEngine;

public class Follow_Player : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Player transform to follow.")]
    [SerializeField] private Transform player;

    [Header("Camera Follow")]
    [Tooltip("Lerp speed of camera follow.")]
    [Min(0f)]
    [SerializeField] private float smoothSpeed = 5f;
    [Tooltip("Offset from target position (usually keep Z at -10 for 2D camera).")]
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10f);

    private void Start()
    {
        if (player == null)
            Debug.LogError("PLAYER NOT ASSIGNED! Drag the Player into the inspector for " + gameObject.name);
        else
            Debug.Log("Player found: " + player.name);
    }

    private void LateUpdate()
    {
        if (player == null)
            return;

        Vector3 desiredPosition = player.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}
