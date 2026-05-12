using DG.Tweening;
using System;
using Unity.VisualScripting;
using UnityEngine;

public class MenuPanelScript : MonoBehaviour
{
    private static readonly int ProHash = Animator.StringToHash("Pro");
    private static readonly int MasterHash = Animator.StringToHash("Master");
    private static readonly int LegendHash = Animator.StringToHash("Legend");
    private static readonly int GeniusHash = Animator.StringToHash("Genius");
    [Header("Score Thresholds")]
    public int proScore, masterScore, legendScore, geniusScore;

    [Header("Animators")]
    public Animator classButtonBannerAnim;
    public Animator beatClockBannerAnim;
    public Animator rackUpPointAnim;

    public Animator AllMaxedBanner;
    public Animator logoAnimator;
    public static Action<CubeSmasher.Mode, int> OnGameEndingShowBanner;
    public CubeSmasher cubeSmasher;

    public int LogoRank {
        get { return PlayerPrefs.GetInt("LogoRank", 0); }
        private set { PlayerPrefs.SetInt("LogoRank", value); } }

    int tempClassicScore, tempClockScore, tempRackUpScore;

    void Awake()
    {
        OnGameEndingShowBanner += SetAnim;
    }
    private void OnEnable()
    {
        // Get all scores once
        tempClassicScore = cubeSmasher.GetHighScore(CubeSmasher.Mode.Classic);
        tempClockScore = cubeSmasher.GetHighScore(CubeSmasher.Mode.Clock);
        tempRackUpScore = cubeSmasher.GetHighScore(CubeSmasher.Mode.Rackup);

        //Debug.LogError($"Temp Classic Score: {tempClassicScore}");
        //Debug.LogError($"Temp Clock Score: {tempClockScore}");
        //Debug.LogError($"Temp RackUp Score: {tempRackUpScore}");

        // Apply score ranks
        SetRankTrigger(classButtonBannerAnim, tempClassicScore);
        SetRankTrigger(beatClockBannerAnim, tempClockScore);
        SetRankTrigger(rackUpPointAnim, tempRackUpScore);
        CheckForLogoRankAtStart();
    }
    void CheckForLogoRankAtStart()
    {
        if(LogoRank == 1)
            logoAnimator.SetTrigger(ProHash);
        else if (LogoRank == 2)
            logoAnimator.SetTrigger(MasterHash);
        else if (LogoRank == 3)
            logoAnimator.SetTrigger(LegendHash);
        else if (LogoRank == 4)
            logoAnimator.SetTrigger(GeniusHash);
    }

    public void CheckForAllMaxed()
    {
        //Debug.LogError("Checking for all maxed...");
        if (PlayerPrefs.GetInt("allPro",0)==0 && tempClassicScore >= proScore 
            && tempClockScore >= proScore && tempRackUpScore >= proScore)
        {
            PlayerPrefs.SetInt("allPro", 1);
            AllMaxedBanner.SetTrigger("Pro");
            LogoRank = 1;
            DOVirtual.DelayedCall(3, CheckLogoTransition);
        }
        if (PlayerPrefs.GetInt("allMaster",0)==0 && tempClassicScore >= masterScore 
            && tempClockScore >= masterScore && tempRackUpScore >= masterScore)
        {
            PlayerPrefs.SetInt("allMaster", 1);
            AllMaxedBanner.SetTrigger("Master");
            LogoRank = 2;
            DOVirtual.DelayedCall(3, CheckLogoTransition);
        }
        if (PlayerPrefs.GetInt("allLegend",0)==0 && tempClassicScore >= legendScore 
            && tempClockScore >= legendScore && tempRackUpScore >= legendScore)
        {
            PlayerPrefs.SetInt("allLegend", 1); 
            AllMaxedBanner.SetTrigger("Legend");
            LogoRank = 3;
            DOVirtual.DelayedCall(3, CheckLogoTransition);
        }
        if(PlayerPrefs.GetInt("allGenius",0)==0 && tempClassicScore >= geniusScore 
            && tempClockScore >= geniusScore && tempRackUpScore >= geniusScore)
        {
            PlayerPrefs.SetInt("allGenius", 1);
            AllMaxedBanner.SetTrigger("Genius");
            LogoRank = 4;
            DOVirtual.DelayedCall(3, CheckLogoTransition);
        }
    }
    void CheckLogoTransition()
    {
        if(LogoRank == 1)
            logoAnimator.SetBool("ProBool", true);
        else if (LogoRank == 2)
        {
            logoAnimator.SetBool("ProBool", true);
            logoAnimator.SetBool("MasterBool", true);
        }
        else if (LogoRank == 3)
        {
            logoAnimator.SetBool("ProBool", true);
            logoAnimator.SetBool("MasterBool", true);
            logoAnimator.SetBool("LegendBool", true);
        }
        else if (LogoRank == 4)
            logoAnimator.SetBool("GeniusBool", true);
    }

    void OnDestroy()
    {
        OnGameEndingShowBanner -= SetAnim;
    }

    /// <summary>
    /// Maps score to the correct animation trigger.
    /// </summary>
    private void SetRankTrigger(Animator animator, int score)
    {
        if(score < proScore || tempAnim==animator)
            return;
        if (score >= geniusScore)
            animator.SetTrigger("Genius2");
        else if (score >= legendScore)
            animator.SetTrigger("Legend2");
        else if (score >= masterScore)
            animator.SetTrigger("Master2");
        else if (score >= proScore)
            animator.SetTrigger("Pro2");
    }
    Animator tempAnim = null;
    int tempScore = 0;

    public void SetAnim(CubeSmasher.Mode mode, int score)
    {
        if (score < proScore  || score < cubeSmasher.GetHighScore(mode))
        {
            tempAnim = null;
            return;
        }
        if (mode is CubeSmasher.Mode.Classic)
        {
            tempAnim = classButtonBannerAnim;
        }
        else if (mode is CubeSmasher.Mode.Clock)
        {
            tempAnim = beatClockBannerAnim;
        }
        else if (mode is CubeSmasher.Mode.Rackup)
        {
            tempAnim = rackUpPointAnim;
        }
        tempScore = score;
    }
   

    public void SetRelativeTrigger(int value)
    {
        if (tempAnim == null)
            return;
        if (value == 0 && tempAnim != rackUpPointAnim) return;
        if (value == 1 && tempAnim != beatClockBannerAnim) return;
        if(value == 2 && tempAnim != classButtonBannerAnim) return;

        if (tempScore >= geniusScore)
            tempAnim.SetTrigger("Genius");
        else if (tempScore >= legendScore)
            tempAnim.SetTrigger("Legend");
        else if (tempScore >= masterScore)
            tempAnim.SetTrigger("Master");
        else if (tempScore >= proScore)
            tempAnim.SetTrigger("Pro");
        tempAnim= null;
        tempScore= 0;
    }
    public GameObject policyPanel;
    public void OpenFirstTimePolicyLink()
    {
        if (PlayerPrefs.GetInt("FirstTimePolicy", 0) == 1)
            return;
          
        policyPanel.SetActive(PlayerPrefs.GetInt("FirstTimePolicy", 0) == 0);
    }
    public void OnPressingAcceptPolicyButton()
    {
        policyPanel.SetActive(false);
        PlayerPrefs.SetInt("FirstTimePolicy", 1);
    }
}
