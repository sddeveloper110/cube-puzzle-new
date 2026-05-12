using UnityEngine;
using GoogleMobileAds.Api;
using System;
using GoogleMobileAds.Common;
using UnityEngine.SceneManagement;

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
        if (!showAds)
        {
            return;
        }

        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            rewardedAd.Show((Reward reward) =>
            {
                Debug.Log($"User earned reward: {reward.Amount} {reward.Type}");
                onRewardEarned?.Invoke();
            });
        }
        else
        {
            Debug.LogWarning("Rewarded ad not ready yet. Reloading...");
            LoadRewardedAd();
      
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



