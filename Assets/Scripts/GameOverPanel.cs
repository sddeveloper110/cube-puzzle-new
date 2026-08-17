using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class GameOverPanel : MonoBehaviour
{
    [Header("Title Elements")]
    [SerializeField] private RectTransform titleTransform;            // "Title" (Game Over)
    [SerializeField] private RectTransform highScorePartTransform;     // "HighScorePart"

    [Header("Score Header Banners (Image)")]
    [SerializeField] private RectTransform scoreHeaderImage;          // "RegularPart/ScoreContainer/Image"
    [SerializeField] private RectTransform highScoreHeaderImage;      // "HighScoreContainer/Image"

    [Header("Score Icons")]
    [SerializeField] private RectTransform scoreStarIcon;             // "RegularPart/ScoreContainer/ScoreStar"
    [SerializeField] private RectTransform highScoreCrownIcon;        // "HighScoreContainer/HighScoreCrown"

    [Header("Score Text Labels")]
    [SerializeField] private TextMeshProUGUI scoreTextLabel;          // "RegularPart/ScoreContainer/_ScoreText"
    [SerializeField] private TextMeshProUGUI highScoreTextLabel;      // "HighScoreContainer/HighScoreText"

    [Header("Containers")]
    [SerializeField] private RectTransform regularPartTransform;       // "RegularPart"
    [SerializeField] private RectTransform highScoreContainerTransform;// "HighScoreContainer"

    [Header("Buttons")]
    [SerializeField] private Button playAgainButton;                  // "playAgainButton"
    [SerializeField] private Button rewardsButton;                    // "RewardsBtn"
    [SerializeField] private Button backToTitleButton;                // "backToTitleButton"

    [Header("Effects & Popups")]
    [SerializeField] private GameObject ringBurstEffect;              // "HighScorePart/RightBurst (1)"
    [SerializeField] private GameObject confettiEffect;               // "HighScorePart/ConfettiFullscreen"
    [SerializeField] private RewardsPopup rewardsPopup;                // "RewardsPopup"

    [Header("Audio Clips")]
    [SerializeField] private AudioClip gameOverAudio;
    [SerializeField] private AudioClip countingUpSound;
    [SerializeField] private AudioClip highScoreAudio;

    private Vector2 origTitlePos;
    private Vector2 origScoreHeaderPos;
    private Vector2 origScoreStarPos;
    private Vector2 origHighScoreHeaderPos;
    private Vector2 origHighScoreCrownPos;
    private Vector2 origRegularPartPos;
    private Vector2 origHighScoreContainerPos;
    private bool positionsSaved = false;

    private Sequence animationSequence;
    private CubeSmasher smasherReference;

    private void Awake()
    {
        AutoSetupReferences();
    }

    [ContextMenu("⚡ Auto-Assign References From Hierarchy")]
    public void AutoSetupReferences()
    {
        if (titleTransform == null)
        {
            Transform t = transform.Find("Title");
            if (t != null) titleTransform = t.GetComponent<RectTransform>();
        }

        if (highScorePartTransform == null)
        {
            Transform t = transform.Find("HighScorePart");
            if (t != null) highScorePartTransform = t.GetComponent<RectTransform>();
        }

        if (regularPartTransform == null)
        {
            Transform t = transform.Find("RegularPart");
            if (t != null) regularPartTransform = t.GetComponent<RectTransform>();
        }

        if (highScoreContainerTransform == null)
        {
            Transform t = transform.Find("HighScoreContainer");
            if (t != null) highScoreContainerTransform = t.GetComponent<RectTransform>();
        }

        if (scoreHeaderImage == null)
        {
            Transform t = transform.Find("RegularPart/ScoreContainer/Image");
            if (t != null) scoreHeaderImage = t.GetComponent<RectTransform>();
        }

        if (scoreStarIcon == null)
        {
            Transform t = transform.Find("RegularPart/ScoreContainer/ScoreStar");
            if (t != null) scoreStarIcon = t.GetComponent<RectTransform>();
        }

        if (scoreTextLabel == null)
        {
            Transform t = transform.Find("RegularPart/ScoreContainer/_ScoreText");
            if (t == null) t = transform.Find("RegularPart/ScoreContainer/ScoreText");
            if (t != null) scoreTextLabel = t.GetComponent<TextMeshProUGUI>();
        }

        if (highScoreHeaderImage == null)
        {
            Transform t = transform.Find("HighScoreContainer/Image");
            if (t != null) highScoreHeaderImage = t.GetComponent<RectTransform>();
        }

        if (highScoreCrownIcon == null)
        {
            Transform t = transform.Find("HighScoreContainer/HighScoreCrown");
            if (t != null) highScoreCrownIcon = t.GetComponent<RectTransform>();
        }

        if (highScoreTextLabel == null)
        {
            Transform t = transform.Find("HighScoreContainer/HighScoreText");
            if (t != null) highScoreTextLabel = t.GetComponent<TextMeshProUGUI>();
        }

        if (playAgainButton == null)
        {
            Transform t = transform.Find("playAgainButton");
            if (t != null) playAgainButton = t.GetComponent<Button>();
        }

        if (rewardsButton == null)
        {
            Transform t = transform.Find("RewardsBtn") ?? transform.Find("CheckRewardsProgressButton");
            if (t != null) rewardsButton = t.GetComponent<Button>();
        }

        if (backToTitleButton == null)
        {
            Transform t = transform.Find("backToTitleButton");
            if (t != null) backToTitleButton = t.GetComponent<Button>();
        }

        if (ringBurstEffect == null)
        {
            Transform t = transform.Find("HighScorePart/RightBurst (1)") 
                       ?? transform.Find("HighScorePart/RightBurst") 
                       ?? transform.Find("RightBurst (1)")
                       ?? transform.Find("RightBurst");
            if (t != null) ringBurstEffect = t.gameObject;
        }

        if (confettiEffect == null)
        {
            Transform t = transform.Find("HighScorePart/ConfettiFullscreen") 
                       ?? transform.Find("ConfettiFullscreen");
            if (t != null) confettiEffect = t.gameObject;
        }

        if (rewardsPopup == null)
        {
            rewardsPopup = GetComponentInChildren<RewardsPopup>(true);
        }
    }

    private void SavePositions()
    {
        if (positionsSaved) return;

        AutoSetupReferences();

        if (titleTransform != null) origTitlePos = titleTransform.anchoredPosition;
        if (scoreHeaderImage != null) origScoreHeaderPos = scoreHeaderImage.anchoredPosition;
        if (scoreStarIcon != null) origScoreStarPos = scoreStarIcon.anchoredPosition;
        if (highScoreHeaderImage != null) origHighScoreHeaderPos = highScoreHeaderImage.anchoredPosition;
        if (highScoreCrownIcon != null) origHighScoreCrownPos = highScoreCrownIcon.anchoredPosition;
        if (regularPartTransform != null) origRegularPartPos = regularPartTransform.anchoredPosition;
        if (highScoreContainerTransform != null) origHighScoreContainerPos = highScoreContainerTransform.anchoredPosition;

        positionsSaved = true;
    }

    public void CancelAnimations()
    {
        animationSequence?.Kill();
        DOTween.Kill(this);
    }

    private void PlayAudioSound(AudioClip primaryClip, AudioClip fallbackClipFromSmasher)
    {
        AudioClip clipToPlay = primaryClip != null ? primaryClip : fallbackClipFromSmasher;
        if (clipToPlay != null)
        {
            AudioManager.PlayAudio(clipToPlay);
        }
    }

    private void PrepareButton(Button btn)
    {
        if (btn == null) return;
        btn.transform.DOKill();
        btn.transform.localScale = Vector3.zero;

        Component dotweenAnim = btn.GetComponent("DOTweenAnimation");
        if (dotweenAnim != null)
        {
            Behaviour b = dotweenAnim as Behaviour;
            if (b != null) b.enabled = false;
        }
    }

    private void PrepareInitialState()
    {
        SavePositions();

        // Disable Animator component to prevent old Unity animation conflict
        Animator anim = GetComponent<Animator>();
        if (anim != null) anim.enabled = false;

        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 0f;
        }

        if (ringBurstEffect != null) ringBurstEffect.SetActive(false);
        if (confettiEffect != null) confettiEffect.SetActive(false);

        // Title starts hidden at CENTER of screen
        if (titleTransform != null)
        {
            titleTransform.gameObject.SetActive(false);
            titleTransform.anchoredPosition = Vector2.zero; // Center of screen
            titleTransform.localScale = Vector3.zero;       // Scale 0
        }

        if (regularPartTransform != null)
        {
            regularPartTransform.gameObject.SetActive(true);
            regularPartTransform.anchoredPosition = origRegularPartPos;
            regularPartTransform.localScale = Vector3.one;
        }

        if (highScoreContainerTransform != null)
        {
            highScoreContainerTransform.gameObject.SetActive(true);
            highScoreContainerTransform.anchoredPosition = origHighScoreContainerPos;
            highScoreContainerTransform.localScale = Vector3.one;
        }

        if (scoreHeaderImage != null)
        {
            scoreHeaderImage.gameObject.SetActive(false);
            scoreHeaderImage.anchoredPosition = new Vector2(origScoreHeaderPos.x + 200f, origScoreHeaderPos.y);
            scoreHeaderImage.localScale = new Vector3(0.5f, 1.4f, 1f);
        }

        if (scoreStarIcon != null)
        {
            scoreStarIcon.gameObject.SetActive(false);
            scoreStarIcon.localScale = Vector3.zero;
        }

        if (highScoreHeaderImage != null)
        {
            highScoreHeaderImage.gameObject.SetActive(false);
            highScoreHeaderImage.anchoredPosition = new Vector2(origHighScoreHeaderPos.x + 200f, origHighScoreHeaderPos.y);
            highScoreHeaderImage.localScale = new Vector3(0.5f, 1.4f, 1f);
        }

        if (highScoreCrownIcon != null)
        {
            highScoreCrownIcon.gameObject.SetActive(false);
            highScoreCrownIcon.localScale = Vector3.zero;
        }

        PrepareButton(playAgainButton);
        PrepareButton(rewardsButton);
        PrepareButton(backToTitleButton);

        if (scoreTextLabel != null)
        {
            scoreTextLabel.gameObject.SetActive(false);
            scoreTextLabel.text = "0";
        }

        if (highScoreTextLabel != null)
        {
            highScoreTextLabel.gameObject.SetActive(false);
            highScoreTextLabel.text = "0";
        }
    }

    public void ResetUI()
    {
        SavePositions();

        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = 1f;

        if (titleTransform != null)
        {
            titleTransform.anchoredPosition = origTitlePos;
            titleTransform.localScale = Vector3.one;
        }
        if (scoreHeaderImage != null)
        {
            scoreHeaderImage.anchoredPosition = origScoreHeaderPos;
            scoreHeaderImage.localScale = Vector3.one;
        }
        if (scoreStarIcon != null)
        {
            scoreStarIcon.anchoredPosition = origScoreStarPos;
            scoreStarIcon.localScale = Vector3.one;
        }
        if (highScoreHeaderImage != null)
        {
            highScoreHeaderImage.anchoredPosition = origHighScoreHeaderPos;
            highScoreHeaderImage.localScale = Vector3.one;
        }
        if (highScoreCrownIcon != null)
        {
            highScoreCrownIcon.anchoredPosition = origHighScoreCrownPos;
            highScoreCrownIcon.localScale = Vector3.one;
        }

        if (playAgainButton != null) playAgainButton.transform.localScale = Vector3.one;
        if (rewardsButton != null) rewardsButton.transform.localScale = Vector3.one;
        if (backToTitleButton != null) backToTitleButton.transform.localScale = Vector3.one;

        if (ringBurstEffect != null) ringBurstEffect.SetActive(false);
        if (confettiEffect != null) confettiEffect.SetActive(false);
    }

    private void DoTallyCountUp(TextMeshProUGUI label, int finalValue, System.Action onComplete = null)
    {
        if (label == null)
        {
            onComplete?.Invoke();
            return;
        }

        label.gameObject.SetActive(true);

        if (finalValue <= 0)
        {
            label.text = "0";
            // Immediate quick stamp effect for 0 score (no long held delay!)
            label.transform.DOKill();
            label.transform.localScale = Vector3.one;
            label.transform.DOPunchScale(new Vector3(0.35f, 0.35f, 0f), 0.35f, 6, 0.5f)
                .OnComplete(() => onComplete?.Invoke());
            return;
        }

        PlayAudioSound(countingUpSound, smasherReference != null ? smasherReference.countingUpSound : null);

        int currentVal = 0;
        label.text = "0";

        // Quick dynamic count-up duration (0.8s - 1.2s max)
        float tallyDuration = Mathf.Clamp(0.8f + (finalValue / 500f) * 0.4f, 0.8f, 1.2f);

        DOTween.To(() => currentVal, x =>
        {
            currentVal = x;
            if (label != null) label.text = $"{x}";
        }, finalValue, tallyDuration)
        .SetEase(Ease.OutExpo)
        .SetTarget(this)
        .OnComplete(() =>
        {
            if (label != null)
            {
                label.text = $"{finalValue}";

                // Stamping Effect!
                label.transform.DOKill();
                label.transform.localScale = Vector3.one;
                label.transform.DOPunchScale(new Vector3(0.45f, 0.45f, 0f), 0.4f, 8, 0.6f);
            }

            onComplete?.Invoke();
        });
    }

    public void ShowRegularScore(CubeSmasher smasher, int score, int bestScore)
    {
        smasherReference = smasher;
        CancelAnimations();
        PrepareInitialState();

        transform.localScale = Vector3.one;
        gameObject.SetActive(true);

        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 0f;
            cg.DOFade(1f, 0.5f);
        }

        PlayAudioSound(gameOverAudio, smasherReference != null ? smasherReference.gameOverAudio : null);

        animationSequence = DOTween.Sequence().SetTarget(this);

        // 1️⃣ Title pops in CENTER smoothly -> pauses -> moves UP to top position
        if (titleTransform != null)
        {
            titleTransform.gameObject.SetActive(true);
            titleTransform.anchoredPosition = Vector2.zero; // Center of screen
            titleTransform.localScale = Vector3.zero;

            animationSequence.Append(titleTransform.DOScale(Vector3.one, 0.7f).SetEase(Ease.OutBack));
            animationSequence.AppendInterval(0.35f);
            animationSequence.Append(titleTransform.DOAnchorPos(origTitlePos, 0.75f).SetEase(Ease.OutCubic));
        }

        // 2️⃣ Score Banner Image comes in from Right + Star Icon pops in + Quick Tally Up + Stamp Effect
        if (scoreHeaderImage != null)
        {
            animationSequence.AppendCallback(() =>
            {
                scoreHeaderImage.gameObject.SetActive(true);
            });
            animationSequence.Append(scoreHeaderImage.DOAnchorPosX(origScoreHeaderPos.x, 0.5f).SetEase(Ease.OutCubic));
            animationSequence.Join(scoreHeaderImage.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack));

            if (scoreStarIcon != null)
            {
                animationSequence.AppendCallback(() =>
                {
                    scoreStarIcon.gameObject.SetActive(true);
                    scoreStarIcon.DOPunchRotation(new Vector3(0, 0, 18f), 0.45f, 6, 0.5f);
                });
                animationSequence.Append(scoreStarIcon.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack));
            }

            // Quick Count Up 0 -> Target Score -> Stamping Effect (No long hold if 0)
            animationSequence.AppendCallback(() =>
            {
                DoTallyCountUp(scoreTextLabel, score);
            });
            animationSequence.AppendInterval(score <= 0 ? 0.45f : 1.3f);
        }

        // 3️⃣ High Score Banner Image comes in from Right + Crown Icon pops in + Quick Tally Up + Stamp Effect
        if (highScoreHeaderImage != null)
        {
            animationSequence.AppendCallback(() =>
            {
                highScoreHeaderImage.gameObject.SetActive(true);
            });
            animationSequence.Append(highScoreHeaderImage.DOAnchorPosX(origHighScoreHeaderPos.x, 0.5f).SetEase(Ease.OutCubic));
            animationSequence.Join(highScoreHeaderImage.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack));

            if (highScoreCrownIcon != null)
            {
                animationSequence.AppendCallback(() =>
                {
                    highScoreCrownIcon.gameObject.SetActive(true);
                    highScoreCrownIcon.DOPunchRotation(new Vector3(0, 0, -18f), 0.45f, 6, 0.5f);
                });
                animationSequence.Append(highScoreCrownIcon.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack));
            }

            // Quick Count Up 0 -> Target High Score -> Stamping Effect (No long hold if 0)
            animationSequence.AppendCallback(() =>
            {
                DoTallyCountUp(highScoreTextLabel, bestScore);
            });
            animationSequence.AppendInterval(bestScore <= 0 ? 0.45f : 1.3f);
        }

        // 4️⃣ Three Buttons pop up one by one with juicy overshoot
        if (playAgainButton != null)
            animationSequence.Append(playAgainButton.transform.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack));
        if (rewardsButton != null)
            animationSequence.Append(rewardsButton.transform.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack));
        if (backToTitleButton != null)
            animationSequence.Append(backToTitleButton.transform.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack));
    }

    public void ShowHighScore(CubeSmasher smasher, int score, int bestScore)
    {
        smasherReference = smasher;
        CancelAnimations();
        PrepareInitialState();

        transform.localScale = Vector3.one;
        gameObject.SetActive(true);

        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 0f;
            cg.DOFade(1f, 0.5f);
        }

        PlayAudioSound(gameOverAudio, smasherReference != null ? smasherReference.gameOverAudio : null);

        animationSequence = DOTween.Sequence().SetTarget(this);

        // 1️⃣ Title pops in CENTER smoothly -> pauses -> moves UP to top position
        if (titleTransform != null)
        {
            titleTransform.gameObject.SetActive(true);
            titleTransform.anchoredPosition = Vector2.zero; // Center of screen
            titleTransform.localScale = Vector3.zero;

            animationSequence.Append(titleTransform.DOScale(Vector3.one, 0.7f).SetEase(Ease.OutBack));
            animationSequence.AppendInterval(0.35f);
            animationSequence.Append(titleTransform.DOAnchorPos(origTitlePos, 0.75f).SetEase(Ease.OutCubic));
        }

        // 2️⃣ Score Banner Image comes in from Right + Star Icon pops in + Quick Count Up + Stamp Effect
        if (scoreHeaderImage != null)
        {
            animationSequence.AppendCallback(() =>
            {
                scoreHeaderImage.gameObject.SetActive(true);
            });
            animationSequence.Append(scoreHeaderImage.DOAnchorPosX(origScoreHeaderPos.x, 0.5f).SetEase(Ease.OutCubic));
            animationSequence.Join(scoreHeaderImage.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack));

            if (scoreStarIcon != null)
            {
                animationSequence.AppendCallback(() =>
                {
                    scoreStarIcon.gameObject.SetActive(true);
                    scoreStarIcon.DOPunchRotation(new Vector3(0, 0, 18f), 0.45f, 6, 0.5f);
                });
                animationSequence.Append(scoreStarIcon.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack));
            }

            // Quick Count Up 0 -> Target Score -> Stamping Effect (No long hold if 0)
            animationSequence.AppendCallback(() =>
            {
                DoTallyCountUp(scoreTextLabel, score);
            });
            animationSequence.AppendInterval(score <= 0 ? 0.45f : 1.3f);
        }

        // 3️⃣ High Score Banner Image & Crown Icon enter -> Quick Count Up -> HIGH SCORE CELEBRATION MOMENT!
        if (highScoreHeaderImage != null)
        {
            animationSequence.AppendCallback(() =>
            {
                highScoreHeaderImage.gameObject.SetActive(true);
                if (highScorePartTransform != null) highScorePartTransform.gameObject.SetActive(true);
            });

            animationSequence.Append(highScoreHeaderImage.DOAnchorPosX(origHighScoreHeaderPos.x, 0.5f).SetEase(Ease.OutCubic));
            animationSequence.Join(highScoreHeaderImage.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack));

            if (highScoreCrownIcon != null)
            {
                animationSequence.AppendCallback(() =>
                {
                    highScoreCrownIcon.gameObject.SetActive(true);
                    highScoreCrownIcon.DOPunchRotation(new Vector3(0, 0, -22f), 0.45f, 6, 0.5f);
                });
                animationSequence.Append(highScoreCrownIcon.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack));
            }

            // Quick Count Up 0 -> Target High Score -> Celebration Stamp Effect
            animationSequence.AppendCallback(() =>
            {
                DoTallyCountUp(highScoreTextLabel, bestScore, () =>
                {
                    // CELEBRATION STAMP MOMENT AT FINISH OF COUNT UP:
                    PlayAudioSound(highScoreAudio, smasherReference != null ? smasherReference.highScoreAudio : null);

                    if (highScorePartTransform != null)
                        highScorePartTransform.gameObject.SetActive(true);

                    if (ringBurstEffect != null)
                    {
                        ringBurstEffect.SetActive(false);
                        ringBurstEffect.SetActive(true);
                    }

                    // Single clean confetti burst activation
                    TriggerConfettiBurst();

                    // Extra Celebration Stamp Effect + Crown & Header Wobble
                    if (highScoreTextLabel != null)
                    {
                        highScoreTextLabel.transform.DOKill();
                        highScoreTextLabel.transform.localScale = Vector3.one;
                        highScoreTextLabel.transform.DOPunchScale(new Vector3(0.55f, 0.55f, 0f), 0.5f, 10, 0.6f);
                    }

                    highScoreHeaderImage.DOPunchScale(new Vector3(0.25f, 0.25f, 0f), 0.5f, 6, 0.5f);

                    if (highScoreCrownIcon != null)
                    {
                        highScoreCrownIcon.DOPunchScale(new Vector3(0.35f, 0.35f, 0f), 0.5f, 8, 0.5f);
                        highScoreCrownIcon.DOPunchRotation(new Vector3(0, 0, 25f), 0.5f, 8, 0.5f);
                    }
                });
            });

            animationSequence.AppendInterval(bestScore <= 0 ? 0.6f : 1.5f);
        }

        // 4️⃣ PAUSE BEFORE BUTTONS APPEAR
        animationSequence.AppendInterval(1.0f);

        // Buttons pop up one by one
        if (playAgainButton != null)
            animationSequence.Append(playAgainButton.transform.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack));
        if (rewardsButton != null)
            animationSequence.Append(rewardsButton.transform.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack));
        if (backToTitleButton != null)
            animationSequence.Append(backToTitleButton.transform.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack));

        // 5️⃣ PAUSE BEFORE REWARDS POPUP PAGE APPEARS
        animationSequence.AppendInterval(1.5f);
        animationSequence.AppendCallback(() =>
        {
            if (rewardsPopup != null)
                rewardsPopup.Show(smasherReference);
            else
                RewardsPopup.ShowPopup(transform, smasherReference);
        });
    }

    private void TriggerConfettiBurst()
    {
        if (highScorePartTransform != null)
            highScorePartTransform.gameObject.SetActive(true);

        if (confettiEffect != null)
        {
            confettiEffect.SetActive(false);
            confettiEffect.SetActive(true);
        }
    }

    public void HidePanel()
    {
        CancelAnimations();
        ResetUI();
        gameObject.SetActive(false);
    }
}
