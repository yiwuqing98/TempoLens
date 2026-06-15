using UnityEngine;
using System.Collections.Generic;

namespace Normalgon.SharedAssets
{

public class BoneRemapper : MonoBehaviour
{
    [Header("Bone Remapper Settings")]
    [SerializeField, HideInInspector]
    private Transform characterRootBone; // The root bone of your character's skeleton (auto-assigned)


    void Start()
    {
        // Auto-assign the root bone by finding the first child named "Hips"
        if (characterRootBone == null)
        {
            characterRootBone = FindChildByName(transform, "Hips");
            if (characterRootBone == null)
            {
                Debug.LogError("Could not find a child named 'Hips' to assign as the character root bone.", this);
            }
        }
        DoBones(); // Find all SkinnedMeshRenderers and remap their bones
    }
    // Recursively search for a child with the given name
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


    void DoBones()
    {
        if (characterRootBone == null)
        {
            Debug.LogError("Character Root Bone is not assigned.", this);
            return;
        }

        var skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foreach (var smr in skinnedMeshRenderers)
        {
            RemapBones(smr);
        }
    }

    

    [ContextMenu("Remap Bones")]
    public void RemapBonesMenu()
    {
        // Auto-assign the root bone by finding the first child named "Hips" if not already assigned
        if (characterRootBone == null)
        {
            characterRootBone = FindChildByName(transform, "Hips");
            if (characterRootBone == null)
            {
                Debug.LogError("Could not find a child named 'Hips' to assign as the character root bone.", this);
            }
        }
        DoBones();
    }

    // Remap bones for a specific SkinnedMeshRenderer
    private void RemapBones(SkinnedMeshRenderer targetSkinnedMeshRenderer)
    {
        if (targetSkinnedMeshRenderer == null)
            return;

        if (characterRootBone == null)
        {
            Debug.LogError("Character Root Bone is not assigned.", this);
            return;
        }

        // Get all the bones from the character's skeleton
        Transform[] characterBones = characterRootBone.GetComponentsInChildren<Transform>(true);

        // Create a dictionary to quickly look up character bones by name
        Dictionary<string, Transform> characterBoneMap = new Dictionary<string, Transform>();
        foreach (Transform bone in characterBones)
        {
            characterBoneMap[bone.name] = bone;
        }

        // Create a new array for the target SkinnedMeshRenderer's bones
        Transform[] newBones = new Transform[targetSkinnedMeshRenderer.bones.Length];

        // Iterate through the target SkinnedMeshRenderer's current bones
        for (int i = 0; i < targetSkinnedMeshRenderer.bones.Length; i++)
        {
            string boneName = targetSkinnedMeshRenderer.bones[i].name;

            // Try to find a matching bone in the character's skeleton
            if (characterBoneMap.TryGetValue(boneName, out Transform matchingBone))
            {
                newBones[i] = matchingBone;
            }
            else
            {
                Debug.LogWarning($"Bone '{boneName}' not found in the character's skeleton. This bone might not be animated correctly.", this);
                newBones[i] = targetSkinnedMeshRenderer.bones[i]; // Keep the original bone if not found
            }
        }

        // Assign the new bones array to the target SkinnedMeshRenderer
        targetSkinnedMeshRenderer.bones = newBones;
    }
}

}
