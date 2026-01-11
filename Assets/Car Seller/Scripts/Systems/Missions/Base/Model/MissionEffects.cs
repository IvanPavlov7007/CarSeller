using System;
[Serializable]
public class UnlockMissionEffect : MissionEffect
{
    public MissionConfig mission;

    public override void Apply(MissionEffectContext context)
    {
        context.EventBus.Emit(new UnlockMissionRequestEvent(context.Mission, mission));
    }
}
[Serializable]
public class SpawnTargetEffect : MissionEffect
{
    public CityMarkerRef targetMarker = new CityMarkerRef();
    public override void Apply(MissionEffectContext context)
    {
        context.EventBus.Emit(new SpawnTargetMissionRequestEvent(context.Mission, targetMarker));
    }
}

[Serializable]
public class SpawnMissionLauncherEffect : MissionEffect
{
    public MissionLauncherConfig launcherConfig = new MissionLauncherConfig();
    public override void Apply(MissionEffectContext context)
    {
        context.EventBus.Emit(new SpawnMissionLauncherRequestEvent(context.Mission, launcherConfig));
    }
}