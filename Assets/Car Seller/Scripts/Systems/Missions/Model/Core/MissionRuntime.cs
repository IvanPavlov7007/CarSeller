using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;

public class MissionRuntime
{
    public readonly MissionConfig Config;
    public MissionStatus Status { get; private set; }

    public List<MissionCondition.MissionConditionRuntime> startConditions =
        new List<MissionCondition.MissionConditionRuntime>();
    public List<MissionCondition.MissionConditionRuntime> successConditions =
        new List<MissionCondition.MissionConditionRuntime>();
    public List<MissionCondition.MissionConditionRuntime> failureConditions =
        new List<MissionCondition.MissionConditionRuntime>();

    public List<MissionEffect> MissionStartEffect => Config.MissionStartEffect;
    public List<MissionEffect> MissionCompleteEffect => Config.MissionCompleteEffect;
    public List<MissionEffect> MissionFailEffect => Config.MissionFailEffect;

    public List<RewardBundle> RewardBundles => Config.RewardBundles;

    public event Action<MissionInternalEvent> onComplete;

    public void SetStatus(MissionStatus newStatus)
    {
        Status = newStatus;
    }

    public MissionRuntime(MissionConfig config, MissionEventBus eventBus)
    {
        Config = config;
        Status = MissionStatus.Locked;

        initializeConditions();
    }

    void Complete()
    {
        SetStatus(MissionStatus.Succeeded);
        foreach (var effect in MissionCompleteEffect)
        {
            effect.Apply(this);
        }
    }

    void initializeConditions()
    {
        foreach (var condition in Config.StartConditions)
        {
            var conditionRuntime = condition.CreateRuntime(this);
            startConditions.Add(conditionRuntime);
        }
        foreach (var condition in Config.SuccessConditions)
        {
            var conditionRuntime = condition.CreateRuntime(this);
            successConditions.Add(conditionRuntime);
        }
        foreach (var condition in Config.FailConditions)
        {
            var conditionRuntime = condition.CreateRuntime(this);
            failureConditions.Add(conditionRuntime);
        }
    }

    public void OnEvent(GameEventData data)
    {
        foreach (var c in successConditions)
            c.OnEvent(e);

        if (successConditions.All(c => c.IsSatisfied))
            Complete();

        //Also check failure conditions
    }
}
public enum MissionStatus
{
    Locked,
    Available,
    Active,
    Succeeded,
    Failed,
}