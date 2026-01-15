using Pixelplacement;
using UnityEngine;

public class GlobalCreationService
{
    public CityObject CreateCityObject(CityMarkerRef positionRef, PinStyle pinStyle)
    {
        return new CityObject(
            "CityObject",
            "A generic city object.",
            createLocationFromMarkerRef(positionRef, out var marker),
            marker,
            pinStyle: pinStyle
            );
    }

    public MissionLauncher CreateMissionLauncher(MissionLauncherConfig config, MissionRuntime mission)
    {
        return new MissionLauncher(
            mission.Config.MissionId,""
            , createLocationFromMarkerRef(config.cityMarkerRef, out var marker),
            marker,
            config.pinStyle,
            mission,
            config
            );
    }

    public CollectableCityObject CreateCollectableCityObject(Collectable collectable, CityMarkerRef positionRef, PinStyle pinStyle)
    {
        return new CollectableCityObject(
            collectable,
            createLocationFromMarkerRef(positionRef, out var marker),
            marker
            );
    }

    ILocation createLocationFromMarkerRef(CityMarkerRef markerRef, out City.CityMarker marker)
    {
        if (!G.City.TryGetMarker(markerRef.MarkerId, out marker))
        {
            Debug.LogError($"Marker with id {markerRef.MarkerId} not found in city.");
            return null;
        }
        if (marker.PositionOnGraph == null)
        {
            Debug.LogError($"Marker with id {markerRef.MarkerId} does not have a valid position on graph.");
            return null;
        }

        return G.City.GetEmptyLocation(marker.PositionOnGraph.Value);
    }
}