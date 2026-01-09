using System;
using System.Collections.Generic;

public abstract class MissionControllerBase
{
    MissionEventBus eventBus = new MissionEventBus();
    Dictionary<MissionConfig, MissionRuntime> runtimes;


    public MissionControllerBase(List<MissionConfig> configs)
    {
        InitializeRuntimes(configs);
    }

    private void InitializeEventBus()
    {
        eventBus = new MissionEventBus();
        // Subscribe to mission internal events
        // General mission events
        eventBus.Subscribe<StartMissionInternalEvent>(onStartMissionRequestEvent);
        eventBus.Subscribe<CompleteMissionInternalEvent>(onCompleteMissionRequestEvent);
        eventBus.Subscribe<FailMissionInternalEvent>(onFailMissionRequestEvent);

        // Specific mission requests
        eventBus.Subscribe<UnlockMissionRequestEvent>(onUnlockMissionRequestEvent);
        eventBus.Subscribe<SpawnTargetMissionRequestEvent>(onSpawnTargetMissionRequestEvent);
        
    }

    private void InitializeRuntimes(List<MissionConfig> configs)
    {
        runtimes = new Dictionary<MissionConfig, MissionRuntime>();
        foreach (var config in configs)
        {
            runtimes[config] = InitializeRuntime(config);
        }
    }

    private MissionRuntime InitializeRuntime(MissionConfig config)
    {
        var runtime = new MissionRuntime(config);

        foreach(var condition in config.StartConditions)
        {
            var conditionRuntime = condition.CreateRuntime(runtime);
        }
    }

    public void ForceUnlockMission(MissionConfig mission)
    {
        throw new NotImplementedException();
    }

    public void UnlockMission(MissionConfig mission)
    {
        // changing and setting conditions
        throw new NotImplementedException();
    }
    // Mission Event Bus

    void onStartMissionRequestEvent(StartMissionInternalEvent requestEvent)
    {
    }

    void onCompleteMissionRequestEvent(CompleteMissionInternalEvent requestEvent)
    {

    }

    void onFailMissionRequestEvent(FailMissionInternalEvent requestEvent)
    {
    }

    void onUnlockMissionRequestEvent(UnlockMissionRequestEvent requestEvent)
    {
        UnlockMission(requestEvent.toUnlock);
    }

    void onSpawnTargetMissionRequestEvent(SpawnTargetMissionRequestEvent requestEvent)
    {
    }

    // Game Events Funneling

    public void OnCityTargetReached(CityTargetReachedEvent targetReachedEvent)
    {
        updateMissionRuntimes(targetReachedEvent);
    }

    public void Update(float deltaTime)
    {
        updateMissionRuntimes(new TimePassEvent(deltaTime));
    }

    //Game Events Handling
    private void updateMissionRuntimes(GameEventData gameEventData)
    {
        foreach (var runtime in runtimes.Values)
        {
            //TODO : Check and branch by mission status

            runtime.OnEvent(gameEventData);
        }
    }
