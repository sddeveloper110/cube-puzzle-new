using UnityEngine;
using GoogleMobileAds.Api;
using System;
using GoogleMobileAds.Common;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class AdmobAdsScript : MonoBehaviour
{
    public static AdmobAdsScript Instance { get; private set; }
    public bool showAds;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keeps the object alive across scenes.
        }
        else
        {
            Destroy(gameObject); // Destroys any duplicate instances that may be created.
        }
        //AppStateEventNotifier.AppStateChanged += OnAppStateChanged;
    }
    //private void OnDestroy()
    //{
    //    //AppStateEventNotifier.AppStateChanged -= OnAppStateChanged;
    //}

    public string appId = "ca-app-pub-3940256099942544~3347511713";


//#if UNITY_ANDROID
   public string bannerId = "ca-app-pub-3940256099942544/6300978111";
   public string interId = "ca-app-pub-3940256099942544/1033173712";
   public string rewardedId = "ca-app-pub-3940256099942544/5224354917";


//#elif UNITY_IPHONE
//    string bannerId = "ca-app-pub-3940256099942544/2934735716";
//    string interId = "ca-app-pub-3940256099942544/4411468910";
//    string rewardedId = "ca-app-pub-3940256099942544/1712485313";
//    string nativeId = "ca-app-pub-8962685452714136/3641769406";

