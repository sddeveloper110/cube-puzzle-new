using UnityEngine;
using UnityEngine.UI;

public class BackgroundChanger : MonoBehaviour
{
    public Sprite[] backgrounds;

    public void OnEnable()
    {
        GetComponent<Image>().sprite = backgrounds[Random.Range(0, backgrounds.Length)];
    }
}