using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class FakeLoading : MonoBehaviour
{
    public bool shouldShowLoadScene = false;
    [SerializeField] private Image fillImage;
    [SerializeField] private Image fakeFillImage;
    [SerializeField] private float duration = 3f;
    [SerializeField] private int steps = 25;   // Higher = smoother, Lower = jerkier

    private Coroutine loadingRoutine;

    private void OnEnable()
    {
        if (!fillImage) return;

        if (loadingRoutine != null)
            StopCoroutine(loadingRoutine);
        if (!shouldShowLoadScene)
            loadingRoutine = StartCoroutine(FakeLoadRoutine());
        else
            loadingRoutine = StartCoroutine(ActualLoading());
    }
    private IEnumerator ActualLoading()
    {
        fakeFillImage.gameObject.SetActive(true);
        fakeFillImage.fillAmount = 0f;

        // Fake loading (0 -> 0.5 in 3 seconds)
        yield return fakeFillImage
            .DOFillAmount(0.5f, 3f)
            .SetEase(Ease.Linear)
            .WaitForCompletion();

        //fakeFillImage.gameObject.SetActive(false);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(1);

        while (asyncLoad.progress < 0.9f)
        {
            fillImage.fillAmount =0.5f+ asyncLoad.progress/2;
            yield return null;
        }
    }


    private IEnumerator FakeLoadRoutine()
    {
        fillImage.fillAmount = 0f;

        float stepTime = duration / steps;

        for (int i = 1; i <= steps; i++)
        {
            yield return new WaitForSeconds(stepTime);

            fillImage.fillAmount = (float)i / steps;
        }
       
        
        
        loadingRoutine = null;
        gameObject.SetActive(false);
    }
}