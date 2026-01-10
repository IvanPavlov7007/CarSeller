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

    public void UnlockMission(MissionConfig mission)
    {
        var runtime = runtimes[mission];
        runtime.Unlock();
        // GameEvents.onMissionUnlocked?.Invoke(runtime);
    }


    // Mission Event Bus
    //Internal Event Handlers
    void onStartMissionRequestEvent(StartMissionInternalEvent e)
    {
        executeMissionEffects(e.Mission.MissionStartEffects, createMissionEffectContext(e.Mission));
        // GameEvents.onMissionStarted?.Invoke(requestEvent.Mission);
    }
    void onCompleteMissionRequestEvent(CompleteMissionInternalEvent e)
    {
        executeMissionEffects(e.Mission.MissionCompleteEffects, createMissionEffectContext(e.Mission));
        // GameEvents.onMissionCompleted?.Invoke(requestEvent.Mission);
    }
    void onFailMissionRequestEvent(FailMissionInternalEvent e)
    {
        executeMissionEffects(e.Mission.MissionFailEffects, createMissionEffectContext(e.Mission));
        // GameEvents.onMissionFailed?.Invoke(requestEvent.Mission);
    }

    void executeMissionEffects(List<MissionEffect> effects, MissionEffectContext context)
    {
        foreach (var effect in effects)
        {
            effect.Apply(context);
        }
    }

    MissionEffectContext createMissionEffectContext(MissionRuntime mission)
    {
        return new MissionEffectContext
        {
            Mission = mission,
            EventBus = eventBus
        };
    }

    // MissionEffects Event Handlers
    void onUnlockMissionRequestEvent(UnlockMissionRequestEvent requestEvent)
    {
        UnlockMission(requestEvent.toUnlock);
    }

    protected abstract void onSpawnTargetMissionRequestEvent(SpawnTargetMissionRequestEvent requestEvent);

    // Game Events Funneling
    #region globalGameEvents


    // Game Events Funneling
    public void OnCityTargetReached(CityTargetReachedEvent targetReachedEvent)
    {
        updateMissionRuntimes(targetReachedEvent);
    }
    public void Update(float deltaTime)
    {
        updateMissionRuntimes(new TimePassEvent(deltaTime));
    }



    // Game Events Handling
    //TODO: split into phases:
    // 1 Evaluation - in MissionRuntime
    // 2 Queue Resolutions - from MissionRuntime
    // 3 Apply Resolutions - which might add new events to the queue
    // (0) Evaluation of the next event in the more global queue,
    //      that might have been added during resolution phase
    private void updateMissionRuntimes(GameEventData gameEventData)
    {
        foreach (var runtime in runtimes.Values)
        {
            runtime.OnEvent(gameEventData);
        }
    }
    #endregion
}