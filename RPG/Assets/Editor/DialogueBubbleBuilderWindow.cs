#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class DialogueBubbleBuilderWindow : EditorWindow
{
    private RectTransform bubbleRoot;

    private Sprite topLeft;
    private Sprite top;
    private Sprite topRight;
    private Sprite left;
    private Sprite center;
    private Sprite right;
    private Sprite bottomLeft;
    private Sprite bottom;
    private Sprite bottomRight;

    private bool clearExistingChildren = true;

    [MenuItem("Tools/Dialogue/Build 9-Slice Bubble")]
    public static void Open()
    {
        GetWindow<DialogueBubbleBuilderWindow>("Dialogue Bubble Builder");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Target", EditorStyles.boldLabel);
        bubbleRoot = (RectTransform)EditorGUILayout.ObjectField("Bubble Root", bubbleRoot, typeof(RectTransform), true);
        clearExistingChildren = EditorGUILayout.Toggle("Clear Existing Children", clearExistingChildren);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Sprites (9 parts)", EditorStyles.boldLabel);

        topLeft = (Sprite)EditorGUILayout.ObjectField("Top Left", topLeft, typeof(Sprite), false);
        top = (Sprite)EditorGUILayout.ObjectField("Top", top, typeof(Sprite), false);
        topRight = (Sprite)EditorGUILayout.ObjectField("Top Right", topRight, typeof(Sprite), false);
        left = (Sprite)EditorGUILayout.ObjectField("Left", left, typeof(Sprite), false);
        center = (Sprite)EditorGUILayout.ObjectField("Center", center, typeof(Sprite), false);
        right = (Sprite)EditorGUILayout.ObjectField("Right", right, typeof(Sprite), false);
        bottomLeft = (Sprite)EditorGUILayout.ObjectField("Bottom Left", bottomLeft, typeof(Sprite), false);
        bottom = (Sprite)EditorGUILayout.ObjectField("Bottom", bottom, typeof(Sprite), false);
        bottomRight = (Sprite)EditorGUILayout.ObjectField("Bottom Right", bottomRight, typeof(Sprite), false);

        EditorGUILayout.Space(10);

        GUI.enabled = bubbleRoot != null;
        if (GUILayout.Button("Build Bubble", GUILayout.Height(32)))
            Build();
        GUI.enabled = true;
    }

    private void Build()
    {
        if (bubbleRoot == null)
        {
            Debug.LogError("Bubble Root is not assigned.");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(bubbleRoot.gameObject, "Build Dialogue Bubble");

        if (clearExistingChildren)
            ClearChildren(bubbleRoot);

        float leftWidth = GetSpriteWidth(left, topLeft, bottomLeft);
        float rightWidth = GetSpriteWidth(right, topRight, bottomRight);
        float topHeight = GetSpriteHeight(top, topLeft, topRight);
        float bottomHeight = GetSpriteHeight(bottom, bottomLeft, bottomRight);

        CreateCorner("TopLeft", bubbleRoot, topLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), leftWidth, topHeight);
        CreateCorner("TopRight", bubbleRoot, topRight, new Vector2(1f, 1f), new Vector2(1f, 1f), rightWidth, topHeight);
        CreateCorner("BottomLeft", bubbleRoot, bottomLeft, new Vector2(0f, 0f), new Vector2(0f, 0f), leftWidth, bottomHeight);
        CreateCorner("BottomRight", bubbleRoot, bottomRight, new Vector2(1f, 0f), new Vector2(1f, 0f), rightWidth, bottomHeight);

        CreateTop("Top", bubbleRoot, top, leftWidth, rightWidth, topHeight);
        CreateBottom("Bottom", bubbleRoot, bottom, leftWidth, rightWidth, bottomHeight);
        CreateLeft("Left", bubbleRoot, left, leftWidth, topHeight, bottomHeight);
        CreateRight("Right", bubbleRoot, right, rightWidth, topHeight, bottomHeight);
        CreateCenter("Center", bubbleRoot, center, leftWidth, rightWidth, topHeight, bottomHeight);

        EditorUtility.SetDirty(bubbleRoot.gameObject);
        Debug.Log("Dialogue bubble built on " + bubbleRoot.name);
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
            Undo.DestroyObjectImmediate(parent.GetChild(i).gameObject);
    }

    private static float GetSpriteWidth(Sprite primary, Sprite fallbackA, Sprite fallbackB)
    {
        if (primary != null) return SpriteWidthInUI(primary);
        if (fallbackA != null) return SpriteWidthInUI(fallbackA);
        if (fallbackB != null) return SpriteWidthInUI(fallbackB);
        return 16f;
    }

    private static float GetSpriteHeight(Sprite primary, Sprite fallbackA, Sprite fallbackB)
    {
        if (primary != null) return SpriteHeightInUI(primary);
        if (fallbackA != null) return SpriteHeightInUI(fallbackA);
        if (fallbackB != null) return SpriteHeightInUI(fallbackB);
        return 16f;
    }

    private static float SpriteWidthInUI(Sprite sprite)
    {
        return (sprite.rect.width / sprite.pixelsPerUnit) * 100f;
    }

    private static float SpriteHeightInUI(Sprite sprite)
    {
        return (sprite.rect.height / sprite.pixelsPerUnit) * 100f;
    }

    private static Image CreateImage(string name, Transform parent, Sprite sprite)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);

        Image img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.type = Image.Type.Simple;
        img.preserveAspect = false;

        return img;
    }

    private static void CreateCorner(string name, RectTransform parent, Sprite sprite, Vector2 anchor, Vector2 pivot, float width, float height)
    {
        Image img = CreateImage(name, parent, sprite);
        RectTransform rt = img.rectTransform;

        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.sizeDelta = new Vector2(width, height);

        float x = anchor.x == 0f ? 0f : 0f;
        float y = anchor.y == 1f ? 0f : 0f;
        rt.anchoredPosition = new Vector2(x, y);
    }

    private static void CreateTop(string name, RectTransform parent, Sprite sprite, float leftWidth, float rightWidth, float topHeight)
    {
        Image img = CreateImage(name, parent, sprite);
        RectTransform rt = img.rectTransform;

        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.offsetMin = new Vector2(leftWidth, -topHeight);
        rt.offsetMax = new Vector2(-rightWidth, 0f);
    }

    private static void CreateBottom(string name, RectTransform parent, Sprite sprite, float leftWidth, float rightWidth, float bottomHeight)
    {
        Image img = CreateImage(name, parent, sprite);
        RectTransform rt = img.rectTransform;

        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.offsetMin = new Vector2(leftWidth, 0f);
        rt.offsetMax = new Vector2(-rightWidth, bottomHeight);
    }

    private static void CreateLeft(string name, RectTransform parent, Sprite sprite, float leftWidth, float topHeight, float bottomHeight)
    {
        Image img = CreateImage(name, parent, sprite);
        RectTransform rt = img.rectTransform;

        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.offsetMin = new Vector2(0f, bottomHeight);
        rt.offsetMax = new Vector2(leftWidth, -topHeight);
    }

    private static void CreateRight(string name, RectTransform parent, Sprite sprite, float rightWidth, float topHeight, float bottomHeight)
    {
        Image img = CreateImage(name, parent, sprite);
        RectTransform rt = img.rectTransform;

        rt.anchorMin = new Vector2(1f, 0f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 0.5f);
        rt.offsetMin = new Vector2(-rightWidth, bottomHeight);
        rt.offsetMax = new Vector2(0f, -topHeight);
    }

    private static void CreateCenter(string name, RectTransform parent, Sprite sprite, float leftWidth, float rightWidth, float topHeight, float bottomHeight)
    {
        Image img = CreateImage(name, parent, sprite);
        RectTransform rt = img.rectTransform;

        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = new Vector2(leftWidth, bottomHeight);
        rt.offsetMax = new Vector2(-rightWidth, -topHeight);
    }
}
#endif
