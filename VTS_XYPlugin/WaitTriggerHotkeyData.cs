using System;
using VTS_XYPlugin_Common;

namespace VTS_XYPlugin
{
    public class WaitTriggerHotkeyData
    {
        public GiftTriggerActionData data;
        public Action<GiftTriggerActionData> onTriggerCallback;
    }
}
