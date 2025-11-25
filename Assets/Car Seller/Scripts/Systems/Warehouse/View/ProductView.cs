using System.Collections;
using UnityEngine;
public class ProductView : MonoBehaviour
{
    public Product Product { get; private set; }
    public IProductLocation RepresentedProductLocation { get; private set; }

    public bool Initialized { get; private set; } = false;

    public void Initialize(Product product, IProductLocation representedProductLocation)
    {
        this.Product = product;
        this.RepresentedProductLocation = representedProductLocation;
        Initialized = true;
    }

    protected virtual void OnEnable()
    {
        GameEvents.Instance.onProductLocationChanged += productLocationChanged;
    }

    protected virtual void OnDisable()
    {
        GameEvents.Instance.onProductLocationChanged -= productLocationChanged;
    }

    public virtual void productLocationChanged(IProductLocation newLocation)
    {
        if(newLocation.Product == Product && RepresentedProductLocation != newLocation)
        {
            OnProductLocationChanged();
        }
    }

    public virtual void OnProductLocationChanged()
    {
        Destroy(gameObject);
    }
}