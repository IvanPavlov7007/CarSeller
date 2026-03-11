//Map (Type,Color,Rarity)
//Data base that allows to easily create cars
//Some identificaitons?

using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct SimplifiedCarIdentifier
{
    public CarType Type;
    public CarRarity Rarity;
    public CarColor Color;
    public SimplifiedCarIdentifier(CarType type, CarRarity rarity, CarColor color)
    {
        Type = type;
        Rarity = rarity;
        Color = color;
    }
}

public class SimplifiedCarsManager
{
    SimplifiedCarsCreationBuilder CreationBuilder => G.SimplifiedCarsCreationBuilder;

    public Car CreateCar(SimplifiedCarIdentifier identifier, ILocation location)
    {
        var car = G.ProductManager.CreateCar(CreationBuilder.RuntimeConfigs[identifier], location);
        return car;
    }

    public Car CreateCarHidden(SimplifiedCarIdentifier identifier)
    {
        var car = CreateCar(identifier, World.Instance.HiddenSpace.GetEmptyLocation());
        return car;
    }
}

public interface IReadOnlyDuplicatesIdentifiedCarList<T> : IReadOnlyDictionary<SimplifiedCarIdentifier, List<T>>
{
}

public interface IReadOnlyNonDuplicatesIdentifiedCarList<T> : IReadOnlyDictionary<SimplifiedCarIdentifier, T>
{
}

public sealed class DuplicatesIdentifiedCarList<T> : IDictionary<SimplifiedCarIdentifier, List<T>>, IReadOnlyDuplicatesIdentifiedCarList<T>
{
    private readonly Dictionary<SimplifiedCarIdentifier, List<T>> _inner;

    public DuplicatesIdentifiedCarList()
        : this(new Dictionary<SimplifiedCarIdentifier, List<T>>())
    {
    }

    public DuplicatesIdentifiedCarList(IEqualityComparer<SimplifiedCarIdentifier> comparer)
        : this(new Dictionary<SimplifiedCarIdentifier, List<T>>(comparer))
    {
    }

    public DuplicatesIdentifiedCarList(IDictionary<SimplifiedCarIdentifier, List<T>> dictionary)
        : this(new Dictionary<SimplifiedCarIdentifier, List<T>>(dictionary))
    {
    }

    public DuplicatesIdentifiedCarList(IReadOnlyDictionary<SimplifiedCarIdentifier, List<T>> dictionary)
        : this(new Dictionary<SimplifiedCarIdentifier, List<T>>(dictionary))
    {
    }

    private DuplicatesIdentifiedCarList(Dictionary<SimplifiedCarIdentifier, List<T>> inner)
    {
        _inner = inner;
    }

    public List<T> this[SimplifiedCarIdentifier key]
    {
        get => _inner[key];
        set => _inner[key] = value;
    }

    public ICollection<SimplifiedCarIdentifier> Keys => _inner.Keys;

    IEnumerable<SimplifiedCarIdentifier> IReadOnlyDictionary<SimplifiedCarIdentifier, List<T>>.Keys => _inner.Keys;

    public ICollection<List<T>> Values => _inner.Values;

    IEnumerable<List<T>> IReadOnlyDictionary<SimplifiedCarIdentifier, List<T>>.Values => _inner.Values;

    public int Count => _inner.Count;

    public bool IsReadOnly => ((IDictionary<SimplifiedCarIdentifier, List<T>>)_inner).IsReadOnly;

    public void Add(SimplifiedCarIdentifier key, List<T> value) => _inner.Add(key, value);

    public void Add(KeyValuePair<SimplifiedCarIdentifier, List<T>> item)
        => ((IDictionary<SimplifiedCarIdentifier, List<T>>)_inner).Add(item);

    public void Clear() => _inner.Clear();

    public bool Contains(KeyValuePair<SimplifiedCarIdentifier, List<T>> item)
        => ((IDictionary<SimplifiedCarIdentifier, List<T>>)_inner).Contains(item);

    public bool ContainsKey(SimplifiedCarIdentifier key) => _inner.ContainsKey(key);

