// By 宵夜97
using System;
using Lean.Gui;
using UnityEngine.UI;

public class UIWindowMessageBox : MonoSingleton<UIWindowMessageBox>
{
    public LeanWindow Window;
    public Text Title;
    public Text Message;
    public LeanButton OkBtn;
    public LeanButton CancelBtn;

    private Action OkAction;
    private Action CancelAction;

    public void ShowOk(string title, string msg, Action onOk = null)
    {
        Title.text = title;
        Message.text = msg.Replace(" ", "\u00A0");
        OkAction = onOk;
        CancelBtn.gameObject.SetActive(false);
        Window.TurnOn();
    }

    public void ShowOkCancel(string title, string msg, Action onOk = null, Action onCancel = null)
    {
        Title.text = title;
        Message.text = msg.Replace(" ", "\u00A0");
        OkAction = onOk;
        CancelAction = onCancel;
        CancelBtn.gameObject.SetActive(true);
        Window.TurnOn();
    }

    public void OnOkClick()
    {
        if (OkAction != null)
        {
            OkAction();
        }
        Close();
    }

    public void OnCancelClick()
    {
        if (CancelAction != null)
        {
            CancelAction();
        }
        Close();
    }

    public void Close()
    {
        Window.TurnOff();
    }
}
