#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class AutoAssignPlayerAnimations
{
    [MenuItem("Tools/Player/Auto Assign All Animations")]
    public static void AutoAssign()
    {
        Move_Player movePlayer = Object.FindFirstObjectByType<Move_Player>();
        if (movePlayer == null)
        {
            Debug.LogError("No Move_Player found in the open scene.");
            return;
        }

        SerializedObject serialized = new SerializedObject(movePlayer);

        Assign(serialized, "idleUp", "idle_up");
        Assign(serialized, "idleDown", "idle_down");
        Assign(serialized, "idleLeft", "idle_left");
        Assign(serialized, "idleRight", "idle_right");

        Assign(serialized, "runUp", "run_up");
        Assign(serialized, "runDown", "run_down");
        Assign(serialized, "runLeft", "run_left");
        Assign(serialized, "runRight", "run_right");

        Assign(serialized, "attack1Up", "attack_1_up");
        Assign(serialized, "attack1Down", "attack_1_down");
        Assign(serialized, "attack1Left", "attack_1_left");
        Assign(serialized, "attack1Right", "attack_1_right");

        Assign(serialized, "blockUp", "block_up");
        Assign(serialized, "blockDown", "block_down");
        Assign(serialized, "blockLeft", "block_left");
        Assign(serialized, "blockRight", "block_right");

        serialized.ApplyModifiedProperties();
        EditorUtility.SetDirty(movePlayer);

        Debug.Log("All clips auto-assigned on " + movePlayer.name);
    }

    private static void Assign(SerializedObject serialized, string propertyName, string clipName)
    {
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property == null)
        {
            Debug.LogWarning("Property not found: " + propertyName);
            return;
        }

        string[] guids = AssetDatabase.FindAssets(clipName + " t:AnimationClip", new[] { "Assets/Player" });
        if (guids.Length == 0)
        {
            Debug.LogWarning("Clip not found: " + clipName);
            return;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        property.objectReferenceValue = clip;
    }
}
#endif
