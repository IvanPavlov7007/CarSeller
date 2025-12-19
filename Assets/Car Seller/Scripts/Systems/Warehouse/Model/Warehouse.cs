using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Warehouse : ILocationsHolder, ILocatable, IPossession
{
    public WarehouseConfig Config;
    public DimensionalPositionData emptyProductLocation { get; set; }

    private readonly Guid _id = Guid.NewGuid();
    public Guid Id => _id;
    public string Name { get; }

    public List<WarehouseProductLocation> products = new List<WarehouseProductLocation>();
    public SuppliesList suppliesList;

    public int AvailableCarParkingSpots { get; private set; }
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
        public ILocatable Occupant => Product;

        public WarehouseProductLocation(Warehouse warehouse, DimensionalPositionData position, Product product)
        {
            Product = product;
            Position = position;
            Warehouse = warehouse;
        }

        public bool Attach(ILocatable locatable)
        {
            var product = locatable as Product;
            if(product == null)
                return false;
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


