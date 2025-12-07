using UnityEngine;
using UnityEngine.Splines;
using Sirenix.OdinInspector;

[ExecuteAlways]
public class RoadEdgeAuthor : MonoBehaviour
{
    [BoxGroup("Endpoints")] public RoadNodeAuthor From;
    [BoxGroup("Endpoints")] public RoadNodeAuthor To;

    [BoxGroup("Spline")] public SplineContainer Container;
    [BoxGroup("Spline"), ReadOnly] public int SplineIndex = 0; // one-spline-per-edge by convention
    [BoxGroup("Options")] public bool Bidirectional = true;
    [BoxGroup("Options")] public bool ClampToXY = true;

    [SerializeField, ReadOnly] private string id;
    [ShowInInspector, ReadOnly] public string Id => id;

    // Optional: let author flip the start look direction
    [BoxGroup("Options")] public bool StartLooksAway = false;

    [Button, DisableInPlayMode]
    public void EnsureId()
    {
        if (string.IsNullOrEmpty(id))
            id = System.Guid.NewGuid().ToString("N");
    }

    [Button("Align Spline To Nodes"), DisableInPlayMode]
    public void AlignToNodes()
    {
        if (From == null || To == null || Container == null)
        {
            Debug.LogWarning("From/To/Container must be set.");
            return;
        }

        var spline = Container.Splines.Count > 0 ? Container.Splines[0] : Container.AddSpline();
        spline.Clear();

        // Positions in container-local (planar XY)
        Vector3 a = Container.transform.InverseTransformPoint(From.transform.position);
        Vector3 b = Container.transform.InverseTransformPoint(To.transform.position);
        if (ClampToXY) { a.z = 0f; b.z = 0f; }

        // Handle length strictly on Z axis per your rule
        float handleLen = 1.0f;
        Vector3 outZ = new Vector3(0f, 0f, Mathf.Abs(handleLen));    // Z > 0
        Vector3 inZ  = new Vector3(0f, 0f, -Mathf.Abs(handleLen));   // Z < 0

        // Rotations per your rule:
        // Beginning node:
        //  - looking towards next (default): (x, 270, 90)
        //  - looking away (if flipped):      (x, 90, 270)
        Quaternion rotStart = StartLooksAway
            ? Quaternion.Euler(0f, 90f, 270f)
            : Quaternion.Euler(0f, 270f, 90f);

        // Ending node: looking away from previous node by default: (x, 270, 90)
        Quaternion rotEnd = Quaternion.Euler(0f, 270f, 90f);

        // BezierKnot expects tangents as local offsets
        var knotA = new BezierKnot(
            position: a,
            tangentIn: Vector3.zero,     // you can give it a small -Z if you want symmetric handles
            tangentOut: outZ,            // only Z > 0
            rotation: rotStart);

        var knotB = new BezierKnot(
            position: b,
            tangentIn: inZ,              // only Z < 0
            tangentOut: Vector3.zero,    // you can set +Z if you want symmetric end handles
            rotation: rotEnd);

        spline.Add(knotA);
        spline.Add(knotB);
    }

    [Button("Add Control Knot Midpoint"), DisableInPlayMode]
    public void AddMidKnot()
    {
        if (Container == null) { Debug.LogWarning("Container missing."); return; }
        var spline = Container.Splines.Count > 0 ? Container.Splines[0] : Container.AddSpline();
        if (spline.Count < 2) { Debug.LogWarning("Need at least two knots."); return; }

        var a = spline[0].Position;
        var b = spline[spline.Count - 1].Position;
        if (ClampToXY) { a.z = 0f; b.z = 0f; }
        var mid = Vector3.Lerp(a, b, 0.5f); if (ClampToXY) mid.z = 0f;

        float handleLen = 0.5f;
        Vector3 outZ = new Vector3(0f, 0f, Mathf.Abs(handleLen));
        Vector3 inZ  = new Vector3(0f, 0f, -Mathf.Abs(handleLen));

        // Mid knot can face same as start (stable) or compute based on (b-a). Here we pick start’s facing rule.
        Quaternion rotMid = StartLooksAway
            ? Quaternion.Euler(0f, 90f, 270f)
            : Quaternion.Euler(0f, 270f, 90f);

        var midKnot = new BezierKnot(mid, inZ, outZ, rotMid);
        spline.Insert(1, midKnot);
    }

    [Button("Reverse Direction"), DisableInPlayMode]
    public void ReverseDirection()
    {
        if (Container == null) return;
        var spline = Container.Splines.Count > 0 ? Container.Splines[0] : null;
        if (spline == null || spline.Count < 2) return;

        // Reverse: flip knot order and swap+negate tangents (offset semantics)
        int count = spline.Count;
        var reversed = new BezierKnot[count];
        for (int i = 0; i < count; i++)
        {
            var k = spline[i];
            var rk = new BezierKnot(
                position: k.Position,
                tangentIn: -k.TangentOut,
                tangentOut: -k.TangentIn,
                rotation: k.Rotation // keep rotation; authored facing remains
            );
            reversed[count - 1 - i] = rk;
        }

        spline.Clear();
        for (int i = 0; i < reversed.Length; i++) spline.Add(reversed[i]);

        (From, To) = (To, From);
        // Optionally flip the authored start facing toggle
        StartLooksAway = !StartLooksAway;
    }

    private void OnValidate()
    {
        if (!Application.isPlaying) EnsureId();
    }
}