using System.Collections.Generic;
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
        GameEvents.Instance.OnLocatableDestroyed += onLocatableDestroyed;
        GameEvents.Instance.OnTargetReached += tryCheckIfMissionLauncherAndShowPopUp;
        GameEvents.Instance.onMissionCompleted += showMissionCompletedInfo;
        GameEvents.Instance.onMissionCompleted += rewardPlayerForMissionCompletion;

        // External to mission event handlers
        monoBehaviourHelper.OnUpdateEvent += OnUpdate;
        GameEvents.Instance.OnTargetReached += OnCityTargetReached;
        GameEvents.Instance.OnPlayerAccept += OnPlayerAccepted;
    }

    public void Disable()
    {
        GameEvents.Instance.OnLocatableDestroyed -= onLocatableDestroyed;
        GameEvents.Instance.OnTargetReached -= tryCheckIfMissionLauncherAndShowPopUp;
        GameEvents.Instance.onMissionCompleted -= showMissionCompletedInfo;
        GameEvents.Instance.onMissionCompleted -= rewardPlayerForMissionCompletion;

        monoBehaviourHelper.OnUpdateEvent -= OnUpdate;
        GameEvents.Instance.OnTargetReached -= OnCityTargetReached;
        GameEvents.Instance.OnPlayerAccept -= OnPlayerAccepted;
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

    // External Event Handlers
    void tryCheckIfMissionLauncherAndShowPopUp(CityTargetReachedEventData evt)
    {
        if (evt.ReachedObject is MissionLauncher missionLauncher)
        {
            ContextMenuManager.Instance.CreateContextMenu(evt.TriggerContext.TriggerView, CTX_Menu_Tools.MissionLauncherTrigger(missionLauncher));
        }
    }

    void onLocatableDestroyed(LocatableDestroyedEventData data)
    {
        if(data.Locatable is IDestroyable destroyable)
        {
            foreach (var kvp in missionOwnedObjects)
            {
                if (kvp.Value.Remove(destroyable))
                    break;
            }
        }
    }

    void showMissionCompletedInfo(MissionCompletedEventData missionCompletedEvent)
    {
        FixedContextMenuManager.Instance.CreateContextMenu(CTX_Menu_Tools.MissionCompletedInfo(missionCompletedEvent.Mission));
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
}

public class MissionLauncher : CityObject
{
    public readonly MissionLauncherConfig Config;
    public readonly MissionRuntime MissionRuntime;
    public MissionLauncher( string name, string infoText, ILocation location, City.CityMarker marker, PinStyle pinStyle, MissionRuntime missionRuntime, MissionLauncherConfig launcherConfig) : base(name, infoText, location, marker, pinStyle)
    {
        this.MissionRuntime = missionRuntime;
        this.Config = launcherConfig;
    }
}