using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string gameSceneName = "SampleScene";
    [SerializeField] private string gameTitle     = "RPG";

    [Header("Audio")]
    [SerializeField] private AudioClip bgMusic;
    [Range(0f, 1f)]
    [SerializeField] private float     bgMusicVolume = 1f;

    [Header("Background")]
    [Tooltip("Optional background sprite. If assigned, used instead of bgColor.")]
    [SerializeField] private Sprite backgroundSprite;

    [Header("Colors")]
    [SerializeField] private Color bgColor      = new Color(0.05f, 0.05f, 0.08f, 1f);
    [SerializeField] private Color titleColor   = new Color(0.90f, 0.75f, 0.30f, 1f);
    [SerializeField] private Color btnColor     = new Color(0.20f, 0.15f, 0.10f, 1f);
    [SerializeField] private Color btnHoverColor= new Color(0.35f, 0.25f, 0.15f, 1f);
    [SerializeField] private Color btnTextColor = new Color(0.95f, 0.90f, 0.75f, 1f);

    private void Awake()
    {
        BuildUI();
    }

    private void BuildUI()
    {
        // EventSystem (required for button clicks)
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        // Canvas
        GameObject cgo = new GameObject("MainMenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = cgo.GetComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        CanvasScaler cs = cgo.GetComponent<CanvasScaler>();
        cs.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920f, 1080f);
        cs.matchWidthOrHeight  = 0.5f;

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // Fond
        GameObject bg = MakeImage(canvas.transform, "BG", bgColor, stretch: true);
        if (backgroundSprite != null)
        {
            Image bgImage = bg.GetComponent<Image>();
            bgImage.sprite = backgroundSprite;
            bgImage.color  = Color.white;
            bgImage.type   = Image.Type.Simple;
            bgImage.preserveAspect = false;
        }

        // Titre
        GameObject titleGo = new GameObject("Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        titleGo.transform.SetParent(canvas.transform, false);
        RectTransform titleRect = titleGo.GetComponent<RectTransform>();
        titleRect.anchorMin        = new Vector2(0.5f, 0.65f);
        titleRect.anchorMax        = new Vector2(0.5f, 0.65f);
        titleRect.pivot            = new Vector2(0.5f, 0.5f);
        titleRect.sizeDelta        = new Vector2(800f, 160f);
        titleRect.anchoredPosition = Vector2.zero;
        Text titleText   = titleGo.GetComponent<Text>();
        titleText.font      = font;
        titleText.text      = gameTitle;
        titleText.fontSize  = 120;
        titleText.fontStyle = FontStyle.Bold;
        titleText.color     = titleColor;
        titleText.alignment = TextAnchor.MiddleCenter;

        // Bouton Play
        MakeButton(canvas.transform, "PlayBtn", "JOUER", font,
            new Vector2(0.5f, 0.45f), new Vector2(320f, 70f),
            btnColor, btnHoverColor, btnTextColor,
            () => SceneManager.LoadScene(gameSceneName));

        // Musique
        if (bgMusic != null)
        {
            AudioSource audio = cgo.AddComponent<AudioSource>();
            audio.clip   = bgMusic;
            audio.loop   = true;
            audio.volume = bgMusicVolume;
            audio.playOnAwake = false;
            audio.Play();
        }

        // Bouton Quitter
        MakeButton(canvas.transform, "QuitBtn", "QUITTER", font,
            new Vector2(0.5f, 0.34f), new Vector2(320f, 70f),
            btnColor, btnHoverColor, btnTextColor,
            () => Application.Quit());
    }

    private static GameObject MakeImage(Transform parent, string name, Color color, bool stretch = false)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform r = go.GetComponent<RectTransform>();
        if (stretch)
        {
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = r.offsetMax = Vector2.zero;
        }
        go.GetComponent<Image>().color = color;
        return go;
    }

    private static void MakeButton(Transform parent, string name, string label, Font font,
        Vector2 anchor, Vector2 size, Color bg, Color hover, Color textCol,
        UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);

        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin        = anchor;
        r.anchorMax        = anchor;
        r.pivot            = new Vector2(0.5f, 0.5f);
        r.sizeDelta        = size;
        r.anchoredPosition = Vector2.zero;

        Image img = go.GetComponent<Image>();
        img.color = bg;

        Button btn = go.GetComponent<Button>();
        btn.onClick.AddListener(onClick);
        ColorBlock cb    = btn.colors;
        cb.normalColor   = bg;
        cb.highlightedColor = hover;
        cb.pressedColor  = Color.white;
        btn.colors       = cb;
        btn.targetGraphic = img;

        GameObject textGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textGo.transform.SetParent(go.transform, false);
        RectTransform tr = textGo.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
        tr.offsetMin = tr.offsetMax = Vector2.zero;
        Text t      = textGo.GetComponent<Text>();
        t.font      = font;
        t.text      = label;
        t.fontSize  = 36;
        t.fontStyle = FontStyle.Bold;
        t.color     = textCol;
        t.alignment = TextAnchor.MiddleCenter;
        t.raycastTarget = false;
    }
}
