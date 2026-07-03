using UnityEngine;
using UnityEditor;

public class FirebaseEventTester : EditorWindow
{
    private string testEventName = "test_event_editor";

    [MenuItem("Software District/Firebase Event Tester")]
    public static void ShowWindow()
    {
        GetWindow<FirebaseEventTester>("Firebase Tester");
    }

    private void OnGUI()
    {
        GUILayout.Label("Firebase Analytics Event Tester", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Display current state
        bool isPlaying = Application.isPlaying;
        EditorGUILayout.LabelField("Editor Play State", isPlaying ? "PLAYING" : "STOPPED");

        if (isPlaying)
        {
            if (FirebaseCall.Instance != null)
            {
                EditorGUILayout.LabelField("FirebaseCall Instance", "Active", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Firebase Initialized", FirebaseCall.Instance.firebaseInitialized.ToString());
            }
            else
            {
                EditorGUILayout.HelpBox("FirebaseCall Instance not found in the current scene. Please start from the 'Splash' scene to ensure FirebaseCall is initialized.", MessageType.Warning);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Enter Play Mode to test active runtime events.", MessageType.Info);
        }

        EditorGUILayout.Space();
        GUILayout.Label("Trigger Test Event", EditorStyles.boldLabel);
        testEventName = EditorGUILayout.TextField("Event Name", testEventName);

        if (GUILayout.Button("Log Test Event"))
        {
            if (isPlaying && FirebaseCall.Instance != null)
            {
                FirebaseCall.Instance.LogEvent(testEventName);
                Debug.Log($"[Firebase Tester] Dispatched test event: '{testEventName}' to FirebaseCall.");
            }
            else
            {
                // If not playing, we can try using the static FirebaseAnalytics class directly
                try
                {
                    Firebase.Analytics.FirebaseAnalytics.LogEvent(testEventName);
                    Debug.Log($"[Firebase Tester] Dispatched test event: '{testEventName}' directly via FirebaseAnalytics static API.");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[Firebase Tester] Failed to log event directly: {ex.Message}. Enter Play Mode with Splash scene loaded to test.");
                }
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "How to test Firebase Events:\n\n" +
            "1. Open and play the game from the 'Splash' scene (Splash.unity).\n" +
            "2. Observe the Console logs in Unity. All events (e.g. game start, tutorial events) will print a log prefix '[FirebaseCall] LogEvent'.\n" +
            "3. If testing on a real device, you can use the Firebase DebugView by running: \n" +
            "   adb shell setprop debug.firebase.analytics.app <package_name>\n" +
            "   And then viewing live logs in the Firebase Console's DebugView.",
            MessageType.None
        );
    }
}
