/// <summary>
/// Class for communication with the mission controller over Event Bus.
/// Signalizing "intentions"
/// </summary>
public abstract class MissionInternalEvent {
    public readonly MissionRuntime Mission;

    protected MissionInternalEvent(MissionRuntime mission)
    {
        this.Mission = mission;
    }
}
public class UnlockMissionRequestEvent : MissionInternalEvent
{
    public readonly MissionConfig toUnlock;

    public UnlockMissionRequestEvent(MissionRuntime thisMission, MissionConfig toUnlock) 
        : base(thisMission)
    {
        this.toUnlock = toUnlock;
    }
}

public class SpawnTargetMissionRequestEvent : MissionInternalEvent
{
    public readonly CityMarkerRef targetMarker;

    public SpawnTargetMissionRequestEvent(MissionRuntime thisMission, CityMarkerRef targetMarker) 
        : base(thisMission)
    {
        this.targetMarker = targetMarker;
    }
}

public class StartMissionInternalEvent : MissionInternalEvent
{
    public StartMissionInternalEvent(MissionRuntime mission) 
        : base(mission)
    {
    }
}

public class CompleteMissionInternalEvent : MissionInternalEvent
{
    public CompleteMissionInternalEvent(MissionRuntime mission) 
        : base(mission)
    {

    }
}

public class FailMissionInternalEvent : MissionInternalEvent
{
    public FailMissionInternalEvent(MissionRuntime mission) 
        : base(mission)
    {
    }
}