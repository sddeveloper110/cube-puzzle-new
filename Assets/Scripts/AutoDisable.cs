using UnityEngine;

public class AutoDisable : MonoBehaviour
{
    [Tooltip("Time (in seconds) after which this object will be set inactive")]
    public float disableDelay = 2f;

    [Tooltip("Should the timer start automatically when the object is enabled?")]
    public bool autoStart = true;

    private void OnEnable()
    {
        if (autoStart)
            Invoke(nameof(DisableObject), disableDelay);
    }

    
   

    private void DisableObject()
    {
        gameObject.SetActive(false);
    }
    public void DestroyMe()
    {
        Destroy(gameObject);   
    }

}
