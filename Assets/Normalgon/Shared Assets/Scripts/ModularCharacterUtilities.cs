using UnityEngine;
using System.Collections.Generic;

namespace Normalgon.SharedAssets
{

/// <summary>
/// Shared utilities and constants for the modular character system
/// </summary>
public static class ModularCharacterUtilities
{
    public enum RiggedPartType
    {
        Torso,
        UpperLegs,
        Head,
        HandLeft,
        HandRight,
        FootLeft,
        FootRight,
        HeadCovering
    }

    public enum SocketPartType
    {
        Hat,
        Mustache,
        Beard,
        BackGear,
        ForearmLeft,
        ForearmRight,
        ShoulderLeft,
        ShoulderRight,
        ShinLeft,
        ShinRight,
        ThighLeft,
        ThighRight,
        HeadAppendageLeft,
        HeadAppendageRight,
    }

    public static string GetSocketNameForSlot(string slotName)
    {
        switch (slotName)
        {
            case "hat": return "Socket_Hat";
            case "beard": return "Socket_Beard";
            case "mustache": return "Socket_Mustache";
            case "forearmLeft": return "Socket_ForearmLeft";
            case "forearmRight": return "Socket_ForearmRight";
            case "shoulderLeft": return "Socket_ShoulderLeft";
            case "shoulderRight": return "Socket_ShoulderRight";
            case "shinLeft": return "Socket_ShinLeft";
            case "shinRight": return "Socket_ShinRight";
            case "thighLeft": return "Socket_ThighLeft";
            case "thighRight": return "Socket_ThighRight";
            case "backGear": return "Socket_BackGear";
            case "headAppendageLeft": return "Socket_HeadAppendageLeft";
            case "headAppendageRight": return "Socket_HeadAppendageRight";
            default: return "Socket_" + slotName;
        }
    }

    public static Transform FindSocketInHierarchy(Transform parent, string socketName)
    {
        if (MatchesSocketName(parent.name, socketName))
            return parent;

        foreach (Transform child in parent)
        {
            Transform found = FindSocketInHierarchy(child, socketName);
            if (found != null)
                return found;
        }

        return null;
    }

    /// <summary>
    /// Checks if a socket name matches by ignoring anything after the second underscore.
    /// e.g., "Socket_ShoulderLeft_Base" matches "Socket_ShoulderLeft"
    /// </summary>
    public static bool MatchesSocketName(string actualName, string targetName)
    {
        // First check exact match for backwards compatibility
        if (actualName == targetName)
            return true;

        // Extract base name (everything up to and including second underscore)
        string actualBaseName = GetSocketBaseName(actualName);
        string targetBaseName = GetSocketBaseName(targetName);
        
        return actualBaseName == targetBaseName;
    }

    /// <summary>
    /// Gets the base socket name by keeping only the part up to the second underscore.
    /// e.g., "Socket_ShoulderLeft_Base" -> "Socket_ShoulderLeft"
    /// </summary>
    private static string GetSocketBaseName(string socketName)
    {
        if (string.IsNullOrEmpty(socketName))
            return socketName;

        int firstUnderscore = socketName.IndexOf('_');
        if (firstUnderscore == -1)
            return socketName;

        int secondUnderscore = socketName.IndexOf('_', firstUnderscore + 1);
        if (secondUnderscore == -1)
            return socketName;

        return socketName.Substring(0, secondUnderscore);
    }

    public static string GetSocketNameForPartType(SocketPartType partType)
    {
        switch (partType)
        {
            case SocketPartType.Hat: return "Socket_Hat";
            case SocketPartType.Mustache: return "Socket_Mustache";
            case SocketPartType.Beard: return "Socket_Beard";
            case SocketPartType.ForearmLeft: return "Socket_ForearmLeft";
            case SocketPartType.ForearmRight: return "Socket_ForearmRight";
            case SocketPartType.ShoulderLeft: return "Socket_ShoulderLeft";
            case SocketPartType.ShoulderRight: return "Socket_ShoulderRight";
            case SocketPartType.ShinLeft: return "Socket_ShinLeft";
            case SocketPartType.ShinRight: return "Socket_ShinRight";
            case SocketPartType.ThighLeft: return "Socket_ThighLeft";
            case SocketPartType.ThighRight: return "Socket_ThighRight";
            case SocketPartType.BackGear: return "Socket_BackGear";
            case SocketPartType.HeadAppendageLeft: return "Socket_HeadAppendageLeft";
            case SocketPartType.HeadAppendageRight: return "Socket_HeadAppendageRight";
            default: return "Socket_Unknown";
        }
    }

