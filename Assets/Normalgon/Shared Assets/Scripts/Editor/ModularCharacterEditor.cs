using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;

namespace Normalgon.SharedAssets.EditorScripts
{

/// <summary>
/// Editor-only script for ModularCharacter. Handles all instantiation, UI, and setup logic.
/// </summary>
[CustomEditor(typeof(ModularCharacter))]
public class ModularCharacterEditor : Editor
{
    // Debug mode toggle
    private static bool debugLogsEnabled = false;
    
    // Store references to ObjectFields for updating when values change programmatically
    private Dictionary<ModularCharacterUtilities.RiggedPartType, ObjectField> riggedPartFields = new Dictionary<ModularCharacterUtilities.RiggedPartType, ObjectField>();
    private Dictionary<ModularCharacterUtilities.SocketPartType, ObjectField> socketPartFields = new Dictionary<ModularCharacterUtilities.SocketPartType, ObjectField>();
    
    // Static references to active editor instances for UI updates
    private static Dictionary<ModularCharacter, ModularCharacterEditor> activeEditors = new Dictionary<ModularCharacter, ModularCharacterEditor>();


    // Define which rigged parts affect which sockets
    private static readonly Dictionary<ModularCharacterUtilities.RiggedPartType, string[]> socketDependencies = new Dictionary<ModularCharacterUtilities.RiggedPartType, string[]>
    {
        { ModularCharacterUtilities.RiggedPartType.Head, new[] { "Socket_Beard", "Socket_Mustache" } },
        { ModularCharacterUtilities.RiggedPartType.HeadCovering, new[] { "Socket_Hat" } },
        { ModularCharacterUtilities.RiggedPartType.HandLeft, new[] { "Socket_ForearmLeft" } },
        { ModularCharacterUtilities.RiggedPartType.HandRight, new[] { "Socket_ForearmRight" } },
        { ModularCharacterUtilities.RiggedPartType.Torso, new[] { "Socket_ShoulderLeft", "Socket_ShoulderRight", "Socket_BackGear", "Socket_HeadAppendageLeft", "Socket_HeadAppendageRight" } },
        { ModularCharacterUtilities.RiggedPartType.UpperLegs, new[] { "Socket_ThighLeft", "Socket_ThighRight" } },
        { ModularCharacterUtilities.RiggedPartType.FootLeft, new[] { "Socket_ShinLeft" } },
        { ModularCharacterUtilities.RiggedPartType.FootRight, new[] { "Socket_ShinRight" } }
    };

    // Define fallback parts for socket restoration (which part to restore from when primary is removed)
    private static readonly Dictionary<string, ModularCharacterUtilities.RiggedPartType> socketFallbacks = new Dictionary<string, ModularCharacterUtilities.RiggedPartType>
    {
        { "Socket_Hat", ModularCharacterUtilities.RiggedPartType.Head },
        { "Socket_ForearmLeft", ModularCharacterUtilities.RiggedPartType.Torso },
        { "Socket_ForearmRight", ModularCharacterUtilities.RiggedPartType.Torso },
        { "Socket_ShinLeft", ModularCharacterUtilities.RiggedPartType.UpperLegs },
        { "Socket_ShinRight", ModularCharacterUtilities.RiggedPartType.UpperLegs }
        // ShoulderLeft/Right and ThighLeft/Right fallback to base rig (no specific part)
    };

