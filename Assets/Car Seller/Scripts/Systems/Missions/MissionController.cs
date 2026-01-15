using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Implementation that wires up the MissionControllerBase to the game systems.
/// Extends with mission-owned object tracking and cleanup.
/// </summary>
public class MissionController : MissionControllerBase
{
    Dictionary<MissionRuntime, HashSet<IDestroyable>> missionOwnedObjects = new();
    MissionMonoBehaviourHelper monoBehaviourHelper;

    public MissionController(List<MissionConfig> configs) : base(configs)
    {
        monoBehaviourHelper = new GameObject("MissionMonoBehaviourHelper")
            .AddComponent<MissionMonoBehaviourHelper>();
        Enable();
    }

    public void Enable()
    {
        // Extension internal event handlers
        GameEvents.Instance.OnTargetReached += tryCheckIfMissionLauncherAndShowPopUp;
        GameEvents.Instance.onMissionCompleted += showMissionCompletedInfo;
        GameEvents.Instance.onMissionCompleted += rewardPlayerForMissionCompletion;
        GameEvents.Instance.onMissionStarted += enterMissionStateOnMissionStart;
        GameEvents.Instance.onMissionCompleted += exitMissionStateOnMissionCompleted;
        GameEvents.Instance.onMissionFailed += exitMissionStateOnMissionFailed;
        GameEvents.Instance.onMissionFailed += showMissionFailedInfo;

        // External to mission event handlers
        monoBehaviourHelper.OnUpdateEvent += OnUpdate;
        GameEvents.Instance.OnTargetReached += OnCityTargetReached;
        GameEvents.Instance.OnPlayerAccept += OnPlayerAccepted;
        GameEvents.Instance.onPlayerBusted += OnPlayerBusted;
    }

    public void Disable()
    {
        GameEvents.Instance.OnTargetReached -= tryCheckIfMissionLauncherAndShowPopUp;
        GameEvents.Instance.onMissionCompleted -= showMissionCompletedInfo;
        GameEvents.Instance.onMissionCompleted -= rewardPlayerForMissionCompletion;
        GameEvents.Instance.onMissionStarted -= enterMissionStateOnMissionStart;
        GameEvents.Instance.onMissionCompleted -= exitMissionStateOnMissionCompleted;
        GameEvents.Instance.onMissionFailed -= exitMissionStateOnMissionFailed;
        GameEvents.Instance.onMissionFailed -= showMissionFailedInfo;

        monoBehaviourHelper.OnUpdateEvent -= OnUpdate;
        GameEvents.Instance.OnTargetReached -= OnCityTargetReached;
        GameEvents.Instance.OnPlayerAccept -= OnPlayerAccepted;
        GameEvents.Instance.onPlayerBusted -= OnPlayerBusted;
    }

    // Internal functions
    protected override void CleanupMissionObjectsImp(MissionRuntime mission)
    {
        if (!missionOwnedObjects.TryGetValue(mission, out var set))
            return;

        // Work on a snapshot to avoid modification-during-iteration issues
        var snapshot = new List<IDestroyable>(set);

        foreach (var obj in snapshot)
        {
            obj.Destroy();
        }

        // Now clear and drop the reference
        set.Clear();
        missionOwnedObjects.Remove(mission);
    }

    // Utility helpers functions

    void registerMissionObject(MissionRuntime mission, IDestroyable obj)
    {
        if (!missionOwnedObjects.TryGetValue(mission, out var set))
        {
            set = new HashSet<IDestroyable>();
            missionOwnedObjects[mission] = set;
        }
        set.Add(obj);
        obj.onBeingDestroyed += onDestroyableDestroyed;
    }

    // Internal Event Handlers Overrides
    protected override void onSpawnTargetMissionRequestEvent(
    SpawnTargetMissionRequestEvent request)
    {
        
        var obj = G.Instance.GlobalCreationService
            .CreateCityObject(request.TargetMarker, G.Instance.WorldMissionsConfig.finishPinStyle); // TODO FIX this backwards reference!!!!

        registerMissionObject(request.Mission, obj);
    }

