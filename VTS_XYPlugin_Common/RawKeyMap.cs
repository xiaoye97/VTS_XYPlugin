﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VTS_XYPlugin_Common
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RawKeyMap : ushort
    {
        LeftButton = 1,
        RightButton,
        Cancel,
        MiddleButton,
        ExtraButton1,
        ExtraButton2,
        Back = 8,
        Tab,
        Clear = 12,
        Return,
        Shift = 16,
        Control,
        Menu,
        Pause,
        CapsLock,
        Kana,
        Hangeul = 21,
        Hangul = 21,
        Junja = 23,
        Final,
        Hanja,
        Kanji = 25,
        Escape = 27,
        Convert,
        NonConvert,
        Accept,
        ModeChange,
        Space,
        Prior,
        Next,
        End,
        Home,
        Left,
        Up,
        Right,
        Down,
        Select,
        Print,
        Execute,
        Snapshot,
        Insert,
        Delete,
        Help,
        N0,
        N1,
        N2,
        N3,
        N4,
        N5,
        N6,
        N7,
        N8,
        N9,
        A = 65,
        B,
        C,
        D,
        E,
        F,
        G,
        H,
        I,
        J,
        K,
        L,
        M,
        N,
        O,
        P,
        Q,
        R,
        S,
        T,
        U,
        V,
        W,
        X,
        Y,
        Z,
        LeftWindows,
        RightWindows,
        Application,
        Sleep = 95,
        Numpad0,
        Numpad1,
        Numpad2,
        Numpad3,
        Numpad4,
        Numpad5,
        Numpad6,
        Numpad7,
        Numpad8,
        Numpad9,
        Multiply,
        Add,
        Separator,
        Subtract,
        Decimal,
        Divide,
        F1,
        F2,
        F3,
        F4,
        F5,
        F6,
        F7,
        F8,
        F9,
        F10,
        F11,
        F12,
        F13,
        F14,
        F15,
        F16,
        F17,
        F18,
        F19,
        F20,
        F21,
        F22,
        F23,
        F24,
        NumLock = 144,
        ScrollLock,
        NEC_Equal,
        Fujitsu_Jisho = 146,
        Fujitsu_Masshou,
        Fujitsu_Touroku,
        Fujitsu_Loya,
        Fujitsu_Roya,
        LeftShift = 160,
        RightShift,
        LeftControl,
        RightControl,
        LeftMenu,
        RightMenu,
        BrowserBack,
        BrowserForward,
        BrowserRefresh,
        BrowserStop,
        BrowserSearch,
        BrowserFavorites,
        BrowserHome,
        VolumeMute,
        VolumeDown,
        VolumeUp,
        MediaNextTrack,
        MediaPrevTrack,
        MediaStop,
        MediaPlayPause,
        LaunchMail,
        LaunchMediaSelect,
        LaunchApplication1,
        LaunchApplication2,
        OEM1 = 186,
        OEMPlus,
        OEMComma,
        OEMMinus,
        OEMPeriod,
        OEM2,
        OEM3,
        OEM4 = 219,
        OEM5,
        OEM6,
        OEM7,
        OEM8,
        OEMAX = 225,
        OEM102,
        ICOHelp,
        ICO00,
        ProcessKey,
        ICOClear,
        Packet,
        OEMReset = 233,
        OEMJump,
        OEMPA1,
        OEMPA2,
        OEMPA3,
        OEMWSCtrl,
        OEMCUSel,
        OEMATTN,
        OEMFinish,
        OEMCopy,
        OEMAuto,
        OEMENLW,
        OEMBackTab,
        ATTN,
        CRSel,
        EXSel,
        EREOF,
        Play,
        Zoom,
        Noname,
        PA1,
        OEMClear
    }
}