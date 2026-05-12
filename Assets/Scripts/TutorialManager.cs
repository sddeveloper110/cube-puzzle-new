using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class TutorialManager : MonoBehaviour
{
    public static int TutoiralNumber
    {
        get => PlayerPrefs.GetInt("TutorialNumber", 0);
        set => PlayerPrefs.SetInt("TutorialNumber", value);
    }

    public CubeSmasher cubeSmasher;
    public TMPNumberAnimator numberAnimator;
    [Header("Game Settings")]
    [SerializeField]
    private UIDragBox boxPrefab;
    public GameObject tutorialCompletedPanel;

    [SerializeField] private RectTransform boxParent;

    public RectTransform handTutorial1;
    public RectTransform handTutorial2;
    public RectTransform handTutorial3;
    public RectTransform handTutorial4;
    public RectTransform handTutorial5;
    public RectTransform handTutorial7;

    public RectTransform firstTutTextPos,remainingTutTextPos;
    public Text tutoraialText;
    public GameObject tutoraialTextPanel;
    public TextMeshProUGUI welcomeText,numOfTutorialText;
    public TextMeshProUGUI coutingTextTutorial1, coutingTextTutorial2, 
        coutingTextTutorial3, coutingTextTutorial4;
    private int centerValue;
    private const int k_boxSize = 180;
    private const int k_grid_spacing = 2;
    //Color highlightOrange = new Color(1f, 0.65f, 0.2f);
    Color highlightOrange = new Color(1f, 0.55f, 0f);

    float baseDelay = 0f;
    float delayStep = 0.5f;

    [HideInInspector]
    public static int totalTutorials = 7;

    public bool TutoiralGoingON = false;
    public TutorialOrigin HowTutorialStarted;
    public void EndTurotial()
    {
        StopAllCoroutines();
        UiManager.Instance.SetUIForTutorial(false);
        TutoiralGoingON = false;
        once = false;
        manualMode = false;
        cubeSmasher.addBoxButton.gameObject.SetActive(true);
        cubeSmasher.helpButton.gameObject.SetActive(true);
        tutoraialTextPanel.SetActive(false);
        handTutorial1.gameObject.SetActive(false);
        handTutorial2.gameObject.SetActive(false);
        handTutorial3.gameObject.SetActive(false);
        handTutorial4.gameObject.SetActive(false);
        handTutorial5.gameObject.SetActive(false);
        handTutorial7.gameObject.SetActive(false);
        coutingTextTutorial1.gameObject.SetActive(false);
        coutingTextTutorial2.gameObject.SetActive(false);
        coutingTextTutorial3.gameObject.SetActive(false);
        coutingTextTutorial4.gameObject.SetActive(false);
        numOfTutorialText.gameObject.SetActive(false);
        tutorialCompletedPanel.SetActive(false);
        cubeSmasher.lineSpawner.SetActive(false);
        

        cubeSmasher.StartGame();
        // Debug.LogError("tutorial ended");
        //Debug.LogError("end tutu "+TutoiralNumber);
        // Note: You'll need to track the origin in a variable to send it again here
    }
    bool once = false;

    // new fields to support manual (non-persistent) rewatch vs auto-first-time
    private bool manualMode = false;
    private int manualIndex = 0;


    public void ShowTutorial()
    {
        AudioManager.PlayBG(true, 0.7f);
        TutoiralGoingON = true;
        UiManager.Instance.SetUIForTutorial(true);
        cubeSmasher.helpButton.gameObject.SetActive(false);
        cubeSmasher.addBoxButton.gameObject.SetActive(false);
        cubeSmasher.addTimeButton.gameObject.SetActive(false);
        tutoraialTextPanel.SetActive(true);
        tutoraialText.text = "";
        numOfTutorialText.gameObject.SetActive(false);
        coutingTextTutorial1.gameObject.SetActive(false);
        coutingTextTutorial2.gameObject.SetActive(false);
        coutingTextTutorial3.gameObject.SetActive(false);
        coutingTextTutorial4.gameObject.SetActive(false);

        if (TutoiralNumber > 0 && TutoiralNumber < totalTutorials)
        {
            if (once == false)
            {
                welcomeText.gameObject.SetActive(true);
                welcomeText.text = "lets finish the tutorial first";
                once = true;
            }
        }
        // If user requested manual replay, use the in-memory manualIndex
        if (manualMode)
        {
            ShowTutorialByIndex(manualIndex);
            return;
        }
        ShowTutorialByIndex(TutoiralNumber);
    }


    // Show tutorial from a specific index without touching PlayerPrefs
    private void ShowTutorialByIndex(int index)
    {
        int num = index + 1;
        UpdateTutorialText(num);
        numOfTutorialText.gameObject.SetActive(true);
        //index = 0;
        switch (index)
        {
            case 0: StartTutorialNumber1(); break;
            case 1: StartTutorialNumber2(); break;
            case 2: StartTutorialNumber3(); break;
            case 3: StartTutorialNumber4(); break;
            case 4: StartTutorialNumber5(); break;
            case 5: StartTutorialNumber6(); break;
            case 6: StartTutorialNumber7(); break;
            //default: EndTurotial(); break;
        }
    }
    void UpdateTutorialText(int num)
    {
        num=Mathf.Clamp(num, 1, totalTutorials);
        numOfTutorialText.text = "Tutorial " + num + "/" + (totalTutorials );

    }
    public void ShowTutorialButtonPressed()
    {
        if (TutoiralNumber >= totalTutorials)
        {
            manualMode = true;
            manualIndex = 0;
        }
        //Debug.LogError("tutorial num "+TutoiralNumber);
        // Manual replay: do not change PlayerPrefs; use in-memory index
        if (FirebaseCall.Instance != null)
        {
            //FirebaseCall.Instance.LogTutorialEvent("how_to_play_Tutorial", "started");
            FirebaseCall.Instance.LogTutorialStarted(TutorialOrigin.HowToPlay);

        }
        HowTutorialStarted = TutorialOrigin.HowToPlay;
        ShowTutorial();
    }

    public void NextTutorial()
    {
        //Debug.LogError("NextTutorial"); 
        StartCoroutine(NextTutorialCoroutine());
    }
    IEnumerator  NextTutorialCoroutine()
    {
        if(TutoiralGoingON==false) yield break;
        handTutorial1.gameObject.SetActive(false);
        handTutorial2.gameObject.SetActive(false);

        if (manualMode)
        {
            manualIndex++;
        }
        else
        {
            TutoiralNumber++;
        }
        yield return new WaitForSeconds(0.8f);
        ShowTutorial();
    }
    public void Swapped(int num)
    {
        //Debug.LogError("numm " + num);
        if (!TutoiralGoingON) return;
        if (manualMode)
            num = manualIndex;
        if (num == 0)
        {
            
            
            manualIndex = 1;
            SettingForTutorial2();
        }
        else if (num == 1)
        {
            SetBoxMovable((2, 2), false);
            handTutorial2.gameObject.SetActive(false);
            tutoraialText.text = string.Empty;
            baseDelay = 0f;
            for (int x = -2; x <= 2; x++)
            {
                Box box = cubeSmasher.grid[(x, 2)];
                box.rt.DOScale(1f, 1.5f)
                      .SetDelay(baseDelay); ;
                box.bg.DOColor(highlightOrange, 1.5f)
                      .SetDelay(baseDelay); 
                baseDelay += delayStep; 
            }
            numberAnimator.Animate(coutingTextTutorial2, 3f);
            cubeSmasher.CheckMatchesForTutorial(3.5f);
        }
        else if (num == 2)
        {
            handTutorial3.gameObject.SetActive(false);
            tutoraialText.text = "";
        }
        else if (num == 3)
        {
            SetBoxMovable((-1, -1), false);
            handTutorial4.gameObject.SetActive(false);
            baseDelay = 0f;
            for (int y = 0; y >= -2; y--)
            {
                Box box = cubeSmasher.grid[(0, y)];
                box.rt.DOScale(1f, 2f) .SetDelay(baseDelay); 
                box.bg.DOColor(highlightOrange, 2f).SetDelay(baseDelay); 
                baseDelay += delayStep;
            }
            numberAnimator.Animate(coutingTextTutorial4, 3f);
            cubeSmasher.CheckMatchesForTutorial(3.5f);

        }

        else if (num == 5) // Doulbe Row ClearEffect    
        {
            //coutingTextTutorial4.gameObject.SetActive(false);
            handTutorial5.gameObject.SetActive(false);
            SetBoxMovable((-1, -1), false);

            StartCoroutine(SpecialDoulbeColumnClearEffect());
        }
        else if (num == 6)
        {
            handTutorial7.gameObject.SetActive(false);
            
            StartCoroutine(EndOfTutorial());
            SetBoxMovable((-1, -1), false);
        }
    }
    IEnumerator SpecialDoulbeColumnClearEffect()
    {
        coutingTextTutorial4.gameObject.SetActive(true);
        coutingTextTutorial4.text= " 4 + 8 + 3 = 15";

        baseDelay = 0;
        for (int y = 0; y >= -2; y--)
        {
            Box box = cubeSmasher.grid[(-1, y)];

            box.rt.DOScale(1f, 2f).SetDelay(baseDelay);
            box.bg.DOColor(highlightOrange, 2f).SetDelay(baseDelay);
            baseDelay += delayStep;
        }
        numberAnimator.Animate(coutingTextTutorial4, 3f);
        yield return new WaitForSeconds(3.5f);
        coutingTextTutorial4.text = " 15 + 0 + 0 = 15";

        baseDelay = 0;
        for (int y = 0; y >= -2; y--)
        {
            Box box = cubeSmasher.grid[(0, y)];

            box.rt.DOScale(1f, 2f).SetDelay(baseDelay);
            box.bg.DOColor(highlightOrange, 2f).SetDelay(baseDelay);
            baseDelay += delayStep;
        }
        numberAnimator.Animate(coutingTextTutorial4, 3f);

        yield return new WaitForSeconds(3f);
        cubeSmasher.CheckMatchesForTutorial(0);
    }

    int attemps = 0;

    public void SetBoxMovable((int x, int y) coord, bool movable)
    {
        if (cubeSmasher == null) return;
        if (!cubeSmasher.grid.TryGetValue(coord, out var box) || box == null || box.rt == null) return;

        // Try to find the UIDragBox component on the visual GameObject
        var drag = box.rt.GetComponent<UIDragBox>();
        drag.enabled = movable;
       
    }
    public void SetBoxGray((int x, int y) coord, bool shouldActive)
    {
        if (cubeSmasher == null) return;
        if (!cubeSmasher.grid.TryGetValue(coord, out var box) || box == null || box.rt == null) return;

        // Try to find the UIDragBox component on the visual GameObject
        var drag = box.rt.GetComponent<UIDragBox>();
        drag.grayImage.SetActive(shouldActive) ;

    }
    int?[] valuesPos5 = { -4, -7, -2, 4, null, -3, 6, -6, 3, -7, 1, -2, -3, -6, -1, null };

    public void StartTutorialNumber1()
    {
        cubeSmasher.UpdateScreen(CubeSmasher.GameState.Game);

        StartCoroutine(GenerateGridCoroutine1());
    }
    private IEnumerator GenerateGridCoroutine1()
    {
        int?[] valuesPos3 = { -5, -4, -9, -2, -5, -8, -9, -8 };
        centerValue = cubeSmasher.centerValue = 15;
        attemps = 0;
        once = true;
        while (true)
        {
            attemps++;
            if (attemps > 50)
            {
                Debug.LogError("Failed to generate valid grid after 50 attempts.");
                yield break;
            }
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

            var empty3 = pos3[7];
            var empty5 = pos5[5];
            //if (empty5 == empty3) continue;

            for (int i = 0; i < pos3.Count; i++)
            {
                if (valuesPos3[i] < 0)
                {
                    var val = valuesPos3[i] * -1;
                    temp[pos3[i]] = MakeBox(val, pos3[i], false);
                    var drag = temp[pos3[i]].rt.GetComponent<UIDragBox>();
                    drag.grayImage.SetActive(true);

                    continue;
                }
                temp[pos3[i]] = MakeBox(valuesPos3[i], pos3[i], false);


            }
            for (int i = 0; i < pos5.Count; i++)
            {
                if (valuesPos5[i] < 0)
                {
                    var val = valuesPos5[i] * -1;
                    temp[pos5[i]] = MakeBox(val, pos5[i], false);
                    var drag = temp[pos5[i]].rt.GetComponent<UIDragBox>();
                    drag.grayImage.SetActive(true);
                    continue;
                }

                temp[pos5[i]] = MakeBox(valuesPos5[i], pos5[i], false);

            }
            if (!ValidGrid(temp)) continue;
            cubeSmasher.grid = temp;
            cubeSmasher.centerValue = 15;

            break;
        }
        SetBoxMovable((-2, 1), true);
        BoxSwapable((-2, 2), true);
        handTutorial1.anchoredPosition = GridToScreen((-2, 1));

        foreach (var kv in cubeSmasher.grid)
        {
            PlaceBox(kv.Value);

            kv.Value.rt.localScale = Vector3.zero;
        }

        var order = cubeSmasher.grid.OrderByDescending(k => k.Key.y).ThenBy(k => k.Key.x).ToList();
        var centerBox = order.FirstOrDefault(kv => kv.Key == (0, 0));

        if (!centerBox.Equals(default(KeyValuePair<(int x, int y), Box>)))
        {
            order.Remove(centerBox);
            order.Add(centerBox);
        }

        foreach (var kv in order)
        {
            kv.Value.rt.DOScale(Vector3.one * 0.9f, 0.1f).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(0.01f);
        }
        handTutorial1.gameObject.SetActive(true);
        PlayTyping(tutoraialText, "Slide the numbered cubes into the white spaces to move the numbers around the grid\n" +
        "<color=red>Note:</color> The red center number never moves and your goal is to get all the rows and columns to add up to the red center number\n\n" +
        "Try it by moving the 4 cube into the white space...");
    }
    void SettingForTutorial2()
    {
        SetBoxMovable((2, 1), true);
        SetBoxMovable((-2, 1), false);
        SetBoxMovable((-2, 2), false);
        BoxSwapable((2, 2), true);
        handTutorial1.gameObject.SetActive(false);
        handTutorial2.gameObject.SetActive(true);
        SetBoxGray((2, 1), false);
        SetBoxGray((-2, 1), true);
        coutingTextTutorial2.gameObject.SetActive(true);
        UpdateTutorialText(2);
        if(!manualMode)
            TutoiralNumber = 1;
        PlayTyping(tutoraialText, "Work to clear rows and columns by moving the numbers around the grid " +
            "until rows and columns sum to the red center number \n\n" +
          "Try it by moving the #1 into the white space to get the top row to sum to 15");
    }
    public void StartTutorialNumber2()
    {

        cubeSmasher.UpdateScreen(CubeSmasher.GameState.Game);

        StartCoroutine(GenerateGridCoroutine2());
    }
    private IEnumerator GenerateGridCoroutine2()
    {

        int?[] valuesPos3 = { -5, -4, -9, -2, -5, -8, -9, -8 };
        int?[] valuesPos5 = { -4, -7, -2, null,4, -3, 6, -6, 3, -7, 1, -2, -3, -6, -1, null };

        centerValue = cubeSmasher.centerValue = 15;
        //welcomeText.gameObject.SetActive(true);
        //welcomeText.text = "Let's learn to play Cube Smasher";
        attemps = 0;
        once = true;
        while (true)
        {
            attemps++;
            if (attemps > 50)
            {
                Debug.LogError("Failed to generate valid grid after 50 attempts.");
                yield break;
            }
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

            //var empty3 = pos3[7];
            //var empty5 = pos5[5];
            //if (empty5 == empty3) continue;

            for (int i = 0; i < pos3.Count; i++)
            {
                if (valuesPos3[i] < 0)
                {
                    var val = valuesPos3[i] * -1;
                    temp[pos3[i]] = MakeBox(val, pos3[i], false);
                    var drag = temp[pos3[i]].rt.GetComponent<UIDragBox>();
                    drag.grayImage.SetActive(true);

                    continue;
                }
                temp[pos3[i]] = MakeBox(valuesPos3[i], pos3[i], false);


            }
            for (int i = 0; i < pos5.Count; i++)
            {
                if (valuesPos5[i] < 0)
                {
                    var val = valuesPos5[i] * -1;
                    temp[pos5[i]] = MakeBox(val, pos5[i], false);
                    var drag = temp[pos5[i]].rt.GetComponent<UIDragBox>();
                    drag.grayImage.SetActive(true);
                    continue;
                }

                temp[pos5[i]] = MakeBox(valuesPos5[i], pos5[i], false);

            }
            if (!ValidGrid(temp)) continue;
            cubeSmasher.grid = temp;
            cubeSmasher.centerValue = 15;

            break;
        }
        SetBoxMovable((2, 1), true);
        handTutorial2.anchoredPosition = GridToScreen((2, 1));

        foreach (var kv in cubeSmasher.grid)
        {
            PlaceBox(kv.Value);

            kv.Value.rt.localScale = Vector3.zero;
        }

        var order = cubeSmasher.grid.OrderByDescending(k => k.Key.y).ThenBy(k => k.Key.x).ToList();
        var centerBox = order.FirstOrDefault(kv => kv.Key == (0, 0));

        if (!centerBox.Equals(default(KeyValuePair<(int x, int y), Box>)))
        {
            order.Remove(centerBox);
            order.Add(centerBox);
        }

        foreach (var kv in order)
        {
            kv.Value.rt.DOScale(Vector3.one * 0.9f, 0.1f).SetEase(Ease.OutBack);
            //AudioManager.PlayAudio(cubeSmasher.popInEffect);
            yield return new WaitForSeconds(0.01f);
        }
        SettingForTutorial2();

    }
    public void StartTutorialNumber3()
    {
        cubeSmasher.UpdateScreen(CubeSmasher.GameState.Game);

        StartCoroutine(GenerateGridCoroutine3());

    }
    private IEnumerator GenerateGridCoroutine3() //help button tutorial
    {
        int?[] valuesPos3 = { 5, 4, 9, 2, 5, 8, 9, 8 };
        int?[] valuesPos5 = { 4, 7, 2, null, 3, 6, 7, 2, 3, 6, null };

        centerValue = cubeSmasher.centerValue = 15;
        //welcomeText.gameObject.SetActive(true);
        //welcomeText.text = "Let's learn to play Cube Smasher";
        attemps = 0;
        once = true;
        while (true)
        {
            attemps++;
            if (attemps > 50)
            {
                Debug.LogError("Failed to generate valid grid after 50 attempts.");
                yield break;
            }
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
                for (var y = -2; y <= 1; y++)
                    if (!temp.ContainsKey((x, y)) && !(Mathf.Abs(x) <= 1 && Mathf.Abs(y) <= 1))
                        pos5.Add((x, y));


            var empty3 = pos3[0];
            var empty5 = pos5[0];
            //if (empty5 == empty3) continue;


            for (int i = 0; i < pos3.Count; i++)
            {
                temp[pos3[i]] = MakeBox(valuesPos3[i], pos3[i], false);
            }
            for (int i = 0; i < pos5.Count; i++)
            {
                temp[pos5[i]] = MakeBox(valuesPos5[i], pos5[i], false);
            }
            if (!ValidGrid(temp)) continue;
            cubeSmasher.grid = temp;
            cubeSmasher.centerValue = 15;

            break;
        }
        
        
        handTutorial3.gameObject.SetActive(true);
        cubeSmasher.helpButton.gameObject.SetActive(true);
        UiManager.Instance.SetRemainingHelpCounterText(4);
        handTutorial3.position = cubeSmasher.addTimeButton.transform.position;
        foreach (var kv in cubeSmasher.grid)
        {
            PlaceBox(kv.Value);
            kv.Value.rt.localScale = Vector3.zero;
        }

        var order = cubeSmasher.grid.OrderByDescending(k => k.Key.y).ThenBy(k => k.Key.x).ToList();
        var centerBox = order.FirstOrDefault(kv => kv.Key == (0, 0));

        if (!centerBox.Equals(default(KeyValuePair<(int x, int y), Box>)))
        {
            order.Remove(centerBox);
            order.Add(centerBox);
        }

        foreach (var kv in order)
        {
            kv.Value.rt.DOScale(Vector3.one * 0.9f, 0.1f).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(0.01f);
        }
        PlayTyping(tutoraialText, "You can get help by pressing the help button which will either give you an extra white space " +
            "on the grid or it will clear an entire row\n\n" +
          "Try it by Pressing the help button");
        helpPressedCount = 0;
    }
    public void StartTutorialNumber4()
    {
        cubeSmasher.UpdateScreen(CubeSmasher.GameState.Game);

        StartCoroutine(GenerateGridCoroutine4());

    }
    private IEnumerator GenerateGridCoroutine4()  //White Space Equal to Zero tutorial
    {
        int?[] valuesPos3 = { null,4 , 2, 8, 9 };
        int?[] valuesPos5 = { 4, 7, 2, 3, null, 7, 2, 3, 6 };
        centerValue = cubeSmasher.centerValue = 15;
        attemps = 0;
        while (true)
        {
            attemps++;
            if (attemps > 50)
            {
                Debug.LogError("Failed to generate valid grid after 50 attempts.");
                yield break;
            }
            ClearGrid();
            var temp = new Dictionary<(int x, int y), Box>
            {
                [(0, 0)] = MakeBox(centerValue, (0, 0), fixedRed: true)
            };

            var pos3 = new List<(int, int)>();
            for (var x = -1; x <= 1; x++)
                for (var y = -1; y <= 0; y++)
                    if (!(x == 0 && y == 0))
                        pos3.Add((x, y));

            var pos5 = new List<(int, int)>();
            for (var x = -2; x <= 2; x++)
                for (var y = -2; y <= 0; y++)
                    if (!temp.ContainsKey((x, y)) && !(Mathf.Abs(x) <= 1 && Mathf.Abs(y) <= 1))
                        pos5.Add((x, y));

            var empty3 = pos3[0];
            var empty5 = pos5[0];
            if (empty5 == empty3) continue;

            for (int i = 0; i < pos3.Count; i++)
            {
                temp[pos3[i]] = MakeBox(valuesPos3[i], pos3[i], false);
            }
            for (int i = 0; i < pos5.Count; i++)
                temp[pos5[i]] = MakeBox(valuesPos5[i], pos5[i], false);

            // if (!ValidGrid(temp)) continue;
            cubeSmasher.grid = temp;
            cubeSmasher.centerValue = 15;

            break;
        }
        SetBoxMovable((0, -1), true);
        BoxSwapable((-1, -1), true);
        handTutorial4.anchoredPosition = GridToScreen((0, -1));
        handTutorial4.gameObject.SetActive(true);

        foreach (var kv in cubeSmasher.grid)
        {
            PlaceBox(kv.Value);
            kv.Value.rt.localScale = Vector3.zero;
        }

        var order = cubeSmasher.grid.OrderByDescending(k => k.Key.y).ThenBy(k => k.Key.x).ToList();
        var centerBox = order.FirstOrDefault(kv => kv.Key == (0, 0));

        if (!centerBox.Equals(default(KeyValuePair<(int x, int y), Box>)))
        {
            order.Remove(centerBox);
            order.Add(centerBox);
        }

        foreach (var kv in order)
        {
            kv.Value.rt.DOScale(Vector3.one * 0.9f, 0.1f).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(0.01f);
        }
        coutingTextTutorial4.gameObject.SetActive(true);

        PlayTyping(tutoraialText, "White spaces equal 0 so you can move them into the row or column with the red center number to clear that row or column" +
            "\n\n Try it by swapping the #2 with the white space to make the center column clear");
       

    }
    public void StartTutorialNumber5()
    {
        cubeSmasher.UpdateScreen(CubeSmasher.GameState.Game);

        StartCoroutine(GenerateGridCoroutine5());

    }
    private IEnumerator GenerateGridCoroutine5() //box adding tutorial
    {
        int?[] valuesPos3 = { null, 4, 2, 8, 9 };
        int?[] valuesPos5 = { 4, 7, 2, 3, null, 7 };

        centerValue = cubeSmasher.centerValue = 15;
        attemps = 0;
        while (true)
        {
            attemps++;
            if (attemps > 50)
            {
                Debug.LogError("Failed to generate valid grid after 50 attempts.");
                yield break;
            }
            ClearGrid();
            var temp = new Dictionary<(int x, int y), Box>
            {
                [(0, 0)] = MakeBox(centerValue, (0, 0), fixedRed: true)
            };

            var pos3 = new List<(int, int)>();
            for (var x = -1; x <= 1; x++)
                for (var y = -1; y <= 0; y++)
                    if (!(x == 0 && y == 0))
                        pos3.Add((x, y));

            var pos5 = new List<(int, int)>();
            for (var x = -2; x <= 1; x++)
                for (var y = -2; y <= 0; y++)
                    if (!temp.ContainsKey((x, y)) && !(Mathf.Abs(x) <= 1 && Mathf.Abs(y) <= 1))
                        pos5.Add((x, y));

            var empty3 = pos3[0];
            var empty5 = pos5[0];
            if (empty5 == empty3) continue;

            for (int i = 0; i < pos3.Count; i++)
            {
                //if (valuesPos3[i] <= 0)
                //{
                //    valuesPos3[i] = valuesPos3[i] * -1;
                //    temp[pos3[i]] = MakeBox(valuesPos3[i], pos3[i], false, true);
                //    continue;
                //}
                temp[pos3[i]] = MakeBox(valuesPos3[i], pos3[i], false);
            }
            for (int i = 0; i < pos5.Count; i++)
                temp[pos5[i]] = MakeBox(valuesPos5[i], pos5[i], false);

            // if (!ValidGrid(temp)) continue;
            cubeSmasher.grid = temp;
            cubeSmasher.centerValue = 15;

            break;
        }

        foreach (var kv in cubeSmasher.grid)
        {
            PlaceBox(kv.Value);
            kv.Value.rt.localScale = Vector3.zero;
        }

        var order = cubeSmasher.grid.OrderByDescending(k => k.Key.y).ThenBy(k => k.Key.x).ToList();
        var centerBox = order.FirstOrDefault(kv => kv.Key == (0, 0));

        if (!centerBox.Equals(default(KeyValuePair<(int x, int y), Box>)))
        {
            order.Remove(centerBox);
            order.Add(centerBox);
        }

        foreach (var kv in order)
        {
            kv.Value.rt.DOScale(Vector3.one * 0.9f, 0.1f).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(0.01f);
        }
        cubeSmasher.addBoxButton.gameObject.SetActive(true);
        UiManager.Instance.SetAddBoxCounterText(10);
        handTutorial3.gameObject.SetActive(true);
        handTutorial3.position = cubeSmasher.addTimeButton.transform.position;

        PlayTyping(tutoraialText, "Sometimes none of the numbers on the grid will sum to the red center number. " +
            "If that happens you can tap the \"<color=white>Add Box</color>\" button to get more numbered cubes to work with \n\n" +
            "Try it by clicking the Add Box button");
        addBoxButtonPressedOnce= false;
    }
    public void StartTutorialNumber6()
    {
        cubeSmasher.UpdateScreen(CubeSmasher.GameState.Game);
        StartCoroutine(GenerateGridCoroutine6());
    }
    private IEnumerator GenerateGridCoroutine6() //double column clear tutorial
    {
        int?[] valuesPos3 = { null, 4, 8, 8, 9 };
        int?[] valuesPos5 = { 3, null, 7 };
        // set both TutorialManager.centerValue and cubeSmasher.centerValue
        centerValue = cubeSmasher.centerValue = 15;
        attemps = 0;
        while (true)
        {
            attemps++;
            if (attemps > 50)
            {
                Debug.LogError("Failed to generate valid grid after 50 attempts.");
                yield break;
            }
            ClearGrid();
            var temp = new Dictionary<(int x, int y), Box>
            {
                [(0, 0)] = MakeBox(centerValue, (0, 0), fixedRed: true)
            };

            var pos3 = new List<(int, int)>();
            for (var x = -1; x <= 1; x++)
                for (var y = -1; y <= 0; y++)
                    if (!(x == 0 && y == 0))
                        pos3.Add((x, y));

            var pos5 = new List<(int, int)>();
            for (var x = -1; x <= 1; x++)
                for (var y = -2; y <= 0; y++)
                    if (!temp.ContainsKey((x, y)) && !(Mathf.Abs(x) <= 1 && Mathf.Abs(y) <= 1))
                        pos5.Add((x, y));

            var empty3 = pos3[0];
            var empty5 = pos5[0];
            if (empty5 == empty3) continue;

            for (int i = 0; i < pos3.Count; i++)
            {
                temp[pos3[i]] = MakeBox(valuesPos3[i], pos3[i], false);
            }
            for (int i = 0; i < pos5.Count; i++)
                temp[pos5[i]] = MakeBox(valuesPos5[i], pos5[i], false);

            // if (!ValidGrid(temp)) continue;
            cubeSmasher.grid = temp;
            cubeSmasher.centerValue = 15;

            break;
        }
        SetBoxMovable((0, -1), true);
        BoxSwapable((-1, -1), true);
        handTutorial5.anchoredPosition = GridToScreen((0, -1));
        handTutorial5.gameObject.SetActive(true);

        foreach (var kv in cubeSmasher.grid)
        {
            PlaceBox(kv.Value);
            kv.Value.rt.localScale = Vector3.zero;
        }

        var order = cubeSmasher.grid.OrderByDescending(k => k.Key.y).ThenBy(k => k.Key.x).ToList();
        var centerBox = order.FirstOrDefault(kv => kv.Key == (0, 0));

        if (!centerBox.Equals(default(KeyValuePair<(int x, int y), Box>)))
        {
            order.Remove(centerBox);
            order.Add(centerBox);
        }

        foreach (var kv in order)
        {
            kv.Value.rt.DOScale(Vector3.one * 0.9f, 0.1f).SetEase(Ease.OutBack);
            //AudioManager.PlayAudio(cubeSmasher.popInEffect);
            yield return new WaitForSeconds(0.01f);
        }
        //coutingTextTutorial4.gameObject.SetActive(true);
        //tutoraialText.rectTransform.position = remainingTutTextPos.position;

        PlayTyping(tutoraialText, "Sometimes a move can result in more than one column/row clearing. That triggers a rare Combo Clear" +
            "\nOnly the most strategic cubers can get these\n\n" +
            "Try it by swapping the #8 cube with the white space so that the left and center column both sum to 15");
    }
    public void StartTutorialNumber7()
    {
        cubeSmasher.UpdateScreen(CubeSmasher.GameState.Game);
        StartCoroutine(GenerateGridCoroutine7());
    }
    private IEnumerator GenerateGridCoroutine7() //Last Gray box tutorial
    {
        //Debug.LogError("7");
        int?[] valuesPos3 = { null, 9};
        int?[] valuesPos5 = { null };
        // set both TutorialManager.centerValue and cubeSmasher.centerValue
        centerValue = cubeSmasher.centerValue = 15;
        attemps = 0;
        while (true)
        {
            attemps++;
            if (attemps > 50)
            {
                Debug.LogError("Failed to generate valid grid after 50 attempts.");
                yield break;
            }
            ClearGrid();
            var temp = new Dictionary<(int x, int y), Box>
            {
                [(0, 0)] = MakeBox(centerValue, (0, 0), fixedRed: true)
            };

            var pos3 = new List<(int, int)>();
            for (var x = -1; x <= -1; x++)
                for (var y = -1; y <= 0; y++)
                    if (!(x == 0 && y == 0))
                        pos3.Add((x, y));

            var pos5 = new List<(int, int)>();
            for (var x = -1; x <= -1; x++)
                for (var y = -2; y <= -2; y++)
                    if (!temp.ContainsKey((x, y)) && !(Mathf.Abs(x) <= 1 && Mathf.Abs(y) <= 1))
                        pos5.Add((x, y));

            var empty3 = pos3[0];
            var empty5 = pos5[0];
            if (empty5 == empty3) continue;

            for (int i = 0; i < pos3.Count; i++)
                temp[pos3[i]] = MakeBox(valuesPos3[i], pos3[i], false);
            for (int i = 0; i < pos5.Count; i++)
                temp[pos5[i]] = MakeBox(valuesPos5[i], pos5[i], false);

            // if (!ValidGrid(temp)) continue;
            cubeSmasher.grid = temp;
            cubeSmasher.centerValue = 15;

            break;
        }

        SetBoxMovable((-1, 0), true);
        BoxSwapable((-1, -1), true);
        handTutorial7.anchoredPosition = GridToScreen((-1, 0));
        handTutorial7.gameObject.SetActive(true);

        foreach (var kv in cubeSmasher.grid)
        {
            PlaceBox(kv.Value);
            kv.Value.rt.localScale = Vector3.zero;
        }

        var order = cubeSmasher.grid.OrderByDescending(k => k.Key.y).ThenBy(k => k.Key.x).ToList();
        var centerBox = order.FirstOrDefault(kv => kv.Key == (0, 0));

        if (!centerBox.Equals(default(KeyValuePair<(int x, int y), Box>)))
        {
            order.Remove(centerBox);
            order.Add(centerBox);
        }

        foreach (var kv in order)
        {
            kv.Value.rt.DOScale(Vector3.one * 0.9f, 0.1f).SetEase(Ease.OutBack);
            //AudioManager.PlayAudio(cubeSmasher.popInEffect);
            yield return new WaitForSeconds(0.01f);
        }
        
        //tutoraialText.rectTransform.position = remainingTutTextPos.position;

        PlayTyping(tutoraialText, "You win when you get the grid down to just one remaining grey numbered cube"); 
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
            p.x * (step + k_grid_spacing),
            p.y * (step + k_grid_spacing)
        );
    }
    private bool ValidGrid(Dictionary<(int, int), Box> test)
    {
        foreach (var (key, box) in test)
        {
            var (x, y) = key;
            if (!box.value.HasValue || box.fixedRed) continue;
            // row
            var sumRow = 0;
            for (var i = -2; i <= 1; i++)
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

    private static int GenNumExcluding(int exclude)
    {
        var v = Random.Range(1, 11);
        while (v == exclude) v = Random.Range(1, 11);
        return v;
    }
    private void ClearGrid()
    {
        foreach (var b in cubeSmasher.grid.Values)
            if (b?.rt)
            {
                b.rt.DOKill();
                Destroy(b.rt.gameObject);
            }

        cubeSmasher.grid.Clear();
        foreach (Transform t in boxParent.transform)
        {
            t.DOKill();
            Destroy(t.gameObject);
        }
    }
    private Box MakeBox(int? value, (int x, int y) pos, bool fixedRed = false)//, bool shouldMove = false)
    {
        var go = Instantiate(boxPrefab, boxParent);
        go.enabled = false;
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
        b.MakeBoxSwappable(false);
       
        // Drag
        go.Init(cubeSmasher, b);

        return b;
    }

    bool addBoxButtonPressedOnce = false;
    public void AddBoxForTuturial()
    {
        if(addBoxButtonPressedOnce) return;
        UiManager.Instance.SetAddBoxCounterText(9);
        addBoxButtonPressedOnce = true;
        var pos = (-2, 1);
        var val = 2;
        var nb = MakeBox(val, pos);
        cubeSmasher.grid[pos] = nb;
        PlaceBox(nb);
        cubeSmasher.CheckMatchesForTutorial(3.5f);
        handTutorial3.gameObject.SetActive(false);
        coutingTextTutorial3.gameObject.SetActive(true);
        baseDelay = 0;
        for (int y = 1; y >= -2; y--)
        {
            Box box = cubeSmasher.grid[(-2, y)];
            box.rt.DOScale(1f, 2f).SetDelay(baseDelay);
            box.bg.DOColor(highlightOrange, 2f).SetDelay(baseDelay);

            baseDelay += delayStep;
        }
        numberAnimator.Animate(coutingTextTutorial3, 3f);
    }

    IEnumerator EndOfTutorial()
    {
        //tutoraialText.text = "";
        handTutorial4.gameObject.SetActive(false);
        handTutorial3.gameObject.SetActive(false);
        //welcomeText.gameObject.SetActive(true);
        //welcomeText.text = "Tutorials Completed\nEnjoy the game";
        PlayTyping(tutoraialText, "Excellent! You completed the tutorial");
        CancelInvoke();
        cubeSmasher.CheckMatchesForTutorial(0);
        yield return new WaitForSeconds(0.5f);
        foreach (var kv in cubeSmasher.grid)
        {
            var box = kv.Value;
            if (box == null || box.rt == null) continue;

            box.rt.GetComponent<BoxExplodeEffect>().enabled = true;
            //Debug.Log("remin box eX");
        }
        yield return new WaitForSeconds(0.5f);
        cubeSmasher.lineSpawner.SetActive(true);

        yield return new WaitForSeconds(1.8f);
        tutorialCompletedPanel.SetActive(true);
        //welcomeText.gameObject.SetActive(true);
        //welcomeText.text = " Time to play the \nCUBE SMASHER";
        //yield return new WaitForSeconds(0.5f);
        if (FirebaseCall.Instance)
        {
            //FirebaseCall.Instance.LogTutorialEvent(HowTutorialStarted, "completed");
            FirebaseCall.Instance.LogTutorialCompleted(HowTutorialStarted);

        }

        //EndTurotial();
    }
    [SerializeField] private float typingSpeed = 0.03f;
    Coroutine typingCoroutine;
    public void PlayTyping(Text textMesh, string message)
    {
        textMesh.text = message;
        return;
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypingRoutine(textMesh, message));
    }

    private IEnumerator TypingRoutine(Text textMesh,string message)
    {
        textMesh.text = "";
        foreach (char c in message)
        {
            textMesh.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        typingCoroutine = null;
    }

    int helpPressedCount=0;
    public void HelpButtonPressed()
    {
        if (helpPressedCount == 0)
        {
            helpPressedCount = 1;
            UiManager.Instance.SetRemainingHelpCounterText(3);
            var candidates = new List<Box>();
            foreach (var b in cubeSmasher.grid.Values)
                if (b.value == 8 && !b.fixedRed)
                {
                    candidates.Add(b);
                }
            if (candidates.Count > 0)
            {
                var t = candidates[1];
                t.value = null;
                t.SetVisual();
                t.rt.transform.SetAsFirstSibling();
                //cubeSmasher.AnimateHelpHeart(t);
            }
            tutoraialText.text = "Great! Now press help again to get an automatic row clear";
        }
        else if (helpPressedCount == 1)
        {
            helpPressedCount = 2;
            cubeSmasher.ClearFirstRow();
            handTutorial3.gameObject.SetActive(false);
            UiManager.Instance.SetRemainingHelpCounterText(2);
            //cubeSmasher.CheckMatchesForTutorial();
            Invoke(nameof(NextTutorial), 1.0f);
        }
        
        //Debug.LogError("tutorial help used" );
    }

    void BoxSwapable((int x, int y)coord, bool swapable)
    {
        if (cubeSmasher == null) return;
        if (!cubeSmasher.grid.TryGetValue(coord, out var box) || box == null || box.rt == null) return;
        // Try to find the UIDragBox component on the visual GameObject
        
        box.MakeBoxSwappable(swapable);
    }

}