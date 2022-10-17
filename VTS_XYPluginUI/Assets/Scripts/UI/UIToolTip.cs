using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIToolTip : MonoBehaviour
{
    public float ToolTipWidth = 500;
    public Vector2 Padding = new Vector2(20, 20);
    public Text TipText;
    public RectTransform TextRT;
    public RectTransform ShadowRT;
    public RectTransform BGRT;

    private bool isShow;

    private void Update()
    {
        if (!isShow) return;
        ChangeSize();
        ChangePosition();
    }

    public void ShowToolTip(string data)
    {
        if (!string.IsNullOrWhiteSpace(data))
        {
            isShow = true;
            TipText.text = data;
            TextRT.sizeDelta = new Vector2(ToolTipWidth, 0);
        }
    }

    public void HideTooltip()
    {
        isShow = false;
        TipText.text = "";
    }

    private void ChangeSize()
    {
        float ph = TipText.preferredHeight;
        float pw = Mathf.Min(TipText.preferredWidth, ToolTipWidth);
        ShadowRT.sizeDelta = new Vector2(pw / 2 + Padding.x, ph / 2 + Padding.y);
        BGRT.sizeDelta = new Vector2(pw / 2 + Padding.x, ph / 2 + Padding.y);
        TextRT.sizeDelta = new Vector2(pw, ph);
    }

    private void ChangePosition()
    {
        Vector2 pos;
        // 反向偏移，当鼠标在屏幕左侧时，向右侧偏移，当鼠标在屏幕上方时，向下方偏移
        Vector2 reverseOffset;
        reverseOffset.x = Input.mousePosition.x < Screen.width / 2 ? 20 + ShadowRT.sizeDelta.x / 2 : -20 - ShadowRT.sizeDelta.x / 2;
        reverseOffset.y = Input.mousePosition.y < Screen.height / 2 ? 20 + ShadowRT.sizeDelta.y / 2 : -20 - ShadowRT.sizeDelta.y / 2;
        // 计算位置
        // 屏幕比例
        float bili = 900f / Screen.height;
        pos.x = Input.mousePosition.x * bili + reverseOffset.x;
        pos.y = Input.mousePosition.y * bili + reverseOffset.y;

        ShadowRT.anchoredPosition = pos;
    }
}