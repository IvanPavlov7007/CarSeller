using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class Car : Product, IProductsHolder
{
    public CarRuntimeConfig runtimeConfig { get; private set; }
    public Dictionary<CarPartLocation, PartSlotRuntimeConfig> carParts { get; private set; }
    private FrameProductLocation FrameLocation;

    public CarFrame CarFrame { get; private set; }

    public override string Name => runtimeConfig.Name;

    public Car(CarRuntimeConfig runtimeConfig)
    {
        this.runtimeConfig = runtimeConfig;
    }

    public void SetCarFrame(CarFrame carFrame)
    {
        Debug.Assert(CarFrame == null, $"Car {UniqueName}: Car frame has already been set.");
        CarFrame = carFrame;
        FrameLocation = new FrameProductLocation(this, carFrame);
    }

    public void addSlots(Dictionary<CarPartLocation, PartSlotRuntimeConfig> slots)
    {
        Debug.Assert(carParts == null, $"Car {UniqueName}: Car parts have already been set.");
        carParts = slots;
    }

    public CarPartLocation AvailableSlot(Product product)
    {
        foreach (var slotLocation in carParts.Keys)
        {
            if (slotLocation.CanAccept(product))
            {
                return slotLocation;
            }
        }
        return null;
    }

    public bool IsComplete()
    {
        Debug.Assert(carParts != null, $"Car {UniqueName}: Car parts have not been set.");
        foreach (var slotLocation in carParts.Keys)
        {
            if (slotLocation.PartSlotRuntimeConfig.partSlotData.Required && slotLocation.Product == null)
            {
                return false;
            }
        }
        return true;
    }

    public IProductLocation[] GetProductLocations()
    {
        Debug.Assert(carParts != null, $"Car {UniqueName}: Car parts have not been set.");
        Debug.Assert(CarFrame != null, $"Car {UniqueName}: Car frame has not been set.");

        return carParts.Keys.Cast<IProductLocation>().Concat(new[] { FrameLocation }).ToArray();
    }

    public override T GetRepresentation<T>(IProductViewBuilder<T> builder)
    {
        return builder.BuildCar(this);
    }

    public class FrameProductLocation : IProductLocation
    {
        public FrameProductLocation(Car car, CarFrame carFrame)
        {
            Car = car;
            CarFrame = carFrame;
        }

        public Car Car { get; private set; }
        public Product Product => CarFrame;
        public IProductsHolder Holder => Car;
        public CarFrame CarFrame { get; private set; }

        public bool Attach(Product product)
        {
            Debug.LogWarning($"Car {Car.UniqueName}: Cannot attach product { product.Name} to FrameLocation: location is fixed to CarFrame {CarFrame.Name}.");
            return false;
        }

        public void Detach()
        {
            Debug.LogWarning($"Car {Car.UniqueName}: Cannot detach product { Product.Name} from FrameLocation: location is fixed to CarFrame {CarFrame.Name}.");
        }
    }

    /// <summary>
    /// Car Slot-representing instances
    /// </summary>
    public class CarPartLocation : IProductLocation
    {
        public CarPartLocation(Car car, PartSlotRuntimeConfig partSlotRuntimeConfig, Product product)
        {
            Car = car;
            PartSlotRuntimeConfig = partSlotRuntimeConfig;
            Product = product;
        }

        public Car Car { get; private set; }
        public IProductsHolder Holder => Car;
        public PartSlotRuntimeConfig PartSlotRuntimeConfig { get; private set; }
        public Product Product { get; private set; }

        public bool CanAccept(Product product)
        {
            return PartSlotRuntimeConfig.CanAccept(product) && Product == null;
        }

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