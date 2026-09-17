using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private GameObject hotbarRoot;

    [Header("Hotbar Style")]
    [SerializeField] private Color slotColor = new Color(0f, 0f, 0f, 0.65f);
    [SerializeField] private Color selectedOutlineColor = Color.red;
    [SerializeField] private Color iconTint = Color.white;

    [Header("Hotbar Layout")]
    [SerializeField] private Vector2 slotSize = new Vector2(72f, 72f);
    [SerializeField] private Vector2 slotSpacing = new Vector2(10f, 10f);
    [SerializeField] private int hotbarSlots = 9;
    [SerializeField] private Vector2 bottomOffset = new Vector2(0f, 48f);

    public static ItemAnimationType SelectedAnimationType { get; private set; } = ItemAnimationType.None;

    private Image[] slotIcons;
    private Text[] slotQuantityTexts;
    private GameObject[] slotOutlines;
    private int selectedSlot;



    private void Awake()
    {
        if (inventoryManager == null)
            inventoryManager = InventoryManager.Instance;

        if (inventoryManager == null)
            inventoryManager = FindFirstObjectByType<InventoryManager>();

        EnsureHotbarExists();

        if (hotbarRoot != null)
            hotbarRoot.SetActive(true);
    }

    private void OnEnable()
    {
        if (inventoryManager != null)
            inventoryManager.InventoryChanged += Refresh;

        Refresh();
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
            MoveSelection(-1);

        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
            MoveSelection(1);

        int numberSelect = ReadNumberSelection();
        if (numberSelect >= 0)
            selectedSlot = numberSelect;

        if (Keyboard.current.eKey.wasPressedThisFrame && inventoryManager != null)
        {
            if (inventoryManager.TryGetItem(selectedSlot, out InventoryItemData itemToUse)
                && itemToUse.animationType == ItemAnimationType.Meat)
            {
                bool used = inventoryManager.UseItemAt(selectedSlot);
                if (used)
                {
                    ApplyItemEffect(itemToUse);
                    if (selectedSlot >= inventoryManager.ItemCount)
                        selectedSlot = Mathf.Max(0, inventoryManager.ItemCount - 1);
                }
            }
        }

        UpdateSelectionOutline();
    }

    private void OnDisable()
    {
        if (inventoryManager != null)
            inventoryManager.InventoryChanged -= Refresh;
    }

    private void Refresh()
    {
        if (slotIcons == null)
            return;

        for (int i = 0; i < slotIcons.Length; i++)
        {
            slotIcons[i].sprite = null;
            slotIcons[i].color = new Color(1f, 1f, 1f, 0f);
            if (slotQuantityTexts != null && i < slotQuantityTexts.Length && slotQuantityTexts[i] != null)
                slotQuantityTexts[i].text = "";
        }

        if (inventoryManager == null)
        {
            UpdateSelectionOutline();
            return;
        }

        int visibleCount = Mathf.Min(inventoryManager.ItemCount, hotbarSlots);
        for (int i = 0; i < visibleCount; i++)
        {
            if (!inventoryManager.TryGetItem(i, out InventoryItemData itemData))
                continue;

            slotIcons[i].sprite = itemData.itemIcon;
            slotIcons[i].color = iconTint;
            slotIcons[i].preserveAspect = true;

            if (slotQuantityTexts != null && i < slotQuantityTexts.Length && slotQuantityTexts[i] != null)
            {
                int qty = itemData.EffectiveQuantity;
                slotQuantityTexts[i].text = qty > 1 ? qty.ToString() : "";
            }
        }

        UpdateSelectionOutline();
    }

    private int ReadNumberSelection()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame) return 0;
        if (Keyboard.current.digit2Key.wasPressedThisFrame) return 1;
        if (Keyboard.current.digit3Key.wasPressedThisFrame) return 2;
        if (Keyboard.current.digit4Key.wasPressedThisFrame) return 3;
        if (Keyboard.current.digit5Key.wasPressedThisFrame) return 4;
        if (Keyboard.current.digit6Key.wasPressedThisFrame) return 5;
        if (Keyboard.current.digit7Key.wasPressedThisFrame) return 6;
        if (Keyboard.current.digit8Key.wasPressedThisFrame) return 7;
        if (Keyboard.current.digit9Key.wasPressedThisFrame) return 8;
        return -1;
    }

    private void MoveSelection(int delta)
    {
        selectedSlot += delta;
        if (selectedSlot < 0)
            selectedSlot = hotbarSlots - 1;
        else if (selectedSlot >= hotbarSlots)
            selectedSlot = 0;

        UpdateSelectionOutline();
    }

    public void SelectSlot(int slotIndex)
    {
        if (hotbarSlots < 1)
            return;

        selectedSlot = Mathf.Clamp(slotIndex, 0, hotbarSlots - 1);
        UpdateSelectionOutline();
    }

    public bool TryTransferSlotToChest(int slotIndex)
    {
        ChestSystem chest = FindFirstObjectByType<ChestSystem>();
        if (chest == null || !chest.IsOpen)
            return false;

        bool moved = chest.TransferPlayerSlotToChest(slotIndex);
        if (moved)
            Refresh();

        return moved;
    }

    private void UpdateSelectionOutline()
    {
        if (slotOutlines == null)
            return;

        for (int i = 0; i < slotOutlines.Length; i++)
            slotOutlines[i].SetActive(i == selectedSlot);

        SelectedAnimationType = inventoryManager != null
            && inventoryManager.TryGetItem(selectedSlot, out InventoryItemData selected)
            ? selected.animationType
            : ItemAnimationType.None;
    }

    private void EnsureHotbarExists()
    {
        if (hotbarSlots < 1)
            hotbarSlots = 9;

        if (hotbarRoot != null && slotIcons != null && slotIcons.Length == hotbarSlots)
            return;

        GameObject canvasGo = new GameObject("InventoryCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;

        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        if (FindFirstObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

        if (hotbarRoot == null)
        {
            hotbarRoot = new GameObject("HotbarRoot", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            hotbarRoot.transform.SetParent(canvas.transform, false);

            RectTransform rootRect = hotbarRoot.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.5f, 0f);
            rootRect.anchorMax = new Vector2(0.5f, 0f);
            rootRect.pivot = new Vector2(0.5f, 0f);
            rootRect.anchoredPosition = bottomOffset;
            rootRect.sizeDelta = new Vector2(
                hotbarSlots * slotSize.x + (hotbarSlots - 1) * slotSpacing.x,
                slotSize.y);

            HorizontalLayoutGroup layout = hotbarRoot.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = slotSpacing.x;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlHeight = false;
            layout.childControlWidth = false;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = false;
        }

        for (int i = hotbarRoot.transform.childCount - 1; i >= 0; i--)
            Destroy(hotbarRoot.transform.GetChild(i).gameObject);

        slotIcons = new Image[hotbarSlots];
        slotQuantityTexts = new Text[hotbarSlots];
        slotOutlines = new GameObject[hotbarSlots];

        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (defaultFont == null)
            defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        for (int i = 0; i < hotbarSlots; i++)
        {
            GameObject slotGo = new GameObject("Slot_" + (i + 1), typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            slotGo.transform.SetParent(hotbarRoot.transform, false);

            Image slotBg = slotGo.GetComponent<Image>();
            slotBg.color = slotColor;

            HotbarSlot slotComp = slotGo.AddComponent<HotbarSlot>();
            slotComp.slotIndex = i;
            slotComp.inventoryUI = this;

            RectTransform slotRect = slotGo.GetComponent<RectTransform>();
            slotRect.sizeDelta = slotSize;

            GameObject iconGo = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconGo.transform.SetParent(slotGo.transform, false);

            RectTransform iconRect = iconGo.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = slotSize - new Vector2(12f, 12f);
            iconRect.anchoredPosition = Vector2.zero;

            Image icon = iconGo.GetComponent<Image>();
            icon.color = new Color(1f, 1f, 1f, 0f);
            slotIcons[i] = icon;

            GameObject quantityGo = new GameObject("Quantity", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            quantityGo.transform.SetParent(slotGo.transform, false);

            RectTransform quantityRect = quantityGo.GetComponent<RectTransform>();
            quantityRect.anchorMin = Vector2.zero;
            quantityRect.anchorMax = Vector2.one;
            quantityRect.offsetMin = new Vector2(2f, 2f);
            quantityRect.offsetMax = new Vector2(-2f, -2f);

            Text quantityText = quantityGo.GetComponent<Text>();
            quantityText.font = defaultFont;
            quantityText.fontSize = 14;
            quantityText.fontStyle = FontStyle.Bold;
            quantityText.alignment = TextAnchor.LowerRight;
            quantityText.color = Color.white;
            quantityText.raycastTarget = false;
            quantityText.text = "";
            slotQuantityTexts[i] = quantityText;

            GameObject outlineGo = new GameObject("SelectionOutline", typeof(RectTransform));
            outlineGo.transform.SetParent(slotGo.transform, false);

            RectTransform outlineRect = outlineGo.GetComponent<RectTransform>();
            outlineRect.anchorMin = Vector2.zero;
            outlineRect.anchorMax = Vector2.one;
            outlineRect.pivot = new Vector2(0.5f, 0.5f);
            outlineRect.offsetMin = Vector2.zero;
            outlineRect.offsetMax = Vector2.zero;

            CreateBorderEdge(outlineGo.transform, "Top", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -2f), Vector2.zero);
            CreateBorderEdge(outlineGo.transform, "Bottom", new Vector2(0f, 0f), new Vector2(1f, 0f), Vector2.zero, new Vector2(0f, 2f));
            CreateBorderEdge(outlineGo.transform, "Left", new Vector2(0f, 0f), new Vector2(0f, 1f), Vector2.zero, new Vector2(2f, 0f));
            CreateBorderEdge(outlineGo.transform, "Right", new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-2f, 0f), Vector2.zero);

            outlineGo.SetActive(false);
            slotOutlines[i] = outlineGo;
        }

        selectedSlot = Mathf.Clamp(selectedSlot, 0, hotbarSlots - 1);
        UpdateSelectionOutline();
    }

    private void CreateBorderEdge(Transform parent, string edgeName, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        GameObject edgeGo = new GameObject(edgeName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        edgeGo.transform.SetParent(parent, false);

        RectTransform edgeRect = edgeGo.GetComponent<RectTransform>();
        edgeRect.anchorMin = anchorMin;
        edgeRect.anchorMax = anchorMax;
        edgeRect.offsetMin = offsetMin;
        edgeRect.offsetMax = offsetMax;

        Image edgeImage = edgeGo.GetComponent<Image>();
        edgeImage.color = selectedOutlineColor;
        edgeImage.raycastTarget = false;
    }

    private void ApplyItemEffect(InventoryItemData itemData)
    {
        switch (itemData.effectType)
        {
            case ItemEffectType.Heal:
                ApplyHealEffect(itemData.effectValue);
                break;
            default:
                Debug.Log("Item used: " + itemData.itemName);
                break;
        }
    }

    private void ApplyHealEffect(int healAmount)
    {
        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.LogWarning("PlayerHealth NOT FOUND! Cannot apply heal effect.");
            return;
        }

        int newHealth = playerHealth.CurrentHealth + healAmount;
        newHealth = Mathf.Min(newHealth, playerHealth.MaxHealth);
        int actualHeal = newHealth - playerHealth.CurrentHealth;

        if (actualHeal > 0)
        {
            Debug.Log("Item consumed: Healed " + actualHeal + " HP!");
        }
        else if (actualHeal == 0)
        {
            Debug.Log("Item consumed: Already at full health!");
        }

        playerHealth.Heal(actualHeal);
    }
}
