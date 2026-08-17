using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;
public class FirebaseCall : MonoBehaviour
{
    public static FirebaseCall Instance;
    public static AdPlacement placement;

    //public static FirebaseRemoteConfiguration remoteConfiguration;
    private List<(string eventName, Parameter[] parameters)> queuedEvents = new List<(string, Parameter[])>();
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        DontDestroyOnLoad(Instance);
        OnFireBase();
        //remoteConfiguration = GetComponent<FirebaseRemoteConfiguration>();
    }
    #region Firebase
    DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;
    [HideInInspector]
    public bool firebaseInitialized = false;
    void OnFireBase()
    {
        Debug.Log("[FirebaseCall] Checking and resolving Firebase dependencies...");
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }
    void InitializeFirebase()
    {
        Debug.Log("[FirebaseCall] Initializing Firebase Analytics...");
        FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);

        // Set the user's sign up method.
        FirebaseAnalytics.SetUserProperty(
            FirebaseAnalytics.UserPropertySignUpMethod,
            "Google");
        firebaseInitialized = true;
        Debug.Log("[FirebaseCall] Firebase Analytics successfully initialized!");

        // Send any queued events
        foreach (var evt in queuedEvents)
        {
            FirebaseAnalytics.LogEvent(evt.eventName, evt.parameters);
            Debug.Log($"[FirebaseCall] Sent queued event: {evt.eventName}");
        }
        queuedEvents.Clear();

        FirebaseApp app = FirebaseApp.DefaultInstance;
        FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventLevelStart);
        Debug.Log($"[FirebaseCall] LogEvent: {FirebaseAnalytics.EventLevelStart} (LevelStart)");
        //Invoke("SendInternetConnectionEvent", 2.0f);
        //remoteConfiguration.StartRemoteConfig();
    }
    public void Event(string name)
    {
        Debug.Log($"[FirebaseCall] Event called: {name}");
        if (firebaseInitialized)
        {
            FirebaseAnalytics.LogEvent(name);
        }
    }

    public void LogEvent(string eventName, params Parameter[] parameters)
    {
        if (string.IsNullOrEmpty(eventName)) return;

        string paramStr = "";
        if (parameters != null && parameters.Length > 0)
        {
            paramStr = $" with {parameters.Length} parameters";
        }

        if (firebaseInitialized)
        {
            Debug.Log($"[FirebaseCall] LogEvent (Immediate): {eventName}{paramStr}");
            FirebaseAnalytics.LogEvent(eventName, parameters);
        }
        else
        {
            Debug.Log($"[FirebaseCall] LogEvent (Queued): {eventName}{paramStr}");
            queuedEvents.Add((eventName, parameters));
        }
    }


    public void SendInternetConnectionEvent()
    {
        int InternetStatus = Application.internetReachability == NetworkReachability.NotReachable ? 0 : 1;
        LogEvent("internetConnection" + InternetStatus);
        //Debug.Log("internetConnection" + InternetStatus);
    }
    #endregion

    //public void LogTutorialEvent(string origin, string result)
    //{
    //    Parameter[] parameters = {
    //    new Parameter("tutorial_origin", origin),
    //    new Parameter("tutorial_result", result)
    //};
    //    //Debug.LogError(origin + " " + result);
    //    LogEvent("tutorial_status", parameters);
    //}
   

   
    public void LogTutorialStarted(TutorialOrigin origin)
    {
        FirebaseCall.Instance?.LogEvent(origin switch
        {
            TutorialOrigin.ModeSelection => "tutorial_mode_selection_started",
            TutorialOrigin.HowToPlay => "tutorial_how_to_play_started",
            _ => null
        });
    }

    public void LogTutorialCompleted(TutorialOrigin origin)
    {
        FirebaseCall.Instance?.LogEvent(origin switch
        {
            TutorialOrigin.ModeSelection => "tutorial_mode_selection_completed",
            TutorialOrigin.HowToPlay => "tutorial_how_to_play_completed",
            _ => null
        });
    }

    public void LogTutorialSkipped(TutorialOrigin origin)
    {
        FirebaseCall.Instance?.LogEvent(origin switch
        {
            TutorialOrigin.ModeSelection => "tutorial_mode_selection_skipped",
            TutorialOrigin.HowToPlay => "tutorial_how_to_play_skipped",
            _ => null
        });
    }
    public void LogAdStarted()
    {
        LogEvent(AdEvents[placement].start);
    }
    public void LogAdCompleted()
    {
        LogEvent(AdEvents[placement].complete);
    }

    public void LogMultiModeStarted(string previousMode = null, string currentMode = null)
    {
        if (!string.IsNullOrEmpty(previousMode) && !string.IsNullOrEmpty(currentMode))
        {
            Parameter[] parameters = {
                new Parameter("previous_mode", previousMode),
                new Parameter("current_mode", currentMode)
            };
            LogEvent("multi_mode_started", parameters);
        }
        else
        {
            LogEvent("multi_mode_started");
        }
    }
   
     private static readonly Dictionary<AdPlacement, (string start, string complete)> AdEvents = new()
    {
        { AdPlacement.rwd_help,      ("ad_rwd_start_help",      "ad_rwd_complete_help") },
        { AdPlacement.rwd_time,     ("ad_rwd_start_time",     "ad_rwd_complete_time") },
        { AdPlacement.rwd_box,  ("ad_rwd_start_box",  "ad_rwd_complete_box") },
        { AdPlacement.inter_consecgames, ("ad_inter_start_consecgms", "ad_inter_complete_consecgms") },
    };
}
public enum TutorialOrigin
{
    ModeSelection,
    HowToPlay
}
public enum AdPlacement
{
    rwd_help,
    rwd_time,
    rwd_box,
    inter_consecgames,
}
