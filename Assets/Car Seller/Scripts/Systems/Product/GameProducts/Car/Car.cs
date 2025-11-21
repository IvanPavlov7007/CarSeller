using UnityEngine;

public sealed class Car : Product, IProductsHolder
{
    CarRuntimeConfig runtimeConfig;
    public CarPartLocation[] carParts;

    public override string Name => runtimeConfig.Name;

    public IProductLocation[] GetProducts()
    {
        return carParts;
    }

    public override T GetRepresentation<T>(IProductViewBuilder<T> builder)
    {
        return builder.BuildCar(this);
    }

    public class CarPartLocation : IProductLocation
    {
        public CarPartLocation(Car car, PartSlotRuntimeConfig partSlotRuntimeConfig, Product product)
        {
            Car = car;
            PartSlotRuntimeConfig = partSlotRuntimeConfig;
            Product = product;
        }

        public Car Car { get; private set; }
        public PartSlotRuntimeConfig PartSlotRuntimeConfig { get; private set; }
        public Product Product { get; private set; }

        public bool Attach(Product product)
        {
            if(Product != null)
            {
                Debug.LogWarning($"Car {Car.UniqueName}: Cannot attach product { product.Name} to CarPartLocation: already occupied by product {Product.Name}.");
                return false;
            }

            if(PartSlotRuntimeConfig.TryAccept(product) == false)
            {
                Debug.LogWarning($"Car {Car.UniqueName}: Cannot attach product { product.Name} to CarPartLocation: incompatible product for slot type {PartSlotRuntimeConfig.SlotType}.");
                return false;
            }
            Product = product;
            return true;
        }

        public void Detach()
        {
            PartSlotRuntimeConfig.Detach();
            Product = null;
        }
    }
}