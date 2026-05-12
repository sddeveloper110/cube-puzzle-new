using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class BoxExplodeEffect : MonoBehaviour
{
    public ParticleSystem explosionParticles; 
    public float moveDistance = 100f;
    public float moveDuration = 0.6f; 
    public float punchPower = 0.4f;  
    public float punchDuration = 0.3f;
    public float rotateAmount = 120f;
    public Image boxImg;
    private RectTransform rt;

    private void OnEnable()
    {
        rt = GetComponent<RectTransform>();
        Explode();
    }

    public void Explode()
    {
        if (rt == null) return;

        Vector2 randomDir = (Random.insideUnitCircle + Vector2.up * 0.5f).normalized;
        Vector2 targetPos = rt.anchoredPosition + (randomDir * moveDistance);

        if (explosionParticles != null)
        {
            explosionParticles.Play();
            Destroy(explosionParticles.gameObject, 2f);
        }

        Sequence seq = DOTween.Sequence();

        seq.Append(rt.DOPunchScale(Vector3.one * punchPower, punchDuration, 5, 0.5f));

        seq.Append(rt.DOAnchorPos(targetPos, moveDuration).SetEase(Ease.OutQuad));
        seq.Join(rt.DORotate(new Vector3(0, 0, Random.Range(-rotateAmount, rotateAmount)), moveDuration, RotateMode.FastBeyond360));
        
        
        seq.Append(boxImg.DOFade(0f, 1f).OnComplete(() => Destroy(gameObject)));
        //seq.Append(rt.DOScale(0f, 0.5f).SetEase(Ease.InBack));
        //seq.OnComplete(() => Destroy(gameObject));

    }
}
