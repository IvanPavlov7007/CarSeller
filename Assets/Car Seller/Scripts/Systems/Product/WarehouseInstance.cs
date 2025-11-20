using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class WarehouseInstance : IProductsHolder
{
    public Dictionary<Product, Vector3> productsWithLocations;

    public Product[] GetProducts()
    {
        return new List<Product>(productsWithLocations.Keys).ToArray();
    }
}

