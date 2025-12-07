using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Warehouse : ILocationsHolder
{
    public WarehouseConfig Config;
    public DimensionalPositionData emptyProductLocation { get; set; }
    public List<WarehouseProductLocation> products = new List<WarehouseProductLocation>();
    public SuppliesList suppliesList;


    public Warehouse(WarehouseConfig config)
    {
        this.Config = config;
    }

    public ILocation GetEmptyLocation()
    {
        return new WarehouseProductLocation(this,emptyProductLocation, null);
    }

    public ILocation[] GetLocations()
    {
        return products.ToArray();
    }

    public class WarehouseProductLocation : ILocation
    {
        public Warehouse Warehouse { get; private set; }
        public Product Product { get; private set; }
        public DimensionalPositionData Position { get; set; }

        public ILocationsHolder Holder => Warehouse;

        public WarehouseProductLocation(Warehouse warehouse, DimensionalPositionData position, Product product)
        {
            Product = product;
            Position = position;
            Warehouse = warehouse;
        }

        public bool Attach(Product product)
        {
            if (Product != null)
                return false;
            Product = product;
            Warehouse.products.Add(this);
            return true;
        }
        public void Detach()
        {
            Product = null;
            Warehouse.products.Remove(this);
        }
    }

    public struct DimensionalPositionData
    {
        public Vector3 LocalPosition;
        public Vector3 LocalRotation;
    }
}


