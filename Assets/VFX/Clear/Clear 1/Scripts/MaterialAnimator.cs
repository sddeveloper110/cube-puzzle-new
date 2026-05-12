using UnityEngine;
using UnityEngine.UI;

public class MaterialAnimator : MonoBehaviour
{
    public Image targetImage;
    private Material materialInstance;

    public float TransitionValue = 0f;

    [SerializeField]
    private string shaderPropertyName = "Transition";

    private int shaderPropertyID;

    void Awake()
    {
        if (targetImage == null)
        {
            return;
        }

        materialInstance = targetImage.material;
        shaderPropertyID = Shader.PropertyToID(shaderPropertyName);

        if (materialInstance.HasProperty(shaderPropertyID))
        {
            materialInstance.SetFloat(shaderPropertyID, TransitionValue);
        }
    }


    void LateUpdate()
    {
        if (materialInstance != null && materialInstance.HasProperty(shaderPropertyID))
        {
            materialInstance.SetFloat(shaderPropertyID, TransitionValue);
        }
    }
}
