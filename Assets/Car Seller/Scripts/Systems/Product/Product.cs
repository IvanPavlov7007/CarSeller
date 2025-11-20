using System.Collections;
using UnityEngine;

/// <summary>
/// An data representation of a product instance in the game
/// </summary>
public abstract class Product : IProductIconProvider
{
    public abstract Sprite GetIcon();
}

public interface IProductIconProvider
{
    public Sprite GetIcon();
}

public interface IProductsHolder
{
    public Product[] GetProducts();
}