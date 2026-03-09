using System;

public sealed class VehicleController
{
    public event Action<Car, Car> OnCurrentVehicleChanged;
    public event Action<Car> OnPrimaryVehicleChanged;

    public Car CurrentVehicle { get; private set; }
    public Car PrimaryVehicle { get; private set; }

    public VehicleController()
    {
        GameEvents.Instance.OnGameStateChanged += OnGameStateChanged;
        GameEvents.Instance.onPrimaryVehicleChanged += OnPrimaryVehicleChangedInternal;

        // Initialize cached values (important for first UI sync).
        CurrentVehicle = G.GameState?.FocusedCar;
        PrimaryVehicle = G.PersonalVehicles?.PrimaryVehicle;
    }

    public void Sync()
    {
        // For UIs that enable after state was already set.
        OnCurrentVehicleChanged?.Invoke(CurrentVehicle, PrimaryVehicle);
        OnPrimaryVehicleChanged?.Invoke(PrimaryVehicle);
    }

    private void OnGameStateChanged(GameStateChangeEventData data)
    {
        var newCurrent = data?.newState?.FocusedCar;
        var primary = G.PersonalVehicles?.PrimaryVehicle;

        var currentChanged = !ReferenceEquals(CurrentVehicle, newCurrent);
        var primaryChanged = !ReferenceEquals(PrimaryVehicle, primary);

        CurrentVehicle = newCurrent;
        PrimaryVehicle = primary;

        if (currentChanged || primaryChanged)
            OnCurrentVehicleChanged?.Invoke(CurrentVehicle, PrimaryVehicle);

        if (primaryChanged)
            OnPrimaryVehicleChanged?.Invoke(PrimaryVehicle);
    }

    private void OnPrimaryVehicleChangedInternal(Car newPrimaryVehicle)
    {
        if (ReferenceEquals(PrimaryVehicle, newPrimaryVehicle))
            return;

        PrimaryVehicle = newPrimaryVehicle;

        OnPrimaryVehicleChanged?.Invoke(PrimaryVehicle);
        OnCurrentVehicleChanged?.Invoke(CurrentVehicle, PrimaryVehicle);
    }
}