    public override VisualElement CreateInspectorGUI()
    {
        ModularCharacter character = (ModularCharacter)target;
        
        // Register this editor instance
        activeEditors[character] = this;
        
        // Load the UXML
        var uxml = Resources.Load<VisualTreeAsset>("UI");
        if (uxml == null)
        {
            var fallback = new Label("UI.uxml file not found in Resources!");
            return fallback;
        }
        
        var root = uxml.Instantiate();
        
        // Bind the object fields
        BindObjectField(root, "override-material", character, "overrideMaterial", typeof(Material));
        BindObjectField(root, "animated-rig", character, "animatedRig", typeof(Transform));
        BindObjectField(root, "base-body", character, "baseBody", typeof(GameObject));
        
        // Bind blend shape sliders
        BindSliderField(root, "blendshape-face", character, "blendShapeFace");
        BindSliderField(root, "blendshape-hips", character, "blendShapeHips");
        BindSliderField(root, "blendshape-waist", character, "blendShapeWaist");
        BindSliderField(root, "blendshape-bust", character, "blendShapeBust");
        BindSliderField(root, "blendshape-feet", character, "blendShapeFeet");

        // Live apply toggle: when enabled, apply blendshape values as sliders are dragged
        var liveToggle = root.Q<Toggle>("toggle-live-blendshape-apply");
        var faceSlider = root.Q<Slider>("blendshape-face");
        var hipsSlider = root.Q<Slider>("blendshape-hips");
        var waistSlider = root.Q<Slider>("blendshape-waist");
        var bustSlider = root.Q<Slider>("blendshape-bust");
        var feetSlider = root.Q<Slider>("blendshape-feet");

        if (faceSlider != null)
        {
            faceSlider.RegisterValueChangedCallback(evt =>
            {
                if (liveToggle != null && liveToggle.value)
                {
                    ApplyBlendShapeToAllChildren(character, "Face", evt.newValue);
                    EditorUtility.SetDirty(character);
                }
            });
        }

        if (hipsSlider != null)
        {
            hipsSlider.RegisterValueChangedCallback(evt =>
            {
                if (liveToggle != null && liveToggle.value)
                {
                    ApplyBlendShapeToAllChildren(character, "Hips", evt.newValue);
                    EditorUtility.SetDirty(character);
                }
            });
        }

        if (waistSlider != null)
        {
            waistSlider.RegisterValueChangedCallback(evt =>
            {
                if (liveToggle != null && liveToggle.value)
                {
                    ApplyBlendShapeToAllChildren(character, "Waist", evt.newValue);
                    EditorUtility.SetDirty(character);
                }
            });
        }

        if (bustSlider != null)
        {
            bustSlider.RegisterValueChangedCallback(evt =>
            {
                if (liveToggle != null && liveToggle.value)
                {
                    ApplyBlendShapeToAllChildren(character, "Bust", evt.newValue);
                    EditorUtility.SetDirty(character);
                }
            });
        }

        if (feetSlider != null)
        {
            feetSlider.RegisterValueChangedCallback(evt =>
            {
                if (liveToggle != null && liveToggle.value)
                {
                    ApplyBlendShapeToAllChildren(character, "Feet", evt.newValue);
                    EditorUtility.SetDirty(character);
                }
            });
        }
        
        // Combined "All Parts" slider: controls Face/Hips/Waist/Bust/Feet together
        var allBlendSlider = root.Q<Slider>("blendshape-allparts");
        if (allBlendSlider != null)
        {
            allBlendSlider.RegisterValueChangedCallback(evt =>
            {
                float v = evt.newValue;

                // Update serialized properties so values persist and individual sliders stay in sync
                var propFace = serializedObject.FindProperty("blendShapeFace");
                var propH = serializedObject.FindProperty("blendShapeHips");
                var propW = serializedObject.FindProperty("blendShapeWaist");
                var propB = serializedObject.FindProperty("blendShapeBust");
                var propF = serializedObject.FindProperty("blendShapeFeet");
                if (propFace != null) propFace.floatValue = v;
                if (propH != null) propH.floatValue = v;
                if (propW != null) propW.floatValue = v;
                if (propB != null) propB.floatValue = v;
                if (propF != null) propF.floatValue = v;
                serializedObject.ApplyModifiedProperties();

                // Update individual sliders without triggering their callbacks
                if (faceSlider != null) faceSlider.SetValueWithoutNotify(v);
                if (hipsSlider != null) hipsSlider.SetValueWithoutNotify(v);
                if (waistSlider != null) waistSlider.SetValueWithoutNotify(v);
                if (bustSlider != null) bustSlider.SetValueWithoutNotify(v);
                if (feetSlider != null) feetSlider.SetValueWithoutNotify(v);

                // If live-apply is enabled, apply immediately to all children
                if (liveToggle != null && liveToggle.value)
                {
                    ApplyBlendShapeToAllChildren(character, "Face", v);
                    ApplyBlendShapeToAllChildren(character, "Hips", v);
                    ApplyBlendShapeToAllChildren(character, "Waist", v);
                    ApplyBlendShapeToAllChildren(character, "Bust", v);
                    ApplyBlendShapeToAllChildren(character, "Feet", v);
                }

                EditorUtility.SetDirty(character);
            });
        }
        
        // Bind rigged part object fields
        foreach (var mapping in ModularCharacterUtilities.RiggedPartFieldMappings)
        {
            BindRiggedPartObjectField(root, mapping.Key, character, mapping.Value);
        }
        
        // Bind socket part object fields
        foreach (var mapping in ModularCharacterUtilities.SocketPartFieldMappings)
        {
            BindSocketPartObjectField(root, mapping.Key, character, mapping.Value);
        }
        
        // Bind button events
        BindButtons(root, character);
        
        // Bind troubleshooting mode toggle
        BindTroubleshootingMode(root);
        
        // Populate foldouts
        PopulateFoldouts(root, character);
        
        return root;
    }
    
    private void OnDestroy()
    {
        // Unregister this editor instance
        ModularCharacter character = (ModularCharacter)target;
        if (character != null && activeEditors.ContainsKey(character))
        {
            activeEditors.Remove(character);
        }
    }

    private void BindTroubleshootingMode(VisualElement root)
    {
        var debugToggle = root.Q<Toggle>("toggle-debug-logs");
        if (debugToggle != null)
        {
            debugToggle.value = debugLogsEnabled;
            debugToggle.RegisterValueChangedCallback(evt =>
            {
                debugLogsEnabled = evt.newValue;
            });
        }
    }

    private void BindObjectField(VisualElement root, string fieldName, ModularCharacter character, string propertyName, System.Type objectType)
    {
        var field = root.Q<ObjectField>(fieldName);
        if (field != null)
        {
            field.objectType = objectType;
            field.BindProperty(serializedObject.FindProperty(propertyName));
        }
    }
    
