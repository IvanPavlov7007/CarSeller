using System;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable, InlineProperty]
public class CityMarkerRef
{
    // Optional explicit override. Usually left null to use auto-resolved graph.
    [HideLabel, AssetsOnly]
    [HorizontalGroup("Row", Width = 80)]
    [LabelText("Graph")]
    public CityGraphAsset Graph;

    [HorizontalGroup("Row")]
    [ShowInInspector, ValueDropdown(nameof(GetMarkerOptions))]
    [LabelText("Marker"), LabelWidth(60)]
    public string MarkerId;

    [HideInInspector]
    public bool IsValid;

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
    /// Resolve graph from explicit Graph override or from owning CityConfig.CityGraph.
    /// </summary>
    public CityGraphAsset ResolveGraph(UnityEngine.Object owner)
    {
        if (Graph != null)
            return Graph;

#if UNITY_EDITOR
        if (owner != null)
        {
            var cityGraphFromOwner = CityMarkerRefGraphResolver.TryResolveGraphFromOwner(owner);
            if (cityGraphFromOwner != null)
                return cityGraphFromOwner;
        }
#endif
        return null;
    }

#if UNITY_EDITOR
    // Odin dropdown source – assumes Graph is already resolved (either explicit or via custom drawer).
    private ValueDropdownList<string> GetMarkerOptions()
    {
        var list = new ValueDropdownList<string>();
        // We don't know the owner here; ResolveGraph(null) will only use explicit Graph.
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