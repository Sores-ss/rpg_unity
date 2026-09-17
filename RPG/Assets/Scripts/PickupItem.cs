using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [Header("Item")]
    [SerializeField] private string           itemName      = "Item";
    [SerializeField] private Sprite           itemIcon;
    [SerializeField] private ItemEffectType   itemEffect    = ItemEffectType.None;
    [SerializeField] private ItemAnimationType animationType = ItemAnimationType.None;
    [SerializeField] private int              effectValue   = 0;
    [SerializeField] private float            pickupDistance = 1f;
    [SerializeField] private int              maxStack      = 1;
    [SerializeField] private bool             destroyOnPickup = true;

    private Transform player;
    private bool      invalidSetup;

    private void Start()
    {
        if (itemIcon == null)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
                itemIcon = sr.sprite;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
            player = playerObject.transform;

        if (player != null && (transform == player || transform.IsChildOf(player)))
        {
            invalidSetup = true;
            Debug.LogError("PickupItem is on Player (or child). Move this script to a world item object.");
            enabled = false;
        }
    }

    private void Update()
    {
        if (invalidSetup || player == null)
            return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance > pickupDistance)
            return;

        if (itemEffect == ItemEffectType.Gold)
        {
            if (PlayerStats.Instance != null)
                PlayerStats.Instance.AddGold(effectValue);

            Collect();
            return;
        }

        if (InventoryManager.Instance == null)
            return;

        bool added = InventoryManager.Instance.AddItem(itemName, itemIcon, itemEffect, animationType, effectValue, 1, maxStack);
        if (!added)
            return;

        Collect();
    }

    public void SetData(string name, Sprite icon, ItemEffectType effect, ItemAnimationType anim, int value, float distance = 1f, int stack = 1)
    {
        itemName       = name;
        itemIcon       = icon;
        itemEffect     = effect;
        animationType  = anim;
        effectValue    = value;
        pickupDistance = distance;
        maxStack       = Mathf.Max(1, stack);
    }

    private void Collect()
    {
        if (destroyOnPickup)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupDistance);
    }
}
