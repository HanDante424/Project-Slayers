using UnityEngine;
using UnityEditor;
using UnityEditor.U2D.Sprites;

public class SpritePivotBatchTool : EditorWindow
{
    private Texture2D targetTexture;

    private PivotPreset pivotPreset = PivotPreset.BottomCenter;
    private Vector2 customPivot = new Vector2(0.5f, 0f);

    private enum PivotPreset
    {
        Center,
        BottomCenter,
        Custom
    }

    [MenuItem("Tools/Slayers/Sprite Pivot Batch Tool")]
    private static void OpenWindow()
    {
        GetWindow<SpritePivotBatchTool>("Sprite Pivot Batch Tool");
    }

    private void OnSelectionChange()
    {
        if (Selection.activeObject is Texture2D texture)
        {
            targetTexture = texture;
            Repaint();
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Batch Change Sprite Pivots", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        targetTexture = (Texture2D)EditorGUILayout.ObjectField(
            "Target Texture",
            targetTexture,
            typeof(Texture2D),
            false
        );

        pivotPreset = (PivotPreset)EditorGUILayout.EnumPopup("Pivot Preset", pivotPreset);

        if (pivotPreset == PivotPreset.Custom)
        {
            customPivot = EditorGUILayout.Vector2Field("Custom Pivot", customPivot);
            customPivot.x = Mathf.Clamp01(customPivot.x);
            customPivot.y = Mathf.Clamp01(customPivot.y);
        }

        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "This tool changes the pivot of all sprites inside the selected sprite sheet.\n" +
            "Recommended for characters: BottomCenter",
            MessageType.Info
        );

        EditorGUILayout.Space();

        GUI.enabled = targetTexture != null;

        if (GUILayout.Button("Apply Pivot To All Sprites", GUILayout.Height(30)))
        {
            ApplyPivotToAllSprites();
        }

        GUI.enabled = true;
    }

    private void ApplyPivotToAllSprites()
    {
        if (targetTexture == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign a target texture.", "OK");
            return;
        }

        string assetPath = AssetDatabase.GetAssetPath(targetTexture);
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

        if (importer == null)
        {
            EditorUtility.DisplayDialog("Error", "Could not find TextureImporter.", "OK");
            return;
        }

        if (importer.textureType != TextureImporterType.Sprite)
        {
            EditorUtility.DisplayDialog("Error", "Selected texture is not imported as Sprite.", "OK");
            return;
        }

        if (importer.spriteImportMode != SpriteImportMode.Multiple)
        {
            bool continueAnyway = EditorUtility.DisplayDialog(
                "Warning",
                "The selected texture is not set to Multiple sprite mode.\n\nContinue anyway?",
                "Continue",
                "Cancel"
            );

            if (!continueAnyway)
                return;
        }

        SpriteDataProviderFactories factory = new SpriteDataProviderFactories();
        factory.Init();

        ISpriteEditorDataProvider dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);

        if (dataProvider == null)
        {
            EditorUtility.DisplayDialog("Error", "Could not get sprite data provider.", "OK");
            return;
        }

        dataProvider.InitSpriteEditorDataProvider();

        SpriteRect[] spriteRects = dataProvider.GetSpriteRects();

        if (spriteRects == null || spriteRects.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "No sprite rects found in this texture.", "OK");
            return;
        }

        for (int i = 0; i < spriteRects.Length; i++)
        {
            switch (pivotPreset)
            {
                case PivotPreset.Center:
                    spriteRects[i].alignment = SpriteAlignment.Center;
                    spriteRects[i].pivot = new Vector2(0.5f, 0.5f);
                    break;

                case PivotPreset.BottomCenter:
                    spriteRects[i].alignment = SpriteAlignment.BottomCenter;
                    spriteRects[i].pivot = new Vector2(0.5f, 0f);
                    break;

                case PivotPreset.Custom:
                    spriteRects[i].alignment = SpriteAlignment.Custom;
                    spriteRects[i].pivot = customPivot;
                    break;
            }
        }

        dataProvider.SetSpriteRects(spriteRects);
        dataProvider.Apply();

        importer.SaveAndReimport();

        EditorUtility.DisplayDialog(
            "Completed",
            $"Updated pivot for {spriteRects.Length} sprites.",
            "OK"
        );
    }
}