    private void BindSliderField(VisualElement root, string fieldName, ModularCharacter character, string propertyName)
    {
        var slider = root.Q<Slider>(fieldName);
        if (slider != null)
        {
            slider.BindProperty(serializedObject.FindProperty(propertyName));
        }
    }
    
    private void BindRiggedPartObjectField(VisualElement root, string fieldName, ModularCharacter character, ModularCharacterUtilities.RiggedPartType partType)
    {
        var field = root.Q<ObjectField>(fieldName);
        if (field != null)
        {
            field.objectType = typeof(GameObject);
            field.value = GetCurrentRiggedPart(character, partType);
            
            // Store reference for later updates
            riggedPartFields[partType] = field;
            
            field.RegisterValueChangedCallback(evt =>
            {
                SetRiggedPart(character, partType, evt.newValue as GameObject);
                RefreshObjectFieldValues(character);
            });
        }
    }
    
    private void BindSocketPartObjectField(VisualElement root, string fieldName, ModularCharacter character, ModularCharacterUtilities.SocketPartType partType)
    {
        var field = root.Q<ObjectField>(fieldName);
        if (field != null)
        {
            field.objectType = typeof(GameObject);
            field.value = GetCurrentSocketPart(character, partType);
            
            // Store reference for later updates
            socketPartFields[partType] = field;
            
            field.RegisterValueChangedCallback(evt =>
            {
                SetSocketPart(character, partType, evt.newValue as GameObject);
                RefreshObjectFieldValues(character);
            });
        }
    }
    
    private void BindButtons(VisualElement root, ModularCharacter character)
    {
        // Apply Material to All button (separate)
        var applyMaterialButton = root.Q<Button>("button-apply-material-to-all") ?? root.Q<Button>("button-apply-material");
        if (applyMaterialButton != null)
        {
            applyMaterialButton.clicked += () => {
                ApplyMaterialToAllParts(character);
            };
        }
        else
        {
            Debug.LogWarning("[ModularCharacterEditor] button-apply-material-to-all not found in UI!");
        }

        // Apply Blendshapes to All button (separate)
        var applyBlendShapesButton = root.Q<Button>("button-apply-blendshapes-to-all") ?? root.Q<Button>("button-apply-blendshapes");
        if (applyBlendShapesButton != null)
        {
            applyBlendShapesButton.clicked += () => {
                ApplyBlendShapesToAllParts(character);
            };
        }
        else
        {
            Debug.LogWarning("[ModularCharacterEditor] button-apply-blendshapes-to-all not found in UI!");
        }
        
        // Sync All Sockets button
        var syncSocketsButton = root.Q<Button>("button-sync-all-sockets");
        if (syncSocketsButton != null)
        {
            syncSocketsButton.clicked += () => SyncAllSockets(character);
        }
        
        // Rigged part buttons
        foreach (var mapping in ModularCharacterUtilities.RiggedPartFieldMappings)
        {
            BindRiggedPartButtons(root, character, mapping.Key, mapping.Value);
        }
        
        // Socket part buttons
        foreach (var mapping in ModularCharacterUtilities.SocketPartFieldMappings)
        {
            BindSocketPartButtons(root, character, mapping.Key, mapping.Value);
        }
    }
    
    private void BindRiggedPartButtons(VisualElement root, ModularCharacter character, string partName, ModularCharacterUtilities.RiggedPartType partType)
    {
        var pickButton = root.Q<Button>($"{partName}-pick-button");
        if (pickButton != null)
        {
            pickButton.clicked += () =>
            {
                var options = GetRiggedPartOptions(character, partType);
                ShowRiggedPartPicker(partType, options, character);
            };
        }
        
        var removeButton = root.Q<Button>($"{partName}-remove-button");
        if (removeButton != null)
        {
            removeButton.clicked += () => 
            {
                SetRiggedPart(character, partType, null);
                RefreshObjectFieldValues(character);
            };
        }
    }
    
    private void BindSocketPartButtons(VisualElement root, ModularCharacter character, string partName, ModularCharacterUtilities.SocketPartType partType)
    {
        var pickButton = root.Q<Button>($"{partName}-pick-button");
        if (pickButton != null)
        {
            pickButton.clicked += () =>
            {
                var options = GetSocketPartOptions(character, partType);
                ShowSocketPartPicker(partType, options, character);
            };
        }
        
        var removeButton = root.Q<Button>($"{partName}-remove-button");
        if (removeButton != null)
        {
            removeButton.clicked += () => 
            {
                SetSocketPart(character, partType, null);
                RefreshObjectFieldValues(character);
            };
        }
    }
    
    private void RefreshObjectFieldValues(ModularCharacter character)
    {
        // Update rigged part fields
        foreach (var kvp in riggedPartFields)
        {
            if (kvp.Value != null)
            {
                kvp.Value.SetValueWithoutNotify(GetCurrentRiggedPart(character, kvp.Key));
            }
        }
        
        // Update socket part fields
        foreach (var kvp in socketPartFields)
        {
            if (kvp.Value != null)
            {
                kvp.Value.SetValueWithoutNotify(GetCurrentSocketPart(character, kvp.Key));
            }
        }
    }
    
