using UnityEngine;

public class WarehouseInteractionManager : IInteractionManager
{
    // when dragging starts -> select target objects
    // when dragging ends -> get the drop target and perform action

    //when clicking -> open popup menu

    Product draggedProduct;

    public void OnProductViewClick(Interactable interactable)
    {
        var productView = interactable.GetComponent<ProductView>();
        if (productView != null)
        {
            //open product details popup
            //WarehouseUIManager.Instance.OpenProductDetailsPopup(productView.Product);
        }
    }
    public void OnDragEnd(Interactable interactable)
    {
        if(draggedProduct == null)
        {
            Debug.LogWarning("Unknown product has been stopped dragging");
            return;
        }

        //casting all products on drop location
        var positionInteractables = InteractionPhysics.RaycastForInteractables(
            GameCursor.Instance.interactableLayersMask, interactable.transform.position, GameCursor.Instance.use3dPhysics);

        tryDropProduct(positionInteractables);

        draggedProduct = null;

    }

    private void tryDropProduct(Interactable[] interactables)
    {
        //trying to attach dragged product to any car on the position
        foreach (var targetInteractable in interactables)
        {
            var productView = targetInteractable.GetComponent<ProductView>();
            var targetProduct = productView?.Product;
            if (targetProduct == draggedProduct)
            {
                continue;
            }
            var car = targetProduct as Car;
            Car.CarPartLocation slot = car?.AvailableSlot(draggedProduct);
            if (slot != null)
            {
                attachProductToCar(draggedProduct, slot);
                break;
            }
        }
    }
    private void attachProductToCar(Product product, Car.CarPartLocation slot)
    {
        if (!G.Instance.LocationService.MoveProduct(product, slot))
        {
            Debug.LogWarning($"Failed to attach product {draggedProduct.Name} to car slot {slot.PartSlotRuntimeConfig}");
        }
    }

    public void OnDragStart(Interactable interactable)
    {
        //getting dragged product
        var productView = interactable.GetComponent<ProductView>();
        if (productView != null)
        {
            draggedProduct = productView.Product;
        }
    }
    public class WarehouseInteractionState
    {

    }

}

