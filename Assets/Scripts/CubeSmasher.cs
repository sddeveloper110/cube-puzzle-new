using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using Sirenix.OdinInspector;
using Firebase.Analytics;



#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
using Random = UnityEngine.Random;

[RequireComponent(typeof(AudioSource))]
public class CubeSmasher : MonoBehaviour
{
    [Header("UI Buttons")] [SerializeField]
    private Button startClassicButton;

    [SerializeField] private Button rackUpButton;
    [SerializeField] private Button beatTheClockButton;
    [SerializeField] private Button howToPlayButton;

    [Header("Mode Info Buttons")]
    [SerializeField] private Button rackUpInfoButton;
    [SerializeField] private Button clockInfoButton;
    [SerializeField] private Button classicInfoButton;
    [SerializeField] private ModeInfoPopup modeInfoPopup;


    [Header("Game Settings")] [SerializeField]
    private UIDragBox boxPrefab;

    [SerializeField] private RectTransform boxParent;
    [SerializeField] public Button addBoxButton;

    [SerializeField] public Button addTimeButton;
    [SerializeField] private Button exitGameButton;
    [SerializeField] public Button helpButton;
    //[SerializeField] private List<Image> helpHearts;
    //[SerializeField] private Image heartIcon;

    [SerializeField] private TMP_Text scoreLabel;
    [SerializeField] private TMP_Text gameEndScoreLabel;
    [SerializeField] private TMP_Text gameEndHighScoreLabel;
    [SerializeField] private TMP_Text bestScoreLabel;
    [SerializeField] private TMP_Text timerLabel;
    [SerializeField] private TMP_Text onScoreIncreaseTxt;
    [SerializeField] private GameObject scoreAddImage50;
    [SerializeField] private GameObject scoreAddImage20;
    [SerializeField] private GameObject scoreAddImage10;
    //[SerializeField] private Text addBoxCounterText;
    [SerializeField] private GameObject breakParticlePrefab;
    [SerializeField] private GameObject adBuyPanel;
    [SerializeField] private TMP_Text adBuyPanelText;
    [SerializeField] private GameObject highScoreImg;
    [SerializeField] private TMP_Text highScoreImgText;

    [Header("Effect Image")] [SerializeField]
    private Sprite[] effectImageSprites;

    [SerializeField] private Sprite comboSprite;
    [SerializeField] private Image effectImage;

    [Header("AudioClips")] [SerializeField]
    private AudioClip[] soundEffects;

    [SerializeField] public AudioClip pickCubeEffect,
        dropCubeEffect,
        matchCubeEffect,
        popInEffect,
        gameOverAudio,
        timerEndAudio,
        cantUseAudio,
        countingUpSound,
        highScoreAudio;

    [Header("UI Screens")] [SerializeField]
    private GameObject titleScreen;

    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private RewardsPopup rewardsPopup;
    [SerializeField] private GameObject instructionsScreen;
    [SerializeField] private GameObject gameScreen;
    [SerializeField] public GameObject lineSpawner;


    [Header("instructions")] [SerializeField]
    private Button nextInstructionsButton;

    [SerializeField] private Button closeInstructionsButton;
    [SerializeField] private Image instructionsImage;
    [SerializeField] private TMP_Text instructionsIndexText;
    [SerializeField] private Sprite[] instructionsSprites;


    //[Header("Game Over")] [SerializeField] private Button playAgainButton;
    [SerializeField] private Button backToTitleButton;
    [HideInInspector] public Dictionary<(int x, int y), Box> grid = new();
    private Box draggedBox;
    private Vector2 dragOffset;

    private bool gameOver;
    private bool animationInProgress;
    private bool hasBrokenRecord;

    private bool removed;
    private bool enterAnimationDone;
    private bool canUseHelp;
    private float lastClearTime;

    //private bool isAddBoxEmpty = false;
    private int boxCounter = 10;
    [HideInInspector] public int centerValue;
    private int moves;
    private int score;
    private int level = 1;
    private int lastBeepSecond;
    private float levelTimer;
    private int currentInstructionsIndex;
    private int helpUses = 4;
    private int timeHelpUses = 2;
    private int addType = -1;
    [HideInInspector] public float timeLeft;
    private string activeSavePath;
    private AudioSource audioSource;
    private Vector3 onScoreIncreaseStartPos;
    [SerializeField] private AudioSource timerAudioSource;


    private HighScores highScores = new();

    private const int k_boxSize = 180;
    private const int k_gridSpacing = 2;
    private const float k_addedTime = 20f;
    private const float k_startTime = 60f;
    private const int k_requiredEmptyCount = 2;

    private const float k_slideSpeed = 0.15f;
    private const float k_dropIntervalL4 = 10.0f;
    private const float k_dropIntervalL3 = 20.0f;
    [SerializeField] float DelayInCreatingBox = 0.2f;


    private static string SaveClassic => Path.Combine(Application.persistentDataPath, "cube_smasher_highscores.json");
    private static string SaveRackup => Path.Combine(Application.persistentDataPath, "cube_smasher_rackup.json");
    private static string SaveClock => Path.Combine(Application.persistentDataPath, "cube_smasher_clock.json");

    private int lastCenterValue = -1;

    public enum GameState
    {
        Title,
        Instructions,
        Game
    }

    private GameState state = GameState.Title;

    public enum Mode
    {
        Classic,
        Rackup,
        Clock
    }

