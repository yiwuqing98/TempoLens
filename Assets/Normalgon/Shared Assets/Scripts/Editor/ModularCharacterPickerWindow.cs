using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Normalgon.SharedAssets.EditorScripts
{

public class ModularCharacterPickerWindow : EditorWindow
{
    private List<GameObject> options;
    private ModularCharacter character;
    private ModularCharacterUtilities.RiggedPartType riggedPartType;
    private ModularCharacterUtilities.SocketPartType socketPartType;
    private bool isRiggedPart;
    private Vector2 scrollPosition;

    public static void ShowWindow(List<GameObject> optionsList, ModularCharacter targetCharacter, ModularCharacterUtilities.RiggedPartType rPartType)
    {
        var window = GetWindow<ModularCharacterPickerWindow>("Part Picker");
        window.options = optionsList;
        window.character = targetCharacter;
        window.riggedPartType = rPartType;
        window.isRiggedPart = true;
        window.minSize = new Vector2(200, 300);
        window.Show();
    }

    public static void ShowWindow(List<GameObject> optionsList, ModularCharacter targetCharacter, ModularCharacterUtilities.SocketPartType sPartType)
    {
        var window = GetWindow<ModularCharacterPickerWindow>("Part Picker");
        window.options = optionsList;
        window.character = targetCharacter;
        window.socketPartType = sPartType;
        window.isRiggedPart = false;
        window.minSize = new Vector2(200, 300);
        window.Show();
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Select Part", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (GUILayout.Button("None"))
        {
            if (isRiggedPart)
            {
                ModularCharacterEditor.SetRiggedPartStatic(character, riggedPartType, null);
            }
            else
            {
                ModularCharacterEditor.SetSocketPartStatic(character, socketPartType, null);
            }
            Close();
        }

        EditorGUILayout.Space();
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        foreach (var option in options)
        {
            if (option != null)
            {
                if (GUILayout.Button(option.name))
                {
                    if (isRiggedPart)
                    {
                        ModularCharacterEditor.SetRiggedPartStatic(character, riggedPartType, option);
                    }
                    else
                    {
                        ModularCharacterEditor.SetSocketPartStatic(character, socketPartType, option);
                    }
                    Close();
                }
            }
        }

        EditorGUILayout.EndScrollView();
    }
}

}
