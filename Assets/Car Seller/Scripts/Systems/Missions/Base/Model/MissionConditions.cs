using System;
using UnityEngine;

[Serializable]
public class ReachTargetCondition : MissionCondition
{
    // TODO instead of coupling by marker ID, consider creating a specific config for target,
    // that can be use to link target runtime between condition and effect.
    public CityMarkerRef target = new CityMarkerRef();

    public override MissionConditionRuntime CreateRuntime(MissionRuntime missionRuntime)
    {
        if (target == null || !target.IsValid)
        {
            Debug.LogError($"ReachTargetCondition: Target marker is not set or invalid in mission '{missionRuntime.Config.MissionId}'");
        }
        if (!G.City.TryGetMarker(target.MarkerId, out var targetRuntime))
        {
            Debug.LogError($"ReachTargetCondition: Target marker with ID '{target.MarkerId}' not found in city for mission '{missionRuntime.Config.MissionId}'");
        }
        return new ReachTargetConditionRuntime(targetRuntime, missionRuntime);
    }

    public class ReachTargetConditionRuntime : MissionConditionRuntime
    {
        public City.CityMarker target;
        bool isSatisfied = false;

        public ReachTargetConditionRuntime(City.CityMarker target, MissionRuntime missionRuntime)
            : base(missionRuntime)
        {
            this.target = target;
        }
        public override void OnEvent(GameEventData data)
        {
            if (data is CityTargetReachedEvent e &&
                //e.OwnerMission == missionRuntime &&
                e.ReachedObject.CityMarker == target)
            {
                isSatisfied = true;
            }
        }

        public override bool IsSatisfied()
        {
            return isSatisfied;
        }
    }
}

[Serializable]
public class MissionLauncherAcceptedCondition : MissionCondition
{
    // TODO check TODO in ReachTargetCondition for similar pattern, consider unifying
    public override MissionConditionRuntime CreateRuntime(MissionRuntime missionRuntime)
    {
        return new MissionLauncherAcceptedConditionRuntime(missionRuntime);
    }
    public class MissionLauncherAcceptedConditionRuntime : MissionConditionRuntime
    {
        private bool isSatisfied = false;
        public MissionLauncherAcceptedConditionRuntime(MissionRuntime missionRuntime)
            : base(missionRuntime)
        {
        }
        public override void OnEvent(GameEventData data)
        {
            if (data is PlayerAcceptedEventData e &&
                e.acceptedMission == missionRuntime)
            {
                isSatisfied = true;
            }
        }
        public override bool IsSatisfied()
        {
            return isSatisfied;
        }
    }
}

[Serializable]
public class TimeElapsedCondition : MissionCondition
{
    public float requiredTimeSeconds;
    public override MissionConditionRuntime CreateRuntime(MissionRuntime missionRuntime)
    {
        return new TimeElapsedConditionRuntime(requiredTimeSeconds, missionRuntime);
    }
    public class TimeElapsedConditionRuntime : MissionConditionRuntime
    {
        private float requiredTimeSeconds;
        private float elapsedTime = 0f;
        public TimeElapsedConditionRuntime(float requiredTimeSeconds, MissionRuntime missionRuntime)
            : base(missionRuntime)
        {
            this.requiredTimeSeconds = requiredTimeSeconds;
        }
        public override void OnEvent(GameEventData data)
        {
            if (data is TimePassEvent e)
            {
                elapsedTime += e.DeltaTime;
            }
        }
        public override bool IsSatisfied()
        {
            return elapsedTime >= requiredTimeSeconds;
        }
    }
}
internal class TimePassEvent : GameEventData
{
    public float DeltaTime;

    public TimePassEvent(float deltaTime)
    {
        DeltaTime = deltaTime;
    }
}