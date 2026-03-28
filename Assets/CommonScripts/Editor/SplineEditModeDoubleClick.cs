using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.Splines;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UIElements;

namespace Rowlan.SplineTools
{
    [InitializeOnLoad]
    static class SplineEditModeDoubleClick
    {
        static int lastClickInstanceId;
        static double lastClickTime;

        const double DoubleClickTime = 0.3; // seconds

        static readonly HashSet<int> hookedHierarchyWindows = new HashSet<int>();

        static SplineEditModeDoubleClick()
        {
            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyWindowItemOnGUI;
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;

            EditorApplication.update -= EnsureUiToolkitHooked;
            EditorApplication.update += EnsureUiToolkitHooked;

            // Try immediately too (first update can be delayed)
            EditorApplication.delayCall += EnsureUiToolkitHooked;
        }

        static void OnHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            Event evt = Event.current;
            if (evt == null)
                return;

            if (evt.type != EventType.MouseDown || evt.button != 0)
                return;

            Rect rowRect = selectionRect;
            rowRect.xMin = 0f;
            rowRect.xMax = EditorGUIUtility.currentViewWidth;

            if (!rowRect.Contains(evt.mousePosition))
                return;

            double now = EditorApplication.timeSinceStartup;
            bool isDoubleClick = instanceID == lastClickInstanceId
                && (now - lastClickTime) <= DoubleClickTime;

            lastClickInstanceId = instanceID;
            lastClickTime = now;

            if (!isDoubleClick)
                return;

            if (!(EditorUtility.InstanceIDToObject(instanceID) is GameObject clickedGo))
                return;

            if (!TryGetSplineContainerOwner(clickedGo, out GameObject splineOwnerGo))
                return;

            evt.Use();

            Selection.activeGameObject = splineOwnerGo;

            EditorApplication.delayCall += () => ActivateSplineContextAndFrame();
        }

        static void EnsureUiToolkitHooked()
        {
            System.Type sceneHierarchyWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
            if (sceneHierarchyWindowType == null)
                return;

            Object[] windows = Resources.FindObjectsOfTypeAll(sceneHierarchyWindowType);
            if (windows == null || windows.Length == 0)
                return;

            for (int i = 0; i < windows.Length; i++)
            {
                if (!(windows[i] is EditorWindow window))
                    continue;

                int id = window.GetInstanceID();
                if (!hookedHierarchyWindows.Add(id))
                    continue;

                window.rootVisualElement.RegisterCallback<MouseDownEvent>(e => OnHierarchyMouseDown(window, e), TrickleDown.TrickleDown);
            }
        }

        static void OnHierarchyMouseDown(EditorWindow window, MouseDownEvent evt)
        {
            if (evt.button != 0 || evt.clickCount != 2)
                return;

            // Only handle when the hierarchy window is actually focused.
            if (EditorWindow.focusedWindow != window)
                return;

            // Stop Unity's default double click behavior (open prefab / rename).
            evt.StopImmediatePropagation();
            evt.PreventDefault();

            // Selection is usually set by the first click; ensure it's processed before switching tool context.
            EditorApplication.delayCall += () =>
            {
                GameObject selected = Selection.activeGameObject;
                if (selected == null)
                    return;

                if (!TryGetSplineContainerOwner(selected, out GameObject splineOwnerGo))
                    return;

                Selection.activeGameObject = splineOwnerGo;
                ActivateSplineContextAndFrame();
            };
        }

        static void ActivateSplineContextAndFrame()
        {
            SceneView.FocusWindowIfItsOpen(typeof(SceneView));
            SceneView.lastActiveSceneView?.FrameSelected();

            if (ToolManager.activeContextType != typeof(SplineToolContext))
                ToolManager.SetActiveContext<SplineToolContext>();
        }

        static bool TryGetSplineContainerOwner(GameObject start, out GameObject owner)
        {
            Transform t = start.transform;
            while (t != null)
            {
                GameObject candidate = t.gameObject;

                if (candidate.TryGetComponent(out SplineContainer _))
                {
                    owner = candidate;
                    return true;
                }

                Component[] components = candidate.GetComponents<Component>();
                for (int i = 0; i < components.Length; i++)
                {
                    if (components[i] is ISplineContainer)
                    {
                        owner = candidate;
                        return true;
                    }
                }

                t = t.parent;
            }

            owner = null;
            return false;
        }
    }
}