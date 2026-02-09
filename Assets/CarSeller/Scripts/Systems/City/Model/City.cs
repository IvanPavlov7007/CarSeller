using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;
using static UnityEditor.FilePathAttribute;

public class City : ILocationsHolder
{
    public CityConfig Config;
    internal Dictionary<ILocatable, CityEntity> Entities { get; private set; } = new();
    public INodesNetwork nodesNet { get; private set; } // Legacy grid (optional)

    public IReadOnlyList<RoadNode> Nodes => _nodes;
    public IReadOnlyList<RoadEdge> Edges => _edges;

    private readonly List<RoadNode> _nodes = new();
    private readonly List<RoadEdge> _edges = new();

    private CityEntityLifetimeService lifetimeService = new CityEntityLifetimeService();
    public static CityEntityLifetimeService EntityLifetimeService => G.City.lifetimeService;
    public static CityEntityAspectsService AspectsService => G.CityEntityAspectsService;
    // MARKERS RUNTIME
    public sealed class CityMarker
    {
        public string Id;
        public string Name;
        public string[] Tags;
        public string RegionId;
        public float Radius;

        public CityPosition? PositionOnGraph; // Node or Edge@T anchor
        public Vector2? WorldPoint;           // WorldPoint anchor fallback

        public Vector2 WorldPosition => PositionOnGraph.HasValue
            ? PositionOnGraph.Value.WorldPosition
            : (WorldPoint ?? Vector2.zero);

