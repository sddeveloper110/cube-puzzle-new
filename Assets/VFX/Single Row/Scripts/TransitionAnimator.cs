using UnityEngine;

public class TransitionAnimator : MonoBehaviour
{
    [SerializeField] private float duration = 3f;
    private Material mat;
    private float elapsed;

    void Awake()
    {
        var image = GetComponent<UnityEngine.UI.Image>();
        mat = new Material(image.material);
        image.material = mat;
    }

    void OnEnable()
    {
        elapsed = 0f;
        mat.SetFloat("_Transition", 0f);
    }

    void Update()
    {
        if (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            mat.SetFloat("_Transition", t);
        }
    }
}
