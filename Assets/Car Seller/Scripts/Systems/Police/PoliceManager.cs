using Pixelplacement;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PoliceManager : Singleton<PoliceManager>
{
    PoliceAISystem aiSystem = new PoliceAISystem();
    PoliceAIStateMachine stateMachine;

    List<PoliceUnit> policeUnits = new List<PoliceUnit>();
    public SpotlightColors SpotlightColors;

    public SpeedVarations policeSpeedVariations;
    float visionRadius = 10f;
    float visionAngle = 10f;

    bool active = false;

    City.CityPosition SuspectPosition
    { get
    {
            if (G.GameState == null)
            {
                Debug.LogWarning("GameState is null in PoliceManager.SuspectPosition");
                return default;
            }
            var location = CityLocatorHelper.GetCityLocation(G.GameState.FocusedCar);
            if (location == null)
            {
                Debug.LogWarning("FocusedCar location is null in PoliceManager.SuspectPosition");
                return default;
            }

            return location.CityPosition;
        }

}

    

    public void CreatePolice()
    {
        active = true;

        var markers = G.City.QueryMarkers("cop");
        var locations = markers.Select(m => G.City.GetEmptyLocation(m.PositionOnGraph.Value)).ToList();

        foreach (var location in locations)
        {
            policeUnits.Add(CreateUnit(location));
        }

        stateMachine = new PoliceAIStateMachine(policeUnits.ToArray());
    }


    public void ClearPolice()
    {
        active = false;
        foreach (var unit in policeUnits)
        {
            unit.Destroy();
        }
        policeUnits.Clear();
    }
    private PoliceUnit CreateUnit(City.CityLocation location)
    {
        return new PoliceUnit("Police Unit", "A police unit patrolling the city.",location,
            null,policeSpeedVariations,
            visionRadius,visionAngle,PersonalityTag.Blinky);
    }

    private void Update()
    {
        if (!active) return;
        updateStateMachineData();
        updateUnits(Time.deltaTime);
        aiSystem.UpdateState(stateMachine);
    }

    void updateStateMachineData()
    {
        stateMachine.suspectRealPosition = SuspectPosition;
    }

    void updateUnits(float deltaTime)
    {
        // actually each unit updates itself as well
        foreach (var unit in policeUnits)
        {
            unit.Update(deltaTime, aiSystem, stateMachine);
        }
    }
}