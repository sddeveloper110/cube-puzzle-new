using UnityEngine;
using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance;
    // Add this field so `app` exists
    private Firebase.FirebaseApp app;

    private bool firebaseReady;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeFirebase();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeFirebase()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                app = Firebase.FirebaseApp.DefaultInstance;
                firebaseReady = true;
                // Set a flag here to indicate whether Firebase is ready to use by your app.
                Debug.LogError("Firebase is ready to use.");
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }

    public void LogEvent(string eventName)
    {
        if (!firebaseReady) return;

        FirebaseAnalytics.LogEvent(eventName);
    }

    public void LevelStarted(int level)
    {
        if (!firebaseReady) return;
        Debug.LogError("Logging level started event for level " + level);
        FirebaseAnalytics.LogEvent(
            "level_started",
            new Parameter("level_number", level)
        );
    }

 
    public void LevelCompleted(int level)
    {
        if (!firebaseReady) return;

        FirebaseAnalytics.LogEvent(
            "level_completed",
            new Parameter("level_number", level)
        );
    }
    public void levelFailed(int level)
    {
        //Debug.LogError ($"{level} failed.");
        if(firebaseReady) return;
        FirebaseAnalytics.LogEvent(
            "level_failed",
            new Parameter("level_number", level)
        );
    }

}