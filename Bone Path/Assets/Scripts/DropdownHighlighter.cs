using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class DropdownHighlighter : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Configuración de Colores")]
    [Tooltip("Color normal del dropdown")]
    public Color normalColor = Color.white;
    [Tooltip("Color cuando está seleccionado/highlighted")]
    public Color highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
    [Tooltip("Multiplicador de color para el highlight")]
    public float colorMultiplier = 1.0f;

    private TMP_Dropdown dropdown;
    private Image dropdownImage;
    private bool isSelected = false;
    private bool isPointerOver = false;

    void Start()
    {
        // Obtener componentes
        dropdown = GetComponent<TMP_Dropdown>();
        dropdownImage = GetComponent<Image>();

        if (dropdown == null)
        {
            Debug.LogWarning($"DropdownHighlighter en {gameObject.name} no encontró TMP_Dropdown");
            return;
        }

        if (dropdownImage == null)
        {
            Debug.LogWarning($"DropdownHighlighter en {gameObject.name} no encontró Image");
            return;
        }

        // Configurar colores desde el ColorBlock del dropdown si están disponibles
        ColorBlock colors = dropdown.colors;
        normalColor = colors.normalColor;
        highlightedColor = colors.highlightedColor;
        colorMultiplier = colors.colorMultiplier;

        // Establecer color inicial
        ActualizarColor();
    }

    // Llamado cuando el dropdown es seleccionado con controlador
    public void OnSelect(BaseEventData eventData)
    {
        isSelected = true;
        ActualizarColor();
        Debug.Log($"🎮 Dropdown {gameObject.name} seleccionado con controlador");
    }

    // Llamado cuando el dropdown pierde la selección
    public void OnDeselect(BaseEventData eventData)
    {
        isSelected = false;
        ActualizarColor();
        Debug.Log($"🎮 Dropdown {gameObject.name} deseleccionado");
    }

    // Llamado cuando el mouse entra (para mantener compatibilidad)
    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
        ActualizarColor();
    }

    // Llamado cuando el mouse sale
    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        ActualizarColor();
    }

    void ActualizarColor()
    {
        if (dropdownImage == null) return;

        Color colorFinal;

        // Priorizar mouse over, luego selección con controlador
        if (isPointerOver || isSelected)
        {
            colorFinal = highlightedColor * colorMultiplier;
        }
        else
        {
            colorFinal = normalColor * colorMultiplier;
        }

        dropdownImage.color = colorFinal;
    }

    // Método público para forzar actualización
    public void ForzarActualizacion()
    {
        ActualizarColor();
    }

    // Método para configurar colores manualmente
    public void ConfigurarColores(Color normal, Color highlighted, float multiplier = 1.0f)
    {
        normalColor = normal;
        highlightedColor = highlighted;
        colorMultiplier = multiplier;
        ActualizarColor();
    }
}
