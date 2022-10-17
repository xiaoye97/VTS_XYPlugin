using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGiftItem : MonoBehaviour
{
    public Text UserNameText;
    public Text PriceText;
    public Text GiftText;
    public UIAutoSize AutoSize;

    public void SetData(string userName, string price, string desc)
    {
        UserNameText.text = userName;
        PriceText.text = price;
        GiftText.text = desc;
        AutoSize.UpdateMode = true;
        Invoke("CloseUpdateMode", 0.2f);
    }

    public void CloseUpdateMode()
    {
        AutoSize.UpdateMode = false;
    }
}