using System.Collections.Generic;
using UnityEngine;

public class CarFlexibleJunctionPolicy : IJunctionPolicy
{
    public readonly CityEntity Entity;
    public readonly Car Car;

    public CarFlexibleJunctionPolicy(CityEntity entity)
    {
        Entity = entity;
        Car = entity.Subject as Car;
    }

    public IEnumerable<RoadEdge> GetAllowedOutgoing(RoadNode atNode)
    {
        bool canNarrow = Car.HasModifier<CanNarrowStreet>();
        foreach (var edge in atNode.Outgoing)
        {
            if (canNarrow || !edge.HasTag("secondary"))
            {
                yield return edge;
            }
        }
    }
}