using UnityEngine;
using UnityEngine.EventSystems;

public class TMPLinkProxy : MonoBehaviour, IPointerClickHandler
{
    // Drag your Settings script/object into this slot in the Inspector
    public Settings settingsManager;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (settingsManager != null)
        {
            settingsManager.OnPointerClick(eventData);
        }
    }
}
