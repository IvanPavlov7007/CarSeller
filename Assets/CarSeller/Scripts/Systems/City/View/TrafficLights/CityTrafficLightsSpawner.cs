using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CityTrafficLightsSpawner
{
    private const string TrafficLightsRootName = "TrafficLights";

    public static void Spawn(City city, CityGraphAsset graph, Transform cityRoot)
    {
        if (city == null || graph == null || cityRoot == null)
            return;

        if (graph.TrafficLights == null || graph.TrafficLights.Count == 0)
            return;

        if (graph.TrafficLightPrefab == null)
        {
            Debug.LogWarning("CityTrafficLightsSpawner: CityGraphAsset.TrafficLightPrefab is null.");
            return;
        }

        var existing = cityRoot.Find(TrafficLightsRootName);
        if (existing != null)
        {
            Object.Destroy(existing.gameObject);
        }

        var root = new GameObject(TrafficLightsRootName);
        root.transform.SetParent(cityRoot, false);

        var nodeById = city.Nodes.Where(n => n != null && !string.IsNullOrEmpty(n.Id)).ToDictionary(n => n.Id);
        var edgeById = city.Edges.Where(e => e != null && !string.IsNullOrEmpty(e.Id)).ToDictionary(e => e.Id);

        foreach (var tl in graph.TrafficLights)
        {
            if (tl == null || string.IsNullOrEmpty(tl.NodeId))
                continue;

            if (!nodeById.TryGetValue(tl.NodeId, out var node))
                continue;

            var go = Object.Instantiate(graph.TrafficLightPrefab, root.transform);
            go.name = string.IsNullOrEmpty(tl.Id) ? "TrafficLight" : $"TrafficLight_{tl.Id}";
            go.transform.position = node.Position;

            var view = go.GetComponent<TrafficLightViewController>();
            if (view == null)
            {
                Debug.LogWarning($"CityTrafficLightsSpawner: '{go.name}' prefab is missing TrafficLightViewController.");
                continue;
            }

            var directionsByKey = new Dictionary<string, Vector2>();

            if (tl.EdgeSlots != null)
            {
                for (int i = 0; i < tl.EdgeSlots.Count; i++)
                {
                    var slot = tl.EdgeSlots[i];
                    if (slot == null || string.IsNullOrEmpty(slot.Key) || string.IsNullOrEmpty(slot.EdgeId))
                        continue;

                    if (!edgeById.TryGetValue(slot.EdgeId, out var edge) || edge == null)
                        continue;

                    Vector2 dir;

                    try
                    {
                        // Tangent leaving the node along this edge (world XY).
                        dir = edge.GetTangentFromNode(node, 0.05f, out _);
                        if (dir.sqrMagnitude <= 0f)
                            throw new System.Exception("Zero tangent.");
                    }
                    catch
                    {
                        // Fallback: direction to the other endpoint.
                        var other = edge.From == node ? edge.To : edge.From;
                        dir = other != null ? (other.Position - node.Position).normalized : Vector2.up;
                    }

                    directionsByKey[slot.Key] = dir;
                }
            }

            view.InstantiateLights(directionsByKey);

            var runtime = go.GetComponent<TrafficLightRuntimeController>();
            if (runtime == null)
                runtime = go.AddComponent<TrafficLightRuntimeController>();

            runtime.Initialize(view, directionsByKey.Keys, tl.Program, tl.PreparationTimeSeconds);
        }
    }
}