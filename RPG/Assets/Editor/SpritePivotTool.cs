using UnityEngine;
using UnityEditor;

public static class SpritePivotTool
{
    [MenuItem("Tools/Set Selected Spritesheets Pivot → Bottom Center")]
    private static void SetBottomCenter()
    {
        int changed = 0;

        foreach (Object obj in Selection.objects)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;

            SpriteMetaData[] sprites = importer.spritesheet;
            for (int i = 0; i < sprites.Length; i++)
            {
                sprites[i].alignment = (int)SpriteAlignment.BottomCenter;
                sprites[i].pivot     = new Vector2(0.5f, 0f);
            }

            importer.spritesheet = sprites;
            importer.SaveAndReimport();
            changed++;
        }

        Debug.Log($"SpritePivotTool: updated {changed} spritesheet(s) to Bottom Center.");
    }

    [MenuItem("Tools/Set Selected Spritesheets Pivot → Bottom Center", validate = true)]
    private static bool Validate()
    {
        foreach (Object obj in Selection.objects)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (AssetImporter.GetAtPath(path) is TextureImporter)
                return true;
        }
        return false;
    }
}
