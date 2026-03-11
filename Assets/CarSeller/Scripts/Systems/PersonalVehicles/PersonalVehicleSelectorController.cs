using System;
using System.Collections.Generic;

public class PersonalVehicleSelectorController
{
    public readonly PersonalVehiclesList PersonalVehiclesList;
    public event Action OnRefresh;

    public PersonalVehicleSelectorController(PersonalVehiclesList personalVehiclesList,Action refreshedCallback)
    {
        PersonalVehiclesList = personalVehiclesList;
        OnRefresh += refreshedCallback;
    }
    public UIElement CreateMenuUIContent()
    {
        UIElement container = new UIElement
        {
            Type = UIElementType.Container,
            Style = "grid",
            Children = new List<UIElement>()
        };
        populateVehicleSelectionContainer(container, G.VehicleController.CurrentPrimaryVehicle);
        return container;
    }

    void populateVehicleSelectionContainer(UIElement container, PersonalVehicle currentVehicle)
    {
        foreach(var vehicle in PersonalVehiclesList.OwnedVehicles)
        {
            if(vehicle == currentVehicle)
            {
                container.Children.Add(selectedVehicleUIContent(vehicle));
            }
            else
            {
                container.Children.Add(availableVehicleUIContent(vehicle));
            }
        }
    }

    UIElement selectedVehicleUIContent(PersonalVehicle vehicle)
    {
        return new UIElement()
        {
            Type = UIElementType.ButtonContainer,
            closePopupOnClick = true,
            Children = new List<UIElement>()
            {
                CTX_Menu_Tools.CarIcon(vehicle.Car),
                CTX_Menu_Tools.CarRarityText(vehicle.Car),
                CTX_Menu_Tools.Header("SELECTED")
            }
        };
    }

    UIElement availableVehicleUIContent(PersonalVehicle vehicle)
    {
        return new UIElement()
        {
            Type = UIElementType.ButtonContainer,
            closePopupOnClick = true,
            OnClick = () =>
            {
                G.VehicleController.SwapPrimaryVehicle(vehicle);
                OnRefresh?.Invoke();
            },
            Children = new List<UIElement>()
            {
                CTX_Menu_Tools.CarIcon(vehicle.Car),
                CTX_Menu_Tools.CarRarityText(vehicle.Car),
            }
        };
    }
}