    public void CopyTo(KeyValuePair<SimplifiedCarIdentifier, List<T>>[] array, int arrayIndex)
        => ((IDictionary<SimplifiedCarIdentifier, List<T>>)_inner).CopyTo(array, arrayIndex);

    public IEnumerator<KeyValuePair<SimplifiedCarIdentifier, List<T>>> GetEnumerator() => _inner.GetEnumerator();

    public bool Remove(SimplifiedCarIdentifier key) => _inner.Remove(key);

    public bool Remove(KeyValuePair<SimplifiedCarIdentifier, List<T>> item)
        => ((IDictionary<SimplifiedCarIdentifier, List<T>>)_inner).Remove(item);

    public bool TryGetValue(SimplifiedCarIdentifier key, out List<T> value) => _inner.TryGetValue(key, out value);

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_inner).GetEnumerator();
}

public sealed class NonDuplicatesIdentifiedCarList<T> : IDictionary<SimplifiedCarIdentifier, T>, IReadOnlyNonDuplicatesIdentifiedCarList<T>
{
    private readonly Dictionary<SimplifiedCarIdentifier, T> _inner;

    public NonDuplicatesIdentifiedCarList()
        : this(new Dictionary<SimplifiedCarIdentifier, T>())
    {
    }

    public NonDuplicatesIdentifiedCarList(IEqualityComparer<SimplifiedCarIdentifier> comparer)
        : this(new Dictionary<SimplifiedCarIdentifier, T>(comparer))
    {
    }

    public NonDuplicatesIdentifiedCarList(IDictionary<SimplifiedCarIdentifier, T> dictionary)
        : this(new Dictionary<SimplifiedCarIdentifier, T>(dictionary))
    {
    }

    public NonDuplicatesIdentifiedCarList(IReadOnlyDictionary<SimplifiedCarIdentifier, T> dictionary)
        : this(new Dictionary<SimplifiedCarIdentifier, T>(dictionary))
    {
    }

    private NonDuplicatesIdentifiedCarList(Dictionary<SimplifiedCarIdentifier, T> inner)
    {
        _inner = inner;
    }

    public T this[SimplifiedCarIdentifier key]
    {
        get => _inner[key];
        set => _inner[key] = value;
    }

    public ICollection<SimplifiedCarIdentifier> Keys => _inner.Keys;

    IEnumerable<SimplifiedCarIdentifier> IReadOnlyDictionary<SimplifiedCarIdentifier, T>.Keys => _inner.Keys;

    public ICollection<T> Values => _inner.Values;

    IEnumerable<T> IReadOnlyDictionary<SimplifiedCarIdentifier, T>.Values => _inner.Values;

    public int Count => _inner.Count;

    public bool IsReadOnly => ((IDictionary<SimplifiedCarIdentifier, T>)_inner).IsReadOnly;

    public void Add(SimplifiedCarIdentifier key, T value) => _inner.Add(key, value);

    public void Add(KeyValuePair<SimplifiedCarIdentifier, T> item)
        => ((IDictionary<SimplifiedCarIdentifier, T>)_inner).Add(item);

    public void Clear() => _inner.Clear();

    public bool Contains(KeyValuePair<SimplifiedCarIdentifier, T> item)
        => ((IDictionary<SimplifiedCarIdentifier, T>)_inner).Contains(item);

    public bool ContainsKey(SimplifiedCarIdentifier key) => _inner.ContainsKey(key);

    public void CopyTo(KeyValuePair<SimplifiedCarIdentifier, T>[] array, int arrayIndex)
        => ((IDictionary<SimplifiedCarIdentifier, T>)_inner).CopyTo(array, arrayIndex);

    public IEnumerator<KeyValuePair<SimplifiedCarIdentifier, T>> GetEnumerator() => _inner.GetEnumerator();

    public bool Remove(SimplifiedCarIdentifier key) => _inner.Remove(key);

    public bool Remove(KeyValuePair<SimplifiedCarIdentifier, T> item)
        => ((IDictionary<SimplifiedCarIdentifier, T>)_inner).Remove(item);

    public bool TryGetValue(SimplifiedCarIdentifier key, out T value) => _inner.TryGetValue(key, out value);

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_inner).GetEnumerator();
}