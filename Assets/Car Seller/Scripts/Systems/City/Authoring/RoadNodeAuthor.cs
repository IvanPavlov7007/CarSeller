using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Splines;

[ExecuteAlways]
public class RoadNodeAuthor : MonoBehaviour
{
    [SerializeField, ReadOnly] private string id;
    [ShowInInspector, ReadOnly] public string Id => id;

    [Button, DisableInPlayMode]
    public void EnsureId()
    {
        if (string.IsNullOrEmpty(id))
            id = System.Guid.NewGuid().ToString("N");
    }

    [Button("Connect To Selected Node"), DisableInPlayMode]
    public void ConnectToSelected()
    {
#if UNITY_EDITOR
        var sel = UnityEditor.Selection.activeGameObject;
        if (sel == null) { Debug.LogWarning("No GameObject selected."); return; }
        var other = sel.GetComponent<RoadNodeAuthor>();
        if (other == null) { Debug.LogWarning("Selected object is not a RoadNodeAuthor."); return; }
        if (other == this) { Debug.LogWarning("Select a different node."); return; }

        EnsureId(); other.EnsureId();

        // Create edge object with a SplineContainer
        var edgeGO = new GameObject($"Edge_{Id}_to_{other.Id}");
        edgeGO.transform.SetParent(transform.parent, false);

        var edge = edgeGO.AddComponent<RoadEdgeAuthor>();
        edge.From = this;
        edge.To = other;
        edge.Bidirectional = true;
        edge.EnsureId();

        var container = edgeGO.AddComponent<SplineContainer>();
        edge.Container = container;
        edge.SplineIndex = 0;

        // Seed spline with 2 BezierKnots between nodes
        var spline = container.Splines.Count > 0 ? container.Splines[0] : container.AddSpline();
        spline.Clear();

        Vector3 a = transform.position;
        Vector3 b = other.transform.position;
        Vector3 dir = (b - a);
        Vector3 forward = dir.sqrMagnitude > 0f ? dir.normalized : Vector3.right;
        float handleLen = 1.0f; // tweak if needed

        // BezierKnot expects: position, tangentIn, tangentOut, rotation
        var knotA = new BezierKnot(
            a,
            tangentIn: a - forward * handleLen,   // points backward from A
            tangentOut: a + forward * handleLen,  // points forward from A
            rotation: Quaternion.identity);

        var knotB = new BezierKnot(
            b,
            tangentIn: b - forward * handleLen,   // points backward from B (towards A)
            tangentOut: b + forward * handleLen,  // points forward from B
            rotation: Quaternion.identity);

        spline.Add(knotA);
        spline.Add(knotB);

        UnityEditor.Undo.RegisterCreatedObjectUndo(edgeGO, "Create Road Edge");
        UnityEditor.Selection.activeGameObject = edgeGO;
#endif
    }

    private void OnValidate()
    {
        if (!Application.isPlaying) EnsureId();
    }
}