// By 宵夜97
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class UITextAutoSize : MonoBehaviour
{
    public bool AutoSetWidth;
    public float WidthPadding;
    public bool AutoSetHeight;
    public float HeightPadding;

    private RectTransform rt;
    private Text textComponent;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        textComponent = GetComponent<Text>();
    }

    void Update()
    {
        if (AutoSetHeight && AutoSetWidth)
        {
            rt.sizeDelta = new Vector2(textComponent.preferredWidth + WidthPadding, textComponent.preferredHeight + HeightPadding);
        }
        else
        {
            if (AutoSetHeight)
            {
                rt.sizeDelta = new Vector2(rt.sizeDelta.x, textComponent.preferredHeight);
            }
            else if (AutoSetWidth)
            {
                rt.sizeDelta = new Vector2(textComponent.preferredWidth, rt.sizeDelta.y);
            }
        }
    }
}