    private void PopulateFoldouts(VisualElement root, ModularCharacter character)
    {
        var rigFoldout = root.Q<Foldout>("foldout-frame-rig");
        if (rigFoldout != null)
        {
            PopulateRigFoldout(rigFoldout, character);
        }
        
        var socketFoldout = root.Q<Foldout>("foldout-frame-socket");
        if (socketFoldout != null)
        {
            PopulateSocketFoldout(socketFoldout, character);
        }
    }
    
    private void PopulateRigFoldout(Foldout foldout, ModularCharacter character)
    {
        foldout.Clear();
        
        AddListPropertyField(foldout, "torsoOptions", "Torso Options");
        AddListPropertyField(foldout, "upperLegsOptions", "Upper Legs Options");
        AddListPropertyField(foldout, "headOptions", "Head Options");
        AddListPropertyField(foldout, "handLeftOptions", "Hand Left Options");
        AddListPropertyField(foldout, "handRightOptions", "Hand Right Options");
        AddListPropertyField(foldout, "footLeftOptions", "Foot Left Options");
        AddListPropertyField(foldout, "footRightOptions", "Foot Right Options");
        AddListPropertyField(foldout, "headCoveringOptions", "Head Covering Options");
    }
    
    private void PopulateSocketFoldout(Foldout foldout, ModularCharacter character)
    {
        foldout.Clear();

        AddListPropertyField(foldout, "hatOptions", "Hat Options");
        AddListPropertyField(foldout, "mustacheOptions", "Mustache Options");
        AddListPropertyField(foldout, "beardOptions", "Beard Options");
        AddListPropertyField(foldout, "forearmLeftOptions", "Forearm Left Options");
        AddListPropertyField(foldout, "forearmRightOptions", "Forearm Right Options");
        AddListPropertyField(foldout, "shoulderLeftOptions", "Shoulder Left Options");
        AddListPropertyField(foldout, "shoulderRightOptions", "Shoulder Right Options");
        AddListPropertyField(foldout, "shinLeftOptions", "Shin Left Options");
        AddListPropertyField(foldout, "shinRightOptions", "Shin Right Options");
        AddListPropertyField(foldout, "thighLeftOptions", "Thigh Left Options");
        AddListPropertyField(foldout, "thighRightOptions", "Thigh Right Options");
        AddListPropertyField(foldout, "backGearOptions", "Back Gear Options");
        AddListPropertyField(foldout, "headAppendageLeftOptions", "Head Appendage Left Options");
        AddListPropertyField(foldout, "headAppendageRightOptions", "Head Appendage Right Options");
    }
    
    private void AddListPropertyField(VisualElement parent, string propertyName, string displayName)
    {
        var property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            var propertyField = new PropertyField(property, displayName);
            parent.Add(propertyField);
        }
    }
    
    private List<GameObject> GetRiggedPartOptions(ModularCharacter character, ModularCharacterUtilities.RiggedPartType partType)
    {
        return ModularCharacterUtilities.RiggedPartOptionsGetters.TryGetValue(partType, out var getter) ? getter(character) : new List<GameObject>();
    }
    
    private List<GameObject> GetSocketPartOptions(ModularCharacter character, ModularCharacterUtilities.SocketPartType partType)
    {
        return ModularCharacterUtilities.SocketPartOptionsGetters.TryGetValue(partType, out var getter) ? getter(character) : new List<GameObject>();
    }


    private void SetRiggedPart(ModularCharacter character, ModularCharacterUtilities.RiggedPartType partType, GameObject prefab)
    {
        
        // Get current instance and remove it
        GameObject currentInstance = GetCurrentRiggedPart(character, partType);
        if (currentInstance != null)
        {
            DestroyImmediate(currentInstance);
            SetCurrentRiggedPart(character, partType, null); // Explicitly clear reference
        }

        // Create new instance if prefab provided
        GameObject newInstance = null;
        if (prefab != null)
        {
            if (Application.isPlaying)
            {
                // At runtime, use regular Instantiate and remap bones
                newInstance = Instantiate(prefab, character.transform);
                
                // Remap bones for animation at runtime
                if (character != null)
                {
                    character.RemapBonesForPart(newInstance);
                }
            }
            else
            {
                // In editor, use PrefabUtility
                newInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, character.transform);
            }
            
            // Sync sockets from this part
            SyncSocketsFromPart(character, newInstance);
            
            // Apply override material if set
            ApplyOverrideMaterial(character, newInstance);
            
            // Apply blend shapes to the new part
            ApplyBlendShapesToPart(character, newInstance);
        }

        // Update the character's reference
        SetCurrentRiggedPart(character, partType, newInstance);

        // Sync or restore sockets affected by this part type
        if (newInstance != null)
        {
            SyncSocketsForRiggedPart(character, partType, newInstance);
        }
        else
        {
            RestoreSocketsForRiggedPart(character, partType);
        }

