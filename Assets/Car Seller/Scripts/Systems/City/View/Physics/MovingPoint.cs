using UnityEngine;
using UnityEngine.Splines;

public class MovingPoint : MonoBehaviour
{
    public static float maxSpeed = 2f;

    City.CityPosition position;
    IDirectionProvider directionProvider;
    Transform arrowRotationPoint;
    Transform body;

    private void Awake()
    {
        directionProvider = GetComponent<IDirectionProvider>();
        body = transform.GetChild(0);
        arrowRotationPoint = transform.GetChild(1);
    }

    public void Initialize(City.CityPosition pos) { position = pos; }

    void LateUpdate()
    {
        Vector2 desired = Vector2.ClampMagnitude(directionProvider.ProvidedDirection, 1f);
        float lerpV = 10f * Time.deltaTime;
        arrowRotationPoint.rotation = Quaternion.Lerp(arrowRotationPoint.rotation, Quaternion.FromToRotation(Vector2.up, desired.normalized), lerpV);
        arrowRotationPoint.localScale = new Vector3(1f, Mathf.Lerp(arrowRotationPoint.localScale.y, Mathf.Clamp01(desired.magnitude), lerpV), 1f);

        if (position.Edge == null)
        {
            var node = position.Node;
            if (node == null || desired.sqrMagnitude < 1e-6f) { return; }

            RoadEdge next = null; float best = 0f; bool forward = true;

            foreach (var e in node.Outgoing)
            {
                var spline = e.GetSpline();
                if (spline == null) continue;

                // Forward tangent at t=0 (local), converted to world
                var tanFLocal = SplineUtility.EvaluateTangent(spline, 0f);
                var tanFWorld = e.Container != null
                    ? e.Container.transform.TransformDirection((Vector3)tanFLocal)
                    : (Vector3)tanFLocal;

                var dFwd = Vector2.Dot(desired.normalized, ((Vector2)tanFWorld.normalized));
                if (dFwd > best) { best = dFwd; next = e; forward = true; }

                if (e.Bidirectional)
                {
                    // Backward tangent at t=1 (local), converted to world
                    var tanBLocal = SplineUtility.EvaluateTangent(spline, 1f);
                    var tanBWorld = e.Container != null
                        ? e.Container.transform.TransformDirection((Vector3)tanBLocal)
                        : (Vector3)tanBLocal;

                    var dBwd = Vector2.Dot(desired.normalized, ((Vector2)tanBWorld.normalized));
                    if (dBwd > best) { best = dBwd; next = e; forward = false; }
                }
            }

            if (next == null || best <= 0f)
            {
                body.up = desired;
                return;
            }

            position = City.CityPosition.On(next, 0f, forward);
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

        // Normalize by arc length (local length). If container is scaled, adjust as needed.
        var length = Mathf.Max(edge.Length, 0.0001f);
        var normalizedDelta = delta / length;

        p += normalizedDelta;

        if (p >= 1f)
        {
            // Arrive at end node
            var endNode = position.Forward ? edge.To : edge.From;
            var posLocalEnd = SplineUtility.EvaluatePosition(splineCurrent, 1f);
            var posWorldEnd = edge.Container != null
                ? edge.Container.transform.TransformPoint((Vector3)posLocalEnd)
                : (Vector3)posLocalEnd;

            transform.position = new Vector2(posWorldEnd.x, posWorldEnd.y);
            position = City.CityPosition.At(endNode);
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
}
