using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("Colors")]
    [SerializeField] private Color borderColor = new Color(0.20f, 0.10f, 0.10f, 1.00f);
    [SerializeField] private Color bgColor     = new Color(0.06f, 0.02f, 0.02f, 0.95f);
    [SerializeField] private Color hpColor     = new Color(0.68f, 0.05f, 0.05f, 1.00f);
    [SerializeField] private Color drainColor  = new Color(0.85f, 0.50f, 0.08f, 1.00f);
    [SerializeField] private Color textColor   = new Color(0.90f, 0.85f, 0.80f, 1.00f);

    [Header("Layout")]
    [SerializeField] private Vector2 barSize      = new Vector2(400f, 28f);
    [SerializeField] private Vector2 screenOffset = new Vector2(24f, -24f);
    [SerializeField] private int     sortOrder    = 500;

    [Header("Drain effect")]
    [Tooltip("Seconds before the drain bar starts following the HP bar.")]
    [SerializeField] private float drainDelay = 0.7f;
    [Tooltip("How fast the drain bar catches up (ratio per second).")]
    [SerializeField] private float drainSpeed = 0.4f;

    private PlayerHealth  playerHealth;
    private RectTransform hpFillRect;
    private RectTransform drainFillRect;
    private Text          hpText;

    private float targetRatio = 1f;
    private float drainRatio  = 1f;
    private float drainTimer  = 0f;

    private void Awake()
    {
        BuildUI();
    }

    private void Start()
    {
        playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (playerHealth == null)
        {
            Debug.LogWarning("PlayerHealthUI: PlayerHealth not found.", this);
            return;
        }

        playerHealth.HealthChanged += OnHealthChanged;
        OnHealthChanged(playerHealth.CurrentHealth, playerHealth.MaxHealth);
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.HealthChanged -= OnHealthChanged;
    }

    private void Update()
    {
        if (drainRatio <= targetRatio + 0.001f) return;

        drainTimer -= Time.deltaTime;
        if (drainTimer > 0f) return;

        drainRatio = Mathf.MoveTowards(drainRatio, targetRatio, drainSpeed * Time.deltaTime);
        if (drainFillRect != null)
            drainFillRect.anchorMax = new Vector2(drainRatio, 1f);
    }

    private void OnHealthChanged(int current, int max)
    {
        float ratio = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;

        if (ratio < targetRatio)
        {
            drainTimer = drainDelay;
        }
        else
        {
            drainRatio = ratio;
            if (drainFillRect != null)
                drainFillRect.anchorMax = new Vector2(drainRatio, 1f);
        }

        targetRatio = ratio;
        if (hpFillRect != null) hpFillRect.anchorMax = new Vector2(targetRatio, 1f);
        if (hpText != null) hpText.text = current + " / " + max;
    }

    private void BuildUI()
    {
        GameObject cgo = new GameObject("HealthCanvas",
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = cgo.GetComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortOrder;

        CanvasScaler cs        = cgo.GetComponent<CanvasScaler>();
        cs.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920f, 1080f);
        cs.matchWidthOrHeight  = 0.5f;

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // Root — anchored bottom-left
        GameObject root = new GameObject("PlayerHealthBar", typeof(RectTransform));
        root.transform.SetParent(canvas.transform, false);

        RectTransform rootRect   = root.GetComponent<RectTransform>();
        rootRect.anchorMin        = new Vector2(0f, 1f);
        rootRect.anchorMax        = new Vector2(0f, 1f);
        rootRect.pivot            = new Vector2(0f, 1f);
        rootRect.anchoredPosition = screenOffset;
        float textHeight = 36f;
        rootRect.sizeDelta = new Vector2(barSize.x + 6f, barSize.y + 6f + textHeight);

        // Border panel (bar) — positioned above text
        GameObject border = MakeBox(root.transform, "Border",
            pos: new Vector2(0f, textHeight), size: new Vector2(barSize.x + 6f, barSize.y + 6f),
            color: borderColor, pivot: Vector2.zero);

        // HP text below the bar
        hpText = MakeText(root.transform, "HPText", font,
            pos: Vector2.zero, size: new Vector2(barSize.x + 6f, textHeight));

        // Dark background inset
        GameObject bg = MakeBox(border.transform, "BG", Vector2.zero, Vector2.zero, bgColor, stretch: true, inset: 3f);

        // Drain fill (behind HP fill)
        drainFillRect = MakeFill(bg.transform, "DrainFill", drainColor);

        // HP fill (on top)
        hpFillRect = MakeFill(bg.transform, "HPFill", hpColor);
    }

    private static RectTransform MakeFill(Transform parent, string name, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);

        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;

        Image img  = go.GetComponent<Image>();
        img.color  = color;
        img.type   = Image.Type.Simple;
        return r;
    }

    private static GameObject MakeBox(Transform parent, string name, Vector2 pos, Vector2 size, Color color,
        bool stretch = false, float inset = 0f, Vector2 pivot = default)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);

        RectTransform r = go.GetComponent<RectTransform>();
        if (stretch)
        {
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = new Vector2(inset, inset);
            r.offsetMax = new Vector2(-inset, -inset);
        }
        else
        {
            r.anchorMin        = Vector2.zero;
            r.anchorMax        = Vector2.zero;
            r.pivot            = pivot == default ? Vector2.zero : pivot;
            r.anchoredPosition = pos;
            r.sizeDelta        = size;
        }

        go.GetComponent<Image>().color = color;
        return go;
    }

    private static Text MakeText(Transform parent, string name, Font font, Vector2 pos, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);

        RectTransform r   = go.GetComponent<RectTransform>();
        r.anchorMin        = Vector2.zero;
        r.anchorMax        = Vector2.zero;
        r.pivot            = Vector2.zero;
        r.anchoredPosition = pos;
        r.sizeDelta        = size;

        Text t          = go.GetComponent<Text>();
        t.font          = font;
        t.fontSize      = 26;
        t.fontStyle     = FontStyle.Bold;
        t.color         = new Color(0.90f, 0.85f, 0.80f, 1f);
        t.alignment     = TextAnchor.MiddleLeft;
        t.raycastTarget = false;
        return t;
    }
}
