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

    [SerializeField, ReadOnly] private string id;
    [ShowInInspector, ReadOnly] public string Id => id;

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

        Vector3 a = From.transform.position;
        Vector3 b = To.transform.position;
        Vector3 dir = (b - a);
        Vector3 forward = dir.sqrMagnitude > 0f ? dir.normalized : Vector3.right;
        float handleLen = 1.0f; // tweak curvature as needed

        var knotA = new BezierKnot(
            a,
            tangentIn: a - forward * handleLen,
            tangentOut: a + forward * handleLen,
            rotation: Quaternion.identity);

        var knotB = new BezierKnot(
            b,
            tangentIn: b - forward * handleLen,   // points back toward A
            tangentOut: b + forward * handleLen,  // points forward past B
            rotation: Quaternion.identity);

        spline.Add(knotA);
        spline.Add(knotB);
    }

    [Button("Add Control Knot Midpoint"), DisableInPlayMode]
    public void AddMidKnot()
    {
        if (Container == null) { Debug.LogWarning("Container missing."); return; }
        var spline = Container.Splines.Count > 0 ? Container.Splines[0] : Container.AddSpline();
        if (spline.Count < 2) { Debug.LogWarning("Need at least two knots."); return; }

        Vector3 a = spline[0].Position;
        Vector3 b = spline[spline.Count - 1].Position;
        Vector3 mid = Vector3.Lerp(a, b, 0.5f);
        float handleLen = 0.5f;
        Vector3 forward = (b - a).sqrMagnitude > 0f ? (b - a).normalized : Vector3.right;

        var midKnot = new BezierKnot(
            mid,
            tangentIn: mid - forward * handleLen,
            tangentOut: mid + forward * handleLen,
            rotation: Quaternion.identity);

        spline.Insert(1, midKnot);
    }

    [Button("Reverse Direction"), DisableInPlayMode]
    public void ReverseDirection()
    {
        if (Container == null) return;
        var spline = Container.Splines.Count > 0 ? Container.Splines[0] : null;
        if (spline == null || spline.Count < 2) return;

        // Manual reverse: flip knot order and swap tangents
        var reversed = new BezierKnot[spline.Count];
        int last = spline.Count - 1;
        for (int i = 0; i < spline.Count; i++)
        {
            var k = spline[i];
            // When reversing direction, in/out tangents swap
            var rk = new BezierKnot(
                position: k.Position,
                tangentIn: k.TangentOut,
                tangentOut: k.TangentIn,
                rotation: k.Rotation);
            reversed[last - i] = rk;
        }

        spline.Clear();
        for (int i = 0; i < reversed.Length; i++)
            spline.Add(reversed[i]);

        // Swap endpoints for semantic consistency
        (From, To) = (To, From);
    }

    private void OnValidate()
    {
        if (!Application.isPlaying) EnsureId();
    }
}