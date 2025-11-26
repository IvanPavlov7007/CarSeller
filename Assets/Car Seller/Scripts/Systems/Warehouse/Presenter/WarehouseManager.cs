using Pixelplacement;
using UnityEngine;


// Loads and manages warehouse in the warehouse scene
public class WarehouseManager : Singleton<WarehouseManager>
{
    public static Warehouse Warehouse { get; private set; }
    public WarehouseProductViewBuilder productViewBuilder;

    private void Awake()
    {
        initialiseWarehouse();
    }

    void initialiseWarehouse()
    {
        foreach(var location in Warehouse.products)
        {
            if (location.Product != null)
            {
                //var productObject = Instantiate(location.Product.Prefab, location.Position, Quaternion.identity);
                //productObject.GetComponent<ProductBehaviour>().Initialise(location.Product, location);
            }
        }
    }

    void newProductCreated()
    {

    }
}
