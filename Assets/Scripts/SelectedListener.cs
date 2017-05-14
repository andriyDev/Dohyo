using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectedListener : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public bool isSelected { get { return selected; } }

    private bool selected;

    public void OnSelect(BaseEventData eventData)
    {
        selected = true;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        selected = false;
    }
}
