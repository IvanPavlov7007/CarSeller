#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;

public static class CityGraphBaker
{
    [MenuItem("City/Bake Graph From Selection")]
    public static void BakeSelected()
    {
        var root = Selection.activeGameObject;
        if (root == null)
        {
            EditorUtility.DisplayDialog("City Bake", "Select a root GameObject containing nodes and edges.", "OK");
            return;
        }

        var nodes = root.GetComponentsInChildren<RoadNodeAuthor>(true);
        var edges = root.GetComponentsInChildren<RoadEdgeAuthor>(true);

        foreach (var n in nodes) n.EnsureId();
        foreach (var e in edges) e.EnsureId();

        var path = EditorUtility.SaveFilePanelInProject("Save CityGraph", "CityGraph", "asset", "Choose asset location");
        if (string.IsNullOrEmpty(path)) return;

        // Load or create asset at path (keeps GUID if exists)
        var graph = AssetDatabase.LoadAssetAtPath<CityGraphAsset>(path);
        if (graph == null)
        {
            graph = ScriptableObject.CreateInstance<CityGraphAsset>();
            AssetDatabase.CreateAsset(graph, path);
        }

        graph.Nodes = nodes.Select(n => new CityGraphAsset.NodeData
        {
            Id = n.Id,
            Position = new Vector2(n.transform.position.x, n.transform.position.y)
        }).ToList();

        graph.Edges = edges.Select(e => new CityGraphAsset.EdgeData
        {
            Id = e.Id,
            FromNodeId = e.From ? e.From.Id : null,
            ToNodeId = e.To ? e.To.Id : null,
            Bidirectional = e.Bidirectional,
            EdgeAuthorId = e.Id,
            EdgeAuthorPath = GetHierarchyPath(e.gameObject),
            SplineIndex = 0
        }).ToList();

        EditorUtility.SetDirty(graph);
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("City Bake", $"Baked {graph.Nodes.Count} nodes and {graph.Edges.Count} edges.", "OK");
    }

    private static string GetHierarchyPath(GameObject go)
    {
        var path = go.name;
        var t = go.transform;
        while (t.parent != null)
        {
            t = t.parent;
            path = $"{t.name}/{path}";
        }
        return path;
    }
}
#endif