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
}

public interface IJunctionPolicy
{
    // Returns allowed outgoing edges given desired direction and time/context.
    IEnumerable<RoadEdge> GetAllowedOutgoing(RoadNode atNode, Vector2 desiredDir);
}