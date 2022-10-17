using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDanMuItem : MonoBehaviour
{
    public Text UserNameText;
    public Text DanMuText;
    public UIAutoSize AutoSize;

    public void SetData(string userName, string danMu)
    {
        UserNameText.text = userName;
        DanMuText.text = danMu;
        AutoSize.UpdateMode = true;
        Invoke("CloseUpdateMode", 0.2f);
    }

    public void CloseUpdateMode()
    {
        AutoSize.UpdateMode = false;
    }
}