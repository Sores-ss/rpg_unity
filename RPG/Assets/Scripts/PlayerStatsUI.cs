using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerStatsUI : MonoBehaviour
{
    [Header("Colors")]
    [SerializeField] private Color panelColor      = new Color(0.10f, 0.10f, 0.10f, 0.95f);
    [SerializeField] private Color titleColor      = new Color(1.00f, 0.85f, 0.20f, 1.00f);
    [SerializeField] private Color statColor       = new Color(0.90f, 0.90f, 0.90f, 1.00f);
    [SerializeField] private Color goldColor       = new Color(1.00f, 0.85f, 0.20f, 1.00f);
    [SerializeField] private Color previewColor    = new Color(0.55f, 0.90f, 0.55f, 1.00f);
    [SerializeField] private Color btnCanAfford    = new Color(0.20f, 0.65f, 0.20f, 1.00f);
    [SerializeField] private Color btnCantAfford   = new Color(0.30f, 0.30f, 0.30f, 1.00f);
    [SerializeField] private int   canvasSortOrder = 600;

    private PlayerStats stats;
    private GameObject  panelRoot;
    private Text        levelText;
    private Text        goldText;
    private Text        hpText;
    private Text        atkText;
    private Text        spdText;
    private Text        costText;
    private Image       btnImage;
    private bool        isOpen;

    private void Awake()
    {
        BuildUI();
        SetOpen(false);
    }

    private void Start()
    {
        stats = FindFirstObjectByType<PlayerStats>();

        if (stats == null)
        {
            Debug.LogWarning("PlayerStatsUI: PlayerStats component not found. Add it to the player.", this);
            return;
        }

        stats.LevelChanged += OnStatsChanged;
        stats.GoldChanged  += OnStatsChanged;
    }

    private void OnDestroy()
    {
        if (stats == null) return;
        stats.LevelChanged -= OnStatsChanged;
        stats.GoldChanged  -= OnStatsChanged;
    }

    private void OnStatsChanged(int _) => Refresh();

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
            SetOpen(!isOpen);
    }

    private void SetOpen(bool open)
    {
        isOpen = open;
        if (panelRoot != null)
            panelRoot.SetActive(isOpen);
        if (isOpen)
            Refresh();
    }

    private void Refresh()
    {
        if (stats == null) return;

        levelText.text = $"Niveau    {stats.Level}";
        goldText.text  = $"Or        {stats.Gold} / {stats.GoldForNextLevel}";
        hpText.text    = $"HP        {stats.MaxHp}  ->  {stats.NextMaxHp}";
        atkText.text   = $"ATK       {stats.Attack}  ->  {stats.NextAttack}";
        spdText.text   = $"SPD       {stats.Speed:F1}  ->  {stats.NextSpeed:F1}";
        costText.text  = $"Cout niveau suivant : {stats.GoldForNextLevel} or";

        btnImage.color = stats.Gold >= stats.GoldForNextLevel ? btnCanAfford : btnCantAfford;
    }

    private void BuildUI()
    {
        GameObject canvasGo = new GameObject("StatsCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
        Canvas canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = canvasSortOrder;

        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // Panel
        panelRoot = new GameObject("StatsPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panelRoot.transform.SetParent(canvas.transform, false);

        panelRoot.GetComponent<Image>().color = panelColor;

        RectTransform panelRect = panelRoot.GetComponent<RectTransform>();
        panelRect.anchorMin        = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax        = new Vector2(0.5f, 0.5f);
        panelRect.pivot            = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta        = new Vector2(650f, 720f);
        panelRect.anchoredPosition = Vector2.zero;

        // Content with vertical layout
        GameObject content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup));
        content.transform.SetParent(panelRoot.transform, false);

        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = Vector2.zero;
        contentRect.anchorMax = Vector2.one;
        contentRect.offsetMin = new Vector2(36f, 36f);
        contentRect.offsetMax = new Vector2(-36f, -36f);

        VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
        layout.spacing             = 18f;
        layout.childAlignment      = TextAnchor.UpperLeft;
        layout.childControlWidth   = true;
        layout.childControlHeight  = false;
        layout.childForceExpandWidth  = true;
        layout.childForceExpandHeight = false;

        Transform c = content.transform;

        MakeText(c, "STATS",  38, titleColor,   font, FontStyle.Bold);
        MakeSeparator(c, font);
        levelText = MakeText(c, "", 28, statColor,  font, FontStyle.Normal);
        goldText  = MakeText(c, "", 28, goldColor,  font, FontStyle.Normal);
        MakeSeparator(c, font);
        MakeText(c, "Apres level up :", 22, previewColor, font, FontStyle.Italic);
        hpText    = MakeText(c, "", 28, statColor,  font, FontStyle.Normal);
        atkText   = MakeText(c, "", 28, statColor,  font, FontStyle.Normal);
        spdText   = MakeText(c, "", 28, statColor,  font, FontStyle.Normal);
        MakeSeparator(c, font);
        costText  = MakeText(c, "", 26, goldColor,  font, FontStyle.Normal);

        // Level up button
        GameObject btnGo = new GameObject("LevelUpBtn", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnGo.transform.SetParent(c, false);
        btnGo.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 72f);

        btnImage = btnGo.GetComponent<Image>();
        Button btn = btnGo.GetComponent<Button>();
        btn.targetGraphic = btnImage;
        btn.onClick.AddListener(() => stats?.TryLevelUp());

        GameObject btnLabel = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        btnLabel.transform.SetParent(btnGo.transform, false);

        RectTransform lblRect = btnLabel.GetComponent<RectTransform>();
        lblRect.anchorMin = Vector2.zero;
        lblRect.anchorMax = Vector2.one;
        lblRect.offsetMin = Vector2.zero;
        lblRect.offsetMax = Vector2.zero;

        Text lblText = btnLabel.GetComponent<Text>();
        lblText.text      = "LEVEL UP";
        lblText.font      = font;
        lblText.fontSize  = 28;
        lblText.fontStyle = FontStyle.Bold;
        lblText.alignment = TextAnchor.MiddleCenter;
        lblText.color     = Color.white;
    }

    private static Text MakeText(Transform parent, string content, int size, Color color, Font font, FontStyle style)
    {
        GameObject go = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);
        go.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, size + 10f);

        Text t = go.GetComponent<Text>();
        t.text        = content;
        t.font        = font;
        t.fontSize    = size;
        t.fontStyle   = style;
        t.color       = color;
        t.raycastTarget = false;
        return t;
    }

    private static void MakeSeparator(Transform parent, Font font)
    {
        MakeText(parent, "________________________", 9, new Color(1f, 1f, 1f, 0.15f), font, FontStyle.Normal);
    }
}
