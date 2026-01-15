using Pixelplacement;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PoliceManager : Singleton<PoliceManager>
{
    PoliceAISystem aiSystem = new PoliceAISystem();
    PoliceAIContext stateMachine;

    List<PoliceCityObject> policeUnits = new List<PoliceCityObject>();
    public SpotlightColors SpotlightColors;
    public SliceVisionSettings SliceVisionSettings;

    public SpeedVarations policeSpeedVariations;

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
            var pos = location.CityPosition;
            Debug.Assert(pos.Edge != null || pos.Node != null, $"SuspectPosition is invalid: edge and node are null");
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

        stateMachine = new PoliceAIContext(policeUnits.Select(item=>item.Data as PoliceUnit).ToArray());
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
    private PoliceCityObject CreateUnit(City.CityLocation location)
    {
        var data = new PoliceUnit(location,policeSpeedVariations,
            SliceVisionSettings,PersonalityTag.Blinky);
        return new PoliceCityObject("Police unit", "a police", location, null, data);
    }

    private void Update()
    {
        if (!active) return;
        updateStateMachineData();
        aiSystem.UpdateState(stateMachine);
        updateUnits(Time.deltaTime);
    }

    void updateStateMachineData()
    {
        stateMachine.SuspectRealPosition = SuspectPosition;
    }

    void updateUnits(float deltaTime)
    {
        // actually each unit updates itself as well
        foreach (var unit in policeUnits)
        {
            (unit.Data as PoliceUnit).Update(deltaTime, aiSystem, stateMachine);
        }
    }
}