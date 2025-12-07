using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class City : ILocationsHolder
{
    public CityConfig Config;
    public Dictionary<ILocatable, CityPosition> Positions { get; private set; } = new();
    private readonly List<ILocation> locations = new();
    public INodesNetwork nodesNet { get; private set; } // Legacy grid (optional)

    public IReadOnlyList<RoadNode> Nodes => _nodes;
    public IReadOnlyList<RoadEdge> Edges => _edges;

    private readonly List<RoadNode> _nodes = new();
    private readonly List<RoadEdge> _edges = new();

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
            Debug.Assert(!string.IsNullOrEmpty(e.Id), "EdgeData.Id must be set.");
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
    }

    public CityLocation GetEmptyLocation(CityPosition position) => new CityLocation(this, position);
    public ILocation[] GetLocations() => locations.ToArray();

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

    public readonly struct CityPosition
    {
        public CityPosition(RoadNode atNode)
        {
            Debug.Assert(atNode != null, "CityPosition.At requires a non-null node.");
            Node = atNode;
            Edge = null;
            Percentage = 0f;
            Forward = true;
        }

        public CityPosition(RoadEdge edge, float t, bool forward = true)
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

                var spline = Edge.GetSpline();
                Debug.Assert(spline != null, "CityPosition.WorldPosition: Edge spline is missing.");

                var p = Forward ? Percentage : (1f - Percentage);
                var localPos = SplineUtility.EvaluatePosition(spline, p);
                var worldPos = Edge.Container != null
                    ? Edge.Container.transform.TransformPoint((Vector3)localPos)
                    : (Vector3)localPos;

                return new Vector2(worldPos.x, worldPos.y);
            }
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

    public class CityLocation : ILocation
    {
        public CityPosition CityPosition { get; private set; }
        public City City { get; private set; }
        public ILocatable Occupant { get; private set; }

        public CityLocation(City city, CityPosition initialCityPosition, ILocatable initialOccupant = null)
        {
            City = city;
            CityPosition = initialCityPosition;
            City.locations.Add(this);
            if (initialOccupant != null) Attach(initialOccupant);
        }

        public ILocationsHolder Holder => City;

        public bool Attach(ILocatable locatable)
        {
            Debug.Assert(locatable != null, "Locatable to attach cannot be null");
            if (Occupant != null || locatable == null) return false;
            Occupant = locatable;
            City.Positions[locatable] = CityPosition;
            return true;
        }

        public void Detach()
        {
            if (Occupant == null) return;
            City.Positions.Remove(Occupant);
            Occupant = null;
        }

        public void MoveTo(CityPosition newPosition)
        {
            CityPosition = newPosition;
            if (Occupant != null) City.Positions[Occupant] = CityPosition;
        }
    }
}

