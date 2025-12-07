using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Splines;

[ExecuteAlways]
public class RoadNodeAuthor : MonoBehaviour
{
    [SerializeField, ReadOnly] private string id;
    [ShowInInspector, ReadOnly] public string Id => id;

    [SerializeField] public bool StartLooksAway = false;

    [Button, DisableInPlayMode]
    public void EnsureId()
    {
        if (string.IsNullOrEmpty(id))
            id = System.Guid.NewGuid().ToString("N");
    }

#if UNITY_EDITOR
    private void EnsureUniqueIdInScene()
    {
        if (string.IsNullOrEmpty(id)) return;

        var all = UnityEngine.Object.FindObjectsOfType<RoadNodeAuthor>(true);
        foreach (var other in all)
        {
            if (other == this) continue;
            if (other.Id == id)
            {
                // Duplicate detected -> assign a new GUID to this instance
                id = System.Guid.NewGuid().ToString("N");
                UnityEditor.EditorUtility.SetDirty(this);
                break;
            }
        }
    }
#endif

    [Button("Connect To Selected Node"), DisableInPlayMode]
    public void ConnectToSelected()
    {
#if UNITY_EDITOR
        var sel = UnityEditor.Selection.activeGameObject;
        if (sel == null) { Debug.Log("No GameObject selected."); return; }
        var other = sel.GetComponent<RoadNodeAuthor>();
        if (other == null) { Debug.Log("Selected object is not a RoadNodeAuthor."); return; }
        if (other == this) { Debug.Log("Select a different node."); return; }

        EnsureId(); other.EnsureId();
        EnsureUniqueIdInScene();
        other.EnsureUniqueIdInScene();

        var edgeGO = new GameObject($"Edge_{Id}_to_{other.Id}");
        edgeGO.transform.SetParent(transform.parent, false);

        var edge = edgeGO.AddComponent<RoadEdgeAuthor>();
        edge.From = this;
        edge.To = other;
        edge.Bidirectional = true;
        edge.StartLooksAway = StartLooksAway;
        edge.EnsureId();

        var container = edgeGO.AddComponent<SplineContainer>();
        edge.Container = container;
        edge.SplineIndex = 0;

        var spline = container.Splines.Count > 0 ? container.Splines[0] : container.AddSpline();
        spline.Clear();

        Vector3 a = container.transform.InverseTransformPoint(transform.position);
        Vector3 b = container.transform.InverseTransformPoint(other.transform.position);
        a.z = 0f; b.z = 0f;

        float handleLen = 1.0f;
        Vector3 outZ = new Vector3(0f, 0f, Mathf.Abs(handleLen));
        Vector3 inZ  = new Vector3(0f, 0f, -Mathf.Abs(handleLen));

        Quaternion rotStart = StartLooksAway
            ? Quaternion.Euler(0f, 90f, 270f)
            : Quaternion.Euler(0f, 270f, 90f);

        Quaternion rotEnd = Quaternion.Euler(0f, 270f, 90f);

        var knotA = new BezierKnot(a, Vector3.zero, outZ, rotStart);
        var knotB = new BezierKnot(b, inZ, Vector3.zero, rotEnd);

        spline.Add(knotA);
        spline.Add(knotB);

        var splineExtrude = edgeGO.AddComponent<SplineExtrude>();
        splineExtrude.Container = container;

        UnityEditor.Undo.RegisterCreatedObjectUndo(edgeGO, "Create Road Edge");
        UnityEditor.Selection.activeGameObject = edgeGO;
#endif
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            EnsureId();
#if UNITY_EDITOR
            EnsureUniqueIdInScene();
#endif
        }
    }
}