using UnityEngine;
using System.Collections;
using System;
public class LineSpawner : MonoBehaviour
{
    [Header("Prefab Settings")]
    //public GameObject linePrefab;
    //public GameObject legndryText;
    public GameObject parentObject;
    public int totalSpawns = 5;
    public float delayBetweenSpawns = 0.1f;
    public GameObject score50Image;
    public CubeSmasher cubeSmasher;

    private GameObject gameCompleteVfx;

    
    int lastEffectIndex = -1;
    int currentEffectIndex = -1;
    [Serializable]
    public struct EffectData
    {
        public GameObject effectText;
        [HideInInspector]
        public GameObject effectTextShadow;
        public GameObject clearEffect;
    }
    public EffectData[] effectData;

    private void OnEnable()
    {
        foreach (var effect in effectData) 
            effect.clearEffect.SetActive(false);
        while (currentEffectIndex == lastEffectIndex)
        {
            currentEffectIndex = UnityEngine.Random.Range(0, effectData.Length);
        }
        lastEffectIndex = currentEffectIndex;
        gameCompleteVfx = effectData[currentEffectIndex].clearEffect;
        SpawnLines();
    }
    public void SpawnLines()
    {
        StartCoroutine(SpawnLineRoutine());
    }

    private IEnumerator SpawnLineRoutine()
    {
        gameCompleteVfx.SetActive(false);
        gameCompleteVfx.SetActive(true);
        gameCompleteVfx.GetComponent<Animator>().SetTrigger("GridClear");

        //if (linePrefab == null)
        //{
        //    Debug.LogWarning("LineSpawner: No prefab assigned!");
        //    yield break;
        //}
        score50Image.SetActive(true);

        if(effectData[currentEffectIndex].effectTextShadow == null)
        {
            effectData[currentEffectIndex].effectTextShadow = 
                Instantiate(effectData[currentEffectIndex].effectText, transform.position, Quaternion.identity);
            effectData[currentEffectIndex].effectTextShadow.transform.SetParent(parentObject.transform);
        }
        else
            effectData[currentEffectIndex].effectTextShadow.SetActive(true);

        //GameObject txt = Instantiate(legndryText, transform.position, Quaternion.identity);
        //txt.transform.SetParent(parentObject.transform);
        yield return new WaitForSeconds(4);
        cubeSmasher.UpdateScoreAndHighScore(50);
        gameCompleteVfx.SetActive(false);
    }
    private void OnDisable()
    {
        gameCompleteVfx.SetActive(false);
    }

}
