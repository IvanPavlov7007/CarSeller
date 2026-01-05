using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

//remove later
public class ContextMenuContext : IInteractionContext
{
    public GameState GameState;
    public ContextMenuContext(GameState gameState)
    {
        GameState = gameState;
    }
}


public class WarehouseInteractionManager : IInteractionManager
{
    // when dragging starts -> select target objects
    // when dragging ends -> get the drop target and perform action

    //when clicking -> open popup menu

    Product draggedProduct;

    WarehouseContextMenuContentProfile ctxMenuProfile = new WarehouseContextMenuContentProfile();

    public void OnProductViewClick(Interactable interactable)
    {
        var ctxMenuContext = new ContextMenuContext(G.GameState);

        var productView = interactable.GetComponent<ProductView>();
        if (productView != null)
        {
            //open product details popup
            //WarehouseUIManager.Instance.OpenProductDetailsPopup(productView.Product);
            var contentProvider = productView.GetComponent<ContentProvider>();
            var ctxMenuContent = contentProvider?.ProvideContent(ctxMenuProfile, ctxMenuContext);
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

    public void OnTriggerEntered(ContentProvider trigger, ContentProvider triggerCause)
    {
        Debug.LogError("WarehouseInteractionManager: OnTriggerEntered is not implemented");
    }

    public class WarehouseContextMenuContentProfile : IInteractionContentProfile<UIElement, ContextMenuContext>
    {
        ProductContextMenuContentBuilder productContextMenuContentBuilder = new ProductContextMenuContentBuilder();

        public UIElement GenerateContent(object model, ContextMenuContext context)
        {

            switch(context.GameState)
            {
                case NeutralGameState neutralGameState:
                    return generateNormalContent(neutralGameState, model);
                default:
                    Debug.LogWarning("No context menu available for game state type " + context.GameState.GetType() + " in this content profile " + this);
                    return null;
            }

        }

        private UIElement generateNormalContent(NeutralGameState neutralGameState, object model)
        {
            switch (model)
            {
                case Product product:
                    return product.GetRepresentation(productContextMenuContentBuilder);
                default:
                    Debug.LogWarning("No context menu available for model type " + model.GetType() + " in this content profile " + this);
                    return null;
            }
        }

        class ProductContextMenuContentBuilder : IProductViewBuilder<UIElement>
        {
            public UIElement BuildCar(Car car)
            {
                var elementsList = CTX_Menu_Tools.CarBaseInfoElements(car);
                elementsList.Add(new UIElement()
                {
                    Type = UIElementType.Button,
                    Text = "Sell",
                    IsInteractable = car.IsComplete(),
                    OnClick = () =>
                    {
                        G.Instance.GameFlowManager.SellCar(G.Economy.CarSellOfferProvider.GetOfferByCar(car));
                    },
                    UnavailabilityReason = "Some car part's are missing"
                });
                elementsList.Add(new UIElement()
                {
                    Type = UIElementType.Button,
                    Text = "Disassemble",
                    IsInteractable = G.Instance.CarMechanicService.CanDisassembleCar(car),
                    OnClick = () =>
                    {
                        G.Instance.CarMechanicService.DisassembleCar(WarehouseSceneManager.SceneWarehouseModel, car);
                    },
                    UnavailabilityReason = "Car has nothing to disassemble"
                });

                //elementsList.Add(new UIElement()
                //{
                //    Type = UIElementType.Button,
                //    Text = "Ride",
                //    IsInteractable = car.IsComplete(),
                //    UnavailabilityReason = "Some car part's are missing",
                //    OnClick = () =>
                //    {
                //        G.Instance.CarMechanicService.RideCarFromWarehouse(car, WarehouseSceneManager.SceneWarehouseModel);
                //    }
                //});                
                return new UIElement()
                {
                    Type = UIElementType.Container,
                    Children = elementsList
                };
            }

            public UIElement BuildCarFrame(CarFrame carFrame)
            {
                var elementsList = new List<UIElement>();
                elementsList.Add(CTX_Menu_Tools.Header(carFrame.Name));
                elementsList.Add(CTX_Menu_Tools.Image(IconBuilderHelper.BuildCarFrameSprite(carFrame)));
                return new UIElement()
                {
                    Type = UIElementType.Container,
                    Children = elementsList
                };
            }

            public UIElement BuildEngine(Engine engine)
            {
                var elementsList = new List<UIElement>();
                elementsList.Add(CTX_Menu_Tools.Header(engine.Name));
                elementsList.Add(CTX_Menu_Tools.Image(IconBuilderHelper.BuildEngineSprite(engine)));

                return new UIElement()
                {
                    Type = UIElementType.Container,
                    Children = elementsList
                };
            }

            public UIElement BuildSpoiler(Spoiler spoiler)
            {
                var elementsList = new List<UIElement>();
                elementsList.Add(CTX_Menu_Tools.Header(spoiler.Name));
                elementsList.Add(CTX_Menu_Tools.Image(IconBuilderHelper.BuildSpoilerSprite(spoiler)));
                return new UIElement()
                {
                    Type = UIElementType.Container,
                    Children = elementsList
                };
            }

            public UIElement BuildWheel(Wheel wheel)
            {
                var elementsList = new List<UIElement>();
                elementsList.Add(CTX_Menu_Tools.Header(wheel.Name));
                elementsList.Add(CTX_Menu_Tools.Image(IconBuilderHelper.BuildWheelSprite(wheel)));
                return new UIElement()
                {
                    Type = UIElementType.Container,
                    Children = elementsList
                };
            }
        }

    }
}

