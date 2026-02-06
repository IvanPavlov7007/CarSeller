using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface ICityPositionable
{
    CityPosition Position { get; set; }
}

/// <summary>
/// 
/// 1 to 1 relationship between a locatable entity and its position in the city
/// Should live as long as having an attached locatable
/// 
/// </summary>
public sealed class CityEntity : ILocation, ICityPositionable
{

    public CityPosition Position { get; set; }
    public ILocatable Subject;
    public IReadOnlyCollection<CityEntityAspect> Aspects => aspects;

    HashSet<CityEntityAspect> aspects = new HashSet<CityEntityAspect>();

    City City;

    public T[] GetAspects<T>() where T : CityEntityAspect
    {
        return aspects.OfType<T>().ToArray();
    }

    internal CityEntity(City city, ILocatable subject, CityPosition initialCityPosition, ICollection<CityEntityAspect> aspects)
    {
        City = city;
        Position = initialCityPosition;
        Attach(subject);
        AddAspects(aspects);
    }

    internal CityEntity(City city, CityPosition initialCityPosition, ICollection<CityEntityAspect> aspects)
    {
        City = city;
        Position = initialCityPosition;
        AddAspects(aspects);
    }

    private void AddAspects(ICollection<CityEntityAspect> aspectsToAdd)
    {
        foreach (var aspect in aspectsToAdd)
        {
            aspects.Add(aspect);
        }
    }

    // ILocation implementation
    // Don't use from outside except for CityLifetimeService
    public ILocatable Occupant => Subject;
    public ILocationsHolder Holder => City;
    public bool Attach(ILocatable locatable)
    {
        Debug.Assert(locatable != null, "Locatable to attach cannot be null");
        if (Subject != null || locatable == null) return false;

        Subject = locatable;
        City.Entities[locatable] = this;

        return true;
    }
    public void Detach()
    {
        if (Subject == null) return;
        City.Entities.Remove(Subject);
        Subject = null;
    }
}