    // Property accessors for rigged parts
    public static readonly Dictionary<RiggedPartType, System.Func<ModularCharacter, GameObject>> RiggedPartGetters = 
        new Dictionary<RiggedPartType, System.Func<ModularCharacter, GameObject>>
        {
            { RiggedPartType.Torso, c => c.currentTorso },
            { RiggedPartType.UpperLegs, c => c.currentUpperLegs },
            { RiggedPartType.Head, c => c.currentHead },
            { RiggedPartType.HandLeft, c => c.currentHandLeft },
            { RiggedPartType.HandRight, c => c.currentHandRight },
            { RiggedPartType.FootLeft, c => c.currentFootLeft },
            { RiggedPartType.FootRight, c => c.currentFootRight },
            { RiggedPartType.HeadCovering, c => c.currentHeadCovering }
        };

    public static readonly Dictionary<RiggedPartType, System.Action<ModularCharacter, GameObject>> RiggedPartSetters = 
        new Dictionary<RiggedPartType, System.Action<ModularCharacter, GameObject>>
        {
            { RiggedPartType.Torso, (c, v) => c.currentTorso = v },
            { RiggedPartType.UpperLegs, (c, v) => c.currentUpperLegs = v },
            { RiggedPartType.Head, (c, v) => c.currentHead = v },
            { RiggedPartType.HandLeft, (c, v) => c.currentHandLeft = v },
            { RiggedPartType.HandRight, (c, v) => c.currentHandRight = v },
            { RiggedPartType.FootLeft, (c, v) => c.currentFootLeft = v },
            { RiggedPartType.FootRight, (c, v) => c.currentFootRight = v },
            { RiggedPartType.HeadCovering, (c, v) => c.currentHeadCovering = v }
        };

    // Property accessors for socket parts
    public static readonly Dictionary<SocketPartType, System.Func<ModularCharacter, GameObject>> SocketPartGetters =
        new Dictionary<SocketPartType, System.Func<ModularCharacter, GameObject>>
        {
            { SocketPartType.Hat, c => c.currentHat },
            { SocketPartType.Mustache, c => c.currentMustache },
            { SocketPartType.Beard, c => c.currentBeard },
            { SocketPartType.ForearmLeft, c => c.currentForearmLeft },
            { SocketPartType.ForearmRight, c => c.currentForearmRight },
            { SocketPartType.ShoulderLeft, c => c.currentShoulderLeft },
            { SocketPartType.ShoulderRight, c => c.currentShoulderRight },
            { SocketPartType.ShinLeft, c => c.currentShinLeft },
            { SocketPartType.ShinRight, c => c.currentShinRight },
            { SocketPartType.ThighLeft, c => c.currentThighLeft },
            { SocketPartType.ThighRight, c => c.currentThighRight },
            { SocketPartType.BackGear, c => c.currentBackGear },
            { SocketPartType.HeadAppendageLeft, c => c.currentHeadAppendageLeft },
            { SocketPartType.HeadAppendageRight, c => c.currentHeadAppendageRight }
        };

    public static readonly Dictionary<SocketPartType, System.Action<ModularCharacter, GameObject>> SocketPartSetters =
        new Dictionary<SocketPartType, System.Action<ModularCharacter, GameObject>>
        {
            { SocketPartType.Hat, (c, v) => c.currentHat = v },
            { SocketPartType.Beard, (c, v) => c.currentBeard = v },
            { SocketPartType.Mustache, (c, v) => c.currentMustache = v },
            { SocketPartType.ForearmLeft, (c, v) => c.currentForearmLeft = v },
            { SocketPartType.ForearmRight, (c, v) => c.currentForearmRight = v },
            { SocketPartType.ShoulderLeft, (c, v) => c.currentShoulderLeft = v },
            { SocketPartType.ShoulderRight, (c, v) => c.currentShoulderRight = v },
            { SocketPartType.ShinLeft, (c, v) => c.currentShinLeft = v },
            { SocketPartType.ShinRight, (c, v) => c.currentShinRight = v },
            { SocketPartType.ThighLeft, (c, v) => c.currentThighLeft = v },
            { SocketPartType.ThighRight, (c, v) => c.currentThighRight = v },
            { SocketPartType.BackGear, (c, v) => c.currentBackGear = v },
            { SocketPartType.HeadAppendageLeft, (c, v) => c.currentHeadAppendageLeft = v },
            { SocketPartType.HeadAppendageRight, (c, v) => c.currentHeadAppendageRight = v }
        };

