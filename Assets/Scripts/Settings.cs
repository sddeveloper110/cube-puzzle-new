using TMPro; // Use this if you are using TextMeshPro (recommended)
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking; // Required for UnityWebRequest.EscapeURL
using UnityEngine.EventSystems; // Required for IPointerClickHandler

public sealed class Settings : MonoBehaviour, IPointerClickHandler
{
    private const string SoundKey = "SOUND_ENABLED";

    [Header("UI Panels")]
    [SerializeField] private GameObject settingsPanel;

    [Header("UI Elements")]
    [SerializeField] private Toggle soundToggle;
    [SerializeField] private TMP_Text versionText; // Assign your Version Text object here
    [SerializeField] private TextMeshProUGUI consentText;

    [Header("Platform Specific IDs")]
    [Tooltip("Only required for iOS. Example: 123456789")]
    [SerializeField] private string iosAppID = "YOUR_APP_ID";

    [Header("Support Links")]
    [SerializeField] private string privacyPolicyURL = "https://yourwebsite.com/privacy";
    [SerializeField] private string termOfServiceURL = "https://yourwebsite.com/privacy";
    [SerializeField] private string ExploreMoreURL = "https://yourwebsite.com/privacy";

    public static bool IsSoundEnabled { get; private set; }

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        // Set Version Text automatically
        if (versionText != null)
        {
            versionText.text = "Version:" + Application.version;
        }

        IsSoundEnabled = PlayerPrefs.GetInt(SoundKey, 1) == 1;
        ApplySoundState(IsSoundEnabled);

        if (soundToggle != null)
        {
            soundToggle.SetIsOnWithoutNotify(IsSoundEnabled);
            soundToggle.onValueChanged.AddListener(ApplySoundState);
        }
    }

    private void ApplySoundState(bool isOn)
    {
        IsSoundEnabled = isOn;
        AudioListener.volume = isOn ? 1f : 0f;
        PlayerPrefs.SetInt(SoundKey, isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    // --- Helper to get Store URL automatically ---
    private string GetAutoStoreURL()
    {
#if UNITY_ANDROID
        // Google Play uses the Package Name automatically
        return "https://play.google.com/store/apps/details?id=" + Application.identifier;
#elif UNITY_IOS
        // iOS requires the specific App ID assigned in App Store Connect
        return "https://apps.apple.com/app/id" + iosAppID;
#else
        return "";
#endif
    }

#if UNITY_IOS && !UNITY_EDITOR
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void _iOS_ShareText(string text);
#endif

    // --- Share Functionality ---
    public void OnShareWithFriends()
    {
        string finalUrl = GetAutoStoreURL();
        string message = "Check out this awesome game! " + finalUrl;

#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
            AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");
            intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
            intentObject.Call<AndroidJavaObject>("setType", "text/plain");
            intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), message);
            
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            
            AndroidJavaObject chooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intentObject, "Share Cube Smasher");
            currentActivity.Call("startActivity", chooser);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Android Native Share failed: " + ex.Message);
            GUIUtility.systemCopyBuffer = message;
        }
#elif UNITY_IOS && !UNITY_EDITOR
        try
        {
            _iOS_ShareText(message);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("iOS Native Share failed: " + ex.Message);
            GUIUtility.systemCopyBuffer = message;
        }
#else
        // Fallback for editor or other platforms
        GUIUtility.systemCopyBuffer = message;
        Debug.Log("Link copied to clipboard (Editor Fallback): " + message);
#endif
    }

    // --- Rate Us Functionality ---
    public void OnRateUs()
    {
        string storeUrl = GetAutoStoreURL();

        if (!string.IsNullOrEmpty(storeUrl))
        {
            Application.OpenURL(storeUrl);
        }
        else
        {
            Debug.LogWarning("Store URL could not be determined for this platform.");
        }
    }

    // --- Navigation & Links ---
    public void OpenSettings() => settingsPanel.SetActive(true);
    public void CloseSettings() => settingsPanel.SetActive(false);

    public void OnPrivacyPolicy() => Application.OpenURL(privacyPolicyURL);
    public void OnTermsOfService() => Application.OpenURL(termOfServiceURL);
    public void OnExploreMore() => Application.OpenURL(ExploreMoreURL);
    public void OpenSocialLink(string url) => Application.OpenURL(url);

    [Header("Email Settings")]
    public string contactEmail = "support@example.com";
    public string defaultSubject = "Game Feedback";
    [TextArea(3, 10)]
    public string defaultBody = "Hello Support team,\n\nI wanted to reach out regarding...";

    public void OnContactUs()
    {
        OpenEmail(contactEmail, defaultSubject, defaultBody);
    }

    public void OpenEmail(string receiver, string subject, string body)
    {
        string escapedSubject = UnityWebRequest.EscapeURL(subject).Replace("+", "%20");
        string escapedBody = UnityWebRequest.EscapeURL(body).Replace("+", "%20");

        string url = $"mailto:{receiver}?subject={escapedSubject}&body={escapedBody}";
#if UNITY_EDITOR
        Debug.Log($"[Editor] Simulating Mail Launch: {url}");
#endif
        Application.OpenURL(url);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Text Clicked!"); // If this doesn't show in Console, Raycast Target is OFF
        if (consentText == null) return;

        int linkIndex = TMP_TextUtilities.FindIntersectingLink(consentText, eventData.position, eventData.pressEventCamera);
        Debug.Log("Link Index: " + linkIndex); // If this is -1, the math missed the link

        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = consentText.textInfo.linkInfo[linkIndex];
            string id = linkInfo.GetLinkID();
            Debug.Log("Link ID Found: " + id);

            if (id == "terms") OnTermsOfService();
            if (id == "privacy") OnPrivacyPolicy();
        }
    }
}