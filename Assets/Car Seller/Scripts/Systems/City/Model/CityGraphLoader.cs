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

                switch (m.Anchor.Kind)
                {
                    case CityGraphAsset.MarkerAnchorKind.WorldPoint:
                        cm.WorldPoint = m.Anchor.WorldPoint;
                        break;

                    case CityGraphAsset.MarkerAnchorKind.Node:
                        if (!string.IsNullOrEmpty(m.Anchor.NodeId) && nodeById.TryGetValue(m.Anchor.NodeId, out var node))
                        {
                            cm.PositionOnGraph = City.CityPosition.At(node);
                        }
                        else
                        {
                            // fallback: treat as world point at baked node position (if you ever store it),
                            // or leave null and handle at use-site.
                        }
                        break;

                    case CityGraphAsset.MarkerAnchorKind.Edge:
                        if (!string.IsNullOrEmpty(m.Anchor.EdgeId) && edgeById.TryGetValue(m.Anchor.EdgeId, out var edge))
                        {
                            var t = Mathf.Clamp01(m.Anchor.T);
                            bool forward = m.Anchor.Forward || !edge.Bidirectional ? true : m.Anchor.Forward;
                            // CityPosition requires resolved spline (done above)
                            var pos = City.CityPosition.On(edge, t, forward);
                            cm.PositionOnGraph = pos;
                        }
                        break;
                }

                // If neither anchor resolved, but WorldPoint absent, you can still handle gracefully at use-site.
                markers.Add(cm);
            }
        }

        city.InitializeMarkers(markers);

        RoadGraphMaintenance.RebuildAdjacency(city.Nodes, city.Edges);
    }
}