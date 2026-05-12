using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonAnim : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private Button button;
    [SerializeField] private AudioClip OnClick;
    [SerializeField] private float popIntensity = 0.9f;
    [SerializeField] private float animSpeed = 0.1f;

    private void Start()
    {
        button = GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        transform.localScale = Vector3.one;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (OnClick != null)
            AudioManager.PlayAudio(OnClick);
        transform.DOKill();
        transform.DOScale(Vector3.one * popIntensity, animSpeed).SetEase(Ease.OutBack);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(Vector3.one, animSpeed).SetEase(Ease.OutBack);
    }
}