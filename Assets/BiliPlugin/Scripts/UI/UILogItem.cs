using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UILogItem : MonoBehaviour
{
    public TextMeshProUGUI TimeText;
    public TextMeshProUGUI LogText;
    public Image TypeImage;

    public void SetData(LogData data, int index)
    {
        LogText.text = data.Msg;
        TimeText.text = data.Time.ToString();
        switch(data.LogType)
        {
            case LogType.Log:
                TypeImage.color = Color.green;
                break;
            case LogType.Warning:
                TypeImage.color = Color.yellow;
                break;
            case LogType.Error:
            case LogType.Exception:
            case LogType.Assert:
                TypeImage.color = Color.red;
                break;
        }
    }
}
