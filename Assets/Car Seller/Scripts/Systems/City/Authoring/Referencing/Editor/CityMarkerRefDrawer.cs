#if UNITY_EDITOR && !ODIN_INSPECTOR
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(CityMarkerRef))]
public class CityMarkerRefDrawer : PropertyDrawer
{
    private readonly float Line = EditorGUIUtility.singleLineHeight;
    private const float Pad = 2f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Graph field (optional) + marker popup
        return Line * 2f + Pad * 3f;
    }

    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(rect, label, property);

        var graphProp = property.FindPropertyRelative("Graph");
        var markerIdProp = property.FindPropertyRelative("MarkerId");

        rect = EditorGUI.IndentedRect(rect);
        var lineRect = new Rect(rect.x, rect.y, rect.width, Line);
        EditorGUI.LabelField(lineRect, label);

        // Optional: let the user override the graph on the field
        lineRect.y += Line + Pad;
        EditorGUI.PropertyField(lineRect, graphProp, new GUIContent("Graph Override"));

        // Resolve final graph using the shared picker logic
        var owner = property.serializedObject.targetObject;
        var temp = new CityMarkerRef
        {
            Graph = graphProp.objectReferenceValue as CityGraphAsset,
            MarkerId = markerIdProp.stringValue
        };

        var graph = CityMarkerPicker.ResolveGraph(temp, owner);

        // Marker popup
        lineRect.y += Line + Pad;

        if (graph == null)
        {
            EditorGUI.HelpBox(lineRect, "Assign a CityGraph (either here or on the owner) to select a marker.", MessageType.Info);
        }
        else
        {
            var options = CityMarkerPicker.BuildMarkerOptions(graph);
            if (options.Count == 0)
            {
                EditorGUI.HelpBox(lineRect, "No markers found in the selected graph.", MessageType.Warning);
            }
            else
            {
                var displayNames = options.Select(o => o.label).ToArray();
                int currentIndex = Mathf.Max(0, options.FindIndex(o => o.id == markerIdProp.stringValue));
                int newIndex = EditorGUI.Popup(lineRect, "Marker", currentIndex, displayNames);
                if (newIndex >= 0 && newIndex < options.Count)
                {
                    markerIdProp.stringValue = options[newIndex].id;
                }
            }
        }

        EditorGUI.EndProperty();
    }
}
#endif