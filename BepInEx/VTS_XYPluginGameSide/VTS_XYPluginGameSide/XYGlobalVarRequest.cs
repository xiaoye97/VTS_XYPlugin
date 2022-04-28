using System;

namespace VTS_XYPluginGameSide
{
    [Serializable]
    public class XYGlobalVarRequest
    {
        [NonSerialized]
        public const string NAME = "XYGlobalVarRequest";

        public string Key;
        public string Json;
    }
}
