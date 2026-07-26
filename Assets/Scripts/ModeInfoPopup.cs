using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ModeInfoPopup : MonoBehaviour
{
    [Header("UI References (Auto-assigned if empty)")]
    [SerializeField] private TMP_Text titleHeader;
    [SerializeField] private TMP_Text bodyContent;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button overlayButton;
    [SerializeField] private Button playModeButton;
    [SerializeField] private Button replayTutorialButton;

    private CubeSmasher cubeSmasher;
    private TutorialManager tutorialManager;
    private CubeSmasher.Mode currentMode;

    public static void ShowPopup(Transform canvasTr, CubeSmasher cubeSmasher, TutorialManager tutManager, CubeSmasher.Mode mode)
    {
        ModeInfoPopup popup = null;
        if (canvasTr != null)
        {
            popup = canvasTr.GetComponentInChildren<ModeInfoPopup>(true);
        }
        if (popup == null)
        {
            popup = FindFirstObjectByType<ModeInfoPopup>(FindObjectsInactive.Include);
        }

        if (popup != null)
        {
            popup.Show(cubeSmasher, tutManager, mode);
        }
    }

    public void Show(CubeSmasher smasher, TutorialManager tutManager, CubeSmasher.Mode mode)
    {
        this.cubeSmasher = smasher;
        this.tutorialManager = tutManager;
        this.currentMode = mode;

        AutoFindComponents();

        // 1. Set Title Header
        if (titleHeader != null)
        {
            titleHeader.text = GetModeTitle(currentMode);
        }

        // 2. Set Body Content Text
        if (bodyContent != null)
        {
            int highScore = (smasher != null) ? smasher.GetHighScore(currentMode) : 0;
            bodyContent.text = GetFormattedBodyText(currentMode, highScore);
        }

        // 3. Wire Close Button
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Hide);
        }

        // 4. Wire Overlay Backdrop Click
        if (overlayButton != null)
        {
            overlayButton.onClick.RemoveAllListeners();
            overlayButton.onClick.AddListener(Hide);
        }

        // 5. Wire Play Mode Button
        if (playModeButton != null)
        {
            playModeButton.onClick.RemoveAllListeners();
            playModeButton.onClick.AddListener(() =>
            {
                Hide();
                if (cubeSmasher != null)
                {
                    cubeSmasher.StartGameMode(currentMode);
                }
            });
        }

        // 6. Wire Replay Tutorial Button
        if (replayTutorialButton != null)
        {
            replayTutorialButton.onClick.RemoveAllListeners();
            replayTutorialButton.onClick.AddListener(() =>
            {
                Hide();
                if (tutorialManager != null)
                {
                    tutorialManager.ShowTutorialButtonPressed();
                }
            });
        }

        gameObject.SetActive(true);
        transform.SetAsLastSibling();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void AutoFindComponents()
    {
        if (titleHeader == null)
        {
            Transform tr = transform.Find("DialogCard/TitleHeader");
            if (tr == null) tr = transform.Find("TitleHeader");
            if (tr != null) titleHeader = tr.GetComponent<TMP_Text>();
        }

        if (bodyContent == null)
        {
            Transform tr = transform.Find("DialogCard/BodyContent");
            if (tr == null) tr = transform.Find("BodyContent");
            if (tr != null) bodyContent = tr.GetComponent<TMP_Text>();
        }

        if (closeButton == null)
        {
            Transform tr = transform.Find("DialogCard/CloseButton");
            if (tr == null) tr = transform.Find("CloseButton");
            if (tr != null) closeButton = tr.GetComponent<Button>();
        }

        if (overlayButton == null)
        {
            Transform tr = transform.Find("Overlay");
            if (tr != null) overlayButton = tr.GetComponent<Button>();
        }

        if (playModeButton == null)
        {
            Transform tr = transform.Find("DialogCard/ButtonsContainer/Btn_PlayMode");
            if (tr == null) tr = transform.Find("Btn_PlayMode");
            if (tr != null) playModeButton = tr.GetComponent<Button>();
        }

        if (replayTutorialButton == null)
        {
            Transform tr = transform.Find("DialogCard/ButtonsContainer/Btn_ReplayTutorial");
            if (tr == null) tr = transform.Find("Btn_ReplayTutorial");
            if (tr != null) replayTutorialButton = tr.GetComponent<Button>();
        }
    }

    private string GetModeTitle(CubeSmasher.Mode mode)
    {
        switch (mode)
        {
            case CubeSmasher.Mode.Rackup: return "Rack Up Points";
            case CubeSmasher.Mode.Clock: return "Beat The Clock";
            case CubeSmasher.Mode.Classic: return "Classic";
            default: return "Mode Info";
        }
    }

    private string GetFormattedBodyText(CubeSmasher.Mode mode, int highScore)
    {
        string modeDesc = "";
        string objective = "";

        switch (mode)
        {
            case CubeSmasher.Mode.Rackup:
                modeDesc = "<b>Beginner Mode:</b> Great for stress free play and also learning how to play Cube Smasher";
                objective = "<b>Objective:</b> Play free style and clear as many grids as you can to earn your highest score";
                break;
            case CubeSmasher.Mode.Clock:
                modeDesc = "<b>Intermediate Mode:</b> Race against the clock and work to clear the grid before time runs out";
                objective = "<b>Objective:</b> Get your highest score and clear as many grids as possible before time runs out";
                break;
            case CubeSmasher.Mode.Classic:
                modeDesc = "<b>Advanced Mode:</b> Expert level play. Clear as many grids as possibe before the boxes re-fill the grid";
                objective = "<b>Objective:</b> Boxes re-fill the grid, clear as many grids as you can and earn your highest score before the grid re-fills";
                break;
        }

        return $"{modeDesc}\n\n" +
               $"{objective}\n\n" +
               $"Earn Rewards for your high score:\n\n" +
               $"Pro: 750             Master: 1,200\n" +
               $"Legend: 3,000      Genius: 5,000\n\n" +
               $"<align=center><b>Your Current High Score: <color=#E67E22>{highScore}</color></b></align>";
    }
}
