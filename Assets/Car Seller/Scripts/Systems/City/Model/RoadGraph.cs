using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

[System.Serializable]
public sealed class RoadNode
{
    public string Id;
    public Vector2 Position;
    public List<RoadEdge> Outgoing = new List<RoadEdge>();
    public List<RoadEdge> Incoming = new List<RoadEdge>();
}

[System.Serializable]
public sealed class RoadEdge
{
    public string Id;
    public RoadNode From;
    public RoadNode To;

    // Unity Splines authoring model: container holding one or more splines
    public SplineContainer Container;
    public int SplineIndex;

    public float Length;
    public bool Bidirectional = true;

    public Spline GetSpline()
    {
        if (Container == null) return null;
        var splines = Container.Splines;
        if (splines == null || SplineIndex < 0 || SplineIndex >= splines.Count) return null;
        return splines[SplineIndex];
    }

    /// <summary>
    /// Returns the unit tangent direction in world XY plane when leaving this edge
    /// from the given node at the given percentage (0..1). Sets forward=true if
    /// motion goes From-&gt;To, false if To-&gt;From.
    /// </summary>
    public Vector2 GetTangentFromNode(RoadNode node, float percentage, out bool forward)
    {
        if (this == null)
            throw new System.ArgumentNullException(nameof(node), "Edge is null in GetTangentFromNode.");

        var spline = GetSpline();
        if (spline == null)
            throw new System.InvalidOperationException($"Edge '{Id}' has no valid spline.");

        forward = true;

        if (node == From)
        {
            var tanLocal = SplineUtility.EvaluateTangent(spline, Mathf.Clamp01(percentage));
            var tanWorld3 = Container != null
                ? Container.transform.TransformDirection((Vector3)tanLocal)
                : (Vector3)tanLocal;
            return ((Vector2)tanWorld3).normalized;
        }

        if (node == To && Bidirectional)
        {
            forward = false;
            var tanLocal = SplineUtility.EvaluateTangent(spline, 1f - Mathf.Clamp01(percentage));
            var tanWorld3 = Container != null
                ? Container.transform.TransformDirection((Vector3)tanLocal)
                : (Vector3)tanLocal;
            return -((Vector2)tanWorld3).normalized;
        }

        // Detailed error for debugging graph issues
        string nodeId = node != null ? node.Id : "null";
        string edgeId = Id ?? "null";
        bool isFrom = node == From;
        bool isTo = node == To;
        bool bidi = Bidirectional;

        throw new System.InvalidOperationException(
            $"Cannot compute tangent direction from node '{nodeId}' on edge '{edgeId}'. " +
            $"Relationship: isFrom={isFrom}, isTo={isTo}, edgeBidirectional={bidi}. " +
            $"If leaving from 'To' endpoint, the edge must be bidirectional."
        );
    }
}

public interface IJunctionPolicy
{
    // Returns allowed outgoing edges given desired direction and time/context.
    IEnumerable<RoadEdge> GetAllowedOutgoing(RoadNode atNode, Vector2 desiredDir);
}