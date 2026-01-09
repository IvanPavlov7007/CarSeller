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

    [InlineEditor]
    public List<MissionCondition> StartConditions = new List<MissionCondition>();
    [InlineEditor]
    public List<MissionCondition> FailConditions = new List<MissionCondition>();
    [Required]
    [InlineEditor]
    public List<MissionCondition> SuccessConditions = new List<MissionCondition>();

    [InlineEditor]
    public List<MissionEffect> MissionStartEffect = new List<MissionEffect>();
    [InlineEditor]
    public List<MissionEffect> MissionCompleteEffect = new List<MissionEffect>();
    [InlineEditor]
    public List<MissionEffect> MissionFailEffect = new List<MissionEffect>();
    [InlineEditor]
    public List<RewardBundle> RewardBundles = new List<RewardBundle>();
}

//Checks for conditions, no control/creation
public abstract class MissionCondition : SerializedScriptableObject
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

public abstract class MissionEffect : SerializedScriptableObject
{
    public abstract void Apply(MissionEffectContext context);
}

//Minimal reward bundle structure
public abstract class RewardBundle : SerializedScriptableObject
{
    public abstract Transaction CreateTransaction();
}