using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class WarehouseInteractionManager : IInteractionManager
{
    // when dragging starts -> select target objects
    // when dragging ends -> get the drop target and perform action

    //when clicking -> open popup menu

    Product draggedProduct;

    WarehouseContextMenuContentProfile ctxMenuProfile = new WarehouseContextMenuContentProfile();

    public void OnProductViewClick(Interactable interactable)
    {
        var productView = interactable.GetComponent<ProductView>();
        if (productView != null)
        {
            //open product details popup
            //WarehouseUIManager.Instance.OpenProductDetailsPopup(productView.Product);
            var contentProvider = productView.GetComponent<ContentProvider>();
            var ctxMenuContent = contentProvider?.ProvideContent(ctxMenuProfile, null);
            if (ctxMenuContent != null)
            {
                ContextMenuManager.Instance.CreateContextMenu(interactable.gameObject, ctxMenuContent);
            }
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
        if (!G.Instance.ProductLocationService.MoveProduct(product, slot))
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
    public class WarehouseContextMenuContentProfile : IInteractionContentProfile<UIElement>, IProductViewBuilder<UIElement>
    {
        public UIElement BuildCar(Car car)
        {
            UIElement element = new UIElement()
            {
                Type = UIElementType.Container,
                Children = new List<UIElement>()
                {
                    new UIElement()
                    {
                        Type = UIElementType.Text,
                        Text = car.Name,
                        Style = "header"
                    },
                    new UIElement()
                    {
                        Type = UIElementType.Button,
                        Text = "Disassemble",
                        IsInteractable = G.Instance.CarMechanicService.CanDisassembleCar(car),
                        OnClick = () =>
                        {
                            G.Instance.CarMechanicService.DisassembleCar(WarehouseSceneManager.SceneWarehouseModel, car);
                        },
                        UnavailabilityReason = "Car has nothing to disassemble"
                    }
                    ,new UIElement()
                    {
                        Type = UIElementType.Button,
                        Text = "Ride",
                        IsInteractable = car.IsComplete(),
                        UnavailabilityReason = "Some car part's are missing",
                        OnClick = () =>
                        {
                            G.Instance.CarMechanicService.RideCarFromWarehouse(car,WarehouseSceneManager.SceneWarehouseModel);
                        }
                    }
                }
            };
            return element;
        }

        public UIElement BuildCarFrame(CarFrame carFrame)
        {
            return null;
        }

        public UIElement BuildEngine(Engine engine)
        {
            return null;
        }

        public UIElement BuildSpoiler(Spoiler spoiler)
        {
            return null;
        }

        public UIElement BuildWheel(Wheel wheel)
        {
            return null;
        }

        public UIElement GenerateContent(object model, IInteractionContext context)
        {
            switch (model)
            {
                case Product product:
                    return product.GetRepresentation(this);
                default:
                    Debug.LogWarning("No context menu available for model type " + model.GetType() + " in this content profile " + this);
                    return null;
            }
        }
    }

}

