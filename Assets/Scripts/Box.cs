using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;

public class Box
{
    private static readonly Color Red = new(0.95f, 0.49f, 0.26f);

    private Color LightGray = new(.7f, .7f, .7f, 1f);
    private static readonly Color White = new(1, 1, 1, 1);
    private static readonly Color Black = new(0.24f, 0.24f, 0.24f);
    
    public int? value; // null == empty
    public (int x, int y) gridPos;
    public bool fixedRed;
    public RectTransform rt;
    public bool Swappable = false;
    public Image bg;
    public TMP_Text label;
    public Vector2 originalScreenPos;

    public void SetVisual()
    {
        if (fixedRed)
        {
            bg.color = Color.red;
            //label.color = Color.red;
        }
        else
        {
            ColorUtility.TryParseHtmlString("#434564", out LightGray);

            bg.color = value.HasValue ? LightGray : White;
            //label.color = Black;
        }

        label.text = value.HasValue ? value.Value.ToString() : "";
    }
    public void MakeBoxSwappable(bool shoubleBeSwapalbe)
    {
        Swappable = shoubleBeSwapalbe;
    }

}