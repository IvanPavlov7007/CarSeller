using Sirenix.Windows.Beta;
using System.Collections;
using System.Linq;
using UnityEngine;

public sealed class Car : Product, IProductsHolder
{
    CarRuntimeConfig runtimeConfig;
    public Product[] carParts;
    public Product[] GetProducts()
    {
        return carParts;
    }

    public override Sprite GetIcon()
    {
        return IconBuilder.BuildCarSprite(runtimeConfig);
    }
}