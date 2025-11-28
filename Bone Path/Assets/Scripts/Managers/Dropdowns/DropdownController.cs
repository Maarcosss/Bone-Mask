using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/*Author: Marcos Isar
Date: 20 - Nov - 2025*/

public class DropdownController : MonoBehaviour, ISelectHandler
{
    private ScrollRect scrollRect;
    private float scrollPosition = 1;

    void Start()
    {
        scrollRect = GetComponentInParent<ScrollRect>(true);

        int childCount = scrollRect.content.transform.childCount - 1;
        int childIndex = transform.GetSiblingIndex();

        childIndex = childIndex < ((float)childCount / 2f) ? childIndex - 1 : childIndex;

        scrollPosition = 1 - ((float)childIndex / childCount);
    }

    //Adjust ScrollRect position when element is selected with gamepad
    public void OnSelect(BaseEventData eventData)
    {
        if (IsGamepadActive() && scrollRect)
        {
            scrollRect.verticalScrollbar.value = scrollPosition;
        }
    }

    //Check if any gamepad input is active
    private bool IsGamepadActive()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        return Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f;
    }
}
