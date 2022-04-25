using System;

namespace VTS_XYPluginGameSide
{
    [Serializable]
    public class XYDropItemRequest : IAPIMessage
    {
        [NonSerialized]
        public const string NAME = "XYDropItemRequest";

        public string ImageName;
        public int DropCount;
    }
}
