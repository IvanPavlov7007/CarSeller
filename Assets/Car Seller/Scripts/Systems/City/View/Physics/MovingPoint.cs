using UnityEngine;
using UnityEngine.Splines;

public class MovingPoint : MonoBehaviour
{
    public static float maxSpeed = 2f;

    City.CityPosition position;
    IDirectionProvider directionProvider;
    Transform arrowRotationPoint;
    Transform body;

    // Track last traversed edge to avoid immediate re-pick causing a jump
    RoadEdge lastEdge = null;
    bool lastForward = true;

    private void Awake()
    {
        directionProvider = GetComponent<IDirectionProvider>();
        body = transform.GetChild(0);
        arrowRotationPoint = transform.GetChild(1);
    }

    public void Initialize(City.CityPosition pos)
    {
        position = pos;
        lastEdge = pos.Edge;
        lastForward = pos.Forward;
    }

    void LateUpdate()
    {
        Vector2 desired = Vector2.ClampMagnitude(directionProvider.ProvidedDirection, 1f);
        float lerpV = 10f * Time.deltaTime;
        if (desired.sqrMagnitude > 1e-6f)
        {
            arrowRotationPoint.rotation = Quaternion.Lerp(arrowRotationPoint.rotation, Quaternion.FromToRotation(Vector2.up, desired.normalized), lerpV);
        }
        arrowRotationPoint.localScale = new Vector3(1f, Mathf.Lerp(arrowRotationPoint.localScale.y, Mathf.Clamp01(desired.magnitude), lerpV), 1f);

        if (position.Edge == null)
        {
            var node = position.Node;
            if (node == null || desired.sqrMagnitude < 1e-6f) { return; }

            // Pick edge to leave this node
            var pick = PickBestOutgoingEdge(node, desired);
            if (pick.edge == null)
            {
                body.up = desired;
                return;
            }

            // Start on the chosen edge with tiny epsilon to avoid sticking at exact 0/1
            const float epsilon = 1e-4f;
            position = City.CityPosition.On(pick.edge, pick.forward ? epsilon : (1f - epsilon), pick.forward);
            lastEdge = pick.edge;
            lastForward = pick.forward;
        }

        // Advance along current edge
        var edge = position.Edge;
        var splineCurrent = edge.GetSpline();
        if (splineCurrent == null) return;

        var p = position.Forward ? position.Percentage : 1f - position.Percentage;

        // Tangent at current t (local), convert to world
        var tanLocal = SplineUtility.EvaluateTangent(splineCurrent, p);
        var tanWorld = edge.Container != null
            ? edge.Container.transform.TransformDirection((Vector3)tanLocal)
            : (Vector3)tanLocal;
        var tangent2 = ((Vector2)tanWorld).normalized;

        var speedAlong = Vector2.Dot(desired, tangent2);
        var delta = Mathf.Max(0f, speedAlong) * Time.deltaTime * maxSpeed;

        // Normalize by arc length
        var length = Mathf.Max(edge.Length, 0.0001f);
        var normalizedDelta = delta / length;

        p += normalizedDelta;

        if (p >= 1f)
        {
            // Overshoot past end; carry leftover into the next edge
            float leftover = p - 1f;

            var endNode = position.Forward ? edge.To : edge.From;

            var posLocalEnd = SplineUtility.EvaluatePosition(splineCurrent, 1f);
            var posWorldEnd = edge.Container != null
                ? edge.Container.transform.TransformPoint((Vector3)posLocalEnd)
                : (Vector3)posLocalEnd;

            transform.position = new Vector2(posWorldEnd.x, posWorldEnd.y);

            // Choose next edge from endNode based on desired
            var pick = PickBestOutgoingEdge(endNode, desired);
            if (pick.edge == null)
            {
                // Stop at node
                position = City.CityPosition.At(endNode);
                body.up = tangent2;
                lastEdge = null;
                return;
            }

            // Move a small epsilon into next edge plus leftover progress
            const float epsilon = 1e-4f;
            float startT = pick.forward ? epsilon : (1f - epsilon);
            float nextP = pick.forward ? startT + leftover : startT - leftover;

            // Clamp nextP to valid range and set percentage accordingly
            nextP = Mathf.Clamp01(nextP);
            position = City.CityPosition.On(pick.edge, pick.forward ? nextP : (1f - nextP), pick.forward);
            lastEdge = pick.edge;
            lastForward = pick.forward;

            // Update transform on new edge
            var nextSpline = pick.edge.GetSpline();
            if (nextSpline != null)
            {
                var posLocal = SplineUtility.EvaluatePosition(nextSpline, nextP);
                var posWorld = pick.edge.Container != null
                    ? pick.edge.Container.transform.TransformPoint((Vector3)posLocal)
                    : (Vector3)posLocal;
                transform.position = new Vector2(posWorld.x, posWorld.y);

                var nextTanLocal = SplineUtility.EvaluateTangent(nextSpline, nextP);
                var nextTanWorld = pick.edge.Container != null
                    ? pick.edge.Container.transform.TransformDirection((Vector3)nextTanLocal)
                    : (Vector3)nextTanLocal;
                body.up = ((Vector2)nextTanWorld).normalized;
            }

            return;
        }
        else
        {
            var posLocal = SplineUtility.EvaluatePosition(splineCurrent, p);
            var posWorld = edge.Container != null
                ? edge.Container.transform.TransformPoint((Vector3)posLocal)
                : (Vector3)posLocal;

            transform.position = new Vector2(posWorld.x, posWorld.y);
            var newPercentage = position.Forward ? p : 1f - p;
            position = position.WithPercentage(newPercentage);
        }

        body.up = tangent2;
    }

    private (RoadEdge edge, bool forward) PickBestOutgoingEdge(RoadNode node, Vector2 desired)
    {
        RoadEdge bestEdge = null;
        bool bestForward = true;
        float bestDot = -Mathf.Infinity;

        var dirN = desired.sqrMagnitude > 1e-6f ? desired.normalized : Vector2.zero;

        foreach (var e in node.Outgoing)
        {
            var spline = e.GetSpline();
            if (spline == null) continue;

            // Leave node along correct tangent depending on which end this node is
            if (node == e.From)
            {
                var tanLocal = SplineUtility.EvaluateTangent(spline, 0f);
                var tanWorld = e.Container != null
                    ? e.Container.transform.TransformDirection((Vector3)tanLocal)
                    : (Vector3)tanLocal;

                var d = Vector2.Dot(dirN, ((Vector2)tanWorld).normalized);
                if (d > bestDot)
                {
                    bestDot = d; bestEdge = e; bestForward = true;
                }
            }
            else if (node == e.To)
            {
                if (!e.Bidirectional) continue;

                var tanLocal = SplineUtility.EvaluateTangent(spline, 1f);
                var tanWorld = e.Container != null
                    ? e.Container.transform.TransformDirection((Vector3)tanLocal)
                    : (Vector3)tanLocal;

                // Leaving from 'To' towards 'From' requires negating the tangent at t=1
                var leaveDir = -((Vector2)tanWorld).normalized;
                var d = Vector2.Dot(dirN, leaveDir);
                if (d > bestDot)
                {
                    bestDot = d; bestEdge = e; bestForward = false;
                }
            }
        }

        return (bestEdge, bestForward);
    }
}
