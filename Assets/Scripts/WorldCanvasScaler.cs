using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldCanvasScaler : MonoBehaviour
{
    public Vector2 targetScale;

    private void Update()
    {
        GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, Screen.height);
        transform.localScale = Screen.width > Screen.height ? new Vector2(targetScale.x / Screen.width, targetScale.y / Screen.width) : new Vector2(targetScale.x / Screen.height, targetScale.y / Screen.height);
    }
}
