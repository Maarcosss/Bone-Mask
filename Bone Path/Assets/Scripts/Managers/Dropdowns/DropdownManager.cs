using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class DropdownManager : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    private Image targetImage;
    private Color normalColor;
    public Color highlightColor = Color.yellow;

    void Awake()
    {
        Toggle toggle = GetComponent<Toggle>();
        if (toggle != null)
        {
            targetImage = toggle.targetGraphic as Image;
        }
        else
        {
            targetImage = GetComponent<Image>();
        }

        if (targetImage != null)
            normalColor = targetImage.color;
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (targetImage != null)
            targetImage.color = highlightColor;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (targetImage != null)
            targetImage.color = normalColor;
    }
}
