using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

    public void OnSelect(BaseEventData eventData)
    {
        if (IsGamepadActive() && scrollRect)
        {
            scrollRect.verticalScrollbar.value = scrollPosition;
        }
    }

    private bool IsGamepadActive()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        return Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f;
    }
}
