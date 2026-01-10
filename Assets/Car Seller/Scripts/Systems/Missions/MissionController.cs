using System.Collections.Generic;

public class MissionController : MissionControllerBase
{
    public MissionController(List<MissionConfig> configs) : base(configs)
    {
    }

    protected override void onSpawnTargetMissionRequestEvent(SpawnTargetMissionRequestEvent requestEvent)
    {
        throw new System.NotImplementedException();
    }
}