    // Part options accessors
    public static readonly Dictionary<RiggedPartType, System.Func<ModularCharacter, List<GameObject>>> RiggedPartOptionsGetters = 
        new Dictionary<RiggedPartType, System.Func<ModularCharacter, List<GameObject>>>
        {
            { RiggedPartType.Torso, c => c.torsoOptions },
            { RiggedPartType.UpperLegs, c => c.upperLegsOptions },
            { RiggedPartType.Head, c => c.headOptions },
            { RiggedPartType.HandLeft, c => c.handLeftOptions },
            { RiggedPartType.HandRight, c => c.handRightOptions },
            { RiggedPartType.FootLeft, c => c.footLeftOptions },
            { RiggedPartType.FootRight, c => c.footRightOptions },
            { RiggedPartType.HeadCovering, c => c.headCoveringOptions }
        };

    public static readonly Dictionary<SocketPartType, System.Func<ModularCharacter, List<GameObject>>> SocketPartOptionsGetters =
        new Dictionary<SocketPartType, System.Func<ModularCharacter, List<GameObject>>>
        {
            { SocketPartType.Hat, c => c.hatOptions },
            { SocketPartType.Beard, c => c.beardOptions },
            { SocketPartType.Mustache, c => c.mustacheOptions },
            { SocketPartType.ForearmLeft, c => c.forearmLeftOptions },
            { SocketPartType.ForearmRight, c => c.forearmRightOptions },
            { SocketPartType.ShoulderLeft, c => c.shoulderLeftOptions },
            { SocketPartType.ShoulderRight, c => c.shoulderRightOptions },
            { SocketPartType.ShinLeft, c => c.shinLeftOptions },
            { SocketPartType.ShinRight, c => c.shinRightOptions },
            { SocketPartType.ThighLeft, c => c.thighLeftOptions },
            { SocketPartType.ThighRight, c => c.thighRightOptions },
            { SocketPartType.BackGear, c => c.backGearOptions },
            { SocketPartType.HeadAppendageLeft, c => c.headAppendageLeftOptions },
            { SocketPartType.HeadAppendageRight, c => c.headAppendageRightOptions }
        };

    // UI field name mappings
    public static readonly Dictionary<string, RiggedPartType> RiggedPartFieldMappings = 
        new Dictionary<string, RiggedPartType>
        {
            { "torso", RiggedPartType.Torso },
            { "upper-legs", RiggedPartType.UpperLegs },
            { "head", RiggedPartType.Head },
            { "hand-left", RiggedPartType.HandLeft },
            { "hand-right", RiggedPartType.HandRight },
            { "foot-left", RiggedPartType.FootLeft },
            { "foot-right", RiggedPartType.FootRight },
            { "head-covering", RiggedPartType.HeadCovering }
        };

    public static readonly Dictionary<string, SocketPartType> SocketPartFieldMappings =
        new Dictionary<string, SocketPartType>
        {
            { "hat", SocketPartType.Hat },
            { "mustache", SocketPartType.Mustache },
            { "beard", SocketPartType.Beard },
            { "forearm-left", SocketPartType.ForearmLeft },
            { "forearm-right", SocketPartType.ForearmRight },
            { "shoulder-left", SocketPartType.ShoulderLeft },
            { "shoulder-right", SocketPartType.ShoulderRight },
            { "shin-left", SocketPartType.ShinLeft },
            { "shin-right", SocketPartType.ShinRight },
            { "thigh-left", SocketPartType.ThighLeft },
            { "thigh-right", SocketPartType.ThighRight },
            { "back-gear", SocketPartType.BackGear },
            { "head-appendage-left", SocketPartType.HeadAppendageLeft },
            { "head-appendage-right", SocketPartType.HeadAppendageRight }
        };

    public static void CopyTransformValues(Transform source, Transform target)
    {
        target.localPosition = source.localPosition;
        target.localRotation = source.localRotation;
        target.localScale = source.localScale;
    }
}

}