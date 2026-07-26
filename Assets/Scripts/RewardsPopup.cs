using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardsPopup : MonoBehaviour
{
    [SerializeField] private CubeSmasher cubeSmasher;
    private static TMP_FontAsset cachedFont;

    public static TMP_FontAsset GetGameFont()
    {
        if (cachedFont != null) return cachedFont;
        TMP_FontAsset[] fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
        foreach (var font in fonts)
        {
            if (font.name.Contains("Starplayer"))
            {
                cachedFont = font;
                return cachedFont;
            }
        }
        foreach (var font in fonts)
        {
            if (font.name.Contains("MightySouly"))
            {
                cachedFont = font;
                return cachedFont;
            }
        }
        if (fonts != null && fonts.Length > 0)
        {
            cachedFont = fonts[0];
            return cachedFont;
        }
        return null;
    }

    public static void ShowPopup(Transform parent, CubeSmasher cubeSmasher)
    {
        if (parent == null) return;

        RewardsPopup popup = parent.GetComponentInChildren<RewardsPopup>(true);
        if (popup == null)
        {
            Transform existingTr = parent.Find("RewardsPopup");
            if (existingTr != null)
            {
                popup = existingTr.GetComponent<RewardsPopup>();
                if (popup == null)
                {
                    popup = existingTr.gameObject.AddComponent<RewardsPopup>();
                }
            }
        }

        if (popup != null)
        {
            popup.gameObject.SetActive(true);
            popup.transform.SetAsLastSibling();
            popup.PopulateData(cubeSmasher);
        }
        else
        {
            // Fallback: Create runtime instance if missing from scene
            GameObject go = new GameObject("RewardsPopup", typeof(RectTransform), typeof(CanvasGroup), typeof(RewardsPopup));
            go.transform.SetParent(parent, false);
            go.transform.SetAsLastSibling();

            popup = go.GetComponent<RewardsPopup>();
            popup.BuildAndPopulateRuntime(cubeSmasher);
        }
    }

    public void Show(CubeSmasher smasher)
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        PopulateData(smasher);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void PopulateData(CubeSmasher smasher)
    {
        this.cubeSmasher = smasher != null ? smasher : FindFirstObjectByType<CubeSmasher>();
        if (this.cubeSmasher == null) return;

        TMP_FontAsset gameFont = GetGameFont();

        // Load high scores
        int classicScore = this.cubeSmasher.GetHighScore(CubeSmasher.Mode.Classic);
        int clockScore = this.cubeSmasher.GetHighScore(CubeSmasher.Mode.Clock);
        int rackupScore = this.cubeSmasher.GetHighScore(CubeSmasher.Mode.Rackup);

        int[] thresholds = new int[] { 750, 1200, 3000, 5000 };
        string[] resourceNames = new string[] { "Pro", "Master", "Legend", "Genius" };

        Color proColor = new Color(0.30f, 0.69f, 0.31f, 1f);   // Green
        Color masterColor = new Color(0.98f, 0.75f, 0.18f, 1f); // Yellow/Gold
        Color legendColor = new Color(1.00f, 0.60f, 0.00f, 1f); // Orange
        Color geniusColor = new Color(0.90f, 0.22f, 0.21f, 1f); // Red

        // Setup Close Button listener
        Transform closeBtnTr = transform.Find("Window/CloseButton");
        if (closeBtnTr == null) closeBtnTr = transform.Find("CloseButton");
        if (closeBtnTr != null)
        {
            Button btn = closeBtnTr.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(Hide);
            }
        }

        // Find TableContainer
        Transform tableContainer = transform.Find("Window/TableContainer");
        if (tableContainer == null) tableContainer = transform.Find("TableContainer");

        if (tableContainer != null)
        {
            // Populate score data for grid cells Cell_1_1 through Cell_3_4
            for (int r = 1; r <= 3; r++)
            {
                int currentScore = (r == 1) ? rackupScore : (r == 2) ? clockScore : classicScore;

                for (int c = 1; c <= 4; c++)
                {
                    Transform cellTr = tableContainer.Find($"Cell_{r}_{c}");
                    if (cellTr != null)
                    {
                        int threshold = thresholds[c - 1];
                        bool isCompleted = currentScore >= threshold;

                        TextMeshProUGUI textComp = cellTr.GetComponentInChildren<TextMeshProUGUI>();
                        if (textComp != null)
                        {
                            if (isCompleted)
                            {
                                textComp.text = $"<color=#2E7D32><size=150%>{currentScore}</size></color>\n<size=120%><color=#9A9A9A>/ {threshold}</color></size>";
                            }
                            else
                            {
                                textComp.text = $"<color=#CCCCCC><size=150%>{currentScore}</size></color>\n<size=120%><color=#9A9A9A>/ {threshold}</color></size>";
                            }
                        }
                    }
                }
            }

            // Populate Character Unlock Badges & Icons for Row 4 (Cell_4_1 through Cell_4_4)
            for (int c = 1; c <= 4; c++)
            {
                Transform cellTr = tableContainer.Find($"Cell_4_{c}");
                if (cellTr != null)
                {
                    int threshold = thresholds[c - 1];
                    bool isUnlocked = (classicScore >= threshold && clockScore >= threshold && rackupScore >= threshold);

                    // Update Badge Status (Icons are left untouched as manually placed in scene/prefab)
                    Transform badgeTr = cellTr.Find("Badge");
                    if (badgeTr != null)
                    {
                        Image badgeImg = badgeTr.GetComponent<Image>();
                        if (badgeImg != null)
                        {
                            badgeImg.color = isUnlocked ? proColor : new Color(0.47f, 0.56f, 0.61f, 1f);
                        }

                        TextMeshProUGUI badgeText = badgeTr.GetComponentInChildren<TextMeshProUGUI>();
                        if (badgeText != null)
                        {
                            badgeText.text = isUnlocked ? "UNLOCKED" : "LOCKED";
                            badgeText.color = Color.white;
                        }
                    }
                }
            }
        }
    }

    public void BuildAndPopulateRuntime(CubeSmasher smasher)
    {
        InitializeRuntimePanel(smasher);
        PopulateData(smasher);
    }

    private void InitializeRuntimePanel(CubeSmasher smasher)
    {
        this.cubeSmasher = smasher;
        TMP_FontAsset gameFont = GetGameFont();

        int[] thresholds = new int[] { 750, 1200, 3000, 5000 };
        string[] columns = new string[] { "PRO", "MASTER", "LEGEND", "GENIUS" };

        Color proColor = new Color(0.30f, 0.69f, 0.31f, 1f);   
        Color masterColor = new Color(0.98f, 0.75f, 0.18f, 1f); 
        Color legendColor = new Color(1.00f, 0.60f, 0.00f, 1f); 
        Color geniusColor = new Color(0.90f, 0.22f, 0.21f, 1f); 

        Color[] headerColors = new Color[] { proColor, masterColor, legendColor, geniusColor };
        Color[] headerTextColors = new Color[] { Color.white, new Color(0.18f, 0.18f, 0.18f, 1f), Color.white, Color.white };

        // Screen Overlay
        RectTransform rootRT = GetComponent<RectTransform>();
        rootRT.anchorMin = Vector2.zero;
        rootRT.anchorMax = Vector2.one;
        rootRT.sizeDelta = Vector2.zero;
        rootRT.anchoredPosition = Vector2.zero;

        Image overlayImage = gameObject.AddComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0.80f);

        // Main Card Window
        GameObject windowGO = new GameObject("Window", typeof(RectTransform), typeof(Image));
        windowGO.transform.SetParent(transform, false);
        RectTransform windowRT = windowGO.GetComponent<RectTransform>();
        windowRT.anchorMin = new Vector2(0.03f, 0.04f);
        windowRT.anchorMax = new Vector2(0.97f, 0.96f);
        windowRT.offsetMin = Vector2.zero;
        windowRT.offsetMax = Vector2.zero;

        Image windowImage = windowGO.GetComponent<Image>();
        windowImage.color = new Color(0.99f, 0.98f, 0.94f, 1f);

        // Close Button
        GameObject closeGO = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
        closeGO.transform.SetParent(windowGO.transform, false);
        RectTransform closeRT = closeGO.GetComponent<RectTransform>();
        closeRT.anchorMin = new Vector2(1f, 1f);
        closeRT.anchorMax = new Vector2(1f, 1f);
        closeRT.pivot = new Vector2(0.5f, 0.5f);
        closeRT.anchoredPosition = new Vector2(-10, -10);
        closeRT.sizeDelta = new Vector2(105, 105);

        Image closeImage = closeGO.GetComponent<Image>();
        closeImage.color = new Color(0.92f, 0.22f, 0.20f, 1f);

        Button closeButton = closeGO.GetComponent<Button>();
        closeButton.onClick.AddListener(Hide);

        GameObject closeTextGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        closeTextGO.transform.SetParent(closeGO.transform, false);
        RectTransform closeTextRT = closeTextGO.GetComponent<RectTransform>();
        closeTextRT.anchorMin = Vector2.zero;
        closeTextRT.anchorMax = Vector2.one;
        closeTextRT.sizeDelta = Vector2.zero;

        TextMeshProUGUI closeText = closeTextGO.GetComponent<TextMeshProUGUI>();
        if (gameFont != null) closeText.font = gameFont;
        closeText.text = "X";
        closeText.fontSize = 52;
        closeText.alignment = TextAlignmentOptions.Center;
        closeText.fontStyle = FontStyles.Bold;
        closeText.color = Color.white;

        // Header Section
        GameObject headerGO = new GameObject("HeaderSection", typeof(RectTransform));
        headerGO.transform.SetParent(windowGO.transform, false);
        RectTransform headerRT = headerGO.GetComponent<RectTransform>();
        headerRT.anchorMin = new Vector2(0f, 1f);
        headerRT.anchorMax = new Vector2(1f, 1f);
        headerRT.pivot = new Vector2(0.5f, 1f);
        headerRT.anchoredPosition = new Vector2(0, -30);
        headerRT.sizeDelta = new Vector2(0, 140);

        GameObject titleGO = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleGO.transform.SetParent(headerGO.transform, false);
        RectTransform titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0f, 1f);
        titleRT.anchorMax = new Vector2(1f, 1f);
        titleRT.pivot = new Vector2(0.5f, 1f);
        titleRT.anchoredPosition = new Vector2(0, 0);
        titleRT.sizeDelta = new Vector2(0, 75);

        TextMeshProUGUI titleText = titleGO.GetComponent<TextMeshProUGUI>();
        if (gameFont != null) titleText.font = gameFont;
        titleText.text = "REWARDS PROGRESS";
        titleText.fontSize = 56;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = new Color(0.90f, 0.45f, 0.05f, 1f);

        GameObject subtitleGO = new GameObject("SubtitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
        subtitleGO.transform.SetParent(headerGO.transform, false);
        RectTransform subtitleRT = subtitleGO.GetComponent<RectTransform>();
        subtitleRT.anchorMin = new Vector2(0f, 0f);
        subtitleRT.anchorMax = new Vector2(1f, 0f);
        subtitleRT.pivot = new Vector2(0.5f, 0f);
        subtitleRT.anchoredPosition = new Vector2(0, 10);
        subtitleRT.sizeDelta = new Vector2(0, 45);

        TextMeshProUGUI subtitleText = subtitleGO.GetComponent<TextMeshProUGUI>();
        if (gameFont != null) subtitleText.font = gameFont;
        subtitleText.text = "Keep up the great work!";
        subtitleText.fontSize = 30;
        subtitleText.alignment = TextAlignmentOptions.Center;
        subtitleText.color = new Color(0.29f, 0.23f, 0.20f, 1f);

        // Table Container
        GameObject tableGO = new GameObject("TableContainer", typeof(RectTransform), typeof(Image));
        tableGO.transform.SetParent(windowGO.transform, false);
        RectTransform tableRT = tableGO.GetComponent<RectTransform>();
        tableRT.anchorMin = new Vector2(0f, 0f);
        tableRT.anchorMax = new Vector2(1f, 1f);
        tableRT.offsetMin = new Vector2(25, 30);
        tableRT.offsetMax = new Vector2(-25, -185);

        Image tableImage = tableGO.GetComponent<Image>();
        tableImage.color = new Color(0.85f, 0.82f, 0.78f, 1f);

        float col0W = 235f;
        float colTiersW = 175f;
        float row0H = 120f;
        float rowDataH = 310f;
        float row4H = 420f;

        for (int r = 0; r < 5; r++)
        {
            for (int c = 0; c < 5; c++)
            {
                float cellW = (c == 0) ? col0W : colTiersW;
                float cellH = (r == 0) ? row0H : (r == 4) ? row4H : rowDataH;

                float xPos = (c == 0) ? 15f : 15f + col0W + (c - 1) * colTiersW;
                float yPos = -(r == 0 ? 15f : 15f + row0H + (r - 1) * rowDataH);

                GameObject cellGO = new GameObject($"Cell_{r}_{c}", typeof(RectTransform), typeof(Image));
                cellGO.transform.SetParent(tableGO.transform, false);
                RectTransform cellRT = cellGO.GetComponent<RectTransform>();
                cellRT.anchorMin = new Vector2(0, 1);
                cellRT.anchorMax = new Vector2(0, 1);
                cellRT.pivot = new Vector2(0, 1);
                cellRT.sizeDelta = new Vector2(cellW - 4, cellH - 4);
                cellRT.anchoredPosition = new Vector2(xPos, yPos);

                Image cellImage = cellGO.GetComponent<Image>();
                cellImage.color = Color.white;

                if (r == 0)
                {
                    if (c > 0)
                    {
                        cellImage.color = headerColors[c - 1];

                        GameObject textGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                        textGO.transform.SetParent(cellGO.transform, false);
                        RectTransform textRT = textGO.GetComponent<RectTransform>();
                        textRT.anchorMin = Vector2.zero;
                        textRT.anchorMax = Vector2.one;
                        textRT.sizeDelta = Vector2.zero;

                        TextMeshProUGUI textComp = textGO.GetComponent<TextMeshProUGUI>();
                        if (gameFont != null) textComp.font = gameFont;
                        textComp.text = $"{columns[c - 1]}\n<size=75%>{thresholds[c - 1]:N0} PTS</size>";
                        textComp.fontSize = 30;
                        textComp.fontStyle = FontStyles.Bold;
                        textComp.alignment = TextAlignmentOptions.Center;
                        textComp.color = headerTextColors[c - 1];
                    }
                    else
                    {
                        cellImage.color = new Color(0.94f, 0.93f, 0.90f, 1f);
                    }
                }
                else if (c == 0)
                {
                    if (r == 1) cellImage.color = proColor;
                    else if (r == 2) cellImage.color = masterColor;
                    else if (r == 3) cellImage.color = legendColor;
                    else if (r == 4) cellImage.color = new Color(0.94f, 0.93f, 0.90f, 1f);

                    GameObject textGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                    textGO.transform.SetParent(cellGO.transform, false);
                    RectTransform textRT = textGO.GetComponent<RectTransform>();
                    textRT.anchorMin = Vector2.zero;
                    textRT.anchorMax = Vector2.one;
                    textRT.sizeDelta = Vector2.zero;

                    TextMeshProUGUI textComp = textGO.GetComponent<TextMeshProUGUI>();
                    if (gameFont != null) textComp.font = gameFont;
                    textComp.fontSize = 24;
                    textComp.fontStyle = FontStyles.Bold;
                    textComp.alignment = TextAlignmentOptions.Center;

                    if (r == 1)
                    {
                        textComp.text = "RACK UP\nPOINTS";
                        textComp.color = Color.white;
                    }
                    else if (r == 2)
                    {
                        textComp.text = "BEAT THE\nCLOCK";
                        textComp.color = new Color(0.18f, 0.18f, 0.18f, 1f);
                    }
                    else if (r == 3)
                    {
                        textComp.text = "CLASSIC\nLEVELS";
                        textComp.color = Color.white;
                    }
                    else if (r == 4)
                    {
                        textComp.text = "UNLOCK\nCHARACTERS\n<size=65%><color=#546E7A>Earn Pro, Master, Legend, Genius in all 3 modes</color></size>";
                        textComp.fontSize = 22;
                        textComp.color = new Color(0.18f, 0.18f, 0.18f, 1f);
                    }
                }
                else if (r < 4)
                {
                    GameObject textGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                    textGO.transform.SetParent(cellGO.transform, false);
                    RectTransform textRT = textGO.GetComponent<RectTransform>();
                    textRT.anchorMin = Vector2.zero;
                    textRT.anchorMax = Vector2.one;
                    textRT.sizeDelta = Vector2.zero;
                }
                else
                {
                    GameObject iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                    iconGO.transform.SetParent(cellGO.transform, false);
                    RectTransform iconRT = iconGO.GetComponent<RectTransform>();
                    iconRT.anchorMin = new Vector2(0.5f, 1f);
                    iconRT.anchorMax = new Vector2(0.5f, 1f);
                    iconRT.pivot = new Vector2(0.5f, 1f);
                    iconRT.anchoredPosition = new Vector2(0, -25);
                    iconRT.sizeDelta = new Vector2(115, 115);

                    GameObject badgeGO = new GameObject("Badge", typeof(RectTransform), typeof(Image));
                    badgeGO.transform.SetParent(cellGO.transform, false);
                    RectTransform badgeRT = badgeGO.GetComponent<RectTransform>();
                    badgeRT.anchorMin = new Vector2(0.5f, 0f);
                    badgeRT.anchorMax = new Vector2(0.5f, 0f);
                    badgeRT.pivot = new Vector2(0.5f, 0f);
                    badgeRT.anchoredPosition = new Vector2(0, 30);
                    badgeRT.sizeDelta = new Vector2(150, 48);

                    GameObject badgeTextGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                    badgeTextGO.transform.SetParent(badgeGO.transform, false);
                    RectTransform badgeTextRT = badgeTextGO.GetComponent<RectTransform>();
                    badgeTextRT.anchorMin = Vector2.zero;
                    badgeTextRT.anchorMax = Vector2.one;
                    badgeTextRT.sizeDelta = Vector2.zero;
                }
            }
        }
    }
}
