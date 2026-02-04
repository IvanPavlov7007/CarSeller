using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public static class CityGraphLoader
{
    public static void LoadFromScene(City city, CityGraphAsset graph, Transform root)
    {
        city.InitializeFromGraph(graph);

        var authors = root.GetComponentsInChildren<RoadEdgeAuthor>(true)
                          .ToDictionary(a => a.Id);

        foreach (var edge in city.Edges)
        {
            var data = graph.Edges.FirstOrDefault(d => d.Id == edge.Id);
            if (data == null) continue;

            if (authors.TryGetValue(data.EdgeAuthorId, out var author))
            {
                edge.Container = author.Container;
                edge.SplineIndex = 0;

                var spline = edge.GetSpline();
                if (spline != null)
                {
                    // Use world-space matrix for consistent length with scaled containers
                    var m = edge.Container != null
                        ? edge.Container.transform.localToWorldMatrix
                        : Matrix4x4.identity;

                    edge.Length = SplineUtility.CalculateLength(spline, m);
                }
                else
                {
                    edge.Length = 0f;
                }
            }
        }

        // Build lookup
        var nodeById = city.Nodes.ToDictionary(n => n.Id);
        var edgeById = city.Edges.ToDictionary(e => e.Id);

        // Optional: author lookup for world positions when missing
        var markerAuthorsById = root.GetComponentsInChildren<CityMarkerAuthor>(true)
                                    .ToDictionary(a => a.Id);

        // Finalize markers (after edges resolved so Edge.GetSpline is valid)
        var markers = new List<City.CityMarker>();
        if (graph.Markers != null)
        {
            foreach (var m in graph.Markers)
            {
                var cm = new City.CityMarker
                {
                    Id = m.Id,
                    Name = m.DisplayName,
                    Tags = m.Tags,
                    RegionId = m.RegionId,
                    Radius = m.Radius
                };

                // Determine a reference position for snapping if needed
                Vector2 refWorld = GetMarkerReferenceWorldPoint(m, markerAuthorsById);

                switch (m.Anchor.Kind)
                {
                    case CityGraphAsset.MarkerAnchorKind.WorldPoint:
                        cm.WorldPoint = m.Anchor.WorldPoint;
                        break;

                    case CityGraphAsset.MarkerAnchorKind.Node:
                        if (!string.IsNullOrEmpty(m.Anchor.NodeId) && nodeById.TryGetValue(m.Anchor.NodeId, out var node))
                        {
                            cm.PositionOnGraph = CityPosition.At(node);
                        }
                        else
                        {
                            // Snap to nearest node using reference world point
                            var nearestNode = FindNearestNode(city, refWorld);
                            if (nearestNode != null)
                            {
                                cm.PositionOnGraph = CityPosition.At(nearestNode);
                            }
                            else
                            {
                                // no nodes – keep as world point
                                cm.WorldPoint = refWorld;
                            }
                        }
                        break;

                    case CityGraphAsset.MarkerAnchorKind.Edge:
                        if (!string.IsNullOrEmpty(m.Anchor.EdgeId) && edgeById.TryGetValue(m.Anchor.EdgeId, out var edgeFromId))
                        {
                            var t = Mathf.Clamp01(m.Anchor.T);
                            bool forward = m.Anchor.Forward || !edgeFromId.Bidirectional ? true : m.Anchor.Forward;
                            cm.PositionOnGraph = CityPosition.On(edgeFromId, t, forward);
                        }
                        else
                        {
                            // Snap to nearest edge using reference world point
                            if (TryFindClosestEdge(city, refWorld, out var bestEdge, out var bestT))
                            {
                                cm.PositionOnGraph = CityPosition.On(bestEdge, bestT, true);
                            }
                            else
                            {
                                // no edges – keep as world point
                                cm.WorldPoint = refWorld;
                            }
                        }
                        break;
                }

                markers.Add(cm);
            }
        }

        city.InitializeMarkers(markers);

        RoadGraphMaintenance.RebuildAdjacency(city.Nodes, city.Edges);
    }

    private static Vector2 GetMarkerReferenceWorldPoint(CityGraphAsset.MarkerData m, Dictionary<string, CityMarkerAuthor> authorsById)
    {
        // Prefer baked world point
        var wp = m.Anchor.WorldPoint;
        // If an author exists in the instantiated root, use its live transform (handles scaled roots, etc.)
        if (!string.IsNullOrEmpty(m.AuthorId) && authorsById.TryGetValue(m.AuthorId, out var author) && author != null)
        {
            var p = author.transform.position;
            return new Vector2(p.x, p.y);
        }
        return wp;
    }

    private static RoadNode FindNearestNode(City city, Vector2 worldPoint)
    {
        RoadNode best = null;
        float bestD2 = float.PositiveInfinity;
        foreach (var n in city.Nodes)
        {
            float d2 = (n.Position - worldPoint).sqrMagnitude;
            if (d2 < bestD2)
            {
                bestD2 = d2;
                best = n;
            }
        }
        return best;
    }

    // Finds closest point on any edge; returns best edge and its t in [0,1].
    private static bool TryFindClosestEdge(City city, Vector2 worldPoint, out RoadEdge bestEdge, out float bestT)
    {
        bestEdge = null;
        bestT = 0f;
        float bestD2 = float.PositiveInfinity;

        foreach (var edge in city.Edges)
        {
            var spline = edge.GetSpline();
            if (spline == null || edge.Container == null) continue;

            const int divisions = 64;
            for (int i = 0; i <= divisions; i++)
            {
                float t = i / (float)divisions;
                var localPos = SplineUtility.EvaluatePosition(spline, t);
                var worldPos3 = edge.Container.transform.TransformPoint((Vector3)localPos);
                var worldPos2 = new Vector2(worldPos3.x, worldPos3.y);
                float d2 = (worldPos2 - worldPoint).sqrMagnitude;
                if (d2 < bestD2)
                {
                    bestD2 = d2;
                    bestEdge = edge;
                    bestT = t;
                }
            }
        }

        return bestEdge != null;
    }
}