using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayVFXSpeed : MonoBehaviour
{
    public Material[] vfxMalerail;
    public Image renderer;
    //public float speed = 1.0f;
    public RectTransform RectTransform;


    Coroutine vfxCoroutine;
    int materialNum;
    private void OnEnable()
    {
        RectTransform = GetComponent<RectTransform>();
    }
    public void PlayVFX(float duration, Vector2 pos, int count)
    {
        if (RectTransform.anchoredPosition != pos)
        {
            if (count == 24)
                materialNum = 2;
            else if (count == 23 || count == 22)
                materialNum = 1;
            else
                materialNum = 0;

            RectTransform.anchoredPosition = pos;
            if (vfxCoroutine != null)
                StopCoroutine(vfxCoroutine);
            vfxCoroutine = StartCoroutine(VFXAnimation(materialNum, duration));
        }
    }
    IEnumerator VFXAnimation(int materialNum, float duration)
    {
        var mat = vfxMalerail[materialNum];
        renderer.material = mat;

        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            renderer.material.SetFloat("_Transition", t / duration);
            yield return null;
        }

        mat.SetFloat("_Transition", 1f); // ensure final value is exactly 1
    }
    public void ShowGlowAtBox(Box readyForGlow, int count)
    {
        if (count == 25)
            readyForGlow.bg.gameObject.GetComponentInParent<Animator>().SetTrigger("Red");
        else if (count == 23 || count == 24)
            readyForGlow.bg.gameObject.GetComponentInParent<Animator>().SetTrigger("Orange");
        else
            readyForGlow.bg.gameObject.GetComponentInParent<Animator>().SetTrigger("Blue");
    }

}
