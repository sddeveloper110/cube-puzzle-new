using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIDragBox : MonoBehaviour, IBeginDragHandler,
    IDragHandler, IEndDragHandler
{
    private CubeSmasher game;
    public Box box;
    private Canvas canvas;
    public TMP_Text label;
    public Image bg;
    public GameObject grayImage;
    public ParticleSystem trail;
    public ParticleSystem OnClickParticle;
    public ParticleSystem OnClickEndParticle;

    public void Init(CubeSmasher g, Box b)
    {
        trail.gameObject.SetActive(false);

        game = g;
        box = b;
        canvas = FindAnyObjectByType<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (canvas == null)
        {
            canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;
        }
        trail.gameObject.SetActive(true);
        OnClickParticle.Play();
        transform.DOKill();
        transform.DOScale(Vector3.one * .8f, 1).SetEase(Ease.OutBack);
        game.BeginDrag(box, eventData.position);

        transform.SetAsLastSibling();

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)canvas.transform, eventData.position, eventData.pressEventCamera, out var lp))
        {
            game.BeginDrag(box, lp);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null)
        {
            canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;
        }
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)canvas.transform, eventData.position, eventData.pressEventCamera, out var lp))
        {
            game.DragMove(lp);
        }
    }
 
     public void OnEndDrag(PointerEventData eventData)
     {
        if (canvas == null)
        {
            canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;
        }

        trail.gameObject.SetActive(false);

         transform.DOKill();
         transform.DOScale(Vector3.one * .9f, 1).SetEase(Ease.OutBack);
         if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                 (RectTransform)canvas.transform, eventData.position, eventData.pressEventCamera, out var lp)) return;
         if (!game.EndDrag(lp)) return;
         OnClickEndParticle.Play();
     }
}