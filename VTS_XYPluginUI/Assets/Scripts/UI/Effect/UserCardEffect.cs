using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Lean.Gui;
using DG.Tweening;

public class UserCardEffect : MonoBehaviour
{
    public bool IsConnected;
    public Image UserHeadImage;
    public Sprite NoFace;
    public Sprite UserFace;
    private bool isConnected;
    public LeanCircle HuXiCircle;
    public LeanCircle StateCircle;
    private Color ConnectingColor;
    private Color ConnectedColor;

    private void Awake()
    {
        ConnectingColor = new Color(200 / 255f, 200 / 255f, 200 / 255f);
        ConnectedColor = new Color(171 / 255f, 215 / 255f, 48 / 255f);
    }

    private void Update()
    {
        float v = Mathf.Sin(Time.time);
        HuXiCircle.Blur = 20 + v * 10;
        if (IsConnected != isConnected)
        {
            SetConnectState(IsConnected);
        }
    }

    /// <summary>
    /// 设置连接状态
    /// </summary>
    /// <param name="connected"></param>
    public void SetConnectState(bool connected)
    {
        if (isConnected != connected)
        {
            isConnected = connected;
            if (isConnected)
            {
                HuXiCircle.DOColor(ConnectedColor, 1f);
                StateCircle.DOColor(ConnectedColor, 1f);
                UserHeadImage.sprite = UserFace;
            }
            else
            {
                HuXiCircle.DOColor(ConnectingColor, 1f);
                StateCircle.DOColor(ConnectingColor, 1f);
                UserHeadImage.sprite = NoFace;
            }
        }
    }
}