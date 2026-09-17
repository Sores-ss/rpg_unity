#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class AutoAssignEnemyAnimations
{
    [MenuItem("Tools/Enemy/Auto Assign Enemy Animations")]
    public static void AutoAssign()
    {
        EnemyAnimation2D enemyAnim = Object.FindFirstObjectByType<EnemyAnimation2D>();
        if (enemyAnim == null)
        {
            Debug.LogError("No EnemyAnimation2D found in the open scene.");
            return;
        }

        SerializedObject serialized = new SerializedObject(enemyAnim);

        Assign(serialized, "idleClip", "Idle", "Assets/Sprites/Tiny Swords (Free Pack)/Units");
        Assign(serialized, "runClip", "Run", "Assets/Sprites/Tiny Swords (Free Pack)/Units");
        Assign(serialized, "attackClip", "Attack", "Assets/Sprites/Tiny Swords (Free Pack)/Units");

        serialized.ApplyModifiedProperties();
        EditorUtility.SetDirty(enemyAnim);

        Debug.Log("Enemy clips auto-assigned on " + enemyAnim.name + ". Check attack clip if none exists in assets.");
    }

    private static void Assign(SerializedObject serialized, string propertyName, string clipHint, string searchPath)
    {
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property == null)
            return;

        string[] guids = AssetDatabase.FindAssets(clipHint + " t:AnimationClip", new[] { searchPath });
        if (guids.Length == 0)
            return;

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        property.objectReferenceValue = clip;
    }
}
#endif
