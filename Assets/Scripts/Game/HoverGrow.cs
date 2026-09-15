using UnityEngine;
using UnityEngine.EventSystems;

public class HoverGrow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float growScale = 1.08f;
    private Vector3 originalScale;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = originalScale * growScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = originalScale;
    }
}
