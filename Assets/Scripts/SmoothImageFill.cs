using UnityEngine;
using UnityEngine.UI;

public class SmoothImageFill : MonoBehaviour
{
    [Header("UI Image to Fill")]
    public Image targetImage;

    [Header("Animation Settings")]
    public float fillSpeed = 1f;  // How fast it fills (1 = 1 second)
    public float startDelay = 0.5f;

    private void OnEnable()
    {
        if (targetImage != null)
            StartCoroutine(FillImageSmoothly());
    }

    private System.Collections.IEnumerator FillImageSmoothly()
    {
        targetImage.fillAmount = 0f;
        yield return new WaitForSeconds(startDelay);    
        while (targetImage.fillAmount < 1f)
        {
            targetImage.fillAmount += Time.deltaTime * fillSpeed;
            yield return null;
        }

        targetImage.fillAmount = 1f; // Ensure it ends at full
    }
}
