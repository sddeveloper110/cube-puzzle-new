using UnityEngine;
using UnityEngine.UI;

public class OutlineAnimator : MonoBehaviour
{
    public RawImage targetImage;

    public float TransitionValue = 0f;
    public bool MasterValue = false;
    public bool GoldenValue = false;

    private Material materialInstance;

    private int TransitionID;
    private int MasterID;
    private int GoldenID;

    private const string TransitionPropertyName = "_Transition";
    private const string MasterPropertyName = "_Master";
    private const string GoldenPropertyName = "_Golden";

    void Awake()
    {
        if (targetImage == null)
        {
            return;
        }

        materialInstance = targetImage.material;

        TransitionID = Shader.PropertyToID(TransitionPropertyName);
        MasterID = Shader.PropertyToID(MasterPropertyName);
        GoldenID = Shader.PropertyToID(GoldenPropertyName);

        SetAllProperties();
    }

    void LateUpdate()
    {
        if (materialInstance == null) return;

        SetAllProperties();
    }

    private void SetAllProperties()
    {
        if (materialInstance.HasProperty(TransitionID)) materialInstance.SetFloat(TransitionID, TransitionValue);

        if (materialInstance.HasProperty(MasterID))
        {
            float masterAsFloat = MasterValue ? 1.0f : 0.0f;
            materialInstance.SetFloat(MasterID, masterAsFloat);
        }

        if (materialInstance.HasProperty(GoldenID))
        {
            float goldenAsFloat = GoldenValue ? 1.0f : 0.0f;
            materialInstance.SetFloat(GoldenID, goldenAsFloat);
        }
    }
}