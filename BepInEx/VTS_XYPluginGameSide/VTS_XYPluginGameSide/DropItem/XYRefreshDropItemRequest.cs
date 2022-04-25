using System;

namespace VTS_XYPluginGameSide
{
    [Serializable]
    public class XYRefreshDropItemRequest : IAPIMessage
    {
        [NonSerialized]
        public const string NAME = "XYRefreshDropItemRequest";
    }
}
