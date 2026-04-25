using Pixelplacement;
using UnityEngine;

public class VehicleControlUI : Singleton<VehicleControlUI>
{
    public VehicleButtonUI CurrentVehicleButton;
    public VehicleButtonUI PrimaryVehicleButton;

    private void Awake()
    {
        PrimaryVehicleButton.button.onClick.AddListener(onPrimaryClick);
        CurrentVehicleButton.button.onClick.AddListener(onCurrentVechicleClick);
    }

    private void onPrimaryClick()
    {
        G.ProcessRunner.Run(new PersonalVehicleShopProcess(G.VehicleShop));
    }

    private void onCurrentVechicleClick()
    {
        if (!G.runIntialized)
            return;
        CameraMovementManager.Instance.Teleport(G.VehicleController.CurrentVehicleEntity.Position.WorldPosition);
    }

    private void OnEnable()
    {
        Debug.Assert(G.VehicleController != null);
        GameEvents.Instance.onVehicleControlStateChanged += OnVehicleStateChanged;
        if(G.VehicleController != null)
            Redraw(G.VehicleController.CurrentState);
    }

    private void OnDisable()
    {
        Debug.Assert(G.VehicleController != null);

        GameEvents.Instance.onVehicleControlStateChanged -= OnVehicleStateChanged;
    }

    void Redraw(VehicleController.VehicleControlState state)
    {
        if(!G.runIntialized)
            return;
        
        var primaryVehicle = G.VehicleController.PrimaryCar;
        var currentVehicle = state.Car;
        RedrawCurrent(currentVehicle, primaryVehicle);
        RedrawPrimary(primaryVehicle);
    }

    private void OnVehicleStateChanged(VehicleControlStateChangedEventData data)
    {
        Redraw(data.NewState);
    }

    private void RedrawCurrent(Car newCurrentVehicle, Car primaryVehicle)
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

    private void RedrawPrimary(Car newPrimaryVehicle)
    {
        if (PrimaryVehicleButton != null)
        {
            PrimaryVehicleButton.SetVehicleImage(GetCarImage(newPrimaryVehicle));
            PrimaryVehicleButton.MakeInteractable();
        }
    }

    private static Sprite GetCarImage(Car car)
    {
        return IconBuilderHelper.BuildProdutSpite(car);
    }
}