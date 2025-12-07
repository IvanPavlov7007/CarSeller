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

        // Authoring linkage to resolve spline in scene/prefab:
        public string EdgeAuthorId;     // RoadEdgeAuthor.Id
        public string EdgeAuthorPath;   // optional: scene hierarchy path for robust lookup
        public int SplineIndex = 0;     // convention: one spline per edge
    }

    public List<NodeData> Nodes = new();
    public List<EdgeData> Edges = new();
}