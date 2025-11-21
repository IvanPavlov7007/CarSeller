using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Warehouse : IProductsHolder
{
    public Vector2 emptyProductLocation;
    public List<WarehouseProductLocation> products;
    public SuppliesList suppliesList;

    public IProductLocation GetEmptyLocation()
    {
        return new WarehouseProductLocation(this,new Vector3(emptyProductLocation.x, 0, emptyProductLocation.y), null);
    }

    public IProductLocation[] GetProducts()
    {
        return products.ToArray();
    }

    public class WarehouseProductLocation : IProductLocation
    {
        public Warehouse Warehouse { get; private set; }
        public Product Product { get; private set; }
        public Vector3 Position { get; private set; }

        public WarehouseProductLocation(Warehouse warehouse, Vector3 position, Product product)
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
}

