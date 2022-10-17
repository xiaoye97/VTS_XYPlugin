using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class UIAutoSize : MonoBehaviour
{
    public bool AutoWidth;
    public bool AutoHeight;
    // 是否在Update中执行
    public bool UpdateMode;
    private ILayoutElement layoutElement;
    private RectTransform rectTransform;

    private void Awake()
    {
        layoutElement = GetComponent<ILayoutElement>();
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (UpdateMode)
        {
            Refresh();
        }
    }

    public void Refresh()
    {
        if (layoutElement != null)
        {
            if (AutoWidth)
            {
                if (rectTransform.sizeDelta.x != layoutElement.preferredWidth)
                {
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, layoutElement.preferredWidth);
                }
            }
            if (AutoHeight)
            {
                if (rectTransform.sizeDelta.y != layoutElement.preferredHeight)
                {
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, layoutElement.preferredHeight);
                }
            }
        }
    }
}
