using UnityEngine;
using UnityEditor;

namespace Normalgon.SharedAssets.EditorScripts
{

public class UV2ColorMapperGUI : ShaderGUI
{
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        Material target = materialEditor.target as Material;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Top 4 Rows - Horizontal Gradients", EditorStyles.boldLabel);
        DrawGradients(materialEditor, target);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Bottom 6 Rows - Color Grid", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Click any color to edit. Alpha > 0.5 enables metallic/smoothness.", EditorStyles.helpBox);
        DrawColorGrid(materialEditor, target);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Surface Properties", EditorStyles.boldLabel);

        // Draw surface properties
        MaterialProperty normalMap = FindProperty("_NormalMap", properties);
        MaterialProperty smoothness = FindProperty("_Smoothness", properties);
        MaterialProperty metallic = FindProperty("_Metallic", properties);

        materialEditor.TexturePropertySingleLine(new GUIContent("Normal Map (UV1)"), normalMap);
        materialEditor.RangeProperty(smoothness, "Smoothness (when color alpha > 0.5)");
        materialEditor.RangeProperty(metallic, "Metallic (when color alpha > 0.5)");

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);

        MaterialProperty gammaCorrect = FindProperty("_GammaCorrectVertexColors", properties);
        materialEditor.ShaderProperty(gammaCorrect, "Gamma Correct Vertex Colors");
        
        MaterialProperty doubleSided = FindProperty("_DoubleSided", properties);
        materialEditor.ShaderProperty(doubleSided, "Double Sided");
        
        MaterialProperty cullMode = FindProperty("_Cull", properties);
        materialEditor.ShaderProperty(cullMode, "Cull Mode");
    }
    
    private void DrawGradients(MaterialEditor materialEditor, Material material)
    {
        const float spacing = 4f;

        // Draw 4 gradient rows (from top to bottom: Row 9, Row 8, Row 7, Row 6)
        string[] gradientRows = { "Row9", "Row8", "Row7", "Hair/Beard" };
        string[] gradientPropertyNames = { "Row9", "Row8", "Row7", "Row6" };
        
        for (int i = 0; i < gradientRows.Length; i++)
        {
            string displayName = gradientRows[i];
            string propName = gradientPropertyNames[i];
            string startProp = $"_Gradient{propName}_Start";
            string endProp = $"_Gradient{propName}_End";

            GUILayout.BeginHorizontal();

            Color startColor = material.GetColor(startProp);
            Color endColor = material.GetColor(endProp);

            EditorGUILayout.LabelField($"{displayName}:", GUILayout.Width(80));

            Color newStart = EditorGUILayout.ColorField(GUIContent.none, startColor, true, true, false, GUILayout.Width(60));
            if (newStart != startColor)
            {
                Undo.RecordObject(material, $"Change {displayName} Start Color");
                material.SetColor(startProp, newStart);
                EditorUtility.SetDirty(material);
            }

            EditorGUILayout.LabelField("→", GUILayout.Width(20));

            Color newEnd = EditorGUILayout.ColorField(GUIContent.none, endColor, true, true, false, GUILayout.Width(60));
            if (newEnd != endColor)
            {
                Undo.RecordObject(material, $"Change {displayName} End Color");
                material.SetColor(endProp, newEnd);
                EditorUtility.SetDirty(material);
            }

            GUILayout.EndHorizontal();

            if (i < gradientRows.Length - 1)
                GUILayout.Space(spacing);
        }
    }

    private void DrawColorGrid(MaterialEditor materialEditor, Material material)
    {
        const int cols = 10;
        const int rows = 6;
        const float cellSize = 24f;
        const float spacing = 2f;

        GUILayout.BeginVertical();

        // Draw column labels
        GUILayout.BeginHorizontal();
        
        string[] columnLabels = { "Face", "Top", "Top", "Bottom", "Shoes", "Gloves", "Hats", "Knee Pads", "Shoulder Pads", "Misc" };
        for (int col = 0; col < cols; col++)
        {
            Rect labelRect = GUILayoutUtility.GetRect(cellSize, 16f);
            GUI.Label(labelRect, columnLabels[col], EditorStyles.miniLabel);
            
            if (col < cols - 1)
                GUILayout.Space(spacing);
        }
        GUILayout.EndHorizontal();
        
        GUILayout.Space(2f);

        // Draw from top to bottom (Row 1, then Row 0)
        for (int row = rows - 1; row >= 0; row--)
        {
            GUILayout.BeginHorizontal();

            for (int col = 0; col < cols; col++)
            {
                string propertyName = $"_Color{row}_{col}";

                Color currentColor = material.GetColor(propertyName);
                Rect colorRect = GUILayoutUtility.GetRect(cellSize, cellSize);
                Color newColor = EditorGUI.ColorField(colorRect, GUIContent.none, currentColor, true, true, false);

                if (newColor != currentColor)
                {
                    Undo.RecordObject(material, "Change Color Grid");
                    material.SetColor(propertyName, newColor);
                    EditorUtility.SetDirty(material);
                }

                if (col < cols - 1)
                    GUILayout.Space(spacing);
            }

            GUILayout.EndHorizontal();

            if (row > 0)
                GUILayout.Space(spacing);
        }

        GUILayout.EndVertical();
        
        // Add utility buttons
        EditorGUILayout.Space();
        GUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Reset All Colors"))
        {
            ResetAllColors(material);
        }
        
        if (GUILayout.Button("Unique Colors"))
        {
            SetUniqueColors(material);
        }
        
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Set All Alpha = 1"))
        {
            SetAllAlpha(material, 1f);
        }
        
        if (GUILayout.Button("Set All Alpha = 0"))
        {
            SetAllAlpha(material, 0f);
        }
        
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Sample Vertex Colors"))
        {
            SampleVertexColors(material);
        }
        
        GUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "UV2 Mapping Guide:\n" +
            "• Bottom 6 rows (0.0-0.6 Y): 6x10 color grid\n" +
            "• Top 4 rows (0.6-1.0 Y): Horizontal gradients\n" +
            "• Alpha > 0.5 = Metallic/Smooth enabled\n" +
            "• Pure Black (0,0,0) = Use vertex colors instead",
            MessageType.Info);
    }
    
    private void ResetAllColors(Material material)
    {
        Undo.RecordObject(material, "Reset All Colors");

        // Reset grid colors to black (which triggers vertex color fallback)
        Color blackColor = new Color(0f, 0f, 0f, 0f);

        for (int row = 0; row < 6; row++)
        {
            for (int col = 0; col < 10; col++)
            {
                string propertyName = $"_Color{row}_{col}";
                material.SetColor(propertyName, blackColor);
            }
        }

        // Reset gradients to black
        material.SetColor("_GradientRow6_Start", blackColor);
        material.SetColor("_GradientRow6_End", blackColor);
        material.SetColor("_GradientRow7_Start", blackColor);
        material.SetColor("_GradientRow7_End", blackColor);
        material.SetColor("_GradientRow8_Start", blackColor);
        material.SetColor("_GradientRow8_End", blackColor);
        material.SetColor("_GradientRow9_Start", blackColor);
        material.SetColor("_GradientRow9_End", blackColor);

        EditorUtility.SetDirty(material);
    }
    
    private void SetUniqueColors(Material material)
    {
        Undo.RecordObject(material, "Set Unique Colors");

        int index = 0;

        // Set unique colors for grid
        for (int row = 0; row < 6; row++)
        {
            for (int col = 0; col < 10; col++)
            {
                string propertyName = $"_Color{row}_{col}";

                // Generate HSV color with good distribution
                float hue = (index * 360f / 64f) / 360f;  // Distribute across 64 total colors (60 grid + 4 gradient)
                float saturation = 0.8f;
                float brightness = 0.8f;

                Color uniqueColor = Color.HSVToRGB(hue, saturation, brightness);
                uniqueColor.a = 1.0f;

                material.SetColor(propertyName, uniqueColor);
                index++;
            }
        }

        // Set unique colors for gradients
        string[] gradientProps = {
            "_GradientRow6_Start", "_GradientRow6_End",
            "_GradientRow7_Start", "_GradientRow7_End",
            "_GradientRow8_Start", "_GradientRow8_End",
            "_GradientRow9_Start", "_GradientRow9_End"
        };

        foreach (string prop in gradientProps)
        {
            float hue = (index * 360f / 64f) / 360f;
            Color uniqueColor = Color.HSVToRGB(hue, 0.8f, 0.8f);
            uniqueColor.a = 1.0f;
            material.SetColor(prop, uniqueColor);
            index++;
        }

        EditorUtility.SetDirty(material);
    }
    
    private void SetAllAlpha(Material material, float alpha)
    {
        Undo.RecordObject(material, $"Set All Alpha to {alpha}");

        // Set alpha for grid colors
        for (int row = 0; row < 6; row++)
        {
            for (int col = 0; col < 10; col++)
            {
                string propertyName = $"_Color{row}_{col}";
                Color color = material.GetColor(propertyName);
                color.a = alpha;
                material.SetColor(propertyName, color);
            }
        }

        // Set alpha for gradient colors
        string[] gradientProps = {
            "_GradientRow6_Start", "_GradientRow6_End",
            "_GradientRow7_Start", "_GradientRow7_End",
            "_GradientRow8_Start", "_GradientRow8_End",
            "_GradientRow9_Start", "_GradientRow9_End"
        };

        foreach (string prop in gradientProps)
        {
            Color color = material.GetColor(prop);
            color.a = alpha;
            material.SetColor(prop, color);
        }

        EditorUtility.SetDirty(material);
    }
    
    private void SampleVertexColors(Material material)
    {
        // Find all renderers using this material
        Renderer[] renderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        MeshFilter meshToSample = null;
        
        foreach (Renderer renderer in renderers)
        {
            if (renderer.sharedMaterial == material)
            {
                MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    meshToSample = meshFilter;
                    break;
                }
            }
        }
        
        if (meshToSample == null)
        {
            EditorUtility.DisplayDialog("No Mesh Found", 
                "Could not find a mesh using this material with vertex colors.", "OK");
            return;
        }
        
        Mesh mesh = meshToSample.sharedMesh;
        Vector2[] uv2 = mesh.uv2;
        Color[] colors = mesh.colors;
        
        if (uv2 == null || uv2.Length == 0)
        {
            EditorUtility.DisplayDialog("No UV2 Data", 
                "The selected mesh does not have UV2 coordinates.", "OK");
            return;
        }
        
        if (colors == null || colors.Length == 0)
        {
            EditorUtility.DisplayDialog("No Vertex Colors", 
                "The selected mesh does not have vertex colors.", "OK");
            return;
        }
        
        Undo.RecordObject(material, "Sample Vertex Colors");
        
        // Sample colors from different UV2 regions
        Color[,] gridSamples = new Color[6, 10]; // 6x10 grid for bottom rows
        int[,] gridCounts = new int[6, 10];
        
        // Initialize arrays
        for (int row = 0; row < 6; row++)
        {
            for (int col = 0; col < 10; col++)
            {
                gridSamples[row, col] = Color.black;
                gridCounts[row, col] = 0;
            }
        }
        
        // Sample vertex colors based on UV2 coordinates
        for (int i = 0; i < Mathf.Min(uv2.Length, colors.Length); i++)
        {
            Vector2 uv = uv2[i];
            Color vertexColor = colors[i];
            
            // Map to grid coordinates (bottom 6 rows only)
            int row = Mathf.FloorToInt(uv.y * 10.0f);
            int col = Mathf.FloorToInt(uv.x * 10.0f);
            
            // Only process bottom 6 rows (0-5)
            if (row >= 0 && row < 6 && col >= 0 && col < 10)
            {
                gridSamples[row, col] += vertexColor;
                gridCounts[row, col]++;
            }
        }
        
        // Average the colors and apply to material
        bool foundColors = false;
        for (int row = 0; row < 6; row++)
        {
            for (int col = 0; col < 10; col++)
            {
                if (gridCounts[row, col] > 0)
                {
                    Color avgColor = gridSamples[row, col] / gridCounts[row, col];
                    avgColor.a = 1.0f; // Set alpha to 1 for metallic/smoothness
                    
                    string propertyName = $"_Color{row}_{col}";
                    material.SetColor(propertyName, avgColor);
                    foundColors = true;
                }
            }
        }
        
        if (foundColors)
        {
            EditorUtility.SetDirty(material);
            EditorUtility.DisplayDialog("Success", 
                "Vertex colors have been sampled and applied to the color grid!", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("No Colors Sampled", 
                "No vertex colors were found in the UV2 regions corresponding to the color grid.", "OK");
        }
    }
}

}