using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/*Author: Marcos Isar
Date: 20 - Nov - 2025*/

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

    //Change image color when UI element is selected
    public void OnSelect(BaseEventData eventData)
    {
        if (targetImage != null)
            targetImage.color = highlightColor;
    }

    //Revert image color when UI element is deselected
    public void OnDeselect(BaseEventData eventData)
    {
        if (targetImage != null)
            targetImage.color = normalColor;
    }
}
