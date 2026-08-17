using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ModeInfoPopup : MonoBehaviour
{
    [Header("UI References (Auto-assigned if empty)")]
    [SerializeField] private TMP_Text titleHeader;
    [SerializeField] private TMP_Text bodyContent;
    [SerializeField] private TMP_Text highScoreText;
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

        // 1. Set Title Header with Golden Yellow color tag (Preserves font)
        if (titleHeader != null)
        {
            titleHeader.text = $"<color=#FFC107>{GetModeTitle(currentMode)}</color>";
        }

        // 2. Set Body Content Text with Golden Yellow headings and white text (Preserves font)
        if (bodyContent != null)
        {
            bodyContent.text = GetFormattedBodyText(currentMode);
        }

        // 3. Set High Score Text (Preserves font)
        int highScore = (smasher != null) ? smasher.GetHighScore(currentMode) : 0;
        if (highScoreText != null)
        {
            highScoreText.text = $"Your Current High Score: <color=#FFC107>{highScore}</color>";
        }

        // 4. Update 2x2 Reward Tier Boxes (Preserves font)
        UpdateRewardBoxes();

        // 5. Wire Close Button
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Hide);
        }

        // 6. Wire Overlay Backdrop Click
        if (overlayButton != null)
        {
            overlayButton.onClick.RemoveAllListeners();
            overlayButton.onClick.AddListener(Hide);
        }

        // 7. Wire Play Mode Button
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

        // 8. Wire Replay Tutorial Button
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

    [ContextMenu("Setup UI / Create Reward Boxes")]
    public void SetupUI()
    {
        // 1. Root Screen Background (Dark overlay)
        Image rootImg = GetComponent<Image>();
        if (rootImg == null) rootImg = gameObject.AddComponent<Image>();
        rootImg.color = new Color(0.02f, 0.04f, 0.10f, 0.85f);

        // 2. Dialog Card Window
        Transform cardTr = transform.Find("DialogCard");
        if (cardTr == null) cardTr = transform.Find("Window");
        if (cardTr != null)
        {
            Image cardImg = cardTr.GetComponent<Image>();
            if (cardImg != null)
            {
                cardImg.color = new Color(0.133f, 0.180f, 0.392f, 1f); // #222E64 Dark Navy Blue
            }

            Outline cardOutline = cardTr.GetComponent<Outline>();
            if (cardOutline == null) cardOutline = cardTr.gameObject.AddComponent<Outline>();
            cardOutline.effectColor = new Color(0.36f, 0.57f, 0.97f, 1f); // #5C92F7 Cyan/Blue outline
            cardOutline.effectDistance = new Vector2(3f, -3f);

            // 3. Title Header
            Transform titleTr = cardTr.Find("TitleHeader");
            if (titleTr != null)
            {
                TextMeshProUGUI titleTmp = titleTr.GetComponent<TextMeshProUGUI>();
                if (titleTmp != null)
                {
                    titleTmp.color = new Color(1.0f, 0.757f, 0.027f, 1f); // #FFC107 Golden Yellow
                    titleTmp.fontStyle = FontStyles.Bold;
                    titleTmp.fontSize = 36;
                    titleTmp.alignment = TextAlignmentOptions.Center;
                }
            }

            // 4. Body Content
            Transform bodyTr = cardTr.Find("BodyContent");
            if (bodyTr != null)
            {
                TextMeshProUGUI bodyTmp = bodyTr.GetComponent<TextMeshProUGUI>();
                if (bodyTmp != null)
                {
                    bodyTmp.color = Color.white;
                    bodyTmp.fontSize = 20;
                    bodyTmp.lineSpacing = 4f;
                    bodyTmp.alignment = TextAlignmentOptions.TopLeft;
                }
            }

            // 5. 2x2 Reward Boxes Container
            Transform boxesContainer = cardTr.Find("RewardBoxesContainer");
            if (boxesContainer == null)
            {
                GameObject containerGo = new GameObject("RewardBoxesContainer", typeof(RectTransform));
                containerGo.transform.SetParent(cardTr, false);
                boxesContainer = containerGo.transform;
            }

            RectTransform containerRt = boxesContainer.GetComponent<RectTransform>();
            containerRt.anchorMin = new Vector2(0.5f, 0.5f);
            containerRt.anchorMax = new Vector2(0.5f, 0.5f);
            containerRt.pivot = new Vector2(0.5f, 0.5f);
            containerRt.anchoredPosition = new Vector2(0f, -20f);
            containerRt.sizeDelta = new Vector2(530f, 180f);

            string[] names = new string[] { "PRO", "MASTER", "LEGEND", "GENIUS" };
            string[] scores = new string[] { "750", "1,200", "3,000", "5,000" };
            Vector2[] positions = new Vector2[]
            {
                new Vector2(-135f, 45f),  // Top Left
                new Vector2(135f, 45f),   // Top Right
                new Vector2(-135f, -45f), // Bottom Left
                new Vector2(135f, -45f)   // Bottom Right
            };

            for (int i = 0; i < 4; i++)
            {
                Transform boxTr = boxesContainer.Find($"Box_{names[i]}");
                if (boxTr == null)
                {
                    GameObject boxGo = new GameObject($"Box_{names[i]}", typeof(RectTransform), typeof(Image), typeof(Outline));
                    boxGo.transform.SetParent(boxesContainer, false);
                    boxTr = boxGo.transform;
                }

                RectTransform boxRt = boxTr.GetComponent<RectTransform>();
                boxRt.anchorMin = new Vector2(0.5f, 0.5f);
                boxRt.anchorMax = new Vector2(0.5f, 0.5f);
                boxRt.pivot = new Vector2(0.5f, 0.5f);
                boxRt.anchoredPosition = positions[i];
                boxRt.sizeDelta = new Vector2(245f, 75f);

                Image boxImg = boxTr.GetComponent<Image>();
                boxImg.color = new Color(0.11f, 0.15f, 0.32f, 1f);

                Outline boxOutline = boxTr.GetComponent<Outline>();
                if (boxOutline == null) boxOutline = boxTr.gameObject.AddComponent<Outline>();
                boxOutline.effectColor = new Color(0.29f, 0.41f, 0.74f, 0.8f);
                boxOutline.effectDistance = new Vector2(1.5f, -1.5f);

                Transform textTr = boxTr.Find("Text");
                if (textTr == null)
                {
                    GameObject textGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                    textGo.transform.SetParent(boxTr, false);
                    textTr = textGo.transform;
                }

                RectTransform textRt = textTr.GetComponent<RectTransform>();
                textRt.anchorMin = Vector2.zero;
                textRt.anchorMax = Vector2.one;
                textRt.offsetMin = Vector2.zero;
                textRt.offsetMax = Vector2.zero;

                TextMeshProUGUI textTmp = textTr.GetComponent<TextMeshProUGUI>();
                textTmp.text = $"<color=#FFC107><b>{names[i]}</b></color>\n<color=#FFFFFF><b>{scores[i]}</b></color>";
                textTmp.fontSize = 18;
                textTmp.alignment = TextAlignmentOptions.Center;
                textTmp.lineSpacing = 2f;
            }

            // 6. High Score Text
            Transform highScoreTr = cardTr.Find("HighScoreText");
            if (highScoreTr == null)
            {
                GameObject hsGo = new GameObject("HighScoreText", typeof(RectTransform), typeof(TextMeshProUGUI));
                hsGo.transform.SetParent(cardTr, false);
                highScoreTr = hsGo.transform;
            }

            RectTransform hsRt = highScoreTr.GetComponent<RectTransform>();
            hsRt.anchorMin = new Vector2(0.5f, 0.5f);
            hsRt.anchorMax = new Vector2(0.5f, 0.5f);
            hsRt.pivot = new Vector2(0.5f, 0.5f);
            hsRt.anchoredPosition = new Vector2(0f, -135f);
            hsRt.sizeDelta = new Vector2(500f, 40f);

            TextMeshProUGUI hsTmp = highScoreTr.GetComponent<TextMeshProUGUI>();
            hsTmp.fontSize = 22;
            hsTmp.fontStyle = FontStyles.Bold;
            hsTmp.color = Color.white;
            hsTmp.alignment = TextAlignmentOptions.Center;
        }

        AutoFindComponents();
    }

    private void UpdateRewardBoxes()
    {
        Transform boxesContainer = transform.Find("DialogCard/RewardBoxesContainer");
        if (boxesContainer == null) boxesContainer = transform.Find("RewardBoxesContainer");
        if (boxesContainer == null) return;

        string[] names = new string[] { "PRO", "MASTER", "LEGEND", "GENIUS" };
        string[] scores = new string[] { "750", "1,200", "3,000", "5,000" };

        for (int i = 0; i < 4; i++)
        {
            Transform boxTr = boxesContainer.Find($"Box_{names[i]}");
            if (boxTr != null)
            {
                TextMeshProUGUI tmp = boxTr.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null)
                {
                    // Only update text values & rich text colors - preserve existing font!
                    tmp.text = $"<color=#FFC107><b>{names[i]}</b></color>\n<color=#FFFFFF><b>{scores[i]}</b></color>";
                }
            }
        }
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

        if (highScoreText == null)
        {
            Transform tr = transform.Find("DialogCard/HighScoreText");
            if (tr == null) tr = transform.Find("HighScoreText");
            if (tr != null) highScoreText = tr.GetComponent<TMP_Text>();
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

    private string GetFormattedBodyText(CubeSmasher.Mode mode)
    {
        string modeDesc = "";
        string objective = "";

        switch (mode)
        {
            case CubeSmasher.Mode.Rackup:
                modeDesc = "<color=#FFC107><b>Beginner Mode:</b></color> <color=#FFFFFF>Great for stress free play and also learning how to play Cube Smasher</color>";
                objective = "<color=#FFC107><b>Objective:</b></color> <color=#FFFFFF>Play free style and clear as many grids as you can to earn your highest score</color>";
                break;
            case CubeSmasher.Mode.Clock:
                modeDesc = "<color=#FFC107><b>Intermediate Mode:</b></color> <color=#FFFFFF>Race against the clock and work to clear the grid before time runs out.</color>";
                objective = "<color=#FFC107><b>Objective:</b></color> <color=#FFFFFF>Get your highest score and clear as many grids as possible before time runs out.</color>";
                break;
            case CubeSmasher.Mode.Classic:
                modeDesc = "<color=#FFC107><b>Advanced Mode:</b></color> <color=#FFFFFF>Expert level play. Clear as many grids as possible before the boxes re-fill the grid</color>";
                objective = "<color=#FFC107><b>Objective:</b></color> <color=#FFFFFF>Boxes re-fill the grid, clear as many grids as you can and earn your highest score before the grid re-fills</color>";
                break;
        }

        return $"{modeDesc}\n\n" +
               $"{objective}\n\n" +
               $"<align=center><color=#FFFFFF><b>Earn rewards for your high score</b></color></align>";
    }
}
