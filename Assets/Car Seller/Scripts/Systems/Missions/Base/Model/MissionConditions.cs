using System;

[Serializable]
public class ReachTargetCondition : MissionCondition
{
    public CityMarkerRef target = new CityMarkerRef();

    public override MissionConditionRuntime CreateRuntime(MissionRuntime missionRuntime)
    {
        return new ReachTargetConditionRuntime(target, missionRuntime);
    }

    public class ReachTargetConditionRuntime : MissionConditionRuntime
    {
        public CityMarkerRef target;
        bool isSatisfied = false;

        public ReachTargetConditionRuntime(CityMarkerRef target, MissionRuntime missionRuntime)
            : base(missionRuntime)
        {
            this.target = target;
        }
        public override void OnEvent(GameEventData data)
        {
            if (data is CityTargetReachedEvent e &&
                e.OwnerMission == missionRuntime &&
                e.Marker == target)
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