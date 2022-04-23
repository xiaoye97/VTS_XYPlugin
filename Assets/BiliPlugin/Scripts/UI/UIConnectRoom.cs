using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIConnectRoom : MonoBehaviour
{
    public TMP_InputField RoomIDInputField;
    public Button ConnectRoomBtn;
    public Button DisconnectRoomBtn;

    private void Awake()
    {
        ConnectRoomBtn.onClick.AddListener(OnClickConnectRoomBtn);
        DisconnectRoomBtn.onClick.AddListener(OnClickDisconnectRoomBtn);
    }

    private void Start()
    {
        int roomID = ES3.Load<int>("RoomID", 362064);
        RoomIDInputField.text = roomID.ToString();
    }

    private void OnClickConnectRoomBtn()
    {
        BiliPlugin.Instance.BiliPython.ChangeRoomID(RoomIDInputField.text);
        BiliPlugin.Instance.BiliPython.StartPy();
        BiliPlugin.Instance.UIActionPanel.OnClickGetHotKeyBtn();
    }

    private void OnClickDisconnectRoomBtn()
    {
        BiliPlugin.Instance.BiliPython.EndPy();
    }
}
