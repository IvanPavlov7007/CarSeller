using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class AspectsViewBuilder
{
    public Dictionary<CityEntity, CityViewObjectController> cityViewObjects;

    public void buildPinAspect(CityEntity entity, PinAspect aspect)
    {
        var view = cityViewObjects[entity];

    }

}
