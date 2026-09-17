using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("PlayerHealth source. If empty, auto-found in scene.")]
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [Tooltip("Pause the game time when game over is shown.")]
    [SerializeField] private bool pauseGameOnDeath = true;

    [Header("Audio")]
    [SerializeField] private AudioClip deathSound;
    [Range(0f, 1f)]
    [SerializeField] private float     deathSoundVolume = 1f;

    [Header("Colors")]
    [SerializeField] private Color overlayColor   = new Color(0f,    0f,    0f,    0.85f);
    [SerializeField] private Color titleColor     = new Color(0.85f, 0.15f, 0.10f, 1f);
    [SerializeField] private Color btnColor       = new Color(0.20f, 0.15f, 0.10f, 1f);
    [SerializeField] private Color btnHoverColor  = new Color(0.35f, 0.25f, 0.15f, 1f);
    [SerializeField] private Color btnTextColor   = new Color(0.95f, 0.90f, 0.75f, 1f);

    private bool      isGameOver;
    private Canvas    gameOverCanvas;

    private void Awake()
    {
        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();

        BuildUI();
    }

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.Died += OnPlayerDied;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.Died -= OnPlayerDied;
    }

    private void Update()
    {
        if (!isGameOver || Keyboard.current == null)
            return;

        if (Keyboard.current.rKey.wasPressedThisFrame)
            RestartScene();
    }

    private void BuildUI()
    {
        GameObject cgo = new GameObject("GameOverCanvas",
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        gameOverCanvas = cgo.GetComponent<Canvas>();
        gameOverCanvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        gameOverCanvas.sortingOrder = 1000;
        cgo.SetActive(false);

        CanvasScaler cs = cgo.GetComponent<CanvasScaler>();
        cs.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920f, 1080f);
        cs.matchWidthOrHeight  = 0.5f;

        if (FindFirstObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // Dark overlay
        MakeImage(gameOverCanvas.transform, "Overlay", overlayColor, stretch: true);

        // "GAME OVER" title
        GameObject titleGo = new GameObject("Title",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        titleGo.transform.SetParent(gameOverCanvas.transform, false);
        RectTransform titleRect = titleGo.GetComponent<RectTransform>();
        titleRect.anchorMin        = new Vector2(0.5f, 0.62f);
        titleRect.anchorMax        = new Vector2(0.5f, 0.62f);
        titleRect.pivot            = new Vector2(0.5f, 0.5f);
        titleRect.sizeDelta        = new Vector2(900f, 180f);
        titleRect.anchoredPosition = Vector2.zero;
        Text titleText   = titleGo.GetComponent<Text>();
        titleText.font      = font;
        titleText.text      = "GAME OVER";
        titleText.fontSize  = 120;
        titleText.fontStyle = FontStyle.Bold;
        titleText.color     = titleColor;
        titleText.alignment = TextAnchor.MiddleCenter;

        // Restart hint
        GameObject hintGo = new GameObject("Hint",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        hintGo.transform.SetParent(gameOverCanvas.transform, false);
        RectTransform hintRect = hintGo.GetComponent<RectTransform>();
        hintRect.anchorMin        = new Vector2(0.5f, 0.50f);
        hintRect.anchorMax        = new Vector2(0.5f, 0.50f);
        hintRect.pivot            = new Vector2(0.5f, 0.5f);
        hintRect.sizeDelta        = new Vector2(700f, 50f);
        hintRect.anchoredPosition = Vector2.zero;
        Text hintText   = hintGo.GetComponent<Text>();
        hintText.font      = font;
        hintText.text      = "Appuyez sur R pour recommencer";
        hintText.fontSize  = 28;
        hintText.fontStyle = FontStyle.Italic;
        hintText.color     = new Color(0.75f, 0.70f, 0.60f, 1f);
        hintText.alignment = TextAnchor.MiddleCenter;

        // "REJOUER" button
        MakeButton(gameOverCanvas.transform, "RestartBtn", "REJOUER", font,
            new Vector2(0.5f, 0.40f), new Vector2(320f, 70f),
            btnColor, btnHoverColor, btnTextColor,
            RestartScene);

        // "MENU PRINCIPAL" button
        MakeButton(gameOverCanvas.transform, "MenuBtn", "MENU PRINCIPAL", font,
            new Vector2(0.5f, 0.29f), new Vector2(320f, 70f),
            btnColor, btnHoverColor, btnTextColor,
            GoToMainMenu);
    }

    private void OnPlayerDied()
    {
        isGameOver = true;

        if (deathSound != null)
            AudioSource.PlayClipAtPoint(deathSound, Camera.main != null ? Camera.main.transform.position : Vector3.zero, deathSoundVolume);

        if (gameOverCanvas != null)
            gameOverCanvas.gameObject.SetActive(true);

        if (pauseGameOnDeath)
            Time.timeScale = 0f;
    }

    private void RestartScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private static GameObject MakeImage(Transform parent, string name, Color color, bool stretch = false)
    {
        GameObject go = new GameObject(name,
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
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
        GameObject go = new GameObject(name,
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
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
        ColorBlock cb       = btn.colors;
        cb.normalColor      = bg;
        cb.highlightedColor = hover;
        cb.pressedColor     = Color.white;
        btn.colors          = cb;
        btn.targetGraphic   = img;

        GameObject textGo = new GameObject("Label",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
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
