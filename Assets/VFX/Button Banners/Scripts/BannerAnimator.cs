using UnityEngine;
using UnityEngine.UI;

public class BannerAnimator : MonoBehaviour
{
    public RawImage targetImage;

    public float ProValue = 0f;
    public float MasterIntroValue = 0f;
    public float MasterValue = 0f;
    public float LegendIntroValue = 0f;
    public float LegendValue = 0f;
    public float GeniusIntroValue = 0f;
    public float GeniusValue = 0f;
    public float ShineValue = 0f;

    private Material materialInstance;

    private int ProID;
    private int MasterIntroID;
    private int MasterID;
    private int LegendIntroID;
    private int LegendID;
    private int GeniusIntroID;
    private int GeniusID;
    private int ShineID;

    private const string ProPropertyName = "_Pro";
    private const string MasterIntroPropertyName = "_Master_Intro";
    private const string MasterPropertyName = "_Master";
    private const string LegendIntroPropertyName = "_Legend_Intro";
    private const string LegendPropertyName = "_Legend";
    private const string GeniusIntroPropertyName = "_Genius_Intro";
    private const string GeniusPropertyName = "_Genius";
    private const string ShinePropertyName = "_Shine";

    void Awake()
    {
        if (targetImage == null)
        {
            return;
        }

        // materialInstance = targetImage.material;

        var srcMat = targetImage.material;
        if (srcMat != null)
        {
            materialInstance = Instantiate(srcMat);
            targetImage.material = materialInstance;
        }
        else
        {
            materialInstance = null;
        }

        ProID = Shader.PropertyToID(ProPropertyName);
        MasterIntroID = Shader.PropertyToID(MasterIntroPropertyName);
        MasterID = Shader.PropertyToID(MasterPropertyName);
        LegendIntroID = Shader.PropertyToID(LegendIntroPropertyName);
        LegendID = Shader.PropertyToID(LegendPropertyName);
        GeniusIntroID = Shader.PropertyToID(GeniusIntroPropertyName);
        GeniusID = Shader.PropertyToID(GeniusPropertyName);
        ShineID = Shader.PropertyToID(ShinePropertyName);

        SetAllProperties();
    }

    void LateUpdate()
    {
        if (materialInstance == null) return;

        SetAllProperties();
    }

    private void SetAllProperties()
    {
        if (materialInstance.HasProperty(ProID)) materialInstance.SetFloat(ProID, ProValue);
        if (materialInstance.HasProperty(MasterIntroID)) materialInstance.SetFloat(MasterIntroID, MasterIntroValue);
        if (materialInstance.HasProperty(MasterID)) materialInstance.SetFloat(MasterID, MasterValue);
        if (materialInstance.HasProperty(LegendIntroID)) materialInstance.SetFloat(LegendIntroID, LegendIntroValue);
        if (materialInstance.HasProperty(LegendID)) materialInstance.SetFloat(LegendID, LegendValue);
        if (materialInstance.HasProperty(GeniusIntroID)) materialInstance.SetFloat(GeniusIntroID, GeniusIntroValue);
        if (materialInstance.HasProperty(GeniusID)) materialInstance.SetFloat(GeniusID, GeniusValue);
        if (materialInstance.HasProperty(ShineID)) materialInstance.SetFloat(ShineID, ShineValue);
    }
}