    [HideInInspector] public Mode gameMode = Mode.Classic;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        RectTransform rt = scoreAddImage10.GetComponent<RectTransform>();
        onScoreIncreaseStartPos = rt.localPosition;
        effectImage.rectTransform.localScale = Vector3.zero;
    }

    private void Start()
    {
        startClassicButton.onClick.AddListener(() => StartGame(Mode.Classic));
        rackUpButton.onClick.AddListener(() => StartGame(Mode.Rackup));
        beatTheClockButton.onClick.AddListener(() => StartGame(Mode.Clock));

        howToPlayButton.onClick.AddListener(ShowInstructions);
        nextInstructionsButton.onClick.AddListener(UpdateInstructions);
        closeInstructionsButton.onClick.AddListener(HideInstructions);


        addTimeButton.onClick.AddListener(HandleAddTime);
        //  exitGameButton.onClick.AddListener(TriggerGameOver);
        addBoxButton.onClick.AddListener(HandleAddBox);
        helpButton.onClick.AddListener(UseHelpButton);

        //playAgainButton.onClick.AddListener(() => StartGame());
        backToTitleButton.onClick.AddListener(() => BackToTitleScreen());

        SetupModeInfoButtons();

        TittleScreen();
    }

    public void StartGameMode(Mode mode)
    {
        StartGame(mode);
    }

    private Button runtimeRackUpInfoBtn;
    private Button runtimeClockInfoBtn;
    private Button runtimeClassicInfoBtn;

    private void SetupModeInfoButtons()
    {
        Button rackUpBtn = rackUpInfoButton != null ? rackUpInfoButton : runtimeRackUpInfoBtn;
        if (rackUpBtn == null && rackUpButton != null)
        {
            runtimeRackUpInfoBtn = CreateModeInfoButton(rackUpButton.transform, new Color(0.96f, 0.72f, 0.12f, 1f));
            rackUpBtn = runtimeRackUpInfoBtn;
        }
        if (rackUpBtn != null)
        {
            rackUpBtn.onClick.RemoveAllListeners();
            rackUpBtn.onClick.AddListener(() => ShowModeInfo(Mode.Rackup));
        }

        Button clockBtn = clockInfoButton != null ? clockInfoButton : runtimeClockInfoBtn;
        if (clockBtn == null && beatTheClockButton != null)
        {
            runtimeClockInfoBtn = CreateModeInfoButton(beatTheClockButton.transform, new Color(0.16f, 0.73f, 0.96f, 1f));
            clockBtn = runtimeClockInfoBtn;
        }
        if (clockBtn != null)
        {
            clockBtn.onClick.RemoveAllListeners();
            clockBtn.onClick.AddListener(() => ShowModeInfo(Mode.Clock));
        }

        Button classicBtn = classicInfoButton != null ? classicInfoButton : runtimeClassicInfoBtn;
        if (classicBtn == null && startClassicButton != null)
        {
            runtimeClassicInfoBtn = CreateModeInfoButton(startClassicButton.transform, new Color(0.18f, 0.8f, 0.44f, 1f));
            classicBtn = runtimeClassicInfoBtn;
        }
        if (classicBtn != null)
        {
            classicBtn.onClick.RemoveAllListeners();
            classicBtn.onClick.AddListener(() => ShowModeInfo(Mode.Classic));
        }
    }

    public void ShowModeInfo(Mode mode)
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindAnyObjectByType<Canvas>();
        Transform canvasTr = canvas != null ? canvas.transform : null;

        if (modeInfoPopup != null)
        {
            modeInfoPopup.Show(this, tutorialManager, mode);
        }
        else
        {
            ModeInfoPopup.ShowPopup(canvasTr, this, tutorialManager, mode);
        }
    }

    private Button CreateModeInfoButton(Transform modeButtonTr, Color circleColor)
    {
        Transform existing = modeButtonTr.Find("InfoButton");
        if (existing != null)
        {
            return existing.GetComponent<Button>();
        }

        GameObject infoGo = new GameObject("InfoButton");
        infoGo.transform.SetParent(modeButtonTr, false);

        RectTransform rt = infoGo.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 0.5f);
        rt.anchorMax = new Vector2(1f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(30f, 0f);
        rt.sizeDelta = new Vector2(75f, 75f);

        Image img = infoGo.AddComponent<Image>();
        img.color = circleColor;

        Outline outline = infoGo.AddComponent<Outline>();
        outline.effectColor = new Color(0.1f, 0.1f, 0.1f, 0.6f);
        outline.effectDistance = new Vector2(2f, -2f);

        Button btn = infoGo.AddComponent<Button>();

        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(infoGo.transform, false);

        RectTransform textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
        TMP_FontAsset fontAsset = RewardsPopup.GetGameFont();
        if (fontAsset != null) tmp.font = fontAsset;
        tmp.text = "i";
        tmp.fontSize = 42;
        tmp.fontStyle = FontStyles.Bold | FontStyles.Italic;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        return btn;
    }

    private void TittleScreen()
    {
        state = GameState.Title;
        UpdateScreen(state);
        CancelScoreTweens();
        AdmobAdsScript.Instance.DestroyBannerAd();
        gameOver = false;
    }

    private void BackToTitleScreen()
    {
        UiManager.Instance.CheckForConsectieGamesAd(gameMode);
        TittleScreen();
        AudioManager.PlayBG(true);
    }

    private void ShowInstructions()
    {
        state = GameState.Instructions;
        UpdateScreen(state);
        currentInstructionsIndex = 0;
        instructionsImage.sprite = instructionsSprites[currentInstructionsIndex];
        instructionsIndexText.text = $"{currentInstructionsIndex + 1}/{instructionsSprites.Length}";
    }

    private void UpdateInstructions()
    {
        currentInstructionsIndex = (currentInstructionsIndex + 1) % instructionsSprites.Length;
        instructionsImage.sprite = instructionsSprites[currentInstructionsIndex];
        instructionsIndexText.text = $"{currentInstructionsIndex + 1}/{instructionsSprites.Length}";
    }

    private void HideInstructions()
    {
        state = GameState.Title;
        UpdateScreen(state);
    }

    public TutorialManager tutorialManager;

    private void StartGame(Mode mode)
    {
        //print("start game");
        gameMode = mode;
        if (TutorialManager.TutoiralNumber < TutorialManager.totalTutorials)
        {
            //Debug.LogError("tutorial num " + TutorialManager.TutoiralNumber);
            if (FirebaseCall.Instance != null)
            {
                FirebaseCall.Instance.LogTutorialStarted(TutorialOrigin.ModeSelection);
                //FirebaseCall.Instance.LogTutorialEvent("mode_selection_Tutorial", "started");
            }
            tutorialManager.HowTutorialStarted = TutorialOrigin.ModeSelection;// "mode_selection_Tutorial";
            tutorialManager.ShowTutorial();

            return;
        }

        UiManager.Instance.CheckForConsectieGamesAd(gameMode, false);
        StartGame();
    }
    public void OnRoundStarted(Mode mode)
    {
        FirebaseCall.Instance?.LogEvent(EventNames[mode]);
    }
    private static readonly Dictionary<Mode, string> EventNames = new()
{
    { Mode.Classic,  "classic_mode_started" },
    { Mode.Rackup,   "rackup_mode_started" },
    { Mode.Clock, "clock_mode_started" }
};
    //public void OnRoundStarted(string modeName)
    //{
    //     1. Get the current lifetime count for THIS mode specifically
    //    string key = "Rounds_" + modeName;
    //    int lifetimeRounds = PlayerPrefs.GetInt(key, 0);

    //     2. Increment it
    //    lifetimeRounds++;
    //    PlayerPrefs.SetInt(key, lifetimeRounds);

    //     3. Log to Firebase
    //    Parameter[] parameters = {
    //    new Parameter("mode_id", modeName),
    //    new Parameter("round_number", lifetimeRounds)};

    //    Debug.LogError(modeName+ " " + lifetimeRounds);
    //    if(FirebaseCall.Instance != null)
    //    FirebaseCall.Instance.LogEvent("round_start", parameters);
    //}
    public void StartGame()
    {
        CancelScoreTweens();
        
        if (FirebaseCall.Instance != null)
        {
            FirebaseCall.Instance.LogEvent("game_start");
        }
        stopTimerOnWinning = false;
        highScoreEffect.SetActive(false);

        AudioManager.PlayBG(false);
        AdmobAdsScript.Instance.LoadBannerAd();
        if (gameMode == Mode.Classic)
        {
            activeSavePath = SaveClassic;
            highScores = LoadHighScoresClassic(activeSavePath);
            Debug.LogError("hi " + level);
            level = 1;
        }
        else
        {
            activeSavePath = (gameMode == Mode.Rackup) ? SaveRackup : SaveClock;
            highScores = LoadHighScoresSingle(activeSavePath);
        }

        helpUses = 4;
        timeHelpUses = 2;
        score = 0;
        boxCounter = 10;
        //addBoxCounterText.text = boxCounter.ToString();
        UiManager.Instance.SetAllHelpCountersText(helpUses, timeHelpUses, boxCounter);
        UpdateScreen(GameState.Game);
        SetDefault();
    }

    public int GetHighScore(Mode gameMode)
    {
        if (gameMode == Mode.Classic)
        {
            var scores = LoadHighScoresClassic(SaveClassic);
            return scores.bestScore;
        }
        else
        {
            var scores = LoadHighScoresSingle(gameMode == Mode.Rackup ? SaveRackup : SaveClock);
            return scores.bestScore;
        }
    }

    private void Update()
    {
        if (state != GameState.Game || gameOver)
        {
            if (timerAudioSource.isPlaying)
                timerAudioSource.Stop();
            return;
        }

        DoGameUpdate(Time.deltaTime);
    }

    public PlayVFXSpeed upComingBox;

    private void DoGameUpdate(float dt)
    {
        if (gameMode == Mode.Classic && removed)
        {
            if (stopAddingBox || animationInProgress)
            {
                upComingBox.gameObject.SetActive(false);
                levelTimer = 0;
                return;
            }

            levelTimer += dt;

            // Slowed down logic:
            // 1. Levels 1-3: Drop interval decreases by 0.5s per level (instead of 1s)
            // 2. Levels 4+: Drop interval decreases by 0.05s per level (instead of 0.1s)
            var interval = level <= 3
                ? Mathf.Max(1.0f, 3.0f - (0.5f * (level - 1)))
                : Mathf.Max(0.1f, 1.0f - (0.05f * (level - 3)));

            Debug.LogError("interval " + interval.ToString("f2"));
            if (TryGetNextAddBoxAnchoredPosition(out var nextPos) && !stopAddingBox)
            {
                if (!upComingBox.gameObject.activeInHierarchy)
                    upComingBox.gameObject.SetActive(true);
                upComingBox.PlayVFX(interval, GridToScreen(nextPos), grid.Count);
                //Debug.LogError("dfsd " + grid.Count);
            }
            else
                upComingBox.gameObject.SetActive(false);


            if (levelTimer >= interval)
            {
                levelTimer = 0f;
                HandleAddBox(true);
            }
        }

        // Timer for clock/levels
        if (gameMode is Mode.Clock && enterAnimationDone && !adBuyPanel.activeInHierarchy && !stopTimerOnWinning &&
            !UiManager.Instance.TimeUpPanel.activeInHierarchy)
        {
            timeLeft -= dt;
            if (timeLeft <= 0)
            {
                //TriggerGameOver();
                UiManager.Instance.OpenTimeUpPanel(true);
                timerAudioSource.Stop();
                Debug.LogError("fa");
            }
            else
            {
                var whole = Mathf.FloorToInt(timeLeft);
                if (timeLeft <= 5f && whole != lastBeepSecond)
                {
                    lastBeepSecond = whole;
                }

                UpdateHUDTimer();
            }
        }

        EnsureSingleRedBox();
        EnsureTwoEmpties();
    }

    public void BeginDrag(Box b, Vector2 localPointer)
    {
        if (gameOver) return;
        if (b.fixedRed || !b.value.HasValue) return;
        AudioManager.PlayAudio(pickCubeEffect);
        draggedBox = b;
        dragOffset = localPointer - b.rt.anchoredPosition;
        dragStartPosition = GridToScreen(b.gridPos);
    }

    Vector2 dragStartPosition = Vector2.zero;

    public void DragMove(Vector2 localPointer)
    {
        if (gameOver || draggedBox == null) return;

        // cell step in screen units
        float step = k_boxSize + k_gridSpacing;

        // desired pointer position relative to drag offset
        var desiredPos = localPointer - dragOffset;

        // displacement from the start drag screen position
        var disp = desiredPos - dragStartPosition;

        // Determine which immediate neighbours exist — only those directions can be dragged up to one cell
        var src = draggedBox.gridPos;
        bool canUp = grid.ContainsKey((src.x, src.y + 1));
        bool canDown = grid.ContainsKey((src.x, src.y - 1));
        bool canRight = grid.ContainsKey((src.x + 1, src.y));
        bool canLeft = grid.ContainsKey((src.x - 1, src.y));

        // Allowed displacement ranges (only allow full step if neighbour exists, otherwise forbid movement on that axis)
        float minX = canLeft ? -step : 0f;
        float maxX = canRight ? step : 0f;
        float minY = canDown ? -step : 0f;
        float maxY = canUp ? step : 0f;

        // Clamp displacement to axis ranges
        float clampedX = Mathf.Clamp(disp.x, minX, maxX);
        float clampedY = Mathf.Clamp(disp.y, minY, maxY);

        // Also ensure the total drag magnitude doesn't exceed one cell diagonally
        var clamped = new Vector2(clampedX, clampedY);
        clamped = Vector2.ClampMagnitude(clamped, step);

        // Apply clamped position (visual)
        draggedBox.rt.anchoredPosition = dragStartPosition + clamped;
    }


    public bool EndDrag(Vector2 localPointer)
    {
        if (gameOver || draggedBox == null || animationInProgress) return false;
        var b = draggedBox;
        draggedBox = null;

        // Use the clamped onscreen position set in DragMove
        var finalPos = b.rt.anchoredPosition;

        // Compute direction from the drag start position (screen space)
        var dragDirection = finalPos - dragStartPosition;

        // Restrict to primary axis
        if (Mathf.Abs(dragDirection.x) > Mathf.Abs(dragDirection.y))
            dragDirection.y = 0;
        else
            dragDirection.x = 0;

        // If movement is too small, snap back
        float step = k_boxSize + k_gridSpacing;
        float minMoveThreshold = step * 0.5f; // require at least half a cell
        if (dragDirection.sqrMagnitude < minMoveThreshold * minMoveThreshold)
        {
            b.rt.anchoredPosition = b.originalScreenPos;
            return false;
        }

        dragDirection.Normalize();
        var src = b.gridPos;

        // Consider only immediate neighbours (one-cell). EndDrag already requires neighbour to exist in grid and be empty.
        var candidates = new List<((int x, int y) dst, float dist2)>();
        var dirs = new (int x, int y)[] { (0, 1), (1, 0), (0, -1), (-1, 0) };
        foreach (var d in dirs)
        {
            var dst = (src.x + d.x, src.y + d.y);

            // only allow if neighbour exists in grid and is empty (prevents moving out of the grid)
            if (grid.TryGetValue(dst, out var neighbor) && !neighbor.value.HasValue &&
                Vector2.Dot(new Vector2(d.x, d.y), dragDirection) > 0)
            {
                if (tutorialManager.TutoiralGoingON && neighbor.Swappable == false)
                {
                    continue;
                }

                var screen = GridToScreen(dst);
                var dist2 = (finalPos - screen).sqrMagnitude;
                candidates.Add((dst, dist2));
            }
        }

        if (candidates.Count > 0)
        {
            candidates.Sort((a, c) => a.dist2.CompareTo(c.dist2));
            moves++;
            SwapBoxes(src, candidates[0].dst);
            AudioManager.PlayAudio(dropCubeEffect);
            return true;
        }

        // Snap back if no valid neighbour
        b.rt.anchoredPosition = b.originalScreenPos;
        return false;
    }


    private void SwapBoxes((int x, int y) src, (int x, int y) dst)
    {
        var a = grid[src];
        var b = grid[dst];
        (a.gridPos, b.gridPos) = (dst, src);
        grid[dst] = a;
        grid[src] = b;
        PlaceBox(a);
        PlaceBox(b);
        tutorialManager.Swapped(TutorialManager.TutoiralNumber);
        // After sliding, check matches
        if (!tutorialManager.TutoiralGoingON)
            Invoke(nameof(CheckMatches), k_slideSpeed + +0.05f);
    }

    [Header("Clear Effects")] [SerializeField]
    private GameObject rowClearEffect;

    [SerializeField] private GameObject columnClearEffect;

    private Vector2 GetRowCenterPosition(int rowY)
    {
        return GridToScreen((0, rowY));
    }

    private Vector2 GetColumnCenterPosition(int columnX)
    {
        return GridToScreen((columnX, 0));
    }

    public void CheckMatchesForTutorial(float stayTime)
    {
        CancelInvoke();
        Invoke(nameof(CheckMatches), stayTime + k_slideSpeed + 0.05f);
    }
    private int lastClipIndex = -1;

    private async Awaitable CheckMatches()
    {
        int linesCleared = 0;
        var toClear = new HashSet<(int x, int y)>();
        var matchedRows = new HashSet<int>();
        var matchedCols = new HashSet<int>();

        var xs = new SortedSet<int>();
        var ys = new SortedSet<int>();
        foreach (var p in grid.Keys)
        {
            xs.Add(p.x);
            ys.Add(p.y);
        }

        // Get grid boundaries
        int minX = xs.Min();
        int maxX = xs.Max();
        int minY = ys.Min();
        int maxY = ys.Max();
        // Calculate grid dimensions
        int gridWidth = maxX - minX + 1;
        int gridHeight = maxY - minY + 1;
        foreach (var y in ys)
        {
            var row = new List<(int x, int y)>();
            foreach (var x in xs)
                if (grid.ContainsKey((x, y)))
                    row.Add((x, y));
            var sum = 0;
            foreach (var p in row) sum += grid[p].value ?? 0;
            if (sum != centerValue) continue;
            {
                foreach (var p in row)
                    if (!grid[p].fixedRed)
                        toClear.Add(p);
                matchedRows.Add(y);
                if (row.Count > 1)
                    if (!grid[row[0]].fixedRed)
                    {
                        //Debug.LogError("row length " + row.Count);
                        vfxShower.ShowRowEffect(GetRowCenterPosition(y), gridWidth / 5f);
                        linesCleared++;
                    }
            }
        }

        foreach (var x in xs)
        {
            var col = new List<(int x, int y)>();
            foreach (var y in ys)
                if (grid.ContainsKey((x, y)))
                    col.Add((x, y));
            var sum = 0;
            foreach (var p in col) sum += grid[p].value ?? 0;
            if (sum != centerValue) continue;
            {
                foreach (var p in col)
                    if (!grid[p].fixedRed)
                        toClear.Add(p);
                matchedCols.Add(x);
                if (col.Count > 1)
                {
                    if (!grid[col[0]].fixedRed)
                    {
                        //Debug.LogError("col length "+col.Count);
                        vfxShower.ShowColumnEffect(GetColumnCenterPosition(x), gridHeight / 5f);
                        linesCleared++;
                    }
                }
            }
        }

        if (toClear.Count == 0) return;

        AudioManager.PlayAudio(matchCubeEffect);



        int index;

        do
        {
            index = Random.Range(0, soundEffects.Length);
        }
        while (index == lastClipIndex && soundEffects.Length > 1);

        lastClipIndex = index;

        audioSource.PlayOneShot(soundEffects[index]);



        //var clip = soundEffects[Random.Range(0, soundEffects.Length)];
        //audioSource.clip = clip;
        //audioSource.Play();

        var addScore = 10 * linesCleared;

        lastClearTime = Time.time;
        OnScoreIncrease(addScore);
        removed = true;

        // await AnimatePreClear(toClear);

        await RemoveBoxes(toClear);


        tutorialManager.NextTutorial();
        //Debug.LogError("chelc ");

        if (tutorialManager.TutoiralGoingON) return;
        if (toClear.Count != 0)
            ShiftGrid(matchedRows, matchedCols);
        if (CheckWinCondition())
            HandleWin();
    }

    [Button]
    private async void TestRemove(int row = -100, int colum = -100)
    {
        var toClear = new HashSet<(int x, int y)>();
        var matchedRows = new HashSet<int>();
        var matchedCols = new HashSet<int>();

        var xs = new SortedSet<int>();
        var ys = new SortedSet<int>();
        foreach (var p in grid.Keys)
        {
            xs.Add(p.x);
            ys.Add(p.y);
        }

        if (row is <= 2 and >= -2)
        {
            foreach (var y in ys)
            {
                foreach (var x in xs)
                    if (grid.ContainsKey((x, y)) && x == row)
                    {
                        var remove = (x, y);
                        toClear.Add(remove);
                    }
            }

            matchedCols.Add(row);
        }

        if (colum is <= 2 and >= -2)
        {
            foreach (var x in xs)
            {
                foreach (var y in ys)
                    if (grid.ContainsKey((x, y)) && y == colum)
                    {
                        var remove = (x, y);
                        toClear.Add(remove);
                    }
            }

            matchedRows.Add(colum);
        }

        if (toClear.Count == 0) return;

        AudioManager.PlayAudio(matchCubeEffect);
        var clip = soundEffects[Random.Range(0, soundEffects.Length)];
        audioSource.clip = clip;
        audioSource.Play();
        removed = true;


        await RemoveBoxes(toClear);


        if (toClear.Count != 0)
            ShiftGrid(matchedRows, matchedCols);
        if (CheckWinCondition())
            HandleWin();
    }

    [Button]
    private void OnScoreIncrease(int addedPoints)
    {
        // onScoreIncreaseTxt.gameObject.SetActive(true);
        var sprite = effectImageSprites[Random.Range(0, effectImageSprites.Length)];
        
        effectImage.sprite = sprite;
        effectImage.gameObject.SetActive(true);
        RectTransform scoreImage;
        if (addedPoints == 20 && addedPoints < 50)
        {
            effectImage.sprite = comboSprite;
            scoreImage = scoreAddImage20.GetComponent<RectTransform>();
            vfxShower.PlayCombeEffect();
            //play vfx here
        }
        else
        {
            scoreImage = scoreAddImage10.GetComponent<RectTransform>();
        }

        scoreImage.gameObject.SetActive(true);
        CheckHighScore(score + addedPoints);

        int oldScore = 0;

        Sequence seq = DOTween.Sequence();
        // seq.Append(scoreImage.DOPunchScale(Vector3.one *1.1f, 0.35f, 10, 2));
        seq.Join(effectImage.rectTransform.DOScale(1, 0.5f).SetEase(Ease.OutBounce));
        seq.Join(DOTween.To(
            () => oldScore,
            x => { oldScore = x; },
            addedPoints,
            1.5f
        ).SetEase(Ease.OutExpo));
        // seq.Join(scoreImage.DOLocalMoveY(onScoreIncreaseStartPos.y + 150f, 1f).SetEase(Ease.OutCubic));
        seq.OnComplete(() =>
        {
            effectImage.rectTransform.DOScale(0, 0.1f).SetEase(Ease.InBack);
            //  scoreImage.DOScale(0f, .75f);
            //  scoreImage.localPosition = onScoreIncreaseStartPos;
            scoreImage.gameObject.SetActive(false);
            score += addedPoints;
            scoreLabel.text = $"Score : {score}";
        });
    }

    private void ShiftGrid(HashSet<int> clearedRows, HashSet<int> clearedCols)
    {
        if (grid.Count == 1)
        {
            return;
        }

        var fresh = new Dictionary<(int x, int y), Box>();

        if (grid.ContainsKey((0, 0)) && !clearedRows.Contains(0) && !clearedCols.Contains(0))
        {
            fresh[(0, 0)] = grid[(0, 0)];
        }

        foreach (var ((x, y), box) in grid)
        {
            if (clearedRows.Contains(y) || clearedCols.Contains(x) || (x == 0 && y == 0))
                continue;

            int shiftX = clearedCols.Count(cx => cx < x);
            int shiftY = clearedRows.Count(ry => ry < y);

            int nx = x - shiftX;
            int ny = y - shiftY;

            if (nx is >= -2 and <= 2 && ny is >= -2 and <= 2)
            {
                if ((nx, ny) == (0, 0))
                {
                    if (!fresh.ContainsKey((nx - 1, ny)))
                        nx -= 1;
                    else if (!fresh.ContainsKey((nx, ny - 1)))
                        ny -= 1;
                }

                if (!fresh.ContainsKey((nx, ny)))
                {
                    box.gridPos = (nx, ny);
                    fresh[(nx, ny)] = box;
                }
            }
        }

        int maxExistingCol = fresh.Keys
            .Where(p => p is not { x: 0, y: 0 })
            .Select(p => p.x)
            .DefaultIfEmpty(int.MinValue)
            .Max();
        int middleExistingCol = fresh.Keys
            .Where(p => p is not { x: 0, y: 0 } && p.x == -1).Select(p => p.x).DefaultIfEmpty(int.MinValue).Max();


        int maxExistingRow = fresh.Keys
            .Where(p => p is not { x: 0, y: 0 })
            .Select(p => p.y)
            .DefaultIfEmpty(int.MinValue)
            .Max();

        int middleExistingRow = fresh.Keys
            .Where(p => p is not { x: 0, y: 0 } && p.y == -1).Select(p => p.y).DefaultIfEmpty(int.MinValue).Max();

        if (clearedCols.Any(c => c <= 0) && maxExistingRow >= 0 && middleExistingCol == -1 &&
            !fresh.ContainsKey((-1, 0)))
        {
            fresh[(-1, 0)] = MakeBox(GenNumExcluding(centerValue), (-1, 0));
        }

        if (clearedRows.Any(c => c <= 0) && maxExistingCol >= 0 && middleExistingRow == -1 &&
            !fresh.ContainsKey((0, -1)))
        {
            fresh[(0, -1)] = MakeBox(GenNumExcluding(centerValue), (0, -1));
        }


        var xs = fresh.Keys.Where(p => p != (0, 0)).Select(p => p.x).Distinct().ToList();
        var ys = fresh.Keys.Where(p => p != (0, 0)).Select(p => p.y).Distinct().ToList();

        bool isSingleRow = ys.Count == 1 && xs.Count > 1;
        bool isSingleColumn = xs.Count == 1 && ys.Count > 1;

        if (isSingleRow || isSingleColumn)
        {
            if (fresh.Count == 0) return;

            var nonCenterBoxes = fresh.Where(kv => kv.Key != (0, 0)).ToList();

            if (nonCenterBoxes.Count == 0) return;

            var newGrid = new Dictionary<(int, int), Box>();
            if (isSingleRow)
            {
                var currentY = nonCenterBoxes.First().Key.y;
                int targetY = currentY > 0 ? 1 : -1;
                int shiftY = targetY - currentY;

                foreach (var (pos, box) in fresh)
                {
                    if (pos == (0, 0)) continue;

                    var newPos = (pos.x, pos.y + shiftY);

                    if (newPos.Item2 is >= -2 and <= 2 && newPos != (0, 0))
                    {
                        box.gridPos = newPos;
                        newGrid[newPos] = box;
                    }
                }
            }
            else
            {
                var currentX = nonCenterBoxes.First().Key.x;

                int targetX = currentX > 0 ? 1 : -1;
                int shiftX = targetX - currentX;

                foreach (var (pos, box) in fresh)
                {
                    if (pos == (0, 0)) continue;
                    var newPos = (pos.x + shiftX, pos.y);

                    if (newPos.Item1 is >= -2 and <= 2 && newPos != (0, 0))
                    {
                        box.gridPos = newPos;
                        newGrid[newPos] = box;
                    }
                }
            }

            fresh = newGrid;
        }

        if (!fresh.ContainsKey((-1, -1)))
        {
            fresh[(-1, -1)] = MakeBox(GenNumExcluding(centerValue), (-1, -1));
        }

        grid = fresh;
        foreach (var b in grid.Values)
            PlaceBox(b);

        EnsureSingleRedBox();
        EnsureTwoEmpties();
        ValidateGrid();
    }

    public bool CheckWinCondition()
    {
        var redFound = false;
        var numberTiles = 0;
        foreach (var b in grid.Values)
        {
            if (b.fixedRed) redFound = true;
            else if (b.value.HasValue) numberTiles++;
        }

        return redFound && numberTiles <= 1; // chf
    }

    public void UpdateScreen(GameState s)
    {
        if (FadeScreen.DoFade(() =>
            {
                titleScreen.SetActive(s == GameState.Title);
                instructionsScreen.SetActive(s == GameState.Instructions);
                gameScreen.SetActive(s == GameState.Game && !gameOver);
                gameOverScreen.SetActive(s == GameState.Game && gameOver);
            })) return;
        titleScreen.SetActive(s == GameState.Title);
        instructionsScreen.SetActive(s == GameState.Instructions);
        gameScreen.SetActive(s == GameState.Game && !gameOver);
        gameOverScreen.SetActive(s == GameState.Game && gameOver);
    }

    private void HandleAddBox()
    {
        if (tutorialManager.TutoiralGoingON)
        {
            tutorialManager.AddBoxForTuturial();
            return;
        }

        if (boxCounter <= 0)
        {
            AudioManager.PlayAudio(cantUseAudio);
            adBuyPanel.SetActive(true);
            adBuyPanelText.text = "Watch Ads to get more <color=#36F64E><size=130%>Boxes</size></color>";

            addType = 3;
            return;
        }

        //DeductBoxCounter();
        HandleAddBox(false);
    }

    private async Awaitable HandleAddBox(bool shouldGameOver)
    {
        Debug.Log("handle box call 1 ::" + shouldGameOver);
        if (animationInProgress)
        {
            // Wait until animation is complete
            while (animationInProgress)
            {
                await Awaitable.NextFrameAsync();
            }

            // Check if game is still in a valid state after waiting
            if (gameOver || state != GameState.Game) return;
        }


        var nonRedKeys = grid.Where(kv => !kv.Value.fixedRed).Select(kv => kv.Key).ToList();
        var keysToUse = nonRedKeys.Any() ? (IEnumerable<(int x, int y)>)nonRedKeys : grid.Keys;

        int minX = keysToUse.Min(p => p.x);
        int maxX = keysToUse.Max(p => p.x);
        int minY = keysToUse.Min(p => p.y);
        int maxY = keysToUse.Max(p => p.y);

        // Check if grid is already full (5x5)
        if (minX == -2 && maxX == 2 && minY == -2 && maxY == 2 &&
            grid.Count == 25)
        {
            if (shouldGameOver)
                UiManager.Instance.OpenSpaceRanOutPanel(true);

            //TriggerGameOver();
            else
            {
                AudioManager.PlayAudio(cantUseAudio);
            }

            return;
        }

        if (!shouldGameOver)
            DeductBoxCounter();
        // First, try to fill empty spots in the top row (left to right)
        for (int x = minX; x <= maxX; x++)
        {
            var pos = (x, maxY);
            if (!grid.ContainsKey(pos))
            {
                AddBoxAt(pos);
                return;
            }
        }

        // Then, try to fill empty spots in the right column (top to bottom)
        for (int y = maxY; y >= minY; y--)
        {
            var pos = (maxX, y);
            if (!grid.ContainsKey(pos))
            {
                AddBoxAt(pos);
                return;
            }
        }

        if (maxY < 2)
        {
            int newY = maxY + 1;
            // Fill the new top row from left to right
            for (int x = minX; x <= maxX; x++)
            {
                var pos = (x, newY);
                if (!grid.ContainsKey(pos))
                {
                    AddBoxAt(pos);
                    return;
                }
            }
        }

        // Then, try to expand the right column
        if (maxX < 2)
        {
            int newX = maxX + 1;
            // Fill the new right column from top to bottom
            for (int y = maxY; y >= minY; y--)
            {
                var pos = (newX, y);
                if (!grid.ContainsKey(pos))
                {
                    AddBoxAt(pos);
                    return;
                }
            }
        }

        // If we couldn't expand in either direction, check if we need to expand diagonally
        if (maxY < 2 && maxX < 2)
        {
            int newY = maxY + 1;
            int newX = maxX + 1;
            var pos = (newX, newY);

            if (!grid.ContainsKey(pos))
            {
                AddBoxAt(pos);
                return;
            }
        }

        if (shouldGameOver)
            UiManager.Instance.OpenSpaceRanOutPanel(true);
        //TriggerGameOver();
    }

    private bool TryGetNextAddBoxGridPosition(out (int x, int y) foundPos)
    {
        foundPos = default;

        var nonRedKeys = grid.Where(kv => !kv.Value.fixedRed).Select(kv => kv.Key).ToList();
        var keysToUse = nonRedKeys.Any() ? (IEnumerable<(int x, int y)>)nonRedKeys : grid.Keys;

        int minX = keysToUse.Min(p => p.x);
        int maxX = keysToUse.Max(p => p.x);
        int minY = keysToUse.Min(p => p.y);
        int maxY = keysToUse.Max(p => p.y);

        // Grid full -> no pos
        if (minX == -2 && maxX == 2 && minY == -2 && maxY == 2 && grid.Count == 25)
        {
            // you may want to treat shouldGameOver differently; here we just report no pos
            return false;
        }

        // First, try to fill empty spots in the top row (left to right)
        for (int x = minX; x <= maxX; x++)
        {
            var pos = (x, maxY);
            if (!grid.ContainsKey(pos))
            {
                foundPos = pos;
                return true;
            }
        }

        // Then, try to fill empty spots in the right column (top to bottom)
        for (int y = maxY; y >= minY; y--)
        {
            var pos = (maxX, y);
            if (!grid.ContainsKey(pos))
            {
                foundPos = pos;
                return true;
            }
        }

        if (maxY < 2)
        {
            int newY = maxY + 1;
            for (int x = minX; x <= maxX; x++)
            {
                var pos = (x, newY);
                if (!grid.ContainsKey(pos))
                {
                    foundPos = pos;
                    return true;
                }
            }
        }

        if (maxX < 2)
        {
            int newX = maxX + 1;
            for (int y = maxY; y >= minY; y--)
            {
                var pos = (newX, y);
                if (!grid.ContainsKey(pos))
                {
                    foundPos = pos;
                    return true;
                }
            }
        }

        if (maxY < 2 && maxX < 2)
        {
            int newY = maxY + 1;
            int newX = maxX + 1;
            var pos = (newX, newY);
            if (!grid.ContainsKey(pos))
            {
                foundPos = pos;
                return true;
            }
        }

        return false;
    }

    private bool TryGetNextAddBoxAnchoredPosition(out (int x, int y) gridPos1)
    {
        gridPos1 = default;
        if (TryGetNextAddBoxGridPosition(out var gridPos))
        {
            gridPos1 = gridPos; // GridToScreen(gridPos);
            return true;
        }

        return false;
    }

    void DeductBoxCounter()
    {
        boxCounter = boxCounter - 1;
        UiManager.Instance.SetAddBoxCounterText(boxCounter);
    }

    private void AddBoxAt((int x, int y) pos)
    {
        // Check if position already exists in grid
        if (grid.ContainsKey(pos))
        {
            Debug.LogWarning($"Trying to add box at occupied position: {pos}");
            return;
        }

        var val = GenNumExcluding(centerValue);
        var nb = MakeBox(val, pos);

        grid[pos] = nb;
        PlaceBox(nb);

        if (gameMode == Mode.Classic)
            upComingBox.ShowGlowAtBox(nb, grid.Count);
    }

    private void ValidateGrid()
    {
        // Check if all grid entries have corresponding visual elements
        foreach (var pos in grid.Keys.ToList())
        {
            if (grid[pos] == null || grid[pos].rt == null)
            {
                Debug.LogWarning($"Grid position {pos} has no visual representation");
                grid.Remove(pos);
            }
        }

        // Check if all visual elements are in the grid
        foreach (Transform child in boxParent)
        {
            var dragBox = child.GetComponent<UIDragBox>();
            if (dragBox != null && dragBox.box != null)
            {
                var boxPos = dragBox.box.gridPos;
                if (!grid.ContainsKey(boxPos) || grid[boxPos] != dragBox.box)
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }

    private void UseHelpButton()
    {
        if (tutorialManager.TutoiralGoingON)
        {
            tutorialManager.HelpButtonPressed();
            return;
        }

        //Debug.LogError("help button pressed");
        if (canUseHelp)
            UseHelp();
    }

    public void ClearFirstRow()
    {
        helpUses++;
        UseHelp(1);
    }

    private async Awaitable UseHelp(float probability = 0.5f)
    {
        if (helpUses == 0)
        {
            AudioManager.PlayAudio(cantUseAudio);
            adBuyPanel.SetActive(true);
            adBuyPanelText.text = "Watch Ads to fill <color=#36F64E><size=130%>Hearts</size></color>";

            addType = 1;
            return;
        }

        canUseHelp = false;
        helpUses--;
        //UpdateHearts();
        UiManager.Instance.SetRemainingHelpCounterText(helpUses);
        // Top row
        var topY = int.MinValue;
        foreach (var p in grid.Keys) topY = Mathf.Max(topY, p.y);
        var redInTop = false;
        foreach (var kv in grid)
            if (kv.Key.Item2 == topY && kv.Value.fixedRed)
            {
                redInTop = true;
                break;
            }

        if (!redInTop && Random.value < probability)
        {
            // Clear top row
            var toRemove = new HashSet<(int, int)>();
            foreach (var kv in grid)
                if (kv.Key.Item2 == topY)
                    toRemove.Add(kv.Key);
            await RemoveBoxes(toRemove);
            removed = true;
        }
        else
        {
            // Turn a random regular tile into empty
            var candidates = new List<Box>();
            foreach (var b in grid.Values)
                if (b.value.HasValue && !b.fixedRed)
                    candidates.Add(b);
            if (candidates.Count > 0)
            {
                var t = candidates[Random.Range(0, candidates.Count)];
                t.value = null;
                t.SetVisual();
                t.rt.transform.SetAsFirstSibling();
                //AnimateHelpHeart(t);
            }
        }


        EnsureTwoEmpties();
        if (CheckWinCondition())
        {
            HandleWin();
        }

        canUseHelp = true;
    }


    //public void AnimateHelpHeart(Box box)
    //{
    //    heartIcon.transform.position = box.rt.position;
    //    heartIcon.gameObject.SetActive(true);
    //    heartIcon.DOFade(0, 0);
    //    heartIcon.transform.localScale = Vector3.zero;

    //    heartIcon.DOFade(1, 0.25f).onComplete = () =>
    //    {
    //        heartIcon.DOFade(0, 0.25f).onComplete = () => heartIcon.gameObject.SetActive(false);
    //    };
    //    heartIcon.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
    //}

    bool stopTimerOnWinning = false;
    bool stopAddingBox = false;

    public async void HandleWin() //HashSet<(int x, int y)> toClear = null)
    {
        // await RemoveBoxesEnd(toClear);
        foreach (var kv in grid)
        {
            var box = kv.Value;
            if (box == null || box.rt == null) continue;

            box.rt.GetComponent<BoxExplodeEffect>().enabled = true;
            //Debug.Log("remin box eX");
        }

        stopTimerOnWinning = true;
        stopAddingBox = true;
        timerAudioSource.Stop();
        timerLabel.text = "";
        await Awaitable.WaitForSecondsAsync(1f);

        //Debug.Log("handle win");
        lineSpawner.SetActive(true);


        if (score > highScores.bestScore)
            highScores.bestScore = score;
        await Awaitable.WaitForSecondsAsync(5f);

        if (gameMode == Mode.Classic)
        {
            var prev = GetBestMovesForLevel(level);
            if (!prev.HasValue || moves < prev.Value)
                SetBestMovesForLevel(level, moves);

            SaveHighScoresClassic(SaveClassic, highScores);
            bestScoreLabel.text = highScores.bestScore.ToString();
            level++;
            ResetGrid();
            stopAddingBox = false;
        }
        else
        {
            var prev = GetSingleBestMoves();
            if (!prev.HasValue || moves < prev.Value)
                SetSingleBestMoves(moves);

            SaveHighScoresSingle(activeSavePath, highScores);
            bestScoreLabel.text = highScores.bestScore.ToString();
            ResetGrid();
            timeLeft = k_startTime;
            stopTimerOnWinning = false;
        }
    }

    [Button]
    public void TriggerGameOver()
    {
        if (tutorialManager.TutoiralGoingON)
        {
            StartGame();
            tutorialManager.EndTurotial();
            if (FirebaseCall.Instance != null)
            {
                //FirebaseCall.Instance.LogTutorialEvent(tutorialManager.HowTutorialStarted, "skipped");
                FirebaseCall.Instance.LogTutorialSkipped(tutorialManager.HowTutorialStarted);
            }
           
            return;
        }

        if (gameOver) return;
        adBuyPanel.SetActive(false);
        gameOver = true;
        AdmobAdsScript.Instance.DestroyBannerAd();
        //
        bool isNewHighScore = score > highScores.bestScore;


        if (isNewHighScore)
        {
            // Update the high score before showing animation
            highScores.bestScore = score;
            PlayerPrefs.SetInt("BestScore", score);
            PlayerPrefs.Save();
            if (gameMode == Mode.Classic)
                SaveHighScoresClassic(activeSavePath, highScores);
            else
                SaveHighScoresSingle(activeSavePath, highScores);

            ShowHighScoreAnimation();
            //Debug.LogError("its high score");
        }
        else
        {
            ShowRegularScore();
        }

        MenuPanelScript.OnGameEndingShowBanner(gameMode, score);
        UpdateScreen(GameState.Game);
    }

    private Sequence scoreSequence;
    private Tween highScoreTween;
    private Tween scoreTween;
    private Tween delayedCallTween;

    public void CancelScoreTweens()
    {
        scoreSequence?.Kill();
        highScoreTween?.Kill();
        scoreTween?.Kill();
        delayedCallTween?.Kill();
        DOTween.Kill("HighScoreAnimation");
    }

    private void ShowRegularScore()
    {
        CancelScoreTweens(); // always kill before creating
        AudioManager.PlayAudio(gameOverAudio);

        gameEndHighScoreLabel.text = "";
        gameEndScoreLabel.text = "";

        int oldScore = 0;
        int highOldScore = 0;

        scoreSequence = DOTween.Sequence()
            .AppendInterval(1.5f)
            .AppendCallback(() =>
            {
                AudioManager.PlayAudio(countingUpSound);

                // store tween reference
                highScoreTween = DOTween.To(() => highOldScore, x =>
                    {
                        highOldScore = x;
                        gameEndHighScoreLabel.text = $"{x}";
                    }, highScores.bestScore, 2f)
                    .SetEase(Ease.OutExpo);

                delayedCallTween = DOVirtual.DelayedCall(2.5f, () =>
                {
                    AudioManager.PlayAudio(countingUpSound);

                    scoreTween = DOTween.To(() => oldScore, x =>
                        {
                            oldScore = x;
                            gameEndScoreLabel.text = $"{x}";
                        }, score, 1.5f)
                        .SetEase(Ease.OutExpo)
                        .OnComplete(() =>
                        {
                            DOVirtual.DelayedCall(1.0f, () =>
                            {
                                if (rewardsPopup != null)
                                    rewardsPopup.Show(this);
                                else
                                    RewardsPopup.ShowPopup(gameOverScreen.transform, this);
                            });
                        });
                });
            });
    }


    public GameObject highScoreEffect;

    private void ShowHighScoreAnimation()
    {
        // 1️⃣ Play Game Over sound first
        AudioManager.PlayAudio(gameOverAudio);

        // Delay before counter starts (either after game over audio or fixed)
        float afterGameOverDelay = 2f; // short cinematic pause
        float counterDuration = 2.5f;
        int displayedScore = 0;

        int oldScore = 0;
        DOVirtual.DelayedCall(0.5f, () =>
        {
            gameOverScreen.SetActive(true);
            gameOverScreen.GetComponent<Animator>().enabled = true;
            gameOverScreen.GetComponent<Animator>().SetTrigger("hi");
            //Debug.LogError("hellog ");
        }).SetId("HighScoreAnimation");
        gameEndScoreLabel.text = "";
        gameEndHighScoreLabel.text = "";

        DOVirtual.DelayedCall(0.8f, () =>
        {
            AudioManager.PlayAudio(countingUpSound);
            DOTween.To(() => oldScore, x =>
            {
                oldScore = x;
                gameEndScoreLabel.text = $"{x}";
            }, score, 1.5f).SetEase(Ease.OutExpo).SetId("HighScoreAnimation");
        }).SetId("HighScoreAnimation");
        DOVirtual.DelayedCall(2.5f, () =>
        {
            AudioManager.PlayAudio(countingUpSound);
            DOTween.To(() => displayedScore, x =>
                    {
                        displayedScore = x;
                        gameEndHighScoreLabel.text = $"{x}";
                    },
                    score, 1.5f)
                .SetEase(Ease.OutExpo)
                .OnComplete(() =>
                {
                    if (highScoreEffect != null)
                        highScoreEffect.SetActive(true);

                    AudioManager.PlayAudio(highScoreAudio);

                    DOVirtual.DelayedCall(1.0f, () =>
                    {
                        if (rewardsPopup != null)
                            rewardsPopup.Show(this);
                        else
                            RewardsPopup.ShowPopup(gameOverScreen.transform, this);
                    }).SetId("HighScoreAnimation");
                }).SetId("HighScoreAnimation");
        }).SetId("HighScoreAnimation");
    }


    private void ResetGrid()
    {
        //Debug.Log("in reset grid");
        lineSpawner.SetActive(false);
        SetDefault();
    }

    private void HandleAddTime()
    {
        if (timeHelpUses == 0)
        {
            AudioManager.PlayAudio(cantUseAudio);
            adBuyPanel.SetActive(true);
            adBuyPanelText.text = "Watch Ads to get more <color=#36F64E><size=130%>Time</size></color>";

            addType = 2;
            return;
        }

        //helpUses--;
        timeHelpUses--;
        UiManager.Instance.SetAddTimerCounterText(timeHelpUses);
        //UpdateHearts();
        timeLeft += k_addedTime;
        // Play SFX if desired
    }

    private void SetDefault()
    {
        StopAllCoroutines();
        if (lineSpawner != null)
        {
            lineSpawner.SetActive(false);
        }
        hasBrokenRecord = false;
        upComingBox.gameObject.SetActive(false);
        state = GameState.Game;
        gameOver = false;
        animationInProgress = false;
        removed = false;
        enterAnimationDone = false;
        timerAudioSource.Stop();
        lastClearTime = 0;
        canUseHelp = true;
        scoreLabel.text = $"Score : {score}";
        moves = 0;
        //movesLabel.text = "0";
        lastBeepSecond = -999;
        //level = 1;
        //UpdateHearts();
        levelTimer = 0f;
        timeLeft = gameMode is Mode.Clock ? k_startTime : 0f;
        timerLabel.color = new Color(0.9725491f, 0.7294118f, 0.1921569f, 1f);
        timerLabel.transform.localScale = Vector3.one;
        timerLabel.gameObject.SetActive(false);
        addTimeButton.gameObject.SetActive(gameMode is Mode.Clock);
        bestScoreLabel.text = highScores.bestScore.ToString();
        GenerateGrid();
        EnsureSingleRedBox();
        EnsureTwoEmpties();
        highScoreImg.SetActive(false);
        stopAddingBox = false;
        if (!tutorialManager.TutoiralGoingON)
            OnRoundStarted(gameMode);
    }

    //private void UpdateHearts()
    //{
    //    foreach (var heart in helpHearts)
    //    {
    //        heart.gameObject.SetActive(false);
    //    }

    //    for (var i = 0; i < helpUses; i++)
    //    {
    //        if (i >= helpHearts.Count) break;
    //        helpHearts[i].gameObject.SetActive(true);
    //    }
    //}

    private void UpdateHUDTimer()
    {
        if (timerLabel == null) return;
        timerLabel.text = gameMode is Mode.Clock
            ? $"Time: {Mathf.Max(0, Mathf.FloorToInt(timeLeft))}"
            : "";

        if (gameMode == Mode.Clock)
        {
            if (timeLeft <= 5 && !timerAudioSource.isPlaying)
            {
                timerAudioSource.clip = timerEndAudio;
                timerAudioSource.Play();
                timerLabel.color = Color.red;
                timerLabel.transform.DOScale(1.2f, 0.3f)
                    .SetEase(Ease.OutBack)
                    .OnComplete(() => { timerLabel.transform.DOScale(1f, 0.2f); });
            }
            else if (timeLeft > 5)
            {
                if (timerAudioSource.isPlaying)
                    timerAudioSource.Stop();
                timerLabel.color = new Color(0.97f, 0.73f, 0.19f);
            }
        }
    }

    private HighScores LoadHighScoresClassic(string path)
    {
        if (!File.Exists(path)) return new HighScores();
        try
        {
            var txt = File.ReadAllText(path);
            return JsonUtility.FromJson<HighScores>(txt);
        }
        catch
        {
            // ignored
        }

        return new HighScores();
    }

    private void SaveHighScoresClassic(string path, HighScores data)
    {
        File.WriteAllText(path, JsonUtility.ToJson(data));
    }

    private HighScores LoadHighScoresSingle(string path)
    {
        var h = LoadHighScoresClassic(path);
        h.bestMoves.TryAdd("single", null);
        for (var i = 1; i <= 4; i++)
            if (!h.bestMoves.ContainsKey(i.ToString()))
                h.bestMoves[i.ToString()] = null;
        return h;
    }

    private void SaveHighScoresSingle(string path, HighScores data)
    {
        data.bestMoves.TryAdd("single", null);
        File.WriteAllText(path, JsonUtility.ToJson(data));
    }

    private int? GetBestMovesForLevel(int lvl)
    {
        var key = lvl.ToString();
        return highScores.bestMoves.GetValueOrDefault(key);
    }

    private void SetBestMovesForLevel(int lvl, int m)
    {
        highScores.bestMoves[lvl.ToString()] = m;
    }

    private int? GetSingleBestMoves()
    {
        return highScores.bestMoves.GetValueOrDefault("single");
    }

    private void SetSingleBestMoves(int m)
    {
        highScores.bestMoves["single"] = m;
    }

    private void ClearGrid()
    {
        foreach (var b in grid.Values)
            if (b?.rt)
            {
                b.rt.DOKill();
                Destroy(b.rt.gameObject);
            }

        grid.Clear();
        foreach (Transform t in boxParent.transform)
        {
            t.DOKill();
            Destroy(t.gameObject);
        }
    }

    private void GenerateGrid()
    {
        StopAllCoroutines();
        StartCoroutine(GenerateGridCoroutine());
    }

    private IEnumerator GenerateGridCoroutine()
    {
        do
        {
            centerValue = Random.Range(10, 22);
        }
        while (centerValue == lastCenterValue);
        lastCenterValue = centerValue;


        while (true)
        {
            ClearGrid();
            var temp = new Dictionary<(int x, int y), Box>
            {
                [(0, 0)] = MakeBox(centerValue, (0, 0), fixedRed: true)
            };

            var pos3 = new List<(int, int)>();
            for (var x = -1; x <= 1; x++)
            for (var y = -1; y <= 1; y++)
                if (!(x == 0 && y == 0))
                    pos3.Add((x, y));

            var pos5 = new List<(int, int)>();
            for (var x = -2; x <= 2; x++)
            for (var y = -2; y <= 2; y++)
                if (!temp.ContainsKey((x, y)) && !(Mathf.Abs(x) <= 1 && Mathf.Abs(y) <= 1))
                    pos5.Add((x, y));

            var empty3 = pos3[Random.Range(0, pos3.Count)];
            var empty5 = pos5[Random.Range(0, pos5.Count)];
            if (empty5 == empty3) continue;

            foreach (var p in pos3)
                temp[p] = (p == empty3) ? MakeBox(null, p) : MakeBox(GenNumExcluding(centerValue), p);

            foreach (var p in pos5)
                temp[p] = (p == empty5) ? MakeBox(null, p) : MakeBox(GenNumExcluding(centerValue), p);

            if (!ValidGrid(temp)) continue;
            grid = temp;
            break;
        }

        foreach (var kv in grid)
        {
            PlaceBox(kv.Value);
            kv.Value.rt.localScale = Vector3.zero;
        }

        var order = grid.OrderByDescending(k => k.Key.y).ThenBy(k => k.Key.x).ToList();
        var centerBox = order.FirstOrDefault(kv => kv.Key == (0, 0));
        if (!centerBox.Equals(default(KeyValuePair<(int x, int y), Box>)))
        {
            order.Remove(centerBox);
            order.Add(centerBox);
        }

        foreach (var kv in order)
        {
            kv.Value.rt.DOScale(Vector3.one * 0.9f, 0.5f).SetEase(Ease.OutBack);
            AudioManager.PlayAudio(popInEffect);
            yield return new WaitForSeconds(DelayInCreatingBox);
        }

        enterAnimationDone = true;
        timerLabel.gameObject.SetActive(gameMode is Mode.Clock);
        UpdateHUDTimer();
    }


    private void EnsureSingleRedBox()
    {
        // Keep (0,0) red, demote others
        var hasRed = false;
        foreach (var (p, box) in grid)
        {
            if (!box.fixedRed) continue;
            if (p == (0, 0)) hasRed = true;
            else
            {
                box.fixedRed = false;
                if (!box.value.HasValue || box.value == centerValue)
                    box.value = GenNumExcluding(centerValue);
                box.SetVisual();
            }
        }

        if (hasRed) return;
        // Make sure (0,0) exists and is red with centerValue
        if (!grid.ContainsKey((0, 0)))
            grid[(0, 0)] = MakeBox(centerValue, (0, 0), true);
        var c = grid[(0, 0)];
        c.fixedRed = true;
        c.value = centerValue;
        c.SetVisual();
        PlaceBox(c);
    }

    private void EnsureTwoEmpties()
    {
        var empties = new List<Box>();
        foreach (var b in grid.Values)
            if (!b.value.HasValue)
                empties.Add(b);
        var need = k_requiredEmptyCount - empties.Count;
        if (need <= 0) return;
        {
            var candidates = new List<Box>();
            foreach (var b in grid.Values)
                if (b.value.HasValue && !b.fixedRed)
                    candidates.Add(b);
            Shuffle(candidates);
            for (var i = 0; i < Mathf.Min(need, candidates.Count); i++)
            {
                candidates[i].value = null;
                candidates[i].SetVisual();
            }
        }
    }

    private Box MakeBox(int? value, (int x, int y) pos, bool fixedRed = false)
    {
        var go = Instantiate(boxPrefab, boxParent);
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(k_boxSize, k_boxSize);
        var img = go.bg;
        img.color = new Color(0.93f, 0.61f, 0.47f);

        var b = new Box
        {
            value = value,
            gridPos = pos,
            fixedRed = fixedRed,
            rt = rt,
            bg = img,
            label = go.label
        };
        b.SetVisual();

        // Drag
        go.Init(this, b);

        return b;
    }

    private bool ValidGrid(Dictionary<(int, int), Box> test)
    {
        foreach (var (key, box) in test)
        {
            var (x, y) = key;
            if (!box.value.HasValue || box.fixedRed) continue;
            // row
            var sumRow = 0;
            for (var i = -2; i <= 2; i++)
            {
                if (test.TryGetValue((i, y), out var b))
                    sumRow += b.value ?? 0;
            }

            if (sumRow == centerValue) return false;

            // col
            var sumCol = 0;
            for (var j = -2; j <= 2; j++)
            {
                if (test.TryGetValue((x, j), out var b))
                    sumCol += b.value ?? 0;
            }

            if (sumCol == centerValue) return false;
        }

        return true;
    }


    private static void PlaceBox(Box b)
    {
        var pos = GridToScreen(b.gridPos);
        b.rt.anchoredPosition = pos;
        b.originalScreenPos = pos;
        b.SetVisual();
    }

    private static Vector2 GridToScreen((int x, int y) p)
    {
        const float step = k_boxSize; // your constant
        return new Vector2(
            p.x * (step + k_gridSpacing),
            p.y * (step + k_gridSpacing)
        );
    }

    private static int GenNumExcluding(int exclude)
    {
        var v = Random.Range(1, 11);
        while (v == exclude) v = Random.Range(1, 11);
        return v;
    }

    private static void Shuffle<T>(IList<T> list)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private async Awaitable RemoveBoxesEnd(HashSet<(int x, int y)> boxes)
    {
        Debug.Log("remove end boxes");
        animationInProgress = true;

        float delayBetweenBoxes = 0.08f;

        Color[] colors = new Color[]
        {
            new Color(0.94f, 0.24f, 0.09f), // red
            new Color(0.16f, 0.56f, 0.96f), // blue
            new Color(1.0f, 0.85f, 0.1f), // yellow
            new Color(0.2f, 0.85f, 0.3f), // green
            new Color(1.0f, 0.55f, 0.1f) // orange
        };

        int colorIndex = 0;

        foreach (var b in boxes)
        {
            if (!grid.TryGetValue(b, out var box) || box == null || box.rt == null)
                continue;

            box.rt.SetAsLastSibling();
            boxParent.DOShakeAnchorPos(0.15f, new Vector2(10f, 5f), 8, 90f);
            // boxParent.DOShakeAnchorPos(0.25f, new Vector2(20f, 10f), 8, 90f);

            // pick next color
            var targetColor = colors[colorIndex % colors.Length];
            colorIndex++;

            if (breakParticlePrefab != null)
            {
                var p = Instantiate(breakParticlePrefab, boxParent);
                var rt = p.GetComponent<RectTransform>();
                if (rt != null)
                    rt.anchoredPosition = box.rt.anchoredPosition;
                else
                    p.transform.position = box.rt.position;
                Destroy(p, 1f);
            }

            var seq = DOTween.Sequence();
            seq.Append(box.bg.DOColor(targetColor, 0.1f));
            seq.Join(box.rt.DOScale(Vector3.one * 1.2f, 0.07f).SetEase(Ease.OutBack));
            seq.Append(box.bg.DOFade(0f, 0.35f)); // ⏳ slightly slower fade
            seq.Join(box.rt.DOScale(Vector3.zero, 0.35f).SetEase(Ease.InBack));
            seq.AppendCallback(() =>
            {
                box.rt.gameObject.SetActive(false);
                grid.Remove(b);
            });

            seq.Play();

            await Awaitable.WaitForSecondsAsync(delayBetweenBoxes);
        }

        animationInProgress = false;
        ValidateGrid();
    }


    public VFXShower vfxShower;

    private async Awaitable RemoveBoxes(HashSet<(int x, int y)> boxes)
    {
        var seq = DOTween.Sequence();
        animationInProgress = true;
        foreach (var b in boxes)
        {
            var box = grid[b];
            seq.AppendCallback(() => { box.rt.SetAsLastSibling(); });
            seq.Append(box.bg.DOColor(new Color(0.94f, 0.24f, 0.09f), 0.05f));
            seq.Join(box.rt.DOScale(Vector3.one * 1.2f, 0.05f).SetEase(Ease.InOutBack));
            seq.AppendCallback(() => { vfxShower.ShowBurst(box); });
            seq.Append(box.rt.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InOutBack));
            seq.AppendCallback(() => { box.rt.gameObject.SetActive(false); });
            grid.Remove(b);
        }

        seq.Play();
        await seq.AsyncWaitForCompletion();
        animationInProgress = false;
        ValidateGrid();
    }

    private async Awaitable AnimatePreClear(HashSet<(int x, int y)> boxes)
    {
        var seq = DOTween.Sequence();

        // Define a 4-color cycle (you can tweak these easily)
        Color[] glowColors = new Color[]
        {
            new Color(1f, 0.8f, 0.1f), // yellow
            new Color(1f, 0.5f, 0f), // orange
            new Color(1f, 0.2f, 0.8f), // pink/magenta
            new Color(0.2f, 0.6f, 1f), // blue
        };

        foreach (var pos in boxes)
        {
            if (!grid.TryGetValue(pos, out var box) || box == null)
                continue;

            box.rt.SetAsLastSibling();
            box.rt.localScale = Vector3.one;

            var boxSeq = DOTween.Sequence();

            // Color cycle loop: smoothly move through 4 colors
            for (int i = 0; i < glowColors.Length; i++)
            {
                Color next = glowColors[(i + 1) % glowColors.Length];
                boxSeq.Append(box.bg.DOColor(next, 0.15f).SetEase(Ease.InOutSine));
            }

            // Repeat the color loop twice for a nice pulse
            boxSeq.SetLoops(2, LoopType.Restart);

            // Add subtle bounce & shake while glowing
            boxSeq.Join(box.rt.DOPunchScale(Vector3.one * 0.2f, 0.4f, 8, 0.6f));
            boxSeq.Join(box.rt.DOShakePosition(0.5f, strength: 10f, vibrato: 10, randomness: 90f));

            seq.Join(boxSeq);
        }

        seq.AppendInterval(0.25f);

        await seq.AsyncWaitForCompletion();
    }


    private void CheckHighScore(int currentScore)
    {
        if (currentScore <= highScores.bestScore || tutorialManager.TutoiralGoingON) return;
        //Debug.LogError("checking highscore");
        int oldScore = int.TryParse(bestScoreLabel.text, out var old) ? old : 0;
        DOTween.To(
            () => oldScore,
            x =>
            {
                oldScore = x;
                bestScoreLabel.text = x.ToString();
            },
            currentScore,
            1f
        ).SetEase(Ease.OutExpo);
        if (hasBrokenRecord) return;
        highScoreImgText.text = currentScore.ToString();
        AnimateHighScore();
        hasBrokenRecord = true;
    }

    private void AnimateHighScore()
    {
        //todo: Show High Score Animation
        DOVirtual.DelayedCall(1.8f, () =>
        {
            highScoreImg.gameObject.SetActive(false);
            highScoreImg.gameObject.SetActive(true);
        });
        bestScoreLabel.transform
            .DOScale(1.3f, 0.3f)
            .SetLoops(2, LoopType.Yoyo)
            .SetEase(Ease.OutBounce);
    }

    private class HighScores
    {
        public int bestScore;

        public readonly Dictionary<string, int?> bestMoves = new()
        {
            { "1", null }, { "2", null }, { "3", null }, { "4", null }
        };
    }

    public void HeartByAd()
    {
        if (addType == 1)
            FirebaseCall.placement = AdPlacement.rwd_help;

        //FirebaseCall.placement = "rewarded_Help_ad";
        else if (addType == 2)
            FirebaseCall.placement = AdPlacement.rwd_time;
        else if (addType == 3)
            FirebaseCall.placement = AdPlacement.rwd_box;


        AdmobAdsScript.Instance.ShowRewardedAd(CompleteAdGetHeart);
    }

    void CompleteAdGetHeart()
    {
        if (addType == 3)
        {
            boxCounter = 10;
            //addBoxCounterText.text = boxCounter.ToString();
            UiManager.Instance.SetAddBoxCounterText(boxCounter); 
        }
        else if (addType == 1)
        {
            helpUses = 4;
            //UpdateHearts();
            UiManager.Instance.SetRemainingHelpCounterText(helpUses);

            //adBuyPanel.SetActive(false);
        }
        else if (addType == 2)
        {
            timeHelpUses = 2;
            //UpdateHearts();
            UiManager.Instance.SetAddTimerCounterText(timeHelpUses);
            timeLeft += 20;
        }

        adBuyPanel.SetActive(false);
    }

    public void UpdateScoreAndHighScore(int newScore)
    {
        // Update current score
        score += newScore;
        scoreLabel.text = $"Score : {score}";
        if (score > highScores.bestScore)
            bestScoreLabel.text = score.ToString();
    }

    [Button]
    public void ClearAllHighScores()
    {
        try
        {
            // Create fresh default highscores object
            var empty = new HighScores();

            // Overwrite each save file with an empty/default HighScores
            SaveHighScoresClassic(SaveClassic, empty);
            SaveHighScoresSingle(SaveRackup, empty);
            SaveHighScoresSingle(SaveClock, empty);

            // Clear any PlayerPrefs copy used by the game UI
            PlayerPrefs.DeleteKey("BestScore");
            PlayerPrefs.Save();

            // Update in-memory state and UI
            highScores = new HighScores();
            if (bestScoreLabel != null) bestScoreLabel.text = "0";

            Debug.Log("CubeSmasher: cleared all highscores.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"CubeSmasher: failed to clear highscores - {ex}");
        }
    }
    //[Button]
    //public void SetRackUpScores()
    //{
    //    var empty = new HighScores();
    //    empty.bestScore = 100;
    //    // Overwrite each save file with an empty/default HighScores
    //    SaveHighScoresClassic(SaveClassic, empty);
    //    SaveHighScoresSingle(SaveRackup, empty);
    //    empty.bestScore -= 10;
    //    SaveHighScoresSingle(SaveClock,empty);
    //}
    
}