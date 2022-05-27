// By 宵夜97
using Lean.Gui;
using UnityEngine.UI;

public class UINotification : MonoSingleton<UINotification>
{
    public LeanPulse Pulse;
    public Text NotificationText;

    public void Show(string notification)
    {
        NotificationText.text = notification;
        Pulse.Pulse();
    }
}