        public bool HasTag(string tag)
        {
            if (Tags == null || tag == null) return false;
            for (int i = 0; i < Tags.Length; i++)
            {
                if (string.Equals(Tags[i], tag, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }
    }

    private readonly Dictionary<string, CityMarker> _markersById = new();
    public IReadOnlyDictionary<string, CityMarker> MarkersById => _markersById;

    internal void InitializeMarkers(IEnumerable<CityMarker> markers)
    {
        _markersById.Clear();
        foreach (var m in markers)
        {
            if (!string.IsNullOrEmpty(m.Id))
            {
                _markersById[m.Id] = m;
            }
        }
    }

    public bool TryGetMarker(string id, out CityMarker marker)
    {
        if (string.IsNullOrEmpty(id))
        {
            marker = null;
            return false;
        }
        return _markersById.TryGetValue(id, out marker);
    }

    public IEnumerable<CityMarker> QueryMarkers(string tag = null, string region = null, Predicate<CityMarker> predicate = null)
    {
        IEnumerable<CityMarker> q = _markersById.Values;
        if (!string.IsNullOrEmpty(tag)) q = q.Where(m => m.HasTag(tag));
        if (!string.IsNullOrEmpty(region)) q = q.Where(m => string.Equals(m.RegionId, region, StringComparison.OrdinalIgnoreCase));
        if (predicate != null) q = q.Where(m => predicate(m));
        return q;
    }

    public CityMarker GetRandomMarker(string tag = null, string region = null, Predicate<CityMarker> predicate = null)
    {
        var list = QueryMarkers(tag, region, predicate).ToList();
        if (list.Count == 0) return null;
        return list[UnityEngine.Random.Range(0, list.Count)];
    }

    public City(CityConfig cityConfig, Transform graphRoot)
    {
        Config = cityConfig;
        // IMPORTANT: pass a valid root that contains your authored edges/nodes when available.
        // If null, CityGraphLoader must scan the active scene root(s).
        CityGraphLoader.LoadFromScene(this, cityConfig.CityGraph, graphRoot);
    }

    public void InitializeFromGraph(CityGraphAsset graphAsset)
    {
        Debug.Assert(graphAsset != null, "CityGraphAsset must not be null.");

        _nodes.Clear();
        _edges.Clear();

        var nodeMap = new Dictionary<string, RoadNode>();
        foreach (var n in graphAsset.Nodes)
        {
            Debug.Assert(!string.IsNullOrEmpty(n.Id), "NodeData.Id must be set.");
            var rn = new RoadNode { Id = n.Id, Position = n.Position };
            nodeMap[n.Id] = rn;
            _nodes.Add(rn);
        }

        foreach (var e in graphAsset.Edges)
        {
            Debug.Assert(!string.IsNullOrEmpty(e.Id), "EdgeData.Id must not be null.");
            Debug.Assert(nodeMap.ContainsKey(e.FromNodeId), $"Edge.FromNodeId '{e.FromNodeId}' not found.");
            Debug.Assert(nodeMap.ContainsKey(e.ToNodeId), $"Edge.ToNodeId '{e.ToNodeId}' not found.");

            var edge = new RoadEdge
            {
                Id = e.Id,
                From = nodeMap[e.FromNodeId],
                To = nodeMap[e.ToNodeId],
                // DO NOT assign scene references here. Let CityGraphLoader resolve Container/Spline later.
                Container = null,
                SplineIndex = Mathf.Max(0, e.SplineIndex),
                Bidirectional = e.Bidirectional,
                Length = 0f
            };

            _edges.Add(edge);
            edge.From.Outgoing.Add(edge);
            edge.To.Incoming.Add(edge);

            if (edge.Bidirectional)
            {
                edge.To.Outgoing.Add(edge);
                edge.From.Incoming.Add(edge);
            }
        }

        // Markers are finalized after edges' containers are resolved in CityGraphLoader.
        InitializeMarkers(Array.Empty<CityMarker>());
    }
    public ILocation[] GetLocations() => Entities.Select(x=>x.Value).ToArray();

    public bool TryGetEntity(ILocatable locatable, out CityEntity entity)
    {
        return Entities.TryGetValue(locatable, out entity);
    }

    public IReadOnlyDictionary<ILocatable,CityEntity> GetEntities() => Entities;

    internal CityPosition GetClosestPosition(Vector2 worldPosition)
    {
        // Prefer closest point on any edge (spline). Fallback to closest node.
        RoadEdge bestEdge = null;
        float bestEdgeT = 0f;
        float bestEdgeDist2 = float.PositiveInfinity;

        foreach (var edge in _edges)
        {
            var spline = edge.GetSpline();
            if (spline == null || edge.Container == null) continue;

            // Coarse sampling. Increase divisions for more accuracy if needed.
            const int divisions = 64;
            for (int i = 0; i <= divisions; i++)
            {
                float t = i / (float)divisions;
                var localPos = SplineUtility.EvaluatePosition(spline, t);
                var worldPos3 = edge.Container.transform.TransformPoint((Vector3)localPos);
                var worldPos2 = new Vector2(worldPos3.x, worldPos3.y);
                float d2 = (worldPos2 - worldPosition).sqrMagnitude;
                if (d2 < bestEdgeDist2)
                {
                    bestEdgeDist2 = d2;
                    bestEdge = edge;
                    bestEdgeT = t;
                }
            }
        }

        if (bestEdge != null)
        {
            // Choose forward=true by default; if you need direction-aware selection, compute tangent dot with a desired direction.
            return CityPosition.On(bestEdge, bestEdgeT, forward: true);
        }

        // No edges resolved; pick closest node
        RoadNode bestNode = null;
        float bestNodeDist2 = float.PositiveInfinity;
        foreach (var node in _nodes)
        {
            float d2 = (node.Position - worldPosition).sqrMagnitude;
            if (d2 < bestNodeDist2)
            {
                bestNodeDist2 = d2;
                bestNode = node;
            }
        }

        return bestNode != null ? CityPosition.At(bestNode) : default;
    }

    internal CityPosition GetRandomPosition()
    {
        if (_edges.Count > 0)
        {
            var edge = _edges[UnityEngine.Random.Range(0, _edges.Count)];
            float t = UnityEngine.Random.value; // [0,1]
            bool forward = edge.Bidirectional ? (UnityEngine.Random.value < 0.5f) : true;
            return CityPosition.On(edge, t, forward);
        }

        // Fallback: random node if there are no edges
        if (_nodes.Count > 0)
        {
            var node = _nodes[UnityEngine.Random.Range(0, _nodes.Count)];
            return CityPosition.At(node);
        }

        // Nothing available
        return default;
    }

    
}


public readonly struct CityPosition
{
    CityPosition(RoadNode atNode)
    {
        Debug.Assert(atNode != null, "CityPosition.At requires a non-null node.");
        Node = atNode;
        Edge = null;
        Percentage = 0f;
        Forward = true;
    }

    CityPosition(RoadEdge edge, float t, bool forward = true)
    {
        Debug.Assert(edge != null, "CityPosition.On requires a non-null edge.");
        var spline = edge?.GetSpline();
        Debug.Assert(spline != null, "CityPosition.On requires an edge with a valid SplineContainer/Spline.");
        Debug.Assert(t >= 0f && t <= 1f, "CityPosition.On percentage t must be in [0,1].");
        if (!forward)
        {
            Debug.Assert(edge.Bidirectional, "Reverse traversal requested but edge is not bidirectional.");
        }

        Node = null;
        Edge = edge;
        Percentage = Mathf.Clamp01(t);
        Forward = forward;
    }

    public RoadNode Node { get; }
    public RoadEdge Edge { get; }
    /// <summary>
    /// A percentage [0..1] along the edge if Edge is not null.
    /// From the 'From' node (0) to the 'To' node (1) if Forward is true,
    /// </summary>
    public float Percentage { get; }
    public bool Forward { get; }

    public Vector2 WorldPosition
    {
        get
        {
            if (Edge == null)
            {
                Debug.Assert(Node != null, "CityPosition must have either Node or Edge.");
                return Node.Position;
            }
            return SpecificWorldPositionOnEdge(Percentage);
        }
    }

    public Vector2 SpecificWorldPositionOnEdge(float t)
    {
        Debug.Assert(Edge != null, "SpecificWorldPositionOnEdge can only be used for edge positions.");
        Debug.Assert(t >= 0f && t <= 1f, "SpecificWorldPositionOnEdge t must be in [0,1].");
        var spline = Edge.GetSpline();
        Debug.Assert(spline != null, "SpecificWorldPositionOnEdge: Edge spline is missing.");
        var localPos = SplineUtility.EvaluatePosition(spline, Forward ? t : 1f - t);
        var worldPos = Edge.Container != null
            ? Edge.Container.transform.TransformPoint((Vector3)localPos)
            : (Vector3)localPos;
        return new Vector2(worldPos.x, worldPos.y);
    }

    public CityPosition WithPercentage(float t)
    {
        Debug.Assert(Edge != null, "WithPercentage can only be used for edge positions.");
        Debug.Assert(t >= 0f && t <= 1f, "WithPercentage t must be in [0,1].");
        return new CityPosition(Edge, t, Forward);
    }

    public static CityPosition At(RoadNode node) => new CityPosition(node);
    public static CityPosition On(RoadEdge edge, float t, bool forward = true) => new CityPosition(edge, t, forward);
}