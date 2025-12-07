using System.Linq;
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

        RoadGraphMaintenance.RebuildAdjacency(city.Nodes, city.Edges);
    }
}