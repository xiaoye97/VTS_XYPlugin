using System;
using System.Collections.Generic;

namespace VTS_XYPlugin_Common
{
    /// <summary>
    /// 全局保存的配置文件
    /// </summary>
    public class XYGlobalConfig
    {
        /// <summary>
        /// 是否自动打开GUI
        /// </summary>
        public bool AutoOpenGUI = true;
        /// <summary>
        /// 测试模式，可以显示隐藏的挂件等
        /// </summary>
        public bool DebugMode = true;
        /// <summary>
        /// 当挂件挂在透明的ArtMesh时隐藏显示
        /// </summary>
        public bool HideItemOnAlphaArtMesh = true;
        /// <summary>
        /// 当收到礼物时下载用户头像
        /// </summary>
        public bool DownloadHeadOnRecvGift = true;
        /// <summary>
        /// 开关消息系统的快捷键，需要按住Ctrl再按此键
        /// </summary>
        public RawKeyMap SwitchMessageSystemHotkey = RawKeyMap.Numpad1;
        public RawKeyMap SwitchDropGiftHotkey = RawKeyMap.Numpad2;
        public RawKeyMap SwitchTriggerActionHotkey = RawKeyMap.Numpad3;
        public BLiveConfig BLiveConfig = new BLiveConfig();
        public GiftDropConfig GiftDropConfig = new GiftDropConfig();
    }
}
