using System;

namespace VTS_XYPluginGameSide
{
    [Serializable]
    public class XYSetModelColliderRequest : IAPIMessage
    {
        [NonSerialized]
        public const string NAME = "XYSetModelColliderRequest";

        public bool Enable;
        public float Radius;
        public float OffsetX;
        public float OffsetY;
    }
}
