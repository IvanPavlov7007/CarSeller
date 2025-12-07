using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.GPUSort;

/// <summary>
/// Car view in the warehouse on a change, view partially updates itself based on what changed
/// </summary>
public class WarehouseCarView : WarehouseProductView
{
    public List<CarPartView> parts;
    public Car car { get; private set; }


    protected override void OnEnable()
    {
        base.OnEnable();
        GameEvents.Instance.OnProductLocationChanged += onAnyProductLocationChanged;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        GameEvents.Instance.OnProductLocationChanged -= onAnyProductLocationChanged;
    }

    public override void Initialize(Product product, ILocation representedProductLocation)
    {
        Debug.Assert(product is Car, "CarView can represent only a Car");
        base.Initialize(product, representedProductLocation);
        car = (Car)product;
    }

    void onAnyProductLocationChanged(ProductLocationChangedEventData data)
    {
        if(data.OldLocation?.Holder == car)
        {
            // Handle removal from old location if needed
        }

        if (data.NewLocation?.Holder == car)
        {
            CarPartViewPlacementHelper.BuildCarPartAtPosition(data.NewLocation as Car.CarPartLocation, transform, G.Instance.carPartViewBuilder);
        }
    }

}