        // Mark dirty for serialization
        EditorUtility.SetDirty(character);
    }

    private void SetSocketPart(ModularCharacter character, ModularCharacterUtilities.SocketPartType partType, GameObject prefab)
    {
        if (debugLogsEnabled) Debug.Log($"[ModularCharacterEditor] SetSocketPart - PartType: {partType}, Prefab: {(prefab != null ? prefab.name : "null")}");
        
        // Get current instance and remove it
        GameObject currentInstance = GetCurrentSocketPart(character, partType);
        if (currentInstance != null)
        {
            if (debugLogsEnabled) Debug.Log($"[ModularCharacterEditor]   Removing existing instance: {currentInstance.name}");
            DestroyImmediate(currentInstance);
            SetCurrentSocketPart(character, partType, null); // Explicitly clear reference
        }

        // Create new instance if prefab provided
        GameObject newInstance = null;
        if (prefab != null && character.animatedRig != null)
        {
            string socketName = GetSocketNameForPartType(partType);
            if (debugLogsEnabled) Debug.Log($"[ModularCharacterEditor]   Looking for socket: {socketName} in rig: {character.animatedRig.name}");
            Transform socket = ModularCharacterUtilities.FindSocketInHierarchy(character.animatedRig, socketName);
            
            if (socket != null)
            {
                if (debugLogsEnabled) Debug.Log($"[ModularCharacterEditor]   ✓ Socket found: {socket.name} at position {socket.position} (local: {socket.localPosition})");
                if (Application.isPlaying)
                {
                    // At runtime, use regular Instantiate
                    newInstance = Instantiate(prefab, socket);
                    
                    // Check if this socket part has SkinnedMeshRenderers that need bone remapping
                    if (newInstance.GetComponentInChildren<SkinnedMeshRenderer>() != null)
                    {
                        character.RemapBonesForPart(newInstance);
                    }
                }
                else
                {
                    // In editor, use PrefabUtility
                    newInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, socket);
                }
                
                // Apply override material and blend shapes if set
                ApplyOverrideMaterial(character, newInstance);
                ApplyBlendShapesToPart(character, newInstance);
            }
            else
            {
                if (debugLogsEnabled) Debug.LogWarning($"[ModularCharacterEditor]   ✗ Socket {socketName} not found for {partType} in rig: {character.animatedRig.name}");
            }
        }
        else
        {
            if (debugLogsEnabled) Debug.Log($"[ModularCharacterEditor]   Skipping - prefab or animatedRig is null");
        }

        // Update the character's reference
        SetCurrentSocketPart(character, partType, newInstance);
        
        // Mark dirty for serialization
        EditorUtility.SetDirty(character);
    }

    private GameObject GetCurrentRiggedPart(ModularCharacter character, ModularCharacterUtilities.RiggedPartType partType)
    {
        return ModularCharacterUtilities.RiggedPartGetters.TryGetValue(partType, out var getter) ? getter(character) : null;
    }

    private void SetCurrentRiggedPart(ModularCharacter character, ModularCharacterUtilities.RiggedPartType partType, GameObject instance)
    {
        if (ModularCharacterUtilities.RiggedPartSetters.TryGetValue(partType, out var setter))
        {
            setter(character, instance);
        }
    }

    private GameObject GetCurrentSocketPart(ModularCharacter character, ModularCharacterUtilities.SocketPartType partType)
    {
        return ModularCharacterUtilities.SocketPartGetters.TryGetValue(partType, out var getter) ? getter(character) : null;
    }

    private void SetCurrentSocketPart(ModularCharacter character, ModularCharacterUtilities.SocketPartType partType, GameObject instance)
    {
        if (ModularCharacterUtilities.SocketPartSetters.TryGetValue(partType, out var setter))
        {
            setter(character, instance);
        }
    }

    private string GetSocketNameForPartType(ModularCharacterUtilities.SocketPartType partType)
    {
        return ModularCharacterUtilities.GetSocketNameForPartType(partType);
    }

    private void ShowRiggedPartPicker(ModularCharacterUtilities.RiggedPartType partType, List<GameObject> options, ModularCharacter character)
    {
        if (options.Count == 0)
        {
            EditorUtility.DisplayDialog("No Options", $"No options available for {partType}.", "OK");
            return;
        }
        
        ModularCharacterPickerWindow.ShowWindow(options, character, partType);
    }
    
    // Static method for the picker window to use
    public static void SetRiggedPartStatic(ModularCharacter character, ModularCharacterUtilities.RiggedPartType partType, GameObject prefab)
    {
        // Try to find existing editor instance first
        if (activeEditors.TryGetValue(character, out ModularCharacterEditor existingEditor))
        {
            existingEditor.SetRiggedPart(character, partType, prefab);
            
            // Ensure serialization is complete before UI refresh
            existingEditor.serializedObject.Update();
            existingEditor.RefreshObjectFieldValues(character);
            
            // Delayed refresh as fallback for edge cases where Unity serialization hasn't completed
            EditorApplication.delayCall += () => 
            {
                if (character != null && existingEditor != null)
                {
                    existingEditor.serializedObject.Update();
                    existingEditor.RefreshObjectFieldValues(character);
                }
            };
            return;
        }
        
        // Fallback to creating temporary editor
        var editor = CreateEditor(character) as ModularCharacterEditor;
        if (editor != null)
        {
            editor.SetRiggedPart(character, partType, prefab);
            DestroyImmediate(editor);
        }
    }

    private void ShowSocketPartPicker(ModularCharacterUtilities.SocketPartType partType, List<GameObject> options, ModularCharacter character)
    {
        if (options.Count == 0)
        {
            EditorUtility.DisplayDialog("No Options", $"No options available for {partType}.", "OK");
            return;
        }
        
        ModularCharacterPickerWindow.ShowWindow(options, character, partType);
    }
    
    // Static method for the picker window to use
    public static void SetSocketPartStatic(ModularCharacter character, ModularCharacterUtilities.SocketPartType partType, GameObject prefab)
    {
        // Try to find existing editor instance first
        if (activeEditors.TryGetValue(character, out ModularCharacterEditor existingEditor))
        {
            existingEditor.SetSocketPart(character, partType, prefab);
            
            // Ensure serialization is complete before UI refresh
            existingEditor.serializedObject.Update();
            existingEditor.RefreshObjectFieldValues(character);
            
            // Delayed refresh as fallback for edge cases where Unity serialization hasn't completed
            EditorApplication.delayCall += () => 
            {
                if (character != null && existingEditor != null)
                {
                    existingEditor.serializedObject.Update();
                    existingEditor.RefreshObjectFieldValues(character);
                }
            };
            return;
        }
        
        // Fallback to creating temporary editor
        var editor = CreateEditor(character) as ModularCharacterEditor;
        if (editor != null)
        {
            editor.SetSocketPart(character, partType, prefab);
            DestroyImmediate(editor);
        }
    }

    private void SyncAllSockets(ModularCharacter character)
    {
        
        if (character.animatedRig == null)
        {
            Debug.LogWarning("[ModularCharacterEditor] Cannot sync sockets - animatedRig is null");
            return;
        }

        // Sync sockets from all currently equipped rigged parts
        GameObject[] riggedParts = {
            character.currentTorso, character.currentUpperLegs, character.currentHead, character.currentHandLeft, character.currentHandRight,
            character.currentFootLeft, character.currentFootRight, character.currentHeadCovering
        };
        
        foreach (GameObject part in riggedParts)
        {
            if (part != null)
            {
                SyncSocketsFromPart(character, part);
            }
        }
    }

    private void SyncSocketsFromPart(ModularCharacter character, GameObject part)
    {
        if (part == null || character.animatedRig == null) return;


        // Find all sockets in the part hierarchy
        Transform[] allTransforms = part.GetComponentsInChildren<Transform>(true);
        
        int socketsFound = 0;
        int socketsSynced = 0;
        
        foreach (Transform partSocket in allTransforms)
        {
            if (partSocket.name.StartsWith("Socket_"))
            {
                socketsFound++;
                
                // Find matching socket in the rig
                Transform rigSocket = ModularCharacterUtilities.FindSocketInHierarchy(character.animatedRig, partSocket.name);
                if (rigSocket != null)
                {
                    ModularCharacterUtilities.CopyTransformValues(partSocket, rigSocket);
                   socketsSynced++;
                }
                else
                {
                    Debug.LogWarning($"[ModularCharacterEditor]   ✗ No matching socket found in rig for: {partSocket.name}");
                }
            }
        }
        
    }

    private void SyncSocketsForRiggedPart(ModularCharacter character, ModularCharacterUtilities.RiggedPartType partType, GameObject partInstance)
    {
        if (partInstance == null || character.animatedRig == null) return;

        if (debugLogsEnabled) Debug.Log($"[ModularCharacterEditor] SyncSocketsForRiggedPart - PartType: {partType}, Part: {partInstance.name}");

        // Check if this part type affects any sockets
        if (!socketDependencies.TryGetValue(partType, out string[] affectedSockets))
        {
            if (debugLogsEnabled) Debug.Log($"[ModularCharacterEditor] No socket dependencies defined for {partType}");
            return;
        }

        if (debugLogsEnabled) Debug.Log($"[ModularCharacterEditor] Part type {partType} affects sockets: {string.Join(", ", affectedSockets)}");

        foreach (string socketName in affectedSockets)
        {
            if (debugLogsEnabled) Debug.Log($"[ModularCharacterEditor]   Searching for socket: {socketName}");
            Transform partSocket = ModularCharacterUtilities.FindSocketInHierarchy(partInstance.transform, socketName);
            Transform rigSocket = ModularCharacterUtilities.FindSocketInHierarchy(character.animatedRig, socketName);

            if (partSocket != null && rigSocket != null)
            {
                if (debugLogsEnabled) Debug.Log($"[ModularCharacterEditor]   ✓ Found both sockets - Part: {partSocket.name} at {partSocket.position}, Rig: {rigSocket.name} at {rigSocket.position}");
                ModularCharacterUtilities.CopyTransformValues(partSocket, rigSocket);
                if (debugLogsEnabled) Debug.Log($"[ModularCharacterEditor]   ✓ Synced! Rig socket now at: {rigSocket.position}");
            }
            else if (partSocket == null)
            {
                if (debugLogsEnabled) Debug.LogWarning($"[ModularCharacterEditor]   ✗ Socket {socketName} not found in {partType} part ({partInstance.name})");
            }
            else if (rigSocket == null)
            {
                if (debugLogsEnabled) Debug.LogWarning($"[ModularCharacterEditor]   ✗ Socket {socketName} not found in rig ({character.animatedRig.name})");
            }
        }
    }

    /// <summary>
    /// Restores sockets to their fallback positions when a rigged part is removed
    /// </summary>
    private void RestoreSocketsForRiggedPart(ModularCharacter character, ModularCharacterUtilities.RiggedPartType partType)
    {
        if (character.animatedRig == null) return;

        // Check if this part type affects any sockets
        if (!socketDependencies.TryGetValue(partType, out string[] affectedSockets))
            return;


        foreach (string socketName in affectedSockets)
        {
            // Try to restore from fallback part
            if (socketFallbacks.TryGetValue(socketName, out ModularCharacterUtilities.RiggedPartType fallbackPartType))
            {
                GameObject fallbackPart = GetCurrentRiggedPart(character, fallbackPartType);
                if (fallbackPart != null)
                {
                    Transform fallbackSocket = ModularCharacterUtilities.FindSocketInHierarchy(fallbackPart.transform, socketName);
                    Transform rigSocket = ModularCharacterUtilities.FindSocketInHierarchy(character.animatedRig, socketName);

                    if (fallbackSocket != null && rigSocket != null)
                    {
                        ModularCharacterUtilities.CopyTransformValues(fallbackSocket, rigSocket);
                    }
                }
                else
                {
                    Debug.LogWarning($"[ModularCharacterEditor] Fallback part {fallbackPartType} not found for {socketName}");
                }
            }
            else
            {
            }
        }
    }

    /// <summary>
    /// Applies the override material to all renderers in the instantiated part
    /// </summary>
    private void ApplyOverrideMaterial(ModularCharacter character, GameObject partInstance)
    {
        if (character.overrideMaterial == null || partInstance == null) return;

        // Step 1: Find and apply to all descendants
        ApplyMaterialToHierarchy(character, partInstance.transform);
    }

    private void ApplyMaterialToHierarchy(ModularCharacter character, Transform node)
    {
        // Apply material to this node's renderers
        Renderer renderer = node.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material[] materials = new Material[renderer.sharedMaterials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = character.overrideMaterial;
            }
            renderer.sharedMaterials = materials;
        }

        // Recursively apply to all children
        foreach (Transform child in node)
        {
            ApplyMaterialToHierarchy(character, child);
        }
    }

    /// <summary>
    /// Applies the override material to all currently instantiated parts
    /// </summary>
    private void ApplyMaterialToAllParts(ModularCharacter character)
    {
        if (character.overrideMaterial == null)
        {
            EditorUtility.DisplayDialog("No Material Set", "Please set an Override Material before applying to all parts.", "OK");
            return;
        }

        // Apply to all descendants of this character object
        ApplyMaterialToHierarchy(character, character.transform);
        
        EditorUtility.SetDirty(character);
    }

    /// <summary>
    /// Applies all blend shapes to a specific part instance
    /// </summary>
    private void ApplyBlendShapesToPart(ModularCharacter character, GameObject partInstance)
    {
        if (partInstance == null) return;

        ApplyBlendShape(character, partInstance, "Face", character.blendShapeFace);
        ApplyBlendShape(character, partInstance, "Hips", character.blendShapeHips);
        ApplyBlendShape(character, partInstance, "Waist", character.blendShapeWaist);
        ApplyBlendShape(character, partInstance, "Bust", character.blendShapeBust);
        ApplyBlendShape(character, partInstance, "Feet", character.blendShapeFeet);
    }

    /// <summary>
    /// Applies all blend shapes (Face, Hips, Waist, Bust, Feet) to all SkinnedMeshRenderers in the character's children
    /// </summary>
    private void ApplyBlendShapesToAllParts(ModularCharacter character)
    {

        ApplyBlendShapeToAllChildren(character, "Face", character.blendShapeFace);
        ApplyBlendShapeToAllChildren(character, "Hips", character.blendShapeHips);
        ApplyBlendShapeToAllChildren(character, "Waist", character.blendShapeWaist);
        ApplyBlendShapeToAllChildren(character, "Bust", character.blendShapeBust);
        ApplyBlendShapeToAllChildren(character, "Feet", character.blendShapeFeet);

        EditorUtility.SetDirty(character);
    }

    /// <summary>
    /// Applies a blend shape to all SkinnedMeshRenderers in all children
    /// </summary>
    private void ApplyBlendShapeToAllChildren(ModularCharacter character, string blendShapeName, float value)
    {
        SkinnedMeshRenderer[] allRenderers = character.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        
        int successCount = 0;
        int notFoundCount = 0;
        
        foreach (SkinnedMeshRenderer renderer in allRenderers)
        {
            if (renderer.sharedMesh == null)
            {
                Debug.LogWarning($"[ModularCharacterEditor] Renderer {renderer.name} has null sharedMesh");
                continue;
            }

            int blendShapeIndex = renderer.sharedMesh.GetBlendShapeIndex(blendShapeName);
            if (blendShapeIndex != -1)
            {
                renderer.SetBlendShapeWeight(blendShapeIndex, value);
                successCount++;
            }
            else
            {
                notFoundCount++;
            }
        }
        
    }

    /// <summary>
    /// DEPRECATED: Use ApplyCharacterOptionsToAll instead
    /// Applies the Hips blend shape value to Torso and UpperLegs parts
    /// </summary>
    private void ApplyHipsToAllParts(ModularCharacter character)
    {
        
        bool hasWarning = false;
        int partsProcessed = 0;

        GameObject[] parts = { character.currentTorso, character.currentUpperLegs };
        foreach (GameObject part in parts)
        {
            if (part != null)
            {
                bool success = ApplyBlendShape(character, part, "Hips", character.blendShapeHips);
                if (!success)
                {
                    Debug.LogWarning($"[ModularCharacterEditor] Blend shape 'Hips' not found in {part.name}");
                    hasWarning = true;
                }
                else
                {
                    partsProcessed++;
                }
            }
        }

        
        if (hasWarning)
        {
            EditorUtility.DisplayDialog("Blend Shape Warning", 
                "One or more parts are missing the 'Hips' blend shape. These parts should have this blend shape defined.", "OK");
        }

        EditorUtility.SetDirty(character);
    }

    /// <summary>
    /// Applies the Waist blend shape value to Torso and UpperLegs parts
    /// </summary>
    private void ApplyWaistToAllParts(ModularCharacter character)
    {
        
        bool hasWarning = false;
        int partsProcessed = 0;

        GameObject[] parts = { character.currentTorso, character.currentUpperLegs };
        foreach (GameObject part in parts)
        {
            if (part != null)
            {
                bool success = ApplyBlendShape(character, part, "Waist", character.blendShapeWaist);
                if (!success)
                {
                    Debug.LogWarning($"[ModularCharacterEditor] Blend shape 'Waist' not found in {part.name}");
                    hasWarning = true;
                }
                else
                {
                    partsProcessed++;
                }
            }
        }

        
        if (hasWarning)
        {
            EditorUtility.DisplayDialog("Blend Shape Warning", 
                "One or more parts are missing the 'Waist' blend shape. These parts should have this blend shape defined.", "OK");
        }

        EditorUtility.SetDirty(character);
    }

    /// <summary>
    /// Applies the Bust blend shape value to Torso and UpperLegs parts
    /// </summary>
    private void ApplyBustToAllParts(ModularCharacter character)
    {
        
        bool hasWarning = false;
        int partsProcessed = 0;

        GameObject[] parts = { character.currentTorso, character.currentUpperLegs };
        foreach (GameObject part in parts)
        {
            if (part != null)
            {
                bool success = ApplyBlendShape(character, part, "Bust", character.blendShapeBust);
                if (!success)
                {
                    Debug.LogWarning($"[ModularCharacterEditor] Blend shape 'Bust' not found in {part.name}");
                    hasWarning = true;
                }
                else
                {
                    partsProcessed++;
                }
            }
        }

        
        if (hasWarning)
        {
            EditorUtility.DisplayDialog("Blend Shape Warning", 
                "One or more parts are missing the 'Bust' blend shape. These parts should have this blend shape defined.", "OK");
        }

        EditorUtility.SetDirty(character);
    }

    /// <summary>
    /// Applies a blend shape to all SkinnedMeshRenderers in a part
    /// Returns true if blend shape was found and applied, false if not found
    /// </summary>
    private bool ApplyBlendShape(ModularCharacter character, GameObject partInstance, string blendShapeName, float value)
    {
        if (partInstance == null)
        {
            return true; // Not an error if part doesn't exist
        }

        
        bool foundBlendShape = false;
        SkinnedMeshRenderer[] renderers = partInstance.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        
        
        foreach (SkinnedMeshRenderer renderer in renderers)
        {
            if (renderer.sharedMesh == null)
            {
                Debug.LogWarning($"[ModularCharacterEditor] Renderer {renderer.name} has null sharedMesh");
                continue;
            }

            
            int blendShapeIndex = renderer.sharedMesh.GetBlendShapeIndex(blendShapeName);
            if (blendShapeIndex != -1)
            {
                renderer.SetBlendShapeWeight(blendShapeIndex, value);
                foundBlendShape = true;
            }
            else
            {
                // List available blend shapes for debugging
                if (renderer.sharedMesh.blendShapeCount > 0)
                {
                    string availableShapes = "Available blend shapes: ";
                    for (int i = 0; i < renderer.sharedMesh.blendShapeCount; i++)
                    {
                        availableShapes += renderer.sharedMesh.GetBlendShapeName(i) + ", ";
                    }
                }
            }
        }

        return foundBlendShape;
    }

}

}