//#endif

    BannerView bannerView;
    InterstitialAd interstitialAd;
    RewardedAd rewardedAd;
   
    private void Start()
    {
        MobileAds.RaiseAdEventsOnUnityMainThread = true;
        MobileAds.Initialize(initStatus => {

            print("Ads Initialised !!");
            //LoadBannerAd();
            //LoadAppOpenAd();
            LoadInterstitialAd();
            LoadRewardedAd();
        });
        
        //Invoke(nameof(ShowAppOpenAd), 4);
       /* #region AppOpen Initialization

        //Invoke("ShowAppOpenAd", 15f);
        #endregion AppOpen Initialization*/
    }

    #region Banner

    public void LoadBannerAd()
    {
        //create a banner
        CreateBannerView();

        //listen to banner events
        ListenToBannerEvents();

        //load the banner
        if (bannerView == null)
        {
            CreateBannerView();
        }

        var adRequest = new AdRequest();
        adRequest.Keywords.Add("unity-admob-sample");

        print("Loading banner Ad !!");
        bannerView.LoadAd(adRequest);
    }
    void CreateBannerView()
    {

        if (bannerView != null)
        {
            DestroyBannerAd();
        }
        bannerView = new BannerView(bannerId, AdSize.Banner, AdPosition.Bottom);


    }
    void ListenToBannerEvents()
    {
        bannerView.OnBannerAdLoaded += () =>
        {
            Debug.Log("Banner view loaded an ad with response : "
                + bannerView.GetResponseInfo());
        };
        // Raised when an ad fails to load into the banner view.
        bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            Debug.LogError("Banner view failed to load an ad with error : "
                + error);
        };
        // Raised when the ad is estimated to have earned money.
        bannerView.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log("Banner view paid {0} {1}." +
                adValue.Value +
                adValue.CurrencyCode);
        };
        // Raised when an impression is recorded for an ad.
        bannerView.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Banner view recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        bannerView.OnAdClicked += () =>
        {
            Debug.Log("Banner view was clicked.");
        };
        // Raised when an ad opened full screen content.
        bannerView.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Banner view full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        bannerView.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Banner view full screen content closed.");
        };
    }
    public void DestroyBannerAd()
    {

        if (bannerView != null)
        {
            print("Destroying banner Ad");
            bannerView.Destroy();
            bannerView = null;
        }
    }
    #endregion

    #region Interstitial

    public void LoadInterstitialAd()
    {

        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }
        var adRequest = new AdRequest();
        adRequest.Keywords.Add("unity-admob-sample");

        InterstitialAd.Load(interId, adRequest, (InterstitialAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                print("Interstitial ad failed to load" + error);
                return;
            }
            print("Interstitial ad loaded !!" + ad.GetResponseInfo());

            interstitialAd = ad;
            InterstitialEvent(interstitialAd);
        });
    }
    public void ShowInterstitialAd()
    {

        if (interstitialAd != null && interstitialAd.CanShowAd())
        {
            interstitialAd.Show();
        }
        else
        {
           
            print("Intersititial ad not ready!!");
        }
    }
    public void InterstitialEvent(InterstitialAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log("Interstitial ad paid {0} {1}." +
                adValue.Value +
                adValue.CurrencyCode);
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Interstitial ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("Interstitial ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Interstitial ad full screen content opened.");
            AudioListener.pause = true;
            if (FirebaseCall.Instance != null)
                FirebaseCall.Instance.LogAdStarted();
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Interstitial ad full screen content closed.");
            LoadInterstitialAd();
            AudioListener.pause = false;
            if (FirebaseCall.Instance != null)
                FirebaseCall.Instance.LogAdCompleted();
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Interstitial ad failed to open full screen content " +
                           "with error : " + error);
            LoadInterstitialAd();
        };
    }

    #endregion

    #region Rewarded

    public void LoadRewardedAd()
    {
        if(showAds)
        {
            if (rewardedAd != null)
            {
                rewardedAd.Destroy();
                rewardedAd = null;
            }
            var adRequest = new AdRequest();
            adRequest.Keywords.Add("unity-admob-sample");

            RewardedAd.Load(rewardedId, adRequest, (RewardedAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    print("Rewarded failed to load" + error);
                    return;
                }

                print("Rewarded ad loaded !!");
                rewardedAd = ad;
                RewardedAdEvents(rewardedAd);
            });
        }

    }
    public void ShowRewardedAd(Action onRewardEarned = null)
    {
        if (showAds && rewardedAd != null && rewardedAd.CanShowAd())
        {
            rewardedAd.Show((Reward reward) =>
            {
                Debug.Log($"User earned reward: {reward.Amount} {reward.Type}");
                onRewardEarned?.Invoke();
            });
        }
        else
        {
            Debug.LogWarning("Rewarded ad not ready yet or ads disabled. Triggering fallback timer...");
            LoadRewardedAd();

            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas != null)
            {
                AdFallbackPopup.Create(canvas, onRewardEarned);
            }
            else
            {
                Debug.LogError("No Canvas found to display Ad Fallback. Rewarding immediately.");
                onRewardEarned?.Invoke();
            }
        }
    }

    public void RewardedAdEvents(RewardedAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log("Rewarded ad paid {0} {1}." +
                adValue.Value +
                adValue.CurrencyCode);
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Rewarded ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("Rewarded ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Rewarded ad full screen content opened.");
            if (FirebaseCall.Instance != null)
                FirebaseCall.Instance.LogAdStarted();
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded ad full screen content closed.");
            LoadRewardedAd();
            if(FirebaseCall.Instance!=null)
            FirebaseCall.Instance.LogAdCompleted();
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded ad failed to open full screen content " +
                           "with error : " + error);
            LoadRewardedAd();
        };
    }




    #endregion


    private AppOpenAd appOpenAdd;
    string AppOpenID = "ca-app-pub-8962685452714136/3397392678";
    private DateTime loadTime;
   
    public void LoadAppOpenAd()
    {
        if (showAds)
        {
            if (appOpenAdd != null)
            {
                appOpenAdd.Destroy();
                appOpenAdd = null;
            }

            //Debug.Log("Loading the app open ad.");

            // Create our request used to load the ad.
            var adRequest = new AdRequest();

            // send the request to load the ad.
            AppOpenAd.Load(AppOpenID, adRequest,
            (AppOpenAd ad, LoadAdError error) =>
            {
                    // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("app open ad failed to load an ad " +
                                       "with error : " + error);
                    return;
                }

                Debug.Log("App open ad loaded with response : "
                    + ad.GetResponseInfo());

                appOpenAdd = ad;
                    RegisterEventHandlers(ad);
            });
        }
            
    }
    private void RegisterEventHandlers(AppOpenAd ad)
    {

        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("App open ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("App open ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("App open ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("App open ad full screen content opened.");
           
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("App open ad full screen content closed.");
            LoadAppOpenAd();
           

        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("App open ad failed to open full screen content " +
                           "with error : " + error);
            LoadAppOpenAd();
        };
    }
    /// <summary>
    /// Shows the app open ad.
    /// </summary>
    public void ShowAppOpenAd()
    {
        if(showAds)
        {
            if (appOpenAdd != null && appOpenAdd.CanShowAd())
            {
                Debug.Log("Showing app open ad.");
                appOpenAdd.Show();
            }
            else
            {
                LoadAppOpenAd();
                Debug.LogError("App open ad is not ready yet.");
            }
        }
    }

    public void OnApplicationPause(bool paused)
    {
        // Display the app open ad when the app is foregrounded
        if (!paused)
        {
            if (showAds)
            {
                //ShowAppOpenAd();
            }
        }
    }
}

