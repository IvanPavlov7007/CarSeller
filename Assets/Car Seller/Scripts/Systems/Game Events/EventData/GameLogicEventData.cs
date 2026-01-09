public class CityTargetReachedEvent : GameEventData
{
    public MissionRuntime OwnerMission;
    public CityMarkerRef Marker;

    public CityTargetReachedEvent(MissionRuntime ownerMission, CityMarkerRef marker)
    {
        OwnerMission = ownerMission;
        Marker = marker;
    }
}