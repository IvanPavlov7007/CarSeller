using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;

public class CityInteractionManager : IInteractionManager
{
    CityContextMenuContentProfile ctxMenuProfile = new CityContextMenuContentProfile();
    public void OnDragEnd(Interactable interactable)
    {
    }

    public void OnDragStart(Interactable interactable)
    {
    }

    public void OnProductViewClick(Interactable interactable)
    {
        var contentProvider = interactable.GetComponent<ContentProvider>();
        var content = contentProvider.ProvideContent<UIElement>(ctxMenuProfile, null);
        if(content != null)
            ContextMenuManager.Instance.CreateContextMenu(interactable.gameObject, content);
    }

    public class CityContextMenuContentProfile : IInteractionContentProfile<UIElement>
    {
        public UIElement GenerateContent(object model, IInteractionContext context)
        {
            switch(model)
            {
                case Car car:
                    return generateCarConent(car);
                case Warehouse warehouse:
                    return generateWarehouseContent(warehouse);
                default:
                    Debug.LogError($"CityInteractionManager: Unsupported model type {model.GetType()}");
                    return null;
            }
        }

        private UIElement generateCarConent(Car car)
        {
            var closesWarehouse = getClosestWarehouse(car, out float distance);
            bool carNearWarehouse = distance < 0.5f;

            UIElement content = new UIElement
            {
                Type = UIElementType.Container,
                Children = new List<UIElement>()
                {
                    new UIElement
                    {
                        Type = UIElementType.Text,
                        Text = $"Car: {car.Name}"
                    },
                    new UIElement
                    {
                        Type = UIElementType.Button,
                        Text = "Put inside warehouse",
                        IsInteractable = carNearWarehouse,
                        UnavailabilityReason = carNearWarehouse ? null : "Car is too far from warehouse",
                        OnClick = () =>
                        {
                            G.Instance.CityActionService.PutCarInsideWarehouse(car, closesWarehouse);
                        }
                    },
                }
            };

            return content;

        }

        private Warehouse getClosestWarehouse(Car car, out float distance)
        {
            

            var city = World.Instance.City;
            Debug.Assert(city.Positions.ContainsKey(car), "CityInteractionManager: Car position not found in city positions");
            Vector2 carPosition = city.Positions[car].WorldPosition;
            Dictionary<Warehouse, float> warehouseDistances = new Dictionary<Warehouse, float>();
            foreach (var obj in city.Positions.Keys)
            {
                if (obj is not Warehouse)
                    continue;
                var warehousePosition = city.Positions[obj].WorldPosition;
                warehouseDistances.Add(obj as Warehouse, Vector2.Distance(warehousePosition, carPosition));
            }
            warehouseDistances = warehouseDistances.OrderBy(kv => kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value);
            distance = warehouseDistances.First().Value;
            return warehouseDistances.First().Key;
        }


        private UIElement generateWarehouseContent(Warehouse warehouse)
        {
            var closestCar = getClosestCar(warehouse, out float distance);
            bool carNearWarehouse = distance < 0.5f;

            UIElement content = new UIElement
            {
                Type = UIElementType.Container,
                Children = new List<UIElement>()
                {
                    new UIElement
                    {
                        Type = UIElementType.Text,
                        Text = $"Warehouse: {warehouse.Config.DisplayName}"
                    },
                    new UIElement
                    {
                        Type = UIElementType.Button,
                        Text = "Open",
                        OnClick = () =>
                        {
                            G.Instance.GameFlowController.EnterWarehouse(warehouse);
                        },
                    },
                    new UIElement
                    {
                        Type = UIElementType.Button,
                        Text = "Put closest car inside",
                        IsInteractable = carNearWarehouse,
                        UnavailabilityReason = carNearWarehouse ? null : "No car near warehouse",
                        OnClick = () =>
                        {
                            G.Instance.CityActionService.PutCarInsideWarehouse(closestCar, warehouse);
                        }
                    },
                }
            };
            return content;
        }

        private Car getClosestCar(Warehouse warehouse, out float distance)
        {
            var city = World.Instance.City;
            Vector2 warehousePosition = city.Positions[warehouse].WorldPosition;
            Dictionary<Car, float> carDistances = new Dictionary<Car, float>();
            foreach (var obj in city.Positions.Keys)
            {
                if (obj is not Car)
                    continue;
                var carPosition = city.Positions[obj].WorldPosition;
                carDistances.Add(obj as Car, Vector2.Distance(carPosition, warehousePosition));
            }


            if(carDistances.Count == 0)
            {
                distance = float.MaxValue;
                return null;
            }
            carDistances = carDistances.OrderBy(kv => kv.Value).ToDictionary(kv => kv.Key, kv => kv.Value);
            distance = carDistances.First().Value;
            return carDistances.First().Key;
        }
    }
}