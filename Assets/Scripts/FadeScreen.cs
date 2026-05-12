using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FadeScreen : MonoBehaviour
{
    private static FadeScreen instance;
    private Image fadeImage; // put your full screen black image here
    [SerializeField] private float fadeSpeed = 2f;

    private Sequence fadeSequence;

    private void Awake()
    {
        instance = this;
        fadeSequence = DOTween.Sequence();
        fadeImage = GetComponent<Image>();
        if (fadeImage == null) return;
        var c = fadeImage.color;
        c.a = 1f;
        fadeImage.color = c;
    }

    public static bool DoFade(Action onMidFade)
    {
        if (instance == null || instance.fadeImage == null) return false;
        instance.fadeSequence.Kill();
        instance.fadeSequence = DOTween.Sequence();
        instance.fadeSequence.Append(instance.fadeImage.DOFade(1, 0.5f).SetUpdate(true).SetEase(Ease.OutExpo));
        instance.fadeSequence.AppendCallback(() => onMidFade?.Invoke());
        instance.fadeSequence.Append(instance.fadeImage.DOFade(0, 0.5f).SetUpdate(true).SetEase(Ease.InExpo));
        instance.fadeSequence.Play();
        return true;
    }
}