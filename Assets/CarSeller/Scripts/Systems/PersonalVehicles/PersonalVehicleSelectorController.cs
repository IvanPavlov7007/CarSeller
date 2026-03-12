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
    public Widget CreateMenuUIContent()
    {
        Widget container = new DoubleRowChoicePanelWidget(
            "Your Personal Vehicles",
            "Select the primary vehicle.");
        populateVehicleSelectionContainer(container, G.VehicleController.CurrentPrimaryVehicle);
        return container;
    }

    void populateVehicleSelectionContainer(Widget container, PersonalVehicle currentVehicle)
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

    Widget selectedVehicleUIContent(PersonalVehicle vehicle)
    {
        return new PrimarySelectionCarButtonWidget(vehicle.Car,
            PrimarySelectionCarButtonWidget.SelectionState.Selected, null);
    }

    Widget availableVehicleUIContent(PersonalVehicle vehicle)
    {
        return new PrimarySelectionCarButtonWidget(vehicle.Car,
            PrimarySelectionCarButtonWidget.SelectionState.Available, () =>
            {
                G.VehicleController.SwapPrimaryVehicle(vehicle);
                OnRefresh?.Invoke();
            });
    }
}