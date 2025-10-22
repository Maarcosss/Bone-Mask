using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class DropdownHighlighter : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Color Settings")]
    public Color normalColor = Color.white;
    public Color highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
    public float colorMultiplier = 1.0f;

    private TMP_Dropdown dropdown;
    private Image dropdownImage;
    private bool isSelected = false;
    private bool isPointerOver = false;

    //Initialize dropdown components and colors
    void Start()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        dropdownImage = GetComponent<Image>();

        if (dropdown != null && dropdownImage != null)
        {
            ColorBlock colors = dropdown.colors;
            normalColor = colors.normalColor;
            highlightedColor = colors.highlightedColor;
            colorMultiplier = colors.colorMultiplier;

            UpdateColor();
        }
    }

    //Called when dropdown is selected with controller
    public void OnSelect(BaseEventData eventData)
    {
        isSelected = true;
        UpdateColor();
    }

    //Called when dropdown is deselected
    public void OnDeselect(BaseEventData eventData)
    {
        isSelected = false;
        UpdateColor();
    }

    //Called when pointer enters the dropdown
    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
        UpdateColor();
    }

    //Called when pointer exits the dropdown
    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        UpdateColor();
    }

    //Update dropdown image color based on selection or pointer hover
    void UpdateColor()
    {
        if (dropdownImage != null)
        {
            Color finalColor;
            if (isPointerOver || isSelected)
            {
                finalColor = highlightedColor * colorMultiplier;
            }
            else
            {
                finalColor = normalColor * colorMultiplier;
            }
            dropdownImage.color = finalColor;
        }
    }

    //Public method to force update color
    public void ForceUpdate()
    {
        UpdateColor();
    }

    //Public method to configure colors manually
    public void SetColors(Color normal, Color highlighted, float multiplier = 1.0f)
    {
        normalColor = normal;
        highlightedColor = highlighted;
        colorMultiplier = multiplier;
        UpdateColor();
    }
}
