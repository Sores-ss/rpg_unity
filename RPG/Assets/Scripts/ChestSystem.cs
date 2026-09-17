using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class ChestSystem : MonoBehaviour
{
    [Header("Interaction")]
    [Tooltip("Player transform used to check interaction distance.")]
    [SerializeField] private Transform player;
    [Tooltip("Distance required to open the chest.")]
    [Min(0.1f)]
    [SerializeField] private float interactionDistance = 2f;
    [Tooltip("If true, the chest closes automatically when the player leaves the range.")]
    [SerializeField] private bool closeWhenPlayerLeaves = true;
    [Tooltip("If true, player movement is disabled while the chest is open.")]
    [SerializeField] private bool lockPlayerMovementWhileOpen = true;

    [Header("Animation")]
    [Tooltip("Animator that controls the chest opening and closing.")]
    [SerializeField] private Animator chestAnimator;
    [Tooltip("Name of the open animation state to play on the animator.")]
    [SerializeField] private string openAnimationName = "chest_open";
    [Tooltip("Name of the closed animation state to play on the animator.")]
    [SerializeField] private string closedAnimationName = "chest_closed";

    [Header("Chest Content")]
    [Tooltip("Items displayed in the chest, shown from left to right then top to bottom.")]
    [SerializeField] private InventoryItemData[] chestItems = new InventoryItemData[27];

    [Header("Audio")]
    [SerializeField] private AudioClip  itemPickupSound;
    [Range(0f, 1f)]
    [SerializeField] private float      itemPickupVolume = 1f;

    [Header("UI")]
    [Tooltip("Number of columns shown in the chest UI.")]
    [Min(1)]
    [SerializeField] private int columns = 9;
    [Tooltip("Number of rows shown in the chest UI.")]
    [Min(1)]
    [SerializeField] private int rows = 3;
    [Tooltip("Grey transparent overlay behind the chest panel.")]
    [SerializeField] private Color overlayColor = new Color(0f, 0f, 0f, 0.45f);
    [Tooltip("Main chest panel color.")]
    [SerializeField] private Color panelColor = new Color(0.16f, 0.16f, 0.16f, 0.92f);
    [Tooltip("Slot background color.")]
    [SerializeField] private Color slotColor = new Color(0f, 0f, 0f, 0.38f);
    [Tooltip("Size of each slot in the grid.")]
    [SerializeField] private Vector2 slotSize = new Vector2(58f, 58f);
    [Tooltip("Spacing between slots.")]
    [SerializeField] private Vector2 slotSpacing = new Vector2(8f, 8f);
    [Tooltip("Inner padding inside the chest panel.")]
    [SerializeField] private Vector2 panelPadding = new Vector2(28f, 28f);
    [Tooltip("Sorting order of the chest UI canvas.")]
    [SerializeField] private int canvasSortingOrder = 700;

    private Canvas uiCanvas;
    private GameObject uiRoot;
    private RectTransform panelRoot;
    private Image[] slotIcons;
    private Image[] slotBackgrounds;
    private int selectedChestIndex = -1;
    private bool        isOpen;
    private Move_Player cachedMovePlayer;
    private AudioSource audioSource;
    public bool IsOpen => isOpen;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (chestAnimator == null)
            chestAnimator = GetComponent<Animator>();

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
                player = playerObject.transform;
        }

        if (player != null)
            cachedMovePlayer = player.GetComponent<Move_Player>();

        EnsureChestContentSize();
        EnsureUIExists();
        // force closed animation initially
        SetChestOpen(false, true);
        if (chestAnimator != null && !string.IsNullOrEmpty(closedAnimationName))
        {
            int closedHash = Animator.StringToHash(closedAnimationName);
            if (chestAnimator.HasState(0, closedHash))
                chestAnimator.Play(closedAnimationName, 0, 0f);
            else
                Debug.LogWarning($"ChestSystem: Animator missing state '{closedAnimationName}' on layer 0. Cannot play closed animation.");
        }
    }

    private void OnValidate()
    {
        columns = Mathf.Max(1, columns);
        rows = Mathf.Max(1, rows);
        EnsureChestContentSize();
    }

    private void Update()
    {
        if (Keyboard.current == null || player == null)
            return;

        bool inRange = IsPlayerInRange();

        if (isOpen && closeWhenPlayerLeaves && !inRange)
        {
            SetChestOpen(false);
            return;
        }

        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (isOpen)
            {
                SetChestOpen(false);
            }
            else if (inRange)
            {
                SetChestOpen(true);
            }
        }

        if (isOpen && Keyboard.current.escapeKey.wasPressedThisFrame)
            SetChestOpen(false);
    }

    private void SetChestOpen(bool open, bool instant = false)
    {
        isOpen = open;

        if (uiRoot != null)
            uiRoot.SetActive(isOpen);

        if (isOpen)
        {
            if (uiRoot != null)
                uiRoot.transform.SetAsLastSibling();

            RefreshUI();
        }

        if (chestAnimator != null)
        {
            if (isOpen && !string.IsNullOrEmpty(openAnimationName))
            {
                int openHash = Animator.StringToHash(openAnimationName);
                if (chestAnimator.HasState(0, openHash))
                    chestAnimator.Play(openAnimationName, 0, 0f);
                else
                    Debug.LogWarning($"ChestSystem: Animator missing state '{openAnimationName}' on layer 0. Cannot play open animation.");
            }
            else if (!isOpen && !string.IsNullOrEmpty(closedAnimationName))
            {
                int closedHash = Animator.StringToHash(closedAnimationName);
                if (chestAnimator.HasState(0, closedHash))
                    chestAnimator.Play(closedAnimationName, 0, 0f);
                else
                    Debug.LogWarning($"ChestSystem: Animator missing state '{closedAnimationName}' on layer 0. Cannot play closed animation.");
            }
        }

        if (lockPlayerMovementWhileOpen && cachedMovePlayer != null)
        {
            // do not disable the component (that destroys the PlayableGraph) — just lock movement
            cachedMovePlayer.SetMovementLocked(isOpen);
        }

        if (instant && uiRoot != null)
            uiRoot.SetActive(isOpen);
    }

    private bool IsPlayerInRange()
    {
        return Vector2.Distance(transform.position, player.position) <= interactionDistance;
    }

    private void RefreshUI()
    {
        if (slotIcons == null)
            return;

        int totalSlots = columns * rows;
        for (int i = 0; i < slotIcons.Length; i++)
        {
            if (i >= chestItems.Length)
            {
                slotIcons[i].sprite = null;
                slotIcons[i].color = new Color(1f, 1f, 1f, 0f);
                continue;
            }

            InventoryItemData itemData = chestItems[i];
            slotIcons[i].sprite = itemData.itemIcon;
            slotIcons[i].color = itemData.itemIcon != null ? Color.white : new Color(1f, 1f, 1f, 0f);
            slotIcons[i].preserveAspect = true;
        }

        // update selection visuals
        for (int i = 0; i < slotBackgrounds.Length; i++)
        {
            if (slotBackgrounds[i] == null)
                continue;

            if (i == selectedChestIndex)
                slotBackgrounds[i].color = Color.Lerp(slotColor, Color.yellow, 0.35f);
            else
                slotBackgrounds[i].color = slotColor;
        }
    }

    public void OnSlotClicked(int index)
    {
        if (index < 0 || index >= chestItems.Length)
            return;

        // if nothing selected yet
        if (selectedChestIndex == -1)
        {
            if (string.IsNullOrWhiteSpace(chestItems[index].itemName) && chestItems[index].itemIcon == null)
            {
                // empty slot clicked -> nothing to select
                return;
            }

            selectedChestIndex = index;
            RefreshUI();
            return;
        }

        // clicked same -> deselect
        if (selectedChestIndex == index)
        {
            selectedChestIndex = -1;
            RefreshUI();
            return;
        }

        // perform move/swap between selectedChestIndex and index
        InventoryItemData src = chestItems[selectedChestIndex];
        InventoryItemData dst = chestItems[index];

        chestItems[index] = src;
        chestItems[selectedChestIndex] = dst;

        selectedChestIndex = -1;
        RefreshUI();
    }

    public void ClearSelection()
    {
        selectedChestIndex = -1;
        RefreshUI();
    }

    public void TransferChestSlotToPlayer(int index)
    {
        if (index < 0 || index >= chestItems.Length)
            return;

        InventoryItemData item = chestItems[index];
        if (string.IsNullOrWhiteSpace(item.itemName) && item.itemIcon == null)
            return; // empty

        if (item.effectType == ItemEffectType.Gold)
        {
            if (PlayerStats.Instance != null)
                PlayerStats.Instance.AddGold(item.effectValue * item.EffectiveQuantity);
            chestItems[index] = default;
            RefreshUI();
            PlayPickupSound();
            return;
        }

        bool added = InventoryManager.Instance != null && InventoryManager.Instance.AddItem(item.itemName, item.itemIcon, item.effectType, item.animationType, item.effectValue, item.EffectiveQuantity, item.EffectiveMaxStack);
        if (added)
        {
            chestItems[index] = default;
            RefreshUI();
            PlayPickupSound();
        }
    }

    public bool TransferPlayerSlotToChest(int playerSlotIndex)
    {
        if (playerSlotIndex < 0)
            return false;

        if (InventoryManager.Instance == null)
            return false;

        if (!InventoryManager.Instance.TryGetItem(playerSlotIndex, out InventoryItemData item))
            return false;

        int emptyIndex = -1;
        for (int i = 0; i < chestItems.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(chestItems[i].itemName) && chestItems[i].itemIcon == null)
            {
                emptyIndex = i;
                break;
            }
        }

        if (emptyIndex < 0)
        {
            return false;
        }

        bool used = InventoryManager.Instance.RemoveSlotAt(playerSlotIndex);
        if (!used)
            return false;

        chestItems[emptyIndex] = item;
        RefreshUI();
        return true;
    }

    private void PlayPickupSound()
    {
        if (itemPickupSound != null && audioSource != null)
            audioSource.PlayOneShot(itemPickupSound, itemPickupVolume);
    }

    private void EnsureChestContentSize()
    {
        int slotCount = columns * rows;
        if (slotCount < 1)
            slotCount = 27;

        if (chestItems == null)
        {
            chestItems = new InventoryItemData[slotCount];
            return;
        }

        if (chestItems.Length != slotCount)
            Array.Resize(ref chestItems, slotCount);
    }

    private void EnsureUIExists()
    {
        if (uiCanvas == null)
        {
            GameObject canvasGo = new GameObject("ChestCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            uiCanvas = canvasGo.GetComponent<Canvas>();
            uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            uiCanvas.sortingOrder = canvasSortingOrder;

            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        if (FindFirstObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

        BuildUI();
    }

    private void BuildUI()
    {
        if (uiRoot != null)
            Destroy(uiRoot);

        uiRoot = new GameObject("ChestUI", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        uiRoot.transform.SetParent(uiCanvas.transform, false);

        Image overlay = uiRoot.GetComponent<Image>();
        overlay.color = overlayColor;
        overlay.raycastTarget = false;

        RectTransform overlayRect = uiRoot.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        GameObject panelGo = new GameObject("ChestPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panelGo.transform.SetParent(uiRoot.transform, false);

        panelRoot = panelGo.GetComponent<RectTransform>();
        Image panelImage = panelGo.GetComponent<Image>();
        panelImage.color = panelColor;
        panelImage.raycastTarget = false;

        float panelWidth = panelPadding.x * 2f + columns * slotSize.x + Mathf.Max(0, columns - 1) * slotSpacing.x;
        float panelHeight = panelPadding.y * 2f + rows * slotSize.y + Mathf.Max(0, rows - 1) * slotSpacing.y;

        panelRoot.anchorMin = new Vector2(0.5f, 0.5f);
        panelRoot.anchorMax = new Vector2(0.5f, 0.5f);
        panelRoot.pivot = new Vector2(0.5f, 0.5f);
        panelRoot.sizeDelta = new Vector2(panelWidth, panelHeight);
        panelRoot.anchoredPosition = Vector2.zero;

        GameObject gridGo = new GameObject("ChestGrid", typeof(RectTransform), typeof(GridLayoutGroup));
        gridGo.transform.SetParent(panelGo.transform, false);

        RectTransform gridRect = gridGo.GetComponent<RectTransform>();
        gridRect.anchorMin = Vector2.zero;
        gridRect.anchorMax = Vector2.one;
        gridRect.offsetMin = panelPadding;
        gridRect.offsetMax = -panelPadding;

        GridLayoutGroup grid = gridGo.GetComponent<GridLayoutGroup>();
        grid.cellSize = slotSize;
        grid.spacing = slotSpacing;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;
        grid.childAlignment = TextAnchor.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;

        slotIcons = new Image[columns * rows];
        slotBackgrounds = new Image[columns * rows];

        for (int i = 0; i < slotIcons.Length; i++)
        {
            GameObject slotGo = new GameObject("Slot_" + (i + 1), typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            slotGo.transform.SetParent(gridGo.transform, false);

            Image slotBg = slotGo.GetComponent<Image>();
            slotBg.color = slotColor;
            slotBackgrounds[i] = slotBg;

            // add a Button so the slot is clickable
            Button btn = slotGo.AddComponent<Button>();
            btn.targetGraphic = slotBg;

            ChestSlot slotComp = slotGo.AddComponent<ChestSlot>();
            slotComp.slotIndex = i;
            slotComp.chestSystem = this;

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
            icon.raycastTarget = false;

            slotIcons[i] = icon;
        }

        uiRoot.SetActive(false);
    }
}