public class AdFallbackPopup : MonoBehaviour
{
    private System.Action onComplete;
    private TextMeshProUGUI descText;
    private int secondsLeft = 10;

    public static void Create(Canvas canvas, System.Action onComplete)
    {
        GameObject overlayGo = new GameObject("AdFallbackOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        overlayGo.transform.SetParent(canvas.transform, false);

        RectTransform overlayRt = overlayGo.GetComponent<RectTransform>();
        overlayRt.anchorMin = Vector2.zero;
        overlayRt.anchorMax = Vector2.one;
        overlayRt.pivot = new Vector2(0.5f, 0.5f);
        overlayRt.offsetMin = Vector2.zero;
        overlayRt.offsetMax = Vector2.zero;

        Image overlayImg = overlayGo.GetComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0f, 0.75f); // semi-transparent dim

        // Create Dialog Box
        GameObject dialogGo = new GameObject("Dialog", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        dialogGo.transform.SetParent(overlayGo.transform, false);

        RectTransform dialogRt = dialogGo.GetComponent<RectTransform>();
        dialogRt.anchorMin = new Vector2(0.5f, 0.5f);
        dialogRt.anchorMax = new Vector2(0.5f, 0.5f);
        dialogRt.pivot = new Vector2(0.5f, 0.5f);
        dialogRt.sizeDelta = new Vector2(550f, 320f);

        Image dialogImg = dialogGo.GetComponent<Image>();
        // Sleek modern grey panel
        dialogImg.color = new Color(0.12f, 0.13f, 0.15f, 1f); 

        // Create Title Text
        GameObject titleGo = new GameObject("Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        titleGo.transform.SetParent(dialogGo.transform, false);

        RectTransform titleRt = titleGo.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.5f, 1f);
        titleRt.anchorMax = new Vector2(0.5f, 1f);
        titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.anchoredPosition = new Vector2(0f, -30f);
        titleRt.sizeDelta = new Vector2(500f, 50f);

        TextMeshProUGUI titleTmp = titleGo.GetComponent<TextMeshProUGUI>();
        titleTmp.text = "Ad Unavailable";
        titleTmp.fontSize = 32;
        titleTmp.fontStyle = FontStyles.Bold;
        titleTmp.color = Color.white;
        titleTmp.alignment = TextAlignmentOptions.Center;

        // Create Message Text
        GameObject msgGo = new GameObject("Message", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        msgGo.transform.SetParent(dialogGo.transform, false);

        RectTransform msgRt = msgGo.GetComponent<RectTransform>();
        msgRt.anchorMin = Vector2.zero;
        msgRt.anchorMax = Vector2.one;
        msgRt.pivot = new Vector2(0.5f, 0.5f);
        msgRt.offsetMin = new Vector2(20f, 20f);
        msgRt.offsetMax = new Vector2(-20f, -80f);

        TextMeshProUGUI msgTmp = msgGo.GetComponent<TextMeshProUGUI>();
        msgTmp.fontSize = 24;
        msgTmp.color = new Color(0.8f, 0.82f, 0.85f, 1f);
        msgTmp.alignment = TextAlignmentOptions.Center;

        AdFallbackPopup popup = overlayGo.AddComponent<AdFallbackPopup>();
        popup.onComplete = onComplete;
        popup.descText = msgTmp;
        popup.StartCountdown();
    }

    private void StartCountdown()
    {
        StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        while (secondsLeft > 0)
        {
            descText.text = $"Ad is unable to load at this time.\n\nYou will be rewarded in\n<color=#F8B531><size=32><b>{secondsLeft} seconds</b></size></color>";
            yield return new WaitForSecondsRealtime(1.0f);
            secondsLeft--;
        }

        descText.text = "Rewarding you now...";
        yield return new WaitForSecondsRealtime(0.5f);

        // Grant reward
        onComplete?.Invoke();

        // Destroy panel
        Destroy(gameObject);
    }
}



