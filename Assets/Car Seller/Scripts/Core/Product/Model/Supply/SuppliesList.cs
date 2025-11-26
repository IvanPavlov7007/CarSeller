using System.Collections.Generic;

public class SuppliesList : IProductsHolder
{
    public List<SupplyProductLocation> supplies;

    public IProductLocation[] GetProducts()
    {
        return supplies.ToArray();
    }

    public class SupplyProductLocation : IProductLocation
    {
        public SupplyProductLocation(SuppliesList suppliesList, Supply supply, Product product)
        {
            SuppliesList = suppliesList;
            Supply = supply;
            Product = product;
        }

        public SuppliesList SuppliesList { get; private set; }
        public Supply Supply { get; private set; }
        public Product Product { get; private set; }

        public IProductsHolder Holder => SuppliesList;

        public bool Attach(Product product)
        {
            if(Product != null)
                return false;
            Product = product;
            SuppliesList.supplies.Add(this);
            return true;
        }

        public void Detach()
        {
            Product = null;
            SuppliesList.supplies.Remove(this);
        }
    }
}