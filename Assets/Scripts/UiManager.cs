using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    public static UiManager Instance { get; private set; }
    public GameObject TimeUpPanel;
    public GameObject spaceRanoutPanel;
    public CubeSmasher cubeSmasher;
    public GameObject scoreParent,settingsImage;
    [Header("Game Over")][SerializeField] private Button playAgainButton;
    public void OpenTimeUpPanel(bool shouldOpen)
    {
        TimeUpPanel.SetActive(shouldOpen);
    }
    public void OpenSpaceRanOutPanel(bool shouldOpen)
    {
        spaceRanoutPanel.SetActive(shouldOpen);
    }
    public void SetUIForTutorial(bool tutorialStarted)
    {
        scoreParent.SetActive(!tutorialStarted);
        settingsImage.SetActive(!tutorialStarted);
    }
    //public static Action<CubeSmasher.Mode, int> OnGameEndingShowBanner;
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
       
    }
    private void Start()
    {
        playAgainButton.onClick.AddListener(() => PlayAgainButtonClicked());
    }
    void PlayAgainButtonClicked()
    {
        CheckForConsectieGamesAd(cubeSmasher.gameMode);
        cubeSmasher.StartGame();
    }
    public int proScore, masterScore, legendScore, geniusScore;
    public Animator classbuttonBannerAnim, beatClockBannerAnim, RackUpPointAnim;

    private Animator tempAnim=null;
    private void OnEnable()
    {
        // OnGameEndingShowBanner+= SetAnim;
        //Debug.LogError("enable");
        ConsectiveGamesAdCounter = 0;
    }
    private void OnDisable()
    {
       // OnGameEndingShowBanner-= SetAnim;
    }
    public void SetAnim(CubeSmasher.Mode mode, int score)
    {
        classbuttonBannerAnim.gameObject.SetActive(false);
        beatClockBannerAnim.gameObject.SetActive(false);
        RackUpPointAnim.gameObject.SetActive(false);
        if (score < proScore)
        {
            tempAnim = null;
            return;
        }
        Debug.Log("SetAnim called with mode: " + mode + " and score: " + score);
        tempAnim =mode is CubeSmasher.Mode.Classic? classbuttonBannerAnim:
           mode is CubeSmasher.Mode.Clock ? beatClockBannerAnim : RackUpPointAnim;
       tempScore= score;
       
    }
    int tempScore;
    public void SetRelativeTrigger()
    {
        if(tempAnim==null)
            return;
        tempAnim.gameObject.SetActive(true);
        if (tempScore >= geniusScore)
            tempAnim.SetTrigger("Genius");
        else if (tempScore >= legendScore)
            tempAnim.SetTrigger("Legend");
        else if (tempScore >= masterScore)
            tempAnim.SetTrigger("Master");
        else if (tempScore >= proScore)
            tempAnim.SetTrigger("Pro");
    }
    
    int ConsectiveGamesAdCounter = 0;
    CubeSmasher.Mode gameMode;
    bool once;
    public void CheckForConsectieGamesAd(CubeSmasher.Mode state,bool showAd=true)//, bool mustReset=false)
    {
        
        if (gameMode == state)
        {
            if (!once)
            {
                once = true;
                if(state==CubeSmasher.Mode.Classic) 
                return;
            }
            ConsectiveGamesAdCounter++;
        }
        else
        {
            gameMode = state;
            ConsectiveGamesAdCounter = 0;
        }
        //Debug.LogError("theres adsa " + ConsectiveGamesAdCounter);
        if(showAd &&  ConsectiveGamesAdCounter >= 3)
        {

            //FirebaseCall.placement= "inter_consecgames";
            FirebaseCall.placement = AdPlacement.inter_consecgames;
            AdmobAdsScript.Instance.ShowInterstitialAd();
            //ConsectiveGamesAdCounter = 0;
        }
        //if (mustReset)
        //{
        //    once = false;
        //    ConsectiveGamesAdCounter = 0;
        //    gameState= CubeSmasher.Mode.Classic;
        //}
    }

    public void ButtonClickedToBuyTimeAd()
    {
        //FirebaseCall.placement = AdPlacement. "rewarded_buytime_ad";
        AdmobAdsScript.Instance.ShowRewardedAd(AdWatchedToBuyTime);
    }
    void AdWatchedToBuyTime()
    {
        if (cubeSmasher.timeLeft<=0)
            cubeSmasher.timeLeft = 0;
        cubeSmasher.timeLeft += 60;

        OpenTimeUpPanel(false);
    }
    public void ButtonClickedToMakeSomeSpace()
    {
        //FirebaseCall.placement = "rewarded_makespace_ad";
        AdmobAdsScript.Instance.ShowRewardedAd(AdWatchedToMakeSomeSpace);
    }
    void AdWatchedToMakeSomeSpace()
    {
        cubeSmasher.ClearFirstRow();
        OpenSpaceRanOutPanel(false);
    }
    
    public void SetAllHelpCountersText(int remaingHelpCounter,int remaingTimeCounter,int remaingAddBoxCounter)
    {
        SetRemainingHelpCounterText(remaingHelpCounter);
        SetAddTimerCounterText(remaingTimeCounter);
        SetAddBoxCounterText(remaingAddBoxCounter);

    }
    public Text remaingHelpCounterText;
    public Text addTimerCounterText;
    public Text addBoxCounterText;
    public void SetRemainingHelpCounterText(int count)
    {
        remaingHelpCounterText.text = count.ToString();
    }
    public void SetAddTimerCounterText(int count)
    {
        addTimerCounterText.text = count.ToString();
    }
    public void SetAddBoxCounterText(int count)
    {
        addBoxCounterText.text = count.ToString();
    }
}
