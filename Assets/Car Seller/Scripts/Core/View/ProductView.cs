using System.Collections;
using UnityEngine;

public interface IProductViewInitializer<TProductView> where TProductView : ProductView
{
    public TProductView InitializeView(GameObject gameObject, Product product);
}

public class ProductView : MonoBehaviour
{
    public Product Product { get; private set; }
    public IProductLocation RepresentedProductLocation { get; private set; }

    public bool Initialized { get; private set; } = false;

    public virtual void Initialize(Product product, IProductLocation representedProductLocation)
    {
        this.Product = product;
        this.RepresentedProductLocation = representedProductLocation;
        Initialized = true;
    }

    protected virtual void OnEnable()
    {
        GameEvents.Instance.OnProductLocationChanged += productLocationChanged;
    }

    protected virtual void OnDisable()
    {
        GameEvents.Instance.OnProductLocationChanged -= productLocationChanged;
    }

    private void productLocationChanged(ProductLocationChangedEventData data)
    {
        if(data.NewLocation.Product == Product && RepresentedProductLocation != data.NewLocation)
        {
            OnProductLocationChanged();
        }
    }

    public virtual void OnProductLocationChanged()
    {
        Destroy(gameObject);
    }
}