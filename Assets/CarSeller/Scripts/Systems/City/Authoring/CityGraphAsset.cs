using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CityGraph", menuName = "City/Graph")]
public class CityGraphAsset : ScriptableObject
{
    public GameObject PrefabRoot; // prefab root for authoring linkage

    [Serializable]
    public class NodeData
    {
        public string Id;
        public Vector2 Position;
    }

    [Serializable]
    public class EdgeData
    {
        public string Id;
        public string FromNodeId;
        public string ToNodeId;
        public bool Bidirectional = true;

        public string[] Tags;

        // Authoring linkage to resolve spline in scene/prefab:
        public string EdgeAuthorId;     // RoadEdgeAuthor.Id
        public string EdgeAuthorPath;   // optional: scene hierarchy path for robust lookup
        public int SplineIndex = 0;     // convention: one spline per edge
    }

    // MARKERS

    [Serializable]
    public enum MarkerAnchorKind
    {
        WorldPoint,
        Node,
        Edge
    }

    [Serializable]
    public class MarkerAnchorData
    {
        public MarkerAnchorKind Kind = MarkerAnchorKind.WorldPoint;

        // Node anchor:
        public string NodeId;

        // Edge anchor:
        [Range(0f, 1f)] public float T;
        public bool Forward = true;
        public string EdgeId;

        // World point anchor:
        public Vector2 WorldPoint;
    }

    [Serializable]
    public class MarkerData
    {
        public string Id;
        public string DisplayName;
        public string[] Tags;
        public string RegionId;
        public float Radius;

        public MarkerAnchorData Anchor = new MarkerAnchorData();

        // Authoring linkage
        public string AuthorId;
        public string AuthorPath;
    }

    // AREAS

    [Serializable]
    public class AreaData
    {
        public string Id;
        public string DisplayName;
        public string[] Tags;

        // Polygon points in PrefabRoot local space (XY).
        public Vector2[] Polygon;

        // Authoring linkage
        public string AuthorId;
        public string AuthorPath;
    }

    public List<NodeData> Nodes = new();
    public List<EdgeData> Edges = new();

    // New: Markers authored in scene/prefab and baked here
    public List<MarkerData> Markers = new();

    // New: Areas authored via PolygonCollider2D and baked here
    public List<AreaData> Areas = new();
}