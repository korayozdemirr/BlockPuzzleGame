#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TMPro;

public class CreateFontAsset
{
    [MenuItem("Tools/Create LilitaOne Font Asset")]
    public static void GenerateFontAsset()
    {
        Font font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/LilitaOne-Regular.ttf");
        if (font == null)
        {
            Debug.LogError("Font not found at Assets/Fonts/LilitaOne-Regular.ttf");
            return;
        }

        TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(font);
        if (fontAsset != null)
        {
            if (!System.IO.Directory.Exists("Assets/Resources/Fonts"))
            {
                System.IO.Directory.CreateDirectory("Assets/Resources/Fonts");
            }

            string path = "Assets/Resources/Fonts/LilitaOne-Regular SDF.asset";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(fontAsset, path);

            if (fontAsset.atlasTexture != null)
            {
                fontAsset.atlasTexture.name = fontAsset.name + " Atlas";
                AssetDatabase.AddObjectToAsset(fontAsset.atlasTexture, fontAsset);
            }
            if (fontAsset.material != null)
            {
                fontAsset.material.name = fontAsset.name + " Material";
                AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("TMP Font Asset created successfully at Assets/Resources/Fonts/LilitaOne-Regular SDF.asset!");
        }
    }
}
#endif
