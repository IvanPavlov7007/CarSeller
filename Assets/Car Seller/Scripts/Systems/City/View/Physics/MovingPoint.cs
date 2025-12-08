using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
    
public class MovingPoint : MonoBehaviour
{
    public static float maxSpeed = 2f;

    City.CityPosition position;
    IDirectionProvider directionProvider;
    Transform arrowRotationPoint;
    Transform body;

    private const float Epsilon = 1e-4f;

    private void Awake()
    {
        directionProvider = GetComponent<IDirectionProvider>();
        body = transform.GetChild(0);
        arrowRotationPoint = transform.GetChild(1);
    }

    public void Initialize(City.CityPosition pos) { position = pos; }

    void LateUpdate()
    {
        if (directionProvider == null) return;

        Vector2 desired = Vector2.ClampMagnitude(directionProvider.ProvidedDirection, 1f);
        float lerpV = 10f * Time.deltaTime;
        var aim = desired.sqrMagnitude > 1e-6f ? desired.normalized : Vector2.up;
        arrowRotationPoint.rotation = Quaternion.Lerp(arrowRotationPoint.rotation, Quaternion.FromToRotation(Vector2.up, aim), lerpV);
        arrowRotationPoint.localScale = new Vector3(1f, Mathf.Lerp(arrowRotationPoint.localScale.y, Mathf.Clamp01(desired.magnitude), lerpV), 1f);

        // If at a node, pick an outgoing edge
        if (position.Edge == null)
        {
            var node = position.Node;
            if (node == null || desired.sqrMagnitude < 1e-6f) return;

            if (!TryPickEdgeFromNode(node, desired, out var nextEdge, out bool forward, out Vector2 leaveDir))
            {
                body.up = leaveDir == Vector2.zero ? aim : leaveDir;
                return;
            }

            // Start on edge at small epsilon
            position = City.CityPosition.On(nextEdge, forward ? Epsilon : (1f - Epsilon), forward);
            body.up = leaveDir;
        }

        // Advance along current edge
        var edge = position.Edge;
        var spline = edge.GetSpline();
        if (spline == null) return;

        float t = position.Forward ? position.Percentage : 1f - position.Percentage;

        // Current tangent (local -> world)
        var tanLocal = SplineUtility.EvaluateTangent(spline, t);
        var tanWorld3 = edge.Container != null
            ? edge.Container.transform.TransformDirection((Vector3)tanLocal)
            : (Vector3)tanLocal;
        var tangent2 = ((Vector2)tanWorld3).normalized;

        // Project desired onto tangent and move forward only
        var speedAlong = Vector2.Dot(desired, tangent2);
        var delta = Mathf.Max(0f, speedAlong) * Time.deltaTime * maxSpeed;

        // Normalize by edge length (world-space length already precomputed)
        float length = Mathf.Max(edge.Length, 0.0001f);
        float dt = delta / length;

        t += dt;

        if (t >= 1f)
        {
            // Arrive at end node
            var endNode = position.Forward ? edge.To : edge.From;

            var posLocalEnd = SplineUtility.EvaluatePosition(spline, 1f);
            var posWorldEnd3 = edge.Container != null
                ? edge.Container.transform.TransformPoint((Vector3)posLocalEnd)
                : (Vector3)posLocalEnd;
            transform.position = new Vector2(posWorldEnd3.x, posWorldEnd3.y);

            // Pick next edge from the end node
            if (!TryPickEdgeFromNode(endNode, desired, out var nextEdge, out bool forward, out Vector2 leaveDir))
            {
                position = City.CityPosition.At(endNode);
                body.up = tangent2;
                return;
            }

            position = City.CityPosition.On(nextEdge, forward ? Epsilon : (1f - Epsilon), forward);
            body.up = leaveDir;

            // Update transform on new edge start
            var nextSpline = nextEdge.GetSpline();
            float nextT = forward ? Epsilon : (1f - Epsilon);
            var posLocal = SplineUtility.EvaluatePosition(nextSpline, nextT);
            var posWorld3 = nextEdge.Container != null
                ? nextEdge.Container.transform.TransformPoint((Vector3)posLocal)
                : (Vector3)posLocal;
            transform.position = new Vector2(posWorld3.x, posWorld3.y);
            return;
        }

        // Stay on edge
        var posLocalCur = SplineUtility.EvaluatePosition(spline, t);
        var posWorldCur3 = edge.Container != null
            ? edge.Container.transform.TransformPoint((Vector3)posLocalCur)
            : (Vector3)posLocalCur;
        transform.position = new Vector2(posWorldCur3.x, posWorldCur3.y);

        float newPercentage = position.Forward ? t : 1f - t;
        position = position.WithPercentage(newPercentage);
        body.up = tangent2;
    }

    // Picks the best outgoing edge from a node based on desired direction.
    // Returns false if no suitable edge exists; out leaveDir is what we aim the body at.
    private bool TryPickEdgeFromNode(RoadNode node, Vector2 desired, out RoadEdge edge, out bool forward, out Vector2 leaveDir)
    {
        edge = null;
        forward = true;
        leaveDir = Vector2.zero;

        var desiredN = desired.sqrMagnitude > 1e-6f ? desired.normalized : Vector2.zero;
        float bestDot = -Mathf.Infinity;

        foreach (var e in node.Outgoing)
        {
            var spline = e.GetSpline();
            if (spline == null) continue;

            // If this node is the 'From', leaving along t=0
            if (node == e.From)
            {
                var tanLocal = SplineUtility.EvaluateTangent(spline, 0f);
                var tanWorld3 = e.Container != null
                    ? e.Container.transform.TransformDirection((Vector3)tanLocal)
                    : (Vector3)tanLocal;
                var outDir = ((Vector2)tanWorld3).normalized;

                float d = Vector2.Dot(desiredN, outDir);
                if (d > bestDot)
                {
                    bestDot = d; edge = e; forward = true; leaveDir = outDir;
                }
            }
            // If this node is the 'To', leaving along -tangent at t=1 (requires bidirectional)
            else if (node == e.To && e.Bidirectional)
            {
                var tanLocal = SplineUtility.EvaluateTangent(spline, 1f);
                var tanWorld3 = e.Container != null
                    ? e.Container.transform.TransformDirection((Vector3)tanLocal)
                    : (Vector3)tanLocal;
                var outDir = -((Vector2)tanWorld3).normalized;

                float d = Vector2.Dot(desiredN, outDir);
                if (d > bestDot)
                {
                    bestDot = d; edge = e; forward = false; leaveDir = outDir;
                }
            }
        }

        // Allow backward choice (dot < 0) by flipping 'forward' flag as picked above.
        // If no edge found at all, return false.
        return edge != null;
    }
}
