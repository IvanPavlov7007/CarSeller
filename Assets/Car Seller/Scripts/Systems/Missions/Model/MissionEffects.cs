public class UnlockMissionEffect : MissionEffect
{
    public MissionConfig mission;

    public override void Apply(MissionEffectContext context)
    {
        context.EventBus.Emit(new UnlockMissionRequestEvent(context.Mission, mission));
    }
}

public class SpawnTargetEffect : MissionEffect
{
    public CityMarkerRef targetMarker;
    public override void Apply(MissionEffectContext context)
    {
        context.EventBus.Emit(new SpawnTargetMissionRequestEvent(context.Mission, targetMarker));
    }
}