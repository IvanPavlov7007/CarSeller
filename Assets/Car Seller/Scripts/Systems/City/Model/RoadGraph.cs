using System;
using System.Collections.Generic;
using UnityEngine;
using Pixelplacement;

[Serializable]
public sealed class RoadNode
{
    public string Id;                      // Stable id for references
    public Vector2 Position;               // World position (editor set)
    public List<RoadEdge> Outgoing = new List<RoadEdge>();
    public List<RoadEdge> Incoming = new List<RoadEdge>();
}

[Serializable]
public sealed class RoadEdge
{
    public string Id;                      // Stable id
    public RoadNode From;
    public RoadNode To;
    public Spline Spline;                  // Runtime reference to the spline instance
    public float Length;                   // Cached from Spline.Length (optional)
    public bool Bidirectional = true;      // Whether both directions are allowed
}

public interface IJunctionPolicy
{
    // Returns allowed outgoing edges given desired direction and time/context.
    IEnumerable<RoadEdge> GetAllowedOutgoing(RoadNode atNode, Vector2 desiredDir);
}