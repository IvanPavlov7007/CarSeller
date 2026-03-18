using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public class City : ILocationsHolder, IDisposable
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
    public AspectsSystem AspectsSystem { get; private set; }
    public CityEntityAspectsService AspectsService => AspectsSystem.AspectsService;

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

    public City(CityConfig cityConfig, Transform graphRoot, AspectsSystem aspectsSystem)
    {
        Config = cityConfig;
        // IMPORTANT: pass a valid root that contains your authored edges/nodes when available.
        // If null, CityGraphLoader must scan the active scene root(s).
        CityGraphLoader.LoadFromScene(this, cityConfig.CityGraph, graphRoot);
        this.AspectsSystem = aspectsSystem;
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
                Tags = e.Tags,
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
            if (!edge.TryGetClosestPoint(worldPosition, out float edgeT, out float edgeDist2))
            {
                continue;
            }

            if (edgeDist2 < bestEdgeDist2)
            {
                bestEdgeDist2 = edgeDist2;
                bestEdge = edge;
                bestEdgeT = edgeT;
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

    public void Dispose()
    {
        AspectsSystem.Dispose();
    }
}

public readonly struct CityPosition
{
    public RoadNode Node { get; }
    public RoadEdge Edge { get; }
    /// <summary>
    /// A percentage [0..1] along the edge if Edge is not null.
    /// From the 'From' node (0) to the 'To' node (1) if Forward is true,
    /// </summary>
    public float Percentage { get; }
    public bool Forward { get; }
    /// <summary>
    /// Important: doesn't get updated automatically if the underlying node/edge moves
    /// </summary>
    public Vector2 WorldPosition { get; }

    CityPosition(RoadNode atNode)
    {
        Debug.Assert(atNode != null, "CityPosition.At requires a non-null node.");
        Node = atNode;
        Edge = null;
        Percentage = 0f;
        Forward = true;
        WorldPosition = atNode.Position;
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
        WorldPosition = SpecificWorldPositionOnEdge(Edge, Percentage, Forward);
    }

    private static Vector2 SpecificWorldPositionOnEdge(RoadEdge edge, float t, bool forward)
    {
        Debug.Assert(edge != null, "SpecificWorldPositionOnEdge can only be used for edge positions.");
        Debug.Assert(t >= 0f && t <= 1f, "SpecificWorldPositionOnEdge t must be in [0,1].");
        var spline = edge.GetSpline();
        Debug.Assert(spline != null, "SpecificWorldPositionOnEdge: Edge spline is missing.");
        var localPos = SplineUtility.EvaluatePosition(spline, forward ? t : 1f - t);
        var worldPos = edge.Container != null
            ? edge.Container.transform.TransformPoint((Vector3)localPos)
            : (Vector3)localPos;
        return new Vector2(worldPos.x, worldPos.y);
    }

    public Vector2 SpecificWorldPositionOnEdge(float t)
    {
        return SpecificWorldPositionOnEdge(Edge, t, Forward);
    }

    public CityPosition WithPercentage(float t)
    {
        Debug.Assert(Edge != null, "WithPercentage can only be used for edge positions.");
        Debug.Assert(t >= 0f && t <= 1f, "WithPercentage t must be in [0,1].");
        return new CityPosition(Edge, t, Forward);
    }

    /// <summary>
    /// Finds the closest point on an edge to <paramref name="worldTarget"/> and returns it as a <see cref="CityPosition"/>.
    /// Uses <see cref="RoadEdge.TryGetClosestPoint"/> (coarse sampling + refinement).
    /// </summary>
    public static CityPosition GetClosestOnEdge(RoadEdge edge, Vector2 worldTarget, int coarseDivisions = 64, int refineIterations = 8)
    {
        edge.TryGetClosestPoint(worldTarget, out float t, out _, coarseDivisions, refineIterations);

        return On(edge, t, forward: true);
    }

    public bool FlowsIntoAnotherOnTheSameEdge(CityPosition other)
    {
        Debug.Assert(this.Edge != null);
        Debug.Assert(Edge == other.Edge);
        float t1 = this.Percentage;
        float t2 = this.Forward == other.Forward ? other.Percentage : 1f - other.Percentage;

        return t2 > t1;
    }

    public CityPosition TowardsAnotherForDistance(CityPosition other, float distance)
    {
        Debug.Assert(this.Edge != null);
        Debug.Assert(Edge == other.Edge);

        float deltaT = distance / Edge.Length;
        float t1 = this.Percentage;
        float t2 = other.Percentage;

        float newT;

        if (Forward == other.Forward)
        {
            if (t1 > t2)
            {
                newT = t1 - deltaT;
            }
            else
            {
                newT = t1 + deltaT;
            }
        }
        else
        {
            if (t1 > (1f - t2))
            {
                newT = t1 - deltaT;
            }
            else
            {
                newT = t1 + deltaT;
            }
        }
        newT = Mathf.Clamp01(newT);
        return On(Edge, newT, Forward);
    }

    public float DistanceToAnotherOnSameEdge(CityPosition other)
    {
        Debug.Assert(this.Edge != null && other.Edge != null, "DistanceTo currently only supports positions on edges.");
        Debug.Assert(this.Edge == other.Edge, "DistanceTo currently only supports positions on the same edge.");

        if (other.Forward != this.Forward)
        {
            other = other.Reversed();
        }
        float deltaT = Mathf.Abs(this.Percentage - other.Percentage);
        return deltaT * Edge.Length;
    }

    public CityPosition Reversed()
    {
        return On(Edge, 1f - Percentage, !Forward);
    }

    public Vector2 GetCurrentTangent()
    {
        Debug.Assert(Edge != null, "GetCurrentTangent can only be used for edge positions.");
        if(Forward)
        {
            return Edge.GetTangentFromNode(Edge.From, Percentage, out _);
        }
        else
        {
            return Edge.GetTangentFromNode(Edge.To, Percentage, out _);
        }
    }

    public static CityPosition At(RoadNode node) => new CityPosition(node);
    public static CityPosition On(RoadEdge edge, float t, bool forward = true) => new CityPosition(edge, t, forward);

    public CityPosition GetConnectionPositionFromAnotherEdge(RoadEdge AnotherEdge)
    {
        Debug.Assert(this.Edge != null, "GetConnectionPositionFromAnotherEdge can only be used for edge positions.");
        
        if (AnotherEdge.From == Edge.From)
        {
            return On(Edge, 0f, forward: true);
        }
        else if (AnotherEdge.To == Edge.From)
        {
            return On(Edge, 0f, forward: true);
        }
        else if (AnotherEdge.From == Edge.To)
        {
            return On(Edge, 0f, forward: false);
        }
        else if (AnotherEdge.To == Edge.To)
        {
            return On(Edge, 0f, forward: false);
        }
        else
        {
            throw new InvalidOperationException("Edges are not connected.");
        }
    }

    public CityPosition GetConnectionPositionTowardsEdge(RoadEdge AnotherEdge)
    {
        Debug.Assert(this.Edge != null, "GetConnectionPositionTowardsEdge can only be used for edge positions.");
        
        if(AnotherEdge.From == Edge.To)
        {
            return On(Edge, 1f, forward: true);
        }
        else if(AnotherEdge.To == Edge.To)
        {
            return On(Edge, 1f, forward: true);
        }
        else if(AnotherEdge.From == Edge.From)
        {
            return On(Edge, 1f, forward: false);
        }
        else if (AnotherEdge.To == Edge.From)
        {
            return On(Edge, 1f, forward: false);
        }
        throw new InvalidOperationException("Edges are not connected.");
    }
}