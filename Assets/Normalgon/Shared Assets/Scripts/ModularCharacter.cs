using System.Collections.Generic;
using UnityEngine;

namespace Normalgon.SharedAssets
{

/// <summary>
/// Runtime-only modular character component. Holds references to equipped parts.
/// All instantiation and editor logic is handled by ModularCharacterEditor.
/// </summary>
public class ModularCharacter : MonoBehaviour
{
    [Header("Core Character")]
    public Material overrideMaterial;
    [HideInInspector]
    public Transform animatedRig;
    [HideInInspector]
    public GameObject baseBody;

    [Header("Blend Shape Controls")]
    [Range(0f, 100f)]
    public float blendShapeFace = 0f;
    [Range(0f, 100f)]
    public float blendShapeHips = 0f;
    [Range(0f, 100f)]
    public float blendShapeWaist = 0f;
    [Range(0f, 100f)]
    public float blendShapeBust = 0f;
    [Range(0f, 100f)]
    public float blendShapeFeet = 0f;

    [Header("Part Options")]
    public List<GameObject> torsoOptions = new List<GameObject>();
    public List<GameObject> upperLegsOptions = new List<GameObject>();
    public List<GameObject> headOptions = new List<GameObject>();
    public List<GameObject> handLeftOptions = new List<GameObject>();
    public List<GameObject> handRightOptions = new List<GameObject>();
    public List<GameObject> footLeftOptions = new List<GameObject>();
    public List<GameObject> footRightOptions = new List<GameObject>();
    public List<GameObject> headCoveringOptions = new List<GameObject>();
    public List<GameObject> hatOptions = new List<GameObject>();
    public List<GameObject> mustacheOptions = new List<GameObject>();
    public List<GameObject> beardOptions = new List<GameObject>();
    public List<GameObject> forearmLeftOptions = new List<GameObject>();
    public List<GameObject> forearmRightOptions = new List<GameObject>();
    public List<GameObject> shoulderLeftOptions = new List<GameObject>();
    public List<GameObject> shoulderRightOptions = new List<GameObject>();
    public List<GameObject> shinLeftOptions = new List<GameObject>();
    public List<GameObject> shinRightOptions = new List<GameObject>();
    public List<GameObject> thighLeftOptions = new List<GameObject>();
    public List<GameObject> thighRightOptions = new List<GameObject>();
    public List<GameObject> backGearOptions = new List<GameObject>();
    public List<GameObject> headAppendageLeftOptions = new List<GameObject>();
    public List<GameObject> headAppendageRightOptions = new List<GameObject>();

    [Header("Currently Equipped Parts")]
    public GameObject currentTorso;
    public GameObject currentUpperLegs;
    public GameObject currentHead;
    public GameObject currentHandLeft;
    public GameObject currentHandRight;
    public GameObject currentFootLeft;
    public GameObject currentFootRight;
    public GameObject currentHeadCovering;
    public GameObject currentHat;
    public GameObject currentBeard;
    public GameObject currentMustache;
    public GameObject currentForearmLeft;
    public GameObject currentForearmRight;
    public GameObject currentShoulderLeft;
    public GameObject currentShoulderRight;
    public GameObject currentShinLeft;
    public GameObject currentShinRight;
    public GameObject currentThighLeft;
    public GameObject currentThighRight;
    public GameObject currentBackGear;
    public GameObject currentHeadAppendageLeft;
    public GameObject currentHeadAppendageRight;

    /// <summary>
    /// Call this at runtime when a new rigged part is added to remap its bones for animation
    /// </summary>
    public void RemapBonesForPart(GameObject partInstance)
    {
        if (partInstance == null) return;

        // Find the character's root bone (usually "Hips")
        Transform rootBone = FindRootBone();
        if (rootBone == null)
        {
            Debug.LogError("[ModularCharacter] Cannot remap bones - no root bone found. Looking for 'Hips' bone.");
            return;
        }

        // Get all SkinnedMeshRenderers in the new part
        SkinnedMeshRenderer[] skinnedRenderers = partInstance.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        
        foreach (var renderer in skinnedRenderers)
        {
            RemapBonesForRenderer(renderer, rootBone);
        }

    }

    private Transform FindRootBone()
    {
        // Look in the animatedRig for the root bone
        if (animatedRig != null)
        {
            return FindChildByName(animatedRig, "Hips");
        }

        // Fallback: look in this GameObject
        return FindChildByName(transform, "Hips");
    }

    private Transform FindChildByName(Transform parent, string name)
    {
        if (parent.name == name)
            return parent;

        foreach (Transform child in parent)
        {
            Transform result = FindChildByName(child, name);
            if (result != null)
                return result;
        }
        return null;
    }

    private void RemapBonesForRenderer(SkinnedMeshRenderer targetRenderer, Transform rootBone)
    {
        if (targetRenderer == null || rootBone == null) return;

        // Get all bones from character's skeleton
        Transform[] characterBones = rootBone.GetComponentsInChildren<Transform>(true);

        // Create lookup dictionary
        var boneMap = new System.Collections.Generic.Dictionary<string, Transform>();
        foreach (Transform bone in characterBones)
        {
            if (bone != null) // Safety check for null bones
            {
                boneMap[bone.name] = bone;
            }
        }

        // Remap the renderer's bones
        Transform[] newBones = new Transform[targetRenderer.bones.Length];
        for (int i = 0; i < targetRenderer.bones.Length; i++)
        {
            // Safety check for null bone in array
            if (targetRenderer.bones[i] == null)
            {
                Debug.LogWarning($"[ModularCharacter] Bone at index {i} is null in {targetRenderer.name}");
                newBones[i] = null;
                continue;
            }
            
            string boneName = targetRenderer.bones[i].name;
            
            if (boneMap.TryGetValue(boneName, out Transform matchingBone))
            {
                newBones[i] = matchingBone;
            }
            else
            {
                Debug.LogWarning($"[ModularCharacter] Bone '{boneName}' not found in character skeleton");
                newBones[i] = targetRenderer.bones[i]; // Keep original if not found
            }
        }

        targetRenderer.bones = newBones;
    }

    private string GetObjectState(UnityEngine.Object obj)
    {
        try
        {
            if (obj == null) return "null";
            return obj.name;
        }
        catch (UnassignedReferenceException)
        {
            return "UNASSIGNED";
        }
        catch (MissingReferenceException)
        {
            return "MISSING";
        }
    }
}

}