using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VTS_XYPluginCommon
{
    [Serializable]
    public class DropItemData
    {
        public long ImageFileSize; // 用于判断图片是否发送了变化，如果发生了变化，则需要重新加载
        public float Scale;
        public float ColliderRadius;
        public float LifeTime;
        public int PerTriggerDropCount;

        public static DropItemData CreateDefault()
        {
            DropItemData data = new DropItemData();
            data.ImageFileSize = 0;
            data.Scale = 1;
            data.ColliderRadius = 1;
            data.LifeTime = 3;
            data.PerTriggerDropCount = 1;
            return data;
        }
    }
}
