using System;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable, InlineProperty]
public class CityMarkerRef
{
    // Optional explicit override. Usually left null to use auto-resolved or default graph.
    [HideLabel, AssetsOnly]
    [HorizontalGroup("Row", Width = 80)]
    [LabelText("Graph")]
    public CityGraphAsset Graph;

    [HorizontalGroup("Row")]
    [ShowInInspector, ValueDropdown(nameof(GetMarkerOptions))]
    [LabelText("Marker"), LabelWidth(60)]
    public string MarkerId;

    [HideInInspector]
    public bool IsValid => !string.IsNullOrEmpty(MarkerId) && HasGraph;

    // --------- Runtime helpers ---------

    public bool HasGraph => ResolveGraph(null) != null;

    public CityGraphAsset.MarkerData GetMarkerData(UnityEngine.Object owner = null)
    {
        var graph = ResolveGraph(owner);
        if (graph == null || string.IsNullOrEmpty(MarkerId)) return null;
        return graph.Markers?.FirstOrDefault(m => m.Id == MarkerId);
    }

    // --------- Shared logic (editor + runtime) ---------

    /// <summary>
    /// Resolve graph from explicit Graph, from owner, or from global fallback.
    /// </summary>
    public CityGraphAsset ResolveGraph(UnityEngine.Object owner)
    {
        // 1) Explicit override on this ref
        if (Graph != null)
            return Graph;

#if UNITY_EDITOR
        // 2) Owner-based resolution (e.g. via CityConfig)
        if (owner != null)
        {
            var cityGraphFromOwner = CityMarkerRefGraphResolver.TryResolveGraphFromOwner(owner);
            if (cityGraphFromOwner != null)
                return cityGraphFromOwner;
        }
#endif

        // 3) Global fallback (resources or first asset in project)
        return CityMarkerRefDefaults.GetDefaultGraph();
    }

#if UNITY_EDITOR
    // Odin dropdown source – uses whichever graph ResolveGraph gives it (via default/override).
    private ValueDropdownList<string> GetMarkerOptions()
    {
        var list = new ValueDropdownList<string>();

        // For dropdown, we don't know the owner here, so pass null.
        // ResolveGraph(null) will use: Graph override OR global default.
        var graph = ResolveGraph(null);
        if (graph == null || graph.Markers == null)
            return list;

        foreach (var m in graph.Markers)
        {
            var tags = m.Tags != null && m.Tags.Length > 0
                ? $" [{string.Join(",", m.Tags)}]"
                : "";
            var label = $"{m.DisplayName}{tags} ({m.Id})";
            list.Add(label, m.Id);
        }

        return list;
    }
#endif
}
public static class CityMarkerRefDefaults
{
    private static CityGraphAsset _cachedDefault;

    /// <summary>
    /// Returns a default CityGraphAsset to use when no explicit/owner graph is present.
    /// Editor: tries AssetDatabase first, then Resources.
    /// Runtime: uses Resources only.
    /// </summary>
    public static CityGraphAsset GetDefaultGraph()
    {
        if (_cachedDefault != null)
            return _cachedDefault;

        // 1. Try Resources at runtime (and in editor as fallback).
        //    If you keep your graphs under Resources/CityGraphs, this can be:
        //    Resources.LoadAll<CityGraphAsset>("CityGraphs");
        var fromResources = Resources.LoadAll<CityGraphAsset>(string.Empty);
        if (fromResources != null && fromResources.Length > 0)
        {
            _cachedDefault = fromResources[0];
            return _cachedDefault;
        }

#if UNITY_EDITOR
        // 2. Editor-only: search all CityGraphAsset assets in the project.
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:CityGraphAsset");
        foreach (var guid in guids)
        {
            var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<CityGraphAsset>(path);
            if (asset != null)
            {
                _cachedDefault = asset;
                return _cachedDefault;
            }
        }
#endif

        return null;
    }
}