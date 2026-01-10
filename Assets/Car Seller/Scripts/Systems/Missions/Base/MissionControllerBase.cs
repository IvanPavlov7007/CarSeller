using System;
using System.Collections.Generic;

public abstract class MissionControllerBase
{
    MissionEventBus eventBus;
    Dictionary<MissionConfig, MissionRuntime> runtimes;


    public MissionControllerBase(List<MissionConfig> configs)
    : this(configs, new MissionEventBus())
    {
    }

    protected MissionControllerBase(List<MissionConfig> configs, MissionEventBus eventBus)
    {
        // Initialize Event Bus
        this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        InitializeEventBus();
        InitializeRuntimes(configs);
    }
    protected MissionEventBus EventBus => eventBus; // Expose for testing
    protected IReadOnlyDictionary<MissionConfig, MissionRuntime> Runtimes => runtimes; // Expose for testing

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
        var runtime = new MissionRuntime(config, eventBus);
        return runtime;
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
            runtime.OnEvent(gameEventData);
        }
    }
}