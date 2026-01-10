using Sirenix.OdinInspector;
using Sirenix.OdinValidator;
using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "MissionConfig", menuName = "Configs/Mission Config")]
public class MissionConfig : SerializedScriptableObject
{
    public string MissionId;
    public string Description;

    public CityMarkerRef MissionStartLocation;

    public List<MissionCondition> StartConditions = new List<MissionCondition>();
    public List<MissionCondition> FailConditions = new List<MissionCondition>();
    public List<MissionCondition> SuccessConditions = new List<MissionCondition>();

    [PropertyTooltip("Things for bringing content into the world for the duration of the mission. Important: those effects should be owned by mission runtimes and destroyed with the missions. ")]
    public List<MissionEffect> MissionStartEffects = new List<MissionEffect>();
    [PropertyTooltip("Things to change in the <i>world</i> after finishing mission, like unlocking next missions.")]
    public List<MissionEffect> MissionCompleteEffects = new List<MissionEffect>();
    [PropertyTooltip("Things to change in the <i>world</i> after failing mission, like locking retry for time or reputation loss.")]
    public List<MissionEffect> MissionFailEffects = new List<MissionEffect>();
    public List<RewardBundle> RewardBundles = new List<RewardBundle>();
}

/// <summary>
/// Checks for conditions, no control/creation
/// </summary>
public abstract class MissionCondition
{
    public abstract MissionConditionRuntime CreateRuntime(MissionRuntime missionRuntime);
    public abstract class MissionConditionRuntime
    {
        protected MissionRuntime missionRuntime;

        protected MissionConditionRuntime(MissionRuntime missionRuntime)
        {
            this.missionRuntime = missionRuntime;
        }

        public abstract void OnEvent(GameEventData data);
        public abstract bool IsSatisfied();
    }
}

public struct MissionEffectContext
{
    public MissionRuntime Mission;
    public MissionEventBus EventBus;
}

public abstract class MissionEffect
{
    public abstract void Apply(MissionEffectContext context);
}

//Minimal reward bundle structure
public abstract class RewardBundle
{
    public abstract Transaction CreateTransaction();
}