using Pixelplacement;
using UnityEngine;

public class VehicleControlUI : Singleton<VehicleControlUI>
{
    public VehicleButtonUI CurrentVehicleButton;
    public VehicleButtonUI PrimaryVehicleButton;

    private void OnEnable()
    {
        Debug.Assert(G.VehicleController != null);

        G.VehicleController.OnCurrentVehicleChanged += OnCurrentVehicleChanged;
        G.VehicleController.OnPrimaryVehicleChanged += OnPrimaryVehicleChanged;

        G.VehicleController.Sync();
    }

    private void OnDisable()
    {
        Debug.Assert(G.VehicleController != null);

        G.VehicleController.OnCurrentVehicleChanged -= OnCurrentVehicleChanged;
        G.VehicleController.OnPrimaryVehicleChanged -= OnPrimaryVehicleChanged;
    }

    private void OnCurrentVehicleChanged(Car newCurrentVehicle, Car primaryVehicle)
    {
        if (CurrentVehicleButton != null)
        {
            CurrentVehicleButton.SetVehicleImage(GetCarImage(newCurrentVehicle));

            if (ReferenceEquals(newCurrentVehicle, primaryVehicle))
                CurrentVehicleButton.MakeNonInteractable();
            else
                CurrentVehicleButton.MakeInteractable();
        }
    }

    private void OnPrimaryVehicleChanged(Car newPrimaryVehicle)
    {
        if (PrimaryVehicleButton != null)
            PrimaryVehicleButton.SetVehicleImage(GetCarImage(newPrimaryVehicle));
    }

    private static Sprite GetCarImage(Car car)
    {
        return IconBuilderHelper.BuildProdutSpite(car);
    }
}