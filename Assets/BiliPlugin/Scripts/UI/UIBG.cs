using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBG : MonoBehaviour
{
    public RectTransform MoveRT;
    private Vector2 startPos = new Vector2(0, 0);
    private Vector2 endPos = new Vector2(-106, 0);
    private float process = 0;
    private float moveTime = 4f;

    void Update()
    {
        process = Time.time % moveTime / moveTime;
        float x = Mathf.Lerp(startPos.x, endPos.x, process);
        float y = Mathf.Lerp(startPos.y, endPos.y, process);
        MoveRT.anchoredPosition = new Vector2(x, y);
    }
}
