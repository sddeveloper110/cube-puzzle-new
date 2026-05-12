// using System;
// using System.Collections.Generic;
// using System.IO;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;
// using UnityEngine.EventSystems;
// #if ENABLE_INPUT_SYSTEM
// using UnityEngine.InputSystem.UI;
// #endif
// using Random = UnityEngine.Random;
//
// public class CubeSmasherOld : MonoBehaviour
// {
//     private void Awake()
//     {
//         Screen.orientation = ScreenOrientation.Portrait;
//     }
//
//     // ========= Constants =========
//     private const int k_boxSize = 60;
//     private const int k_gridSpacing = 2;
//     private const float k_slideSpeed = 0.15f;
//     private const int k_requiredEmptyCount = 2;
//     private const float k_dropIntervalL3 = 20.0f;
//     private const float k_dropIntervalL4 = 10.0f;
//     private const float k_startTime = 60f;
//     private const float k_addedTime = 20f;
//
//     // Colors
//     private static readonly Color NavyBlue = Hex("#001f3f");
//     private static readonly Color Red = Color.red;
//     private static readonly Color LightGray = new(0.83f, 0.83f, 0.83f);
//     private static readonly Color White = Color.white;
//     private static readonly Color Black = Color.black;
//     private static readonly Color Yellow = new(1f, 0.92f, 0.016f);
//     private static readonly Color Lime = new(0.5f, 1f, 0f);
//     private static readonly Color Magenta = new(1f, 0f, 1f);
//     private static readonly Color Cyan = Color.cyan;
//     private static readonly Color Orange = new(1f, 0.55f, 0f);
//
//     // Saves (Unity: use persistentDataPath)
//     private static string SaveClassic => Path.Combine(Application.persistentDataPath, "cube_smasher_highscores.json");
//     private static string SaveRackup => Path.Combine(Application.persistentDataPath, "cube_smasher_rackup.json");
//     private static string SaveClock => Path.Combine(Application.persistentDataPath, "cube_smasher_clock.json");
//
//     // Resources (place in Assets/Resources/)
//     private const string k_imgSplash = "HubcapSplash2"; // HubcapSplash2.jpg
//     private const string k_imgTitle = "Cube_Background"; // Cube_Background.jpg
//     private const string k_imgInstr1 = "Instruction1";
//     private const string k_imgInstr2 = "Instruction2";
//     private const string k_imgInstr3 = "Instruction3";
//     private const string k_imgInstr4 = "Instruction4";
//
//     // ======== State machine ========
//     private enum GameState
//     {
//         InitialSplash,
//         Title,
//         Instructions,
//         Game
//     }
//
//     private GameState state = GameState.InitialSplash;
//
//     private enum Mode
//     {
//         Classic,
//         Rackup,
//         Clock
//     }
//
//     private Mode gameMode = Mode.Classic;
//
//     // ======== UI Roots ========
//     private Canvas canvas;
//     private RectTransform root;
//     private Image fullBg; // background image holder
//     private TextMeshProUGUI hudTop; // timer or page
//     private TextMeshProUGUI hudHelp; // 'help' clickable
//     private List<TextMeshProUGUI> helpHearts = new();
//     private TextMeshProUGUI addTimeBtn;
//     private TextMeshProUGUI addBoxBtn;
//     private TextMeshProUGUI topExitBtn;
//     private TextMeshProUGUI playAgainBtn;
//     private TextMeshProUGUI exitGameBtn;
//     private TextMeshProUGUI bestScoreLabel, bestMovesLabel, scoreLabel, movesLabel;
//
//     // Title buttons
//     private readonly List<(Button btn, Mode mode)> titleModeButtons = new();
//
//     // Instructions
//     private int instrPage;
//     private readonly string[] instrImgs = { k_imgInstr1, k_imgInstr2, k_imgInstr3, k_imgInstr4 };
//
//     // ======== Game Data ========
//     private int centerValue;
//     private Dictionary<(int x, int y), Box> grid = new();
//     private Box draggedBox;
//     private Vector2 dragOffset;
//     private int score;
//     private int moves;
//     private int level = 1;
//     private float levelTimer;
//     private float timeLeft;
//     private int helpUses = 3;
//     private bool gameOver;
//     private int lastBeepSecond = -999;
//
//     // Highscores
//     private class Highscores
//     {
//         public int bestScore;
//
//         // classic: per-level best moves; others: single int best_moves
//         public Dictionary<string, int?> bestMoves = new()
//             { { "1", null }, { "2", null }, { "3", null }, { "4", null } };
//     }
//
//     private Highscores highscores = new();
//     private string activeSavePath;
//
//     // ======== Box (UI element) ========
//     public class Box
//     {
//         public int? value; // null == empty
//         public (int x, int y) gridPos;
//         public bool fixedRed;
//         public RectTransform rt;
//         public Image bg;
//         public TextMeshProUGUI label;
//         public Vector2 originalScreenPos;
//
//         public void SetVisual()
//         {
//             if (fixedRed)
//             {
//                 bg.color = Red;
//                 label.color = White;
//             }
//             else
//             {
//                 bg.color = value.HasValue ? LightGray : White;
//                 label.color = Black;
//             }
//
//             label.text = (value.HasValue) ? value.Value.ToString() : "";
//         }
//     }
//
//     // ========= Unity lifecycle =========
//     private void Start()
//     {
//         // SetupCanvas();
//         // GoInitialSplash();
//         HandleWin();
//     }
//
//     // private void Update()
//     // {
//     //     switch (state)
//     //     {
//     //         case GameState.InitialSplash:
//     //             initialTimer += Time.deltaTime;
//     //             if (initialTimer >= 3f && !initialTransitioned)
//     //             {
//     //                 initialTransitioned = true;
//     //                 GoTitle();
//     //             }
//     //
//     //             break;
//     //
//     //         case GameState.Game:
//     //             if (gameOver) return;
//     //             DoGameUpdate(Time.deltaTime);
//     //             break;
//     //     }
//     // }
//
//     // ========= Initial Splash =========
//     private float initialTimer;
//     private bool initialTransitioned;
//
//     private void GoInitialSplash()
//     {
//         state = GameState.InitialSplash;
//         ClearUI();
//
//         SetBackground(k_imgSplash, cover: true);
//         // Tap to skip
//         AddFullScreenButton(GoTitle);
//     }
//
//     // ========= Title (SplashScreen) =========
//     private void GoTitle()
//     {
//         state = GameState.Title;
//         ClearUI();
//
//         SetBackground(k_imgTitle, cover: true);
//         // DEBUG: big green button to start Classic mode
//         var dbgGo = new GameObject("DEBUG_StartClassic_Button", typeof(RectTransform), typeof(Image), typeof(Button));
//         dbgGo.transform.SetParent(root, false);
//
//         var dbgRT = dbgGo.GetComponent<RectTransform>();
//         dbgRT.anchorMin = dbgRT.anchorMax = new Vector2(0.5f, 0f);
//         dbgRT.anchoredPosition = new Vector2(0, 140f);
//         dbgRT.sizeDelta = new Vector2(300, 90);
//
//         var dbgImg = dbgGo.GetComponent<Image>();
//         dbgImg.color = new Color(0.2f, 0.8f, 0.2f, 1f);
//
//         var dbgTextGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
//         dbgTextGo.transform.SetParent(dbgGo.transform, false);
//         var dbgTrt = dbgTextGo.GetComponent<RectTransform>();
//         dbgTrt.anchorMin = dbgTrt.anchorMax = new Vector2(0.5f, 0.5f);
//         dbgTrt.anchoredPosition = Vector2.zero;
//         var dbgText = dbgTextGo.GetComponent<TextMeshProUGUI>();
//         // dbgText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
//         dbgText.fontSize = 24;
//         dbgText.alignment = TextAlignmentOptions.Center;
//         dbgText.color = Color.black;
//         dbgText.text = "DEBUG: Start Classic";
//
//         var debugBtn = dbgGo.GetComponent<Button>();
//         debugBtn.onClick.RemoveAllListeners();
//         debugBtn.onClick.AddListener(() =>
//         {
//             Debug.Log("DEBUG button clicked!");
//             gameMode = Mode.Classic;
//             GoGame();
//         });
//
//
//         // "How to play"
//         var how = AddText("How to play", 22, Color.white, new Vector2(0.5f, 0), new Vector2(0.5f, 0),
//             new Vector2(0, 50));
//         AddButtonOverlay(how, GoInstructions);
//
//         // Triangle layout: Rackup (left), Clock (right), Classic (bottom)
//         float topY = 300f, bottomY = 220f, spacingX = 90f;
//
//         titleModeButtons.Clear();
//         titleModeButtons.Add((
//             AddRoundedButton("Rack Up Points", new Vector2(-spacingX, topY), new Color(0.2f, 0.8f, 0.2f)),
//             Mode.Rackup));
//         titleModeButtons.Add((
//             AddRoundedButton("Beat The Clock", new Vector2(spacingX, topY), new Color(1f, 0.9f, 0.2f)), Mode.Clock));
//         titleModeButtons.Add((AddRoundedButton("Classic Levels", new Vector2(0, bottomY), new Color(1f, 0.6f, 0.2f)),
//             Mode.Classic));
//
//         foreach (var (btn, m) in titleModeButtons)
//         {
//             btn.onClick.AddListener(() =>
//             {
//                 gameMode = m;
//                 GoGame();
//             });
//         }
//     }
//
//     // ========= Instructions =========
//     private void GoInstructions()
//     {
//         state = GameState.Instructions;
//         ClearUI();
//         instrPage = 0;
//
//         SetBackground(instrImgs[instrPage], cover: false, fit: 0.75f, yOffset: 40f);
//
//         hudTop = AddText(GetPageText(), 20, Color.red, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
//             new Vector2(0, -250));
//         var playBtn = AddText("Play Game", 24, Color.yellow, new Vector2(0.5f, 0), new Vector2(0.5f, 0),
//             new Vector2(0, 50));
//         AddButtonOverlay(playBtn, GoTitle);
//
//         // Tap anywhere else => next page
//         AddFullScreenButton(() =>
//         {
//             instrPage = (instrPage + 1) % instrImgs.Length;
//             SetBackground(instrImgs[instrPage], cover: false, fit: 0.75f, yOffset: 40f);
//             hudTop.text = GetPageText();
//         });
//     }
//
//     private string GetPageText() => $"{instrPage + 1}/{instrImgs.Length}";
//
//     // ========= Game =========
//     private void GoGame()
//     {
//         state = GameState.Game;
//         ClearUI();
//         gameOver = false;
//
//         // Background color (navy)
//         SetSolidBackground(NavyBlue);
//
//         // HUD
//         hudTop = AddText("", 26, Color.white, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -50));
//         hudHelp = AddText("help", 20, Color.white, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-80, -80));
//         AddButtonOverlay(hudHelp, UseHelp);
//
//         helpHearts.Clear();
//         for (var i = 0; i < 3; i++)
//         {
//             var heart = AddText("❤️", 24, Color.white, new Vector2(1f, 1f), new Vector2(1f, 1f),
//                 new Vector2(-110 + i * 30, -50));
//             helpHearts.Add(heart);
//         }
//
//         if (gameMode == Mode.Clock || gameMode == Mode.Classic)
//         {
//             addTimeBtn = AddText("add time", 20, Color.white, new Vector2(0f, 1f), new Vector2(0f, 1f),
//                 new Vector2(80, -80));
//             AddButtonOverlay(addTimeBtn, HandleAddTime);
//         }
//
//         addBoxBtn = AddText("add box", 20, Color.white, new Vector2(1f, 0f), new Vector2(1f, 0f),
//             new Vector2(-200, 40));
//         AddButtonOverlay(addBoxBtn, HandleAddBox);
//
//         scoreLabel = AddText("Score: 0", 20, Color.white, new Vector2(0f, 0f), new Vector2(0f, 0f),
//             new Vector2(80, 70));
//         bestScoreLabel = AddText("🏆 0", 20, Yellow, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(80, 40));
//         movesLabel = AddText("Moves: 0", 20, Color.white, new Vector2(1f, 0f), new Vector2(1f, 0f),
//             new Vector2(-80, 70));
//         bestMovesLabel = AddText("🏆 -", 20, Yellow, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-80, 40));
//
//         topExitBtn = AddText("Exit Game", 24, Color.white, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
//             new Vector2(0, -120));
//         AddButtonOverlay(topExitBtn, GoTitle);
//
//         // Init game data
//         score = 0;
//         moves = 0;
//         lastBeepSecond = -999;
//         helpUses = 3;
//         level = 1;
//         levelTimer = 0f;
//         timeLeft = (gameMode == Mode.Clock || gameMode == Mode.Classic) ? k_startTime : 0f;
//
//         // Load highscores save
//         if (gameMode == Mode.Classic)
//         {
//             activeSavePath = SaveClassic;
//             highscores = LoadHighscoresClassic(activeSavePath);
//             bestMovesLabel.text = $"🏆 {GetBestMovesForLevel(1)}";
//         }
//         else
//         {
//             activeSavePath = (gameMode == Mode.Rackup) ? SaveRackup : SaveClock;
//             highscores = LoadHighscoresSingle(activeSavePath); // will store best_score and best_moves (single)
//             bestMovesLabel.text = $"🏆 {(GetSingleBestMoves() is { } v ? v.ToString() : "-")}";
//         }
//
//         bestScoreLabel.text = $"🏆 {highscores.bestScore}";
//
//         GenerateGrid();
//         EnsureSingleRedBox();
//         EnsureTwoEmpties();
//
//         UpdateHUDTimer();
//     }
//
//     // ====== Game per-frame ======
//     private void DoGameUpdate(float dt)
//     {
//         // Auto drop logic for Classic level >= 3
//         if (gameMode == Mode.Classic && level >= 3)
//         {
//             levelTimer += dt;
//             var interval = (level >= 4) ? Mathf.Max(5f, k_dropIntervalL4 - 0.5f * (level - 4)) : k_dropIntervalL3;
//             if (levelTimer >= interval)
//             {
//                 levelTimer = 0f;
//                 HandleAddBox();
//             }
//         }
//
//         // Timer for clock/levels
//         if ((gameMode == Mode.Clock) || (gameMode == Mode.Classic && level >= 2))
//         {
//             timeLeft -= dt;
//             if (timeLeft <= 0)
//             {
//                 TriggerGameOver();
//             }
//             else
//             {
//                 var whole = Mathf.FloorToInt(timeLeft);
//                 if (timeLeft <= 5f && whole != lastBeepSecond)
//                 {
//                     lastBeepSecond = whole;
//                     // Simple beep: you can attach an AudioSource with a click, or leave silent
//                 }
//
//                 UpdateHUDTimer();
//             }
//         }
//
//         EnsureSingleRedBox();
//         EnsureTwoEmpties();
//     }
//
//     private void UpdateHUDTimer()
//     {
//         if (hudTop == null) return;
//         if (gameMode == Mode.Clock || (gameMode == Mode.Classic && level >= 2))
//             hudTop.text = $"Time: {Mathf.Max(0, Mathf.FloorToInt(timeLeft))}";
//         else
//             hudTop.text = "";
//     }
//
//     // ========= Grid logic =========
//     // We build a 5x5 centered on (0,0). Positions in range [-2..2] for x and y.
//     // Center (0,0) is the fixed red box with centerValue (random 10..21).
//     private void GenerateGrid()
//     {
//         grid.Clear();
//         centerValue = Random.Range(10, 22);
//         // Build 3x3 and 5x5, choose one empty in 3x3 and one in 5x5 ring
//         while (true)
//         {
//             var temp = new Dictionary<(int x, int y), Box>
//             {
//                 [(0, 0)] = MakeBox(centerValue, (0, 0), fixedRed: true)
//             };
//
//             var pos3 = new List<(int, int)>();
//             for (var x = -1; x <= 1; x++)
//             for (var y = -1; y <= 1; y++)
//                 if (!(x == 0 && y == 0))
//                     pos3.Add((x, y));
//
//             var pos5 = new List<(int, int)>();
//             for (var x = -2; x <= 2; x++)
//             for (var y = -2; y <= 2; y++)
//                 if (!temp.ContainsKey((x, y)) && !(Mathf.Abs(x) <= 1 && Mathf.Abs(y) <= 1))
//                     pos5.Add((x, y));
//
//             var empty3 = pos3[Random.Range(0, pos3.Count)];
//             var empty5 = pos5[Random.Range(0, pos5.Count)];
//             if (empty5 == empty3) continue;
//
//             foreach (var p in pos3)
//                 temp[p] = (p == empty3) ? MakeBox(null, p) : MakeBox(GenNumExcluding(centerValue), p);
//
//             foreach (var p in pos5)
//                 temp[p] = (p == empty5) ? MakeBox(null, p) : MakeBox(GenNumExcluding(centerValue), p);
//
//             if (ValidGrid(temp))
//             {
//                 grid = temp;
//                 break;
//             }
//         }
//
//         foreach (var kv in grid)
//             PlaceBox(kv.Value);
//
//         // Ensure center (0,0) drawn on top
//         if (grid.TryGetValue((0, 0), out var red))
//             red.rt.SetAsLastSibling();
//     }
//
//     private bool ValidGrid(Dictionary<(int, int), Box> test)
//     {
//         foreach (var kv in test)
//         {
//             var (x, y) = kv.Key;
//             var box = kv.Value;
//             if (!box.value.HasValue || box.fixedRed) continue;
//             // row
//             var sumRow = 0;
//             for (var i = -2; i <= 2; i++)
//             {
//                 if (test.TryGetValue((i, y), out var b))
//                     sumRow += b.value ?? 0;
//             }
//
//             if (sumRow == centerValue) return false;
//
//             // col
//             var sumCol = 0;
//             for (var j = -2; j <= 2; j++)
//             {
//                 if (test.TryGetValue((x, j), out var b))
//                     sumCol += b.value ?? 0;
//             }
//
//             if (sumCol == centerValue) return false;
//         }
//
//         return true;
//     }
//
//     private int GenNumExcluding(int exclude)
//     {
//         var v = Random.Range(1, 11);
//         while (v == exclude) v = Random.Range(1, 11);
//         return v;
//     }
//
//     private Box MakeBox(int? value, (int x, int y) pos, bool fixedRed = false)
//     {
//         var go = new GameObject($"Box_{pos.x}_{pos.y}", typeof(RectTransform), typeof(Image));
//         go.transform.SetParent(root, false);
//         var rt = go.GetComponent<RectTransform>();
//         rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f); // 👈 add
//         rt.pivot = new Vector2(0.5f, 0.5f); // 👈 add
//         rt.anchoredPosition = Vector2.zero; // 👈 add
//         rt.sizeDelta = new Vector2(k_boxSize, k_boxSize);
//         // clear any inherited offset
//         // keep your size
//
//         var img = go.GetComponent<Image>();
//         img.color = White;
//
//         // Label
//         var textGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
//         textGo.transform.SetParent(go.transform, false);
//         var trt = textGo.GetComponent<RectTransform>();
//         trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 0.5f);
//         trt.pivot = new Vector2(0.5f, 0.5f);
//         trt.anchoredPosition = Vector2.zero;
//         var t = textGo.GetComponent<TextMeshProUGUI>();
//         t.alignment = TextAlignmentOptions.Center;
//         t.raycastTarget = false;
//         t.fontSize = 20;
//         t.color = Black;
//
//         var b = new Box
//         {
//             value = value,
//             gridPos = pos,
//             fixedRed = fixedRed,
//             rt = rt,
//             bg = img,
//             label = t
//         };
//         b.SetVisual();
//
//         // Drag
//         var drag = go.AddComponent<UIDragBox>();
//         // drag.Init(this, b);
//
//         return b;
//     }
//
//     // Center grid around (Screen center x - 30, Screen center y - 100)
//     private Vector2 GridOrigin => Vector2.zero; // 👈 center of the canvas
//     // (delete any "- 30" or "- 100" offsets you had)
//
//     private float ScreenWidthHalf => root.rect.width / 2f;
//     private float ScreenHeightHalf => root.rect.height / 2f;
//
//     private Vector2 GridToScreen((int x, int y) p)
//     {
//         float step = k_boxSize; // your constant
//         return GridOrigin + new Vector2(
//             p.x * (step + k_gridSpacing),
//             p.y * (step + k_gridSpacing)
//         );
//     }
//
//
//     private void PlaceBox(Box b)
//     {
//         var pos = GridToScreen(b.gridPos);
//         b.rt.anchoredPosition = pos;
//         b.originalScreenPos = pos;
//         b.SetVisual();
//     }
//
//     private void EnsureSingleRedBox()
//     {
//         // Keep (0,0) red, demote others
//         var hasRed = false;
//         foreach (var kv in grid)
//         {
//             var p = kv.Key;
//             var box = kv.Value;
//             if (box.fixedRed)
//             {
//                 if (p == (0, 0)) hasRed = true;
//                 else
//                 {
//                     box.fixedRed = false;
//                     if (!box.value.HasValue || box.value == centerValue)
//                         box.value = GenNumExcluding(centerValue);
//                     box.SetVisual();
//                 }
//             }
//         }
//
//         if (!hasRed)
//         {
//             // Make sure (0,0) exists and is red with centerValue
//             if (!grid.ContainsKey((0, 0)))
//                 grid[(0, 0)] = MakeBox(centerValue, (0, 0), true);
//             var c = grid[(0, 0)];
//             c.fixedRed = true;
//             c.value = centerValue;
//             c.SetVisual();
//             PlaceBox(c);
//         }
//     }
//
//     private void EnsureTwoEmpties()
//     {
//         var empties = new List<Box>();
//         foreach (var b in grid.Values)
//             if (!b.value.HasValue)
//                 empties.Add(b);
//         var need = k_requiredEmptyCount - empties.Count;
//         if (need > 0)
//         {
//             var candidates = new List<Box>();
//             foreach (var b in grid.Values)
//                 if (b.value.HasValue && !b.fixedRed)
//                     candidates.Add(b);
//             Shuffle(candidates);
//             for (var i = 0; i < Mathf.Min(need, candidates.Count); i++)
//             {
//                 candidates[i].value = null;
//                 candidates[i].SetVisual();
//             }
//         }
//     }
//
//     // ========= Interactions =========
//     public void BeginDrag(Box b, Vector2 localPointer)
//     {
//         if (gameOver) return;
//         if (b.fixedRed || !b.value.HasValue) return;
//         draggedBox = b;
//         dragOffset = localPointer - b.rt.anchoredPosition;
//     }
//
//     public void DragMove(Vector2 localPointer)
//     {
//         if (gameOver || draggedBox == null) return;
//         draggedBox.rt.anchoredPosition = localPointer - dragOffset;
//     }
//
//     public void EndDrag(Vector2 localPointer)
//     {
//         if (gameOver || draggedBox == null) return;
//         var b = draggedBox;
//         draggedBox = null;
//
//         var src = b.gridPos;
//         // Find empty neighbors and choose the closest to touch end
//         var candidates = new List<((int x, int y) dst, float dist2)>();
//         var dirs = new (int x, int y)[] { (0, 1), (1, 0), (0, -1), (-1, 0) };
//         foreach (var d in dirs)
//         {
//             var dst = (src.x + d.x, src.y + d.y);
//             if (grid.TryGetValue(dst, out var neighbor) && !neighbor.value.HasValue)
//             {
//                 var screen = GridToScreen(dst);
//                 var dist2 = (localPointer - screen).sqrMagnitude;
//                 candidates.Add((dst, dist2));
//             }
//         }
//
//         if (candidates.Count > 0)
//         {
//             candidates.Sort((a, c) => a.dist2.CompareTo(c.dist2));
//             moves++;
//             movesLabel.text = $"Moves: {moves}";
//             SwapBoxes(src, candidates[0].dst);
//         }
//         else
//         {
//             // Snap back
//             b.rt.anchoredPosition = b.originalScreenPos;
//         }
//     }
//
//     private void SwapBoxes((int x, int y) src, (int x, int y) dst)
//     {
//         var a = grid[src];
//         var b = grid[dst];
//         (a.gridPos, b.gridPos) = (dst, src);
//         grid[dst] = a;
//         grid[src] = b;
//         PlaceBox(a);
//         PlaceBox(b);
//
//         // After sliding, check matches
//         Invoke(nameof(CheckMatches), k_slideSpeed + 0.05f);
//     }
//
//     private void CheckMatches()
//     {
//         var toClear = new HashSet<(int x, int y)>();
//         var matchedRows = new HashSet<int>();
//         var matchedCols = new HashSet<int>();
//
//         var xs = new SortedSet<int>();
//         var ys = new SortedSet<int>();
//         foreach (var p in grid.Keys)
//         {
//             xs.Add(p.x);
//             ys.Add(p.y);
//         }
//
//         foreach (var y in ys)
//         {
//             var row = new List<(int x, int y)>();
//             foreach (var x in xs)
//                 if (grid.ContainsKey((x, y)))
//                     row.Add((x, y));
//             var sum = 0;
//             foreach (var p in row) sum += grid[p].value ?? 0;
//             if (sum == centerValue)
//             {
//                 foreach (var p in row)
//                     if (!grid[p].fixedRed)
//                         toClear.Add(p);
//                 matchedRows.Add(y);
//             }
//         }
//
//         foreach (var x in xs)
//         {
//             var col = new List<(int x, int y)>();
//             foreach (var y in ys)
//                 if (grid.ContainsKey((x, y)))
//                     col.Add((x, y));
//             var sum = 0;
//             foreach (var p in col) sum += grid[p].value ?? 0;
//             if (sum == centerValue)
//             {
//                 foreach (var p in col)
//                     if (!grid[p].fixedRed)
//                         toClear.Add(p);
//                 matchedCols.Add(x);
//             }
//         }
//
//         if (toClear.Count == 0) return;
//
//         // Sound & TTS praise (you can add an AudioSource for sounds)
//         var phrases = new[] { "Genius Move", "well done", "Perfect", "Great Job", "" };
//         IOSSpeech.Speak(phrases[Random.Range(0, phrases.Length)]);
//
//         score += 10;
//         scoreLabel.text = $"Score: {score}";
//
//         // Floaty +10
//         var plus = AddText("+10", 28, RandomChoice(new[] { Orange, Yellow, White }), new Vector2(0.5f, 0.5f),
//             new Vector2(0.5f, 0.5f), new Vector2(-80, 0));
//         FadeAndRise(plus, 2f);
//
//         // Praise label
//         var phrase2 = RandomChoice(new[] { "Genius Move", "well done", "Perfect", "Great Job" });
//         var praise = AddText(phrase2, 32, RandomChoice(new[] { Yellow, Cyan, Lime, Orange, Magenta }),
//             new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 60));
//         FadeAndRise(praise, 1.5f);
//
//         // Clear
//         foreach (var p in toClear)
//         {
//             var box = grid[p];
//             Destroy(box.rt.gameObject);
//             grid.Remove(p);
//         }
//
//         ShiftGrid(matchedRows, matchedCols);
//
//         if (CheckWinCondition())
//             HandleWin();
//     }
//
//     private void ShiftGrid(HashSet<int> clearedRows, HashSet<int> clearedCols)
//     {
//         var old = new List<KeyValuePair<(int, int), Box>>(grid);
//         var fresh = new Dictionary<(int, int), Box>();
//         foreach (var kv in old)
//         {
//             var (x, y) = kv.Key;
//             if (!grid.ContainsKey((x, y))) continue;
//             int nx = x, ny = y;
//             foreach (var cx in clearedCols)
//                 if (cx < x)
//                     nx--;
//             foreach (var ry in clearedRows)
//                 if (ry < y)
//                     ny--;
//
//             var box = kv.Value;
//             box.gridPos = (nx, ny);
//             fresh[(nx, ny)] = box;
//         }
//
//         grid = fresh;
//
//         foreach (var b in grid.Values) PlaceBox(b);
//
//         EnsureSingleRedBox();
//         EnsureTwoEmpties();
//     }
//
//     private bool CheckWinCondition()
//     {
//         var redFound = false;
//         var numberTiles = 0;
//         foreach (var b in grid.Values)
//         {
//             if (b.fixedRed) redFound = true;
//             else if (b.value.HasValue) numberTiles++;
//         }
//
//         return redFound && numberTiles <= 1;
//     }
//
//     private void HandleWin()
//     {
//         // Best score
//         if (score > highscores.bestScore) highscores.bestScore = score;
//
//         if (gameMode == Mode.Classic)
//         {
//             var key = level.ToString();
//             var prev = GetBestMovesForLevel(level);
//             if (!prev.HasValue || moves < prev.Value) SetBestMovesForLevel(level, moves);
//             SaveHighscoresClassic(SaveClassic, highscores);
//
//             // bestScoreLabel.text = $"🏆 {highscores.bestScore}";
//             // bestMovesLabel.text = $"🏆 {GetBestMovesForLevel(level)}";
//
//             IOSSpeech.Speak("nice work");
//
//             if (level < 4)
//             {
//                 level++;
//                 if (level == 3 || level == 4) timeLeft = k_startTime;
//             }
//             else
//             {
//                 level++;
//                 timeLeft = k_startTime;
//             }
//
//             ResetGrid();
//             FlashCenterLabel($"Level {level}", Yellow);
//         }
//         else
//         {
//             var prev = GetSingleBestMoves();
//             if (!prev.HasValue || moves < prev.Value) SetSingleBestMoves(moves);
//             SaveHighscoresSingle(activeSavePath, highscores);
//
//             // bestScoreLabel.text = $"🏆 {highscores.bestScore}";
//             // bestMovesLabel.text = $"🏆 {GetSingleBestMoves()?.ToString() ?? "-"}";
//
//             ResetGrid();
//             FlashCenterLabel("Smashed It", Lime);
//             if (gameMode == Mode.Clock) timeLeft = k_startTime;
//         }
//     }
//
//     private void ResetGrid()
//     {
//         // Destroy all box objects
//         foreach (var b in grid.Values)
//             if (b?.rt)
//                 Destroy(b.rt.gameObject);
//         grid.Clear();
//
//         centerValue = Random.Range(10, 22);
//         GenerateGrid();
//         EnsureSingleRedBox();
//         EnsureTwoEmpties();
//         moves = 0;
//         movesLabel.text = "Moves: 0";
//     }
//
//     // ========= Buttons / helpers =========
//     private void HandleAddBox()
//     {
//         // Limit X to the maximum X of any red (always 0 here, but keep logic)
//         var redPositions = new List<(int, int)>();
//         foreach (var kv in grid)
//             if (kv.Value.fixedRed)
//                 redPositions.Add(kv.Key);
//         if (redPositions.Count == 0) return;
//         var redX = int.MinValue;
//         foreach (var p in redPositions) redX = Mathf.Max(redX, p.Item1);
//
//         var topY = int.MinValue;
//         foreach (var p in grid.Keys) topY = Mathf.Max(topY, p.Item2);
//
//         // Try fill top row left to right within boundary
//         var rowPositions = new List<(int, int)>();
//         for (var x = -2; x <= redX; x++) rowPositions.Add((x, topY));
//         var emptySpots = new List<(int, int)>();
//         foreach (var p in rowPositions)
//             if (!grid.ContainsKey(p))
//                 emptySpots.Add(p);
//
//         (int x, int y) pos = (0, 0);
//         if (emptySpots.Count > 0)
//         {
//             emptySpots.Sort((a, b) => a.Item1.CompareTo(b.Item1));
//             pos = emptySpots[0];
//         }
//         else
//         {
//             pos = (-2, topY + 1);
//         }
//
//         var val = GenNumExcluding(centerValue);
//         var nb = MakeBox(val, pos);
//         grid[pos] = nb;
//         PlaceBox(nb);
//
//         // Game over if too close to top overlay (roughly)
//         var maxY = float.MinValue;
//         foreach (var b in grid.Values) maxY = Mathf.Max(maxY, b.rt.anchoredPosition.y);
//         if (maxY + k_boxSize / 2f >= root.rect.height - 120f)
//             TriggerGameOver();
//     }
//
//     private void HandleAddTime()
//     {
//         if (helpUses == 0) return;
//         helpUses--;
//         RemoveOneHelpHeart();
//         timeLeft += k_addedTime;
//         // Play SFX if desired
//     }
//
//     private void UseHelp()
//     {
//         if (helpUses == 0) return;
//         helpUses--;
//         RemoveOneHelpHeart();
//
//         // Top row
//         var topY = int.MinValue;
//         foreach (var p in grid.Keys) topY = Mathf.Max(topY, p.Item2);
//         var redInTop = false;
//         foreach (var kv in grid)
//             if (kv.Key.Item2 == topY && kv.Value.fixedRed)
//             {
//                 redInTop = true;
//                 break;
//             }
//
//         if (!redInTop && Random.value < 0.5f)
//         {
//             // Clear top row
//             var toRemove = new List<(int, int)>();
//             foreach (var kv in grid)
//                 if (kv.Key.Item2 == topY)
//                     toRemove.Add(kv.Key);
//             foreach (var p in toRemove)
//             {
//                 if (!grid.TryGetValue(p, out var value)) continue;
//                 Destroy(value.rt.gameObject);
//                 grid.Remove(p);
//             }
//
//             // Flying hearts (simple move left)
//             var y = GridToScreen((0, topY)).y + k_boxSize / 2f;
//             for (var i = 0; i < 3; i++)
//             {
//                 var h = AddText("❤️", 24, Color.white, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
//                     new Vector2(root.rect.width + i * 30f, y));
//                 MoveLeftAndRemove(h, 1.5f, -50f);
//             }
//         }
//         else
//         {
//             // Turn a random regular tile into empty
//             var candidates = new List<Box>();
//             foreach (var b in grid.Values)
//                 if (b.value.HasValue && !b.fixedRed)
//                     candidates.Add(b);
//             if (candidates.Count > 0)
//             {
//                 var t = candidates[Random.Range(0, candidates.Count)];
//                 t.value = null;
//                 t.SetVisual();
//                 var heartIcon = AddText("❤️", 24, Color.white,
//                     new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
//                     t.rt.anchoredPosition);
//                 FadeOutAndRemove(heartIcon, 2f, 0.5f);
//             }
//         }
//
//         EnsureTwoEmpties();
//         if (CheckWinCondition()) HandleWin();
//     }
//
//     private void TriggerGameOver()
//     {
//         if (gameOver) return;
//         gameOver = true;
//
//         if (topExitBtn) Destroy(topExitBtn.gameObject);
//
//         var yTop = root.rect.height - 150f;
//         AddText("Game Over", 40, Red, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
//             new Vector2(0, -150));
//         playAgainBtn = AddText("Play Again", 32, Lime, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
//             new Vector2(0, -200));
//         exitGameBtn = AddText("Exit Game", 32, Lime, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
//             new Vector2(0, -250));
//         AddButtonOverlay(playAgainBtn, () => GoGame());
//         AddButtonOverlay(exitGameBtn, () => GoTitle());
//     }
//
//     // ========= Highscores (JSON) =========
//     private Highscores LoadHighscoresClassic(string path)
//     {
//         if (File.Exists(path))
//         {
//             try
//             {
//                 var txt = File.ReadAllText(path);
//                 return JsonUtility.FromJson<Highscores>(txt);
//             }
//             catch
//             {
//             }
//         }
//
//         return new Highscores();
//     }
//
//     private void SaveHighscoresClassic(string path, Highscores data)
//     {
//         File.WriteAllText(path, JsonUtility.ToJson(data));
//     }
//
//     private Highscores LoadHighscoresSingle(string path)
//     {
//         // Use same struct; store best_moves["single"]
//         var h = LoadHighscoresClassic(path);
//         h.bestMoves.TryAdd("single", null);
//         // ensure 1..4 exist too so serialization stays consistent
//         for (var i = 1; i <= 4; i++)
//             if (!h.bestMoves.ContainsKey(i.ToString()))
//                 h.bestMoves[i.ToString()] = null;
//         return h;
//     }
//
//     private void SaveHighscoresSingle(string path, Highscores data)
//     {
//         data.bestMoves.TryAdd("single", null);
//         File.WriteAllText(path, JsonUtility.ToJson(data));
//     }
//
//     private int? GetBestMovesForLevel(int lvl)
//     {
//         var key = lvl.ToString();
//         return highscores.bestMoves.GetValueOrDefault(key);
//     }
//
//     private void SetBestMovesForLevel(int lvl, int moves)
//     {
//         highscores.bestMoves[lvl.ToString()] = moves;
//     }
//
//     private int? GetSingleBestMoves()
//     {
//         return highscores.bestMoves.GetValueOrDefault("single");
//     }
//
//     private void SetSingleBestMoves(int moves)
//     {
//         highscores.bestMoves["single"] = moves;
//     }
//
//     // ========= UI helpers =========
//     private void SetupCanvas()
//     {
//         // Canvas + scaler
//         var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
//         canvas = go.GetComponent<Canvas>();
//         canvas.renderMode = RenderMode.ScreenSpaceOverlay;
//
//         var scaler = go.GetComponent<CanvasScaler>();
//         scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
//         scaler.referenceResolution = new Vector2(1080, 1920); // tall phone reference
//         scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
//         scaler.matchWidthOrHeight = 0f; // balance width/height
//         canvas.pixelPerfect = true;
//
//
//         // Root rect
//         var r = new GameObject("Root", typeof(RectTransform)).GetComponent<RectTransform>();
//         r.SetParent(go.transform, false);
//         r.anchorMin = Vector2.zero;
//         r.anchorMax = Vector2.one;
//         r.offsetMin = r.offsetMax = Vector2.zero;
//         r.pivot = new Vector2(0.5f, 0.5f); // 👈 add this
//         root = r;
//
//
//         // Background (behind everything + does NOT block clicks)
//         var bgGo = new GameObject("BG", typeof(RectTransform), typeof(Image));
//         bgGo.transform.SetParent(root, false);
//         var bgrt = bgGo.GetComponent<RectTransform>();
//         bgrt.anchorMin = Vector2.zero;
//         bgrt.anchorMax = Vector2.one;
//         bgrt.offsetMin = bgrt.offsetMax = Vector2.zero;
//         fullBg = bgGo.GetComponent<Image>();
//         fullBg.color = Color.black;
//         fullBg.raycastTarget = false; // <— important so clicks pass through
//         bgGo.transform.SetAsFirstSibling(); // draw BG behind everything
//
//         // Ensure an EventSystem exists with the correct input module
//         if (EventSystem.current == null)
//         {
//             var es = new GameObject("EventSystem", typeof(EventSystem));
// #if ENABLE_INPUT_SYSTEM
//             es.AddComponent<InputSystemUIInputModule>(); // New Input System
// #else
//             es.AddComponent<StandaloneInputModule>();      // Old Input System
// #endif
//         }
//     }
//
//
//     private void ClearUI()
//     {
//         foreach (Transform child in root)
//         {
//             if (child.gameObject.name == "BG") continue;
//             Destroy(child.gameObject);
//         }
//
//         helpHearts.Clear();
//         hudTop = hudHelp = addTimeBtn = addBoxBtn = topExitBtn = playAgainBtn = exitGameBtn = null;
//         bestScoreLabel = bestMovesLabel = scoreLabel = movesLabel = null;
//     }
//
//     private void SetSolidBackground(Color c)
//     {
//         fullBg.sprite = null;
//         fullBg.color = c;
//     }
//
//     private void SetBackground(string resourceName, bool cover, float fit = 1f, float yOffset = 0f)
//     {
//         var spr = Resources.Load<Sprite>(resourceName);
//         fullBg.color = Color.white;
//         fullBg.sprite = spr;
//         fullBg.type = Image.Type.Simple;
//         fullBg.preserveAspect = true;
//
//         // scale/offset handled by Image on full screen; to emulate "cover"/"fit" we could add a child Image.
//         // Simpler: full-screen with preserveAspect = true (works fine).
//     }
//
//     private TextMeshProUGUI AddText(string s, int size, Color c, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchored)
//     {
//         var go = new GameObject($"Text_{s}", typeof(RectTransform), typeof(TextMeshProUGUI));
//         go.transform.SetParent(root, false);
//         var rt = go.GetComponent<RectTransform>();
//         rt.anchorMin = anchorMin;
//         rt.anchorMax = anchorMax;
//         rt.anchoredPosition = anchored;
//         var t = go.GetComponent<TextMeshProUGUI>();
//         t.text = s;
//         // t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
//         t.fontSize = size;
//         t.color = c;
//         t.alignment = TextAlignmentOptions.Center;
//         return t;
//     }
//
//     private Button AddRoundedButton(string label, Vector2 anchored, Color fill)
//     {
//         var go = new GameObject($"Btn_{label}", typeof(RectTransform), typeof(Image), typeof(Button));
//         go.transform.SetParent(root, false);
//         var rt = go.GetComponent<RectTransform>();
//         rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
//         rt.anchoredPosition = anchored;
//         rt.sizeDelta = new Vector2(150, 60);
//         var img = go.GetComponent<Image>();
//         img.color = fill;
//         var btn = go.GetComponent<Button>();
//
//         var text = AddText(label, 18, Black, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), anchored);
//         text.transform.SetParent(go.transform, false);
//         text.rectTransform.anchorMin = text.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
//         text.rectTransform.anchoredPosition = Vector2.zero;
//
//         return btn;
//     }
//
//     private void AddButtonOverlay(TextMeshProUGUI t, Action onClick)
//     {
//         var btnGo = new GameObject($"BtnOverlay_{t.text}", typeof(RectTransform), typeof(Button), typeof(Image));
//         btnGo.transform.SetParent(t.transform, false);
//         var rt = btnGo.GetComponent<RectTransform>();
//         rt.anchorMin = new Vector2(0, 0);
//         rt.anchorMax = new Vector2(1, 1);
//         rt.offsetMin = rt.offsetMax = Vector2.zero;
//         var img = btnGo.GetComponent<Image>();
//         img.color = new Color(0, 0, 0, 0); // transparent hit area
//         var btn = btnGo.GetComponent<Button>();
//         btn.onClick.AddListener(() => onClick());
//     }
//
//     private void AddFullScreenButton(Action onClick)
//     {
//         var go = new GameObject("FullScreenBtn", typeof(RectTransform), typeof(Button), typeof(Image));
//         go.transform.SetParent(root, false);
//         var rt = go.GetComponent<RectTransform>();
//         rt.anchorMin = Vector2.zero;
//         rt.anchorMax = Vector2.one;
//         rt.offsetMin = rt.offsetMax = Vector2.zero;
//         var img = go.GetComponent<Image>();
//         img.color = new Color(0, 0, 0, 0);
//         var b = go.GetComponent<Button>();
//         b.onClick.AddListener(() => onClick());
//     }
//
//     private void FlashCenterLabel(string s, Color c)
//     {
//         var t = AddText(s, 40, c, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
//         StartCoroutine(FadeOutRemove(t, 1f, 3f));
//     }
//
//     private void FadeAndRise(TextMeshProUGUI t, float dur)
//     {
//         StartCoroutine(FadeAndRiseCo(t, dur));
//     }
//
//     private System.Collections.IEnumerator FadeAndRiseCo(TextMeshProUGUI t, float dur)
//     {
//         var t0 = 0f;
//         var start = t.rectTransform.anchoredPosition;
//         var end = start + new Vector2(0, 150f);
//         var startCol = t.color;
//         while (t0 < dur && t)
//         {
//             t0 += Time.deltaTime;
//             var u = t0 / dur;
//             t.rectTransform.anchoredPosition = Vector2.Lerp(start, end, u);
//             t.color = new Color(startCol.r, startCol.g, startCol.b, 1f - u);
//             yield return null;
//         }
//
//         if (t) Destroy(t.gameObject);
//     }
//
//     private void FadeOutAndRemove(TextMeshProUGUI t, float delay, float fade)
//     {
//         StartCoroutine(FadeOutRemove(t, fade, delay));
//     }
//
//     private System.Collections.IEnumerator FadeOutRemove(TextMeshProUGUI t, float fade, float delay)
//     {
//         yield return new WaitForSeconds(delay);
//         var a0 = 1f;
//         var t0 = 0f;
//         var col = t.color;
//         while (t0 < fade && t)
//         {
//             t0 += Time.deltaTime;
//             var u = t0 / fade;
//             t.color = new Color(col.r, col.g, col.b, Mathf.Lerp(a0, 0f, u));
//             yield return null;
//         }
//
//         if (t) Destroy(t.gameObject);
//     }
//
//     private void MoveLeftAndRemove(TextMeshProUGUI t, float dur, float targetX)
//     {
//         StartCoroutine(MoveLeftAndRemoveCo(t, dur, targetX));
//     }
//
//     private System.Collections.IEnumerator MoveLeftAndRemoveCo(TextMeshProUGUI t, float dur, float targetX)
//     {
//         var t0 = 0f;
//         var start = t.rectTransform.anchoredPosition;
//         var end = new Vector2(targetX, start.y);
//         while (t0 < dur && t)
//         {
//             t0 += Time.deltaTime;
//             var u = t0 / dur;
//             t.rectTransform.anchoredPosition = Vector2.Lerp(start, end, u);
//             yield return null;
//         }
//
//         if (t) Destroy(t.gameObject);
//     }
//
//     private void RemoveOneHelpHeart()
//     {
//         if (helpHearts.Count == 0) return;
//         var h = helpHearts[^1];
//         helpHearts.RemoveAt(helpHearts.Count - 1);
//         Destroy(h.gameObject);
//     }
//
//     private static T RandomChoice<T>(IList<T> list) => list[Random.Range(0, list.Count)];
//
//     private static void Shuffle<T>(IList<T> list)
//     {
//         for (var i = list.Count - 1; i > 0; i--)
//         {
//             var j = Random.Range(0, i + 1);
//             (list[i], list[j]) = (list[j], list[i]);
//         }
//     }
//
//     private static Color Hex(string hex)
//     {
//         return ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.white;
//     }
// }