    protected override void onSpawnMissionLauncherRequestEvent(SpawnMissionLauncherRequestEvent requestEvent)
    {
        var missionLauncher = G.Instance.GlobalCreationService.CreateMissionLauncher(
                requestEvent.LauncherConfig,
                requestEvent.Mission);
        registerMissionObject(requestEvent.Mission, missionLauncher);
    }
    protected override void onSpawnMoneyCollectablesRequestEvent(SpawnMoneyCollectablesRequestEvent requestEvent)
    {
        var markers = G.City.QueryMarkers("cash");
        var locations = markers.Select(a => (G.City.GetEmptyLocation(a.PositionOnGraph.Value),a)).ToArray();
        locations.Shuffle();
        for (int i = 0; i < requestEvent.count; i++)
        {
            var collectable = new Collectable { MoneyAmount = requestEvent.reward };
            var co = new CollectableCityObject(collectable, locations[i].Item1, locations[i].a);
            registerMissionObject(requestEvent.Mission, co);
        }
    }
    protected override void onPoliceRequestEvent(PoliceRequestEvent requestEvent)
    {
        var policeWrapper = new PoliceMissionWrapper();
        registerMissionObject(requestEvent.Mission, policeWrapper);
    }
    class PoliceMissionWrapper : IDestroyable
    {
        public event Action<IDestroyable> onBeingDestroyed;
        public PoliceMissionWrapper()
        {
            PoliceManager.Instance.CreatePolice();
        }
        public void Destroy()
        {
            PoliceManager.Instance.ClearPolice();
            onBeingDestroyed?.Invoke(this);
        }
    }

    // External Event Handlers
    void tryCheckIfMissionLauncherAndShowPopUp(CityTargetReachedEventData evt)
    {
        if (evt.ReachedObject is MissionLauncher missionLauncher)
        {
            ContextMenuManager.Instance.CreateContextMenu(evt.TriggerContext.TriggerView, CTX_Menu_Tools.MissionLauncherTrigger(missionLauncher));
        }
    }

    void onDestroyableDestroyed(IDestroyable destroyable)
    {
        Debug.Assert(destroyable != null);
        foreach (var kvp in missionOwnedObjects)
        {
            if (kvp.Value.Remove(destroyable))
                break;
        }
        destroyable.onBeingDestroyed -= onDestroyableDestroyed;
    }

    void showMissionCompletedInfo(MissionCompletedEventData missionCompletedEvent)
    {
        FixedContextMenuManager.Instance.CreateContextMenu(CTX_Menu_Tools.MissionCompletedInfo(missionCompletedEvent.Mission));
    }

    void showMissionFailedInfo(MissionFailedEventData missionFailedEvent)
    {
        FixedContextMenuManager.Instance.CreateContextMenu(CTX_Menu_Tools.MissionFailedInfo(missionFailedEvent.Mission));
    }

    void rewardPlayerForMissionCompletion(MissionCompletedEventData e)
    {
        var rewards = e.Mission.Config.RewardBundles;

        foreach (var reward in rewards)
        {
            var result = G.TransactionProcessor.Process(reward.CreateTransaction());
            if (result.Type != TransactionResultType.Success)
            {
                Debug.LogError($"Failed to process mission reward transaction: {result.Type}");
            }
        }
    }

    #region GameState controlling
    void enterMissionStateOnMissionStart(MissionStartedEventData e)
    {
        MissionGameState missionGameState = new MissionGameState(G.GameState.FocusedCar, e.Mission);
        G.Instance.GameFlowController.SetGameState(missionGameState);
    }
    void exitMissionStateOnMissionCompleted(MissionCompletedEventData e)
    {
        FreeRoamGameState missionGameState = new FreeRoamGameState(G.GameState.FocusedCar);
        G.Instance.GameFlowController.SetGameState(missionGameState);
    }
    void exitMissionStateOnMissionFailed(MissionFailedEventData e)
    {
        FreeRoamGameState missionGameState = new FreeRoamGameState(G.GameState.FocusedCar);
        G.Instance.GameFlowController.SetGameState(missionGameState);
    }
    #endregion
}

public class MissionLauncher : CityObject
{
    public readonly MissionLauncherConfig Config;
    public readonly MissionRuntime MissionRuntime;
    public MissionLauncher( string name, string infoText, ILocation location, City.CityMarker marker, PinStyle pinStyle, MissionRuntime missionRuntime, MissionLauncherConfig launcherConfig) : base(name, infoText, location, marker, pinStyle: pinStyle)
    {
        this.MissionRuntime = missionRuntime;
        this.Config = launcherConfig;
    }
}