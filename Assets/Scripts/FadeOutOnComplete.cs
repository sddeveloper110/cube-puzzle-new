using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class FadeOutOnComplete : MonoBehaviour
{
    public GameObject endPart;
    public GameObject subObject;

    [Header("Tween Settings")]
    public float moveY = 792f;
    public float duration = 0.5f;
    public Image[] boxes;

    void OnEnable()
    {
        float randomOffset = Random.Range(0.5f, 0.8f); //0.1 0.9
        float randomYOffset = Random.Range(770f, 792f);

        transform.DOLocalMoveY(/*moveY+*/ randomYOffset, duration + randomOffset)
            //.SetEase(Ease.OutFlash)
            .OnComplete(() =>
            {

                int randomX = Random.Range(-5, 5);
                Vector3 spawnPos = transform.position + new Vector3(randomX,0, -10f);

                GameObject sp = Instantiate(endPart, spawnPos, Quaternion.identity);
                sp.SetActive(true);
                sp.transform.SetParent(transform);

                foreach (var img in boxes)
                {
                    if (img != null)
                    {
                        img.DOFade(0f, 1.5f).OnComplete(() => Destroy(gameObject));

                    }
                   
                }

            
              
            });
    }
}
