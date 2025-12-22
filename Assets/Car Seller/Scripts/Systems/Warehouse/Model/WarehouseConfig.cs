using System;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "WarehouseConfig", menuName = "Configs/Economy/WarehouseConfig")]
public class WarehouseConfig : SerializedScriptableObject
{
    public string DisplayName;

    public Sprite image;
    public string SizeCategory = "Small garage";

    public string SceneToLoad;

    [Title("Placement")]
    [InlineProperty, HideLabel]
    public CityMarkerRef Marker;

    [ShowInInspector, ValueDropdown(nameof(GetMarkerOptions)), LabelText("Marker")]
    [EnableIf(nameof(HasGraph))]
    [OnValueChanged(nameof(OnMarkerChanged))]
    private string MarkerIdProxy
    {
        get => Marker.MarkerId;
        set => Marker.MarkerId = value;
    }

    [ShowInInspector, LabelText("Graph"), InlineButton(nameof(PingGraph), "Ping")]
    [AssetsOnly]
    private CityGraphAsset GraphProxy
    {
        get => Marker.Graph;
        set => Marker.Graph = value;
    }

    [InfoBox("Assign a CityGraph to enable marker selection.", InfoMessageType.Info, VisibleIf = nameof(GraphMissing))]
    [HideInInspector] // hide backing field from non-Odin drawers
    public Vector2 warehouseClosestInitialPosition;

    public SimpleProductSpawnConfig[] initialProductsToSpawn;

    // Odin helpers
    private bool HasGraph => Marker.Graph != null;
    private bool GraphMissing => Marker.Graph == null;

    private void PingGraph()
    {
        if (Marker.Graph != null)
        {
#if UNITY_EDITOR
            UnityEditor.EditorGUIUtility.PingObject(Marker.Graph);
#endif
        }
    }

    private void OnMarkerChanged()
    {
        // Optional: validate marker id when changed
    }

    private ValueDropdownList<string> GetMarkerOptions()
    {
        var list = new ValueDropdownList<string>();
        var graph = Marker.Graph;
        if (graph == null || graph.Markers == null) return list;

        foreach (var m in graph.Markers)
        {
            var tags = m.Tags != null && m.Tags.Length > 0 ? $" [{string.Join(",", m.Tags)}]" : "";
            var label = $"{m.DisplayName}{tags} ({m.Id})";
            list.Add(label, m.Id);
        }
        return list;
    }
}

[Serializable]
public class SimpleProductSpawnConfig
{
    public ScriptableObject productBaseConfig;
    public ScriptableObject productVariantConfig;
}