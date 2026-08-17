using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardsPopup : MonoBehaviour
{
    [SerializeField] private CubeSmasher cubeSmasher;
    private static TMP_FontAsset cachedFont;

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

        private void SetRowHeaderPill(Transform cellTr, string title, Color color, TMP_FontAsset font)
    {
        if (cellTr == null) return;
        Image cellImg = cellTr.GetComponent<Image>();
        if (cellImg != null)
        {
            cellImg.color = color;
        }

        TextMeshProUGUI textComp = cellTr.GetComponentInChildren<TextMeshProUGUI>();
        if (textComp != null)
        {
            if (font != null) textComp.font = font;
            textComp.text = title;
            textComp.fontSize = 20;
            textComp.fontStyle = FontStyles.Bold;
            textComp.color = Color.white;
            textComp.alignment = TextAlignmentOptions.Center;
        }
    }

    public void PopulateData(CubeSmasher smasher)
    {
        this.cubeSmasher = smasher != null ? smasher : FindFirstObjectByType<CubeSmasher>();

        int classicScore = (this.cubeSmasher != null) ? this.cubeSmasher.GetHighScore(CubeSmasher.Mode.Classic) : 0;
        int clockScore = (this.cubeSmasher != null) ? this.cubeSmasher.GetHighScore(CubeSmasher.Mode.Clock) : 0;
        int rackupScore = (this.cubeSmasher != null) ? this.cubeSmasher.GetHighScore(CubeSmasher.Mode.Rackup) : 0;

        int[] thresholds = new int[] { 750, 1200, 3000, 5000 };

        Color unlockedBadgeColor = new Color(0.18f, 0.80f, 0.44f, 1f); // Bright Green
        Color lockedBadgeColor = new Color(0.48f, 0.14f, 0.14f, 1f);   // Dark Burgundy Red

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
                        Image cellImg = cellTr.GetComponent<Image>();
                        if (cellImg != null) cellImg.color = Color.white;

                        int threshold = thresholds[c - 1];
                        bool isCompleted = currentScore >= threshold;

                        TextMeshProUGUI textComp = cellTr.GetComponentInChildren<TextMeshProUGUI>();
                        if (textComp != null)
                        {
                            if (isCompleted)
                            {
                                // Earned: Bold, slightly green, 110% size
                                textComp.text = $"<b><color=#27AE60><size=110%>{currentScore} / {threshold}</size></color></b>";
                            }
                            else
                            {
                                // Unearned/Future: Subdued grey, normal size, non-bold
                                textComp.text = $"<color=#7B8D9E>{currentScore} / {threshold}</color>";
                            }
                            textComp.alignment = TextAlignmentOptions.Center;
                        }
                    }
                }
            }

            // Populate Character Unlock Badges for Row 4 (Cell_4_1 through Cell_4_4)
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
                            badgeImg.color = isUnlocked ? unlockedBadgeColor : lockedBadgeColor;
                        }

                        TextMeshProUGUI badgeText = badgeTr.GetComponentInChildren<TextMeshProUGUI>();
                        if (badgeText != null)
                        {
                       
                            badgeText.text = isUnlocked ? "UNLOCKED" : "LOCKED";
                            badgeText.color = Color.white;
                            badgeText.fontStyle = FontStyles.Bold;
                        }
                    }
                }
            }
        }
    }
}
