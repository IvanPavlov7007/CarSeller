using System;
using System.Collections.Generic;
using UnityEngine;

public class CityViewStreetsBuilder
{
    private readonly Material _lineMaterial;
    private readonly float _lineWidth;
    private readonly HashSet<(Node, Node)> _createdEdges = new HashSet<(Node, Node)>(new EdgeComparer());

    public CityViewStreetsBuilder(Material lineMaterial = null, float lineWidth = 0.05f)
    {
        _lineMaterial = lineMaterial;
        _lineWidth = lineWidth;
    }

    public void BuildCity(City city)
    {
        if (city == null || city.nodesNet == null || city.nodesNet.Nodes == null)
            return;

        Transform _streetsRoot = new GameObject("City Streets").transform;

        // Create or reuse a parent container for street segments
        //if (_streetsRoot == null)
        //{
        //    _streetsRoot = new GameObject("City Streets");
        //}

        foreach (var node in city.nodesNet.Nodes)
        {
            if (node == null || node.connectedNeighbors == null)
                continue;

            foreach (var neighbor in node.connectedNeighbors)
            {
                if (neighbor == null) continue;

                // Ensure we only create one segment per undirected edge
                var edgeKey = GetOrderedEdge(node, neighbor);
                if (_createdEdges.Contains(edgeKey))
                    continue;

                _createdEdges.Add(edgeKey);

                CreateLineSegment(_streetsRoot, node, neighbor);
            }
        }
    }

    private void CreateLineSegment(Transform _streetsRoot, Node a, Node b)
    {
        var go = new GameObject($"Street [{a.InitialPosition}] -> [{b.InitialPosition}]");
        go.transform.SetParent(_streetsRoot.transform, false);

        var lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.useWorldSpace = true;

        // Pick current position if available, otherwise initial
        Vector3 p0 = a.CurrentPosition != default ? (Vector3)a.CurrentPosition : (Vector3)a.InitialPosition;
        Vector3 p1 = b.CurrentPosition != default ? (Vector3)b.CurrentPosition : (Vector3)b.InitialPosition;

        lr.SetPosition(0, p0);
        lr.SetPosition(1, p1);

        lr.startWidth = _lineWidth;
        lr.endWidth = _lineWidth;

        // Basic material setup (falls back to a default if none provided)
        if (_lineMaterial != null)
        {
            lr.material = _lineMaterial;
        }
        else
        {
            // Minimal default material to avoid pink lines
            var mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = Color.gray;
            lr.material = mat;
        }

        // Optional visual settings
        lr.sortingOrder = -50;
        lr.numCornerVertices = 2;
        lr.numCapVertices = 2;
        lr.alignment = LineAlignment.View;
        lr.textureMode = LineTextureMode.Stretch;
    }

    private static (Node, Node) GetOrderedEdge(Node a, Node b)
    {
        // Order deterministically to treat (a,b) == (b,a)
        return ReferenceEquals(a, b) || a.GetHashCode() <= b.GetHashCode() ? (a, b) : (b, a);
    }

    private class EdgeComparer : IEqualityComparer<(Node, Node)>
    {
        public bool Equals((Node, Node) x, (Node, Node) y)
        {
            return ReferenceEquals(x.Item1, y.Item1) && ReferenceEquals(x.Item2, y.Item2);
        }

        public int GetHashCode((Node, Node) obj)
        {
            unchecked
            {
                int h1 = obj.Item1 != null ? obj.Item1.GetHashCode() : 0;
                int h2 = obj.Item2 != null ? obj.Item2.GetHashCode() : 0;
                return (h1 * 397) ^ h2;
            }
        }
    }
}