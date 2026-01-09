public abstract class MissionInternalEvent {
    public readonly MissionRuntime mission;

    protected MissionInternalEvent(MissionRuntime mission)
    {
        this.mission = mission;
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