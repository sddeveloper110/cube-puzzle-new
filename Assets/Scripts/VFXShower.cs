using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

public enum VFXType { Blue, Red, White, Black }

public class VFXShower : MonoBehaviour
{
    [Header("Burst Prefabs")]
    //// [SerializeField] GameObject bluePrefab;
    [SerializeField] GameObject redPrefab;
    [SerializeField] GameObject whitePrefab;
    [SerializeField] GameObject blackPrefab;
    [SerializeField] GameObject ComboEffect;

    readonly Dictionary<VFXType, List<GameObject>> pools = new();
    readonly Dictionary<VFXType, GameObject> prefabs = new();

    void Awake()
    {
        //prefabs[VFXType.Blue] = bluePrefab;
        prefabs[VFXType.Red] = redPrefab;
        prefabs[VFXType.White] = whitePrefab;
        prefabs[VFXType.Black] = blackPrefab;

        //InitPool(VFXType.Blue);
        InitPool(VFXType.Red);
        InitPool(VFXType.White);
        InitPool(VFXType.Black);
    }

    void InitPool(VFXType type)
    {
        pools[type] = new List<GameObject>();
        AddToPool(type);
    }

    GameObject AddToPool(VFXType type)
    {
        var o = Instantiate(prefabs[type], transform);
        o.SetActive(false);
        pools[type].Add(o);
        return o;
    }

    GameObject GetFromPool(VFXType type)
    {
        foreach (var o in pools[type])
            if (!o.activeSelf)
                return o;

        return AddToPool(type); 
    }

   
    public void ShowBurst(Box bx)
    {

        var vfx = GetFromPool(bx.bg.color == Color.white ? VFXType.White :
            bx.bg.color == Color.red ? VFXType.Red : VFXType.Black);

        vfx.transform.position = bx.rt.position;
        vfx.SetActive(true);

        vfx.transform.DOKill(); // safety if reused

        DOVirtual.DelayedCall(0.8f, () =>
        {
            vfx.SetActive(false);
        });
    }
    public void PlayCombeEffect()
    {
        ComboEffect.SetActive(true);
        ComboEffect.GetComponent<Animator>().SetTrigger("Combo");
        DOVirtual.DelayedCall(1.5f, () =>
        {
            ComboEffect.SetActive(false);
        });
    }
    [SerializeField] private GameObject rowClearEffect;
    [SerializeField] private GameObject columnClearEffect;
    public void ShowRowEffect(Vector2 pos, float fillAmount)
    {
        if (rowClearEffect != null)
        {
            //Debug.LogError("fill amount before : " + fillAmount);

            if (fillAmount==0.8f)   fillAmount=0.78f;
            //Debug.LogError("fill amount: " + fillAmount);
            rowClearEffect.SetActive(true);

            rowClearEffect.GetComponent<RectTransform>().anchoredPosition = pos;

            // Set fill amount based on number of columns
            Image rowEffectImage = rowClearEffect.GetComponent<Image>();
            if (rowEffectImage != null)
            {
                // Calculate fill amount: 0.2 for 1 column, scaling up to 1.0 for 5 columns
                rowEffectImage.fillAmount = fillAmount;
            }
            // Deactivate after delay
            DOVirtual.DelayedCall(2f, () => rowClearEffect.SetActive(false));
        }
    }
    public void ShowColumnEffect(Vector2 pos, float fillAmount)
    {
        if (columnClearEffect != null)
        {
            //Debug.LogError("fill amount before : " + fillAmount);

            if (fillAmount == 0.8f) fillAmount = 0.78f;
            //Debug.LogError("fill amount: " + fillAmount);

            columnClearEffect.SetActive(true);
            // Optional: reset position if needed  
            columnClearEffect.GetComponent<RectTransform>().anchoredPosition = pos;
            Image rowEffectImage = columnClearEffect.GetComponent<Image>();
            if (rowEffectImage != null)
            {
                // Calculate fill amount: 0.2 for 1 column, scaling up to 1.0 for 5 columns
                rowEffectImage.fillAmount = fillAmount;
            }
            // Deactivate after delay
            DOVirtual.DelayedCall(2f, () => columnClearEffect.SetActive(false));
        }
    }
}
