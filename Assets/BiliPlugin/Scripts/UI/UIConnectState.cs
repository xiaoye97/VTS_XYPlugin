using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIConnectState : MonoBehaviour
{
    public Button ReconnectBtn;
    public Image LightImage;
    public TextMeshProUGUI StateText;

    private ConnectState nowState;
    public ConnectState NowState
    {
        get { return nowState; }
        set 
        { 
            nowState = value;
            switch (nowState)
            {
                case ConnectState.None:
                    LightImage.color = Color.white;
                    StateText.text = "无连接";
                    ReconnectBtn.gameObject.SetActive(true);
                    break;
                case ConnectState.Connecting:
                    LightImage.color = Color.yellow;
                    StateText.text = "正在连接...";
                    ReconnectBtn.gameObject.SetActive(false);
                    break;
                case ConnectState.Connected:
                    LightImage.color = Color.green;
                    StateText.text = "已连接";
                    ReconnectBtn.gameObject.SetActive(false);
                    break;
                case ConnectState.Disconnected:
                    LightImage.color = Color.gray;
                    StateText.text = "已断开连接";
                    ReconnectBtn.gameObject.SetActive(true);
                    break;
                case ConnectState.Error:
                    LightImage.color = Color.red;
                    StateText.text = "连接出现错误";
                    ReconnectBtn.gameObject.SetActive(true);
                    break;
            }
        }
    }

    private void Awake()
    {
        ReconnectBtn.onClick.AddListener(OnClickReconnectBtn);
    }

    private void OnClickReconnectBtn()
    {
        BiliPlugin.Instance.ConnectToVTS();
    }
}
