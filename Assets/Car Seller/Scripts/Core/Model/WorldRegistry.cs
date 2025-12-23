using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Should I also implement it as a system, like with ownership?

public class WorldRegistry
{
    private Dictionary<(Type runtimeType, ScriptableObject config), List<object>> map
        = new();

    public void Register<T>(T instance, ScriptableObject config)
    {
        var key = (typeof(T), config);

        if (!map.TryGetValue(key, out var list))
        {
            list = new List<object>();
            map[key] = list;
        }

        list.Add(instance);
    }

    public IReadOnlyList<T> GetByConfig<T>(ScriptableObject config)
    {
        var key = (typeof(T), config);

        return map.TryGetValue(key, out var list)
            ? list.Cast<T>().ToList()
            : Array.Empty<T>();
    }
}