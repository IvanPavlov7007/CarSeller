using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[ExecuteAlways]
public sealed class TrafficLightAuthor : MonoBehaviour
{
    [Serializable]
    public sealed class EdgeSlot
    {
        [TableColumnWidth(60, Resizable = false)]
        [ReadOnly] public string Key;

        [ReadOnly] public RoadEdgeAuthor Edge;
    }

    [Serializable]
    public sealed class ProgramStep
    {
        [MinValue(0.1f)]
        public float DurationSeconds = 5f;

        [LabelText("Go Edges")]
        [ValueDropdown("@$root.GetEdgeKeys()")]
        [ListDrawerSettings(ShowFoldout = false)]
        public List<string> GoEdgeKeys = new List<string>();
    }

    [SerializeField, ReadOnly] private string id;

    [ShowInInspector, ReadOnly]
    public string Id => id;

    [BoxGroup("Anchor")]
    [OnValueChanged(nameof(OnNodeChanged))]
    public RoadNodeAuthor Node;

    [BoxGroup("Timing")]
    [MinValue(0f)]
    public float PreparationTimeSeconds = 0.75f;

    [BoxGroup("Anchor")]
    [Button, DisableInPlayMode]
    [EnableIf(nameof(CanSnapToNode))]
    private void SnapToNode()
    {
        transform.position = Node.transform.position;
    }

    [BoxGroup("Edges")]
    [Button("Refresh Connected Edges"), DisableInPlayMode]
    [EnableIf(nameof(HasNode))]
    private void RefreshConnectedEdgesButton()
    {
        RefreshConnectedEdges();
    }

    [BoxGroup("Edges")]
    [TableList(IsReadOnly = true, AlwaysExpanded = true)]
    [SerializeField]
    private List<EdgeSlot> edgeSlots = new List<EdgeSlot>();

    [BoxGroup("Program")]
    [ListDrawerSettings(ShowFoldout = false)]
    public List<ProgramStep> Program = new List<ProgramStep>();

    private bool HasNode => Node != null;

    private bool CanSnapToNode => Node != null;

    public IReadOnlyList<EdgeSlot> EdgeSlots => edgeSlots;

    private void OnEnable()
    {
        if (!Application.isPlaying)
        {
            EnsureId();
            RefreshConnectedEdges();
        }
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            EnsureId();
            RefreshConnectedEdges();
        }
    }

    public void EnsureId()
    {
        if (string.IsNullOrEmpty(id))
            id = Guid.NewGuid().ToString("N");
    }

#if UNITY_EDITOR
    private void EnsureUniqueIdInScene()
    {
        var all = FindObjectsOfType<TrafficLightAuthor>(true);
        if (all.Count(x => x != this && x.id == id) > 0)
            id = Guid.NewGuid().ToString("N");
    }
#endif

    private void OnNodeChanged()
    {
        RefreshConnectedEdges();
    }

    private void RefreshConnectedEdges()
    {
        if (Node == null)
        {
            edgeSlots.Clear();
            return;
        }

        var root = transform.root;
        if (root == null)
            return;

        var allEdges = root.GetComponentsInChildren<RoadEdgeAuthor>(true);

        var connected = new List<RoadEdgeAuthor>();
        for (int i = 0; i < allEdges.Length; i++)
        {
            var e = allEdges[i];
            if (e == null)
                continue;

            if (e.From == Node || e.To == Node)
                connected.Add(e);
        }

        connected.Sort((a, b) =>
        {
            float aa = GetSignedAngleAroundNode(a);
            float bb = GetSignedAngleAroundNode(b);
            return aa.CompareTo(bb);
        });

        var existingKeyByEdgeId = new Dictionary<string, string>();
        for (int i = 0; i < edgeSlots.Count; i++)
        {
            var slot = edgeSlots[i];
            if (slot?.Edge == null)
                continue;

            var edgeId = slot.Edge.Id;
            if (string.IsNullOrEmpty(edgeId))
                continue;

            if (!existingKeyByEdgeId.ContainsKey(edgeId) && !string.IsNullOrEmpty(slot.Key))
                existingKeyByEdgeId.Add(edgeId, slot.Key);
        }

        var usedKeys = new HashSet<string>(existingKeyByEdgeId.Values);
        var nextKeyIndex = 0;

        string AllocateNextKey()
        {
            while (true)
            {
                var candidate = IndexToKey(nextKeyIndex++);
                if (usedKeys.Add(candidate))
                    return candidate;
            }
        }

        var nextSlots = new List<EdgeSlot>(connected.Count);
        for (int i = 0; i < connected.Count; i++)
        {
            var e = connected[i];
            var edgeId = e != null ? e.Id : null;

            string key = null;
            if (!string.IsNullOrEmpty(edgeId) && existingKeyByEdgeId.TryGetValue(edgeId, out var preserved))
                key = preserved;

            key ??= AllocateNextKey();

            nextSlots.Add(new EdgeSlot
            {
                Key = key,
                Edge = e
            });
        }

        edgeSlots = nextSlots;

        var validKeys = new HashSet<string>(edgeSlots.Select(s => s.Key));
        for (int i = 0; i < Program.Count; i++)
        {
            var step = Program[i];
            if (step?.GoEdgeKeys == null)
                continue;

            step.GoEdgeKeys.RemoveAll(k => string.IsNullOrEmpty(k) || !validKeys.Contains(k));
        }
    }

    private float GetSignedAngleAroundNode(RoadEdgeAuthor edge)
    {
        if (Node == null || edge == null || edge.From == null || edge.To == null)
            return 0f;

        var nodePos = (Vector2)Node.transform.position;
        var other = edge.From == Node ? edge.To : edge.From;
        var otherPos = (Vector2)other.transform.position;

        var dir = (otherPos - nodePos).normalized;
        return Mathf.Atan2(dir.y, dir.x);
    }

    private static string IndexToKey(int index)
    {
        const int alphabet = 26;
        index = Mathf.Max(0, index);

        var s = string.Empty;
        do
        {
            int r = index % alphabet;
            s = (char)('a' + r) + s;
            index = (index / alphabet) - 1;
        } while (index >= 0);

        return s;
    }

    public IEnumerable<string> GetEdgeKeys()
    {
        if (edgeSlots == null)
            yield break;

        for (int i = 0; i < edgeSlots.Count; i++)
        {
            var k = edgeSlots[i]?.Key;
            if (!string.IsNullOrEmpty(k))
                yield return k;
        }
    }
}