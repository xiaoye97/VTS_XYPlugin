using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VTS_XYPluginCommon
{
    [Serializable]
    public class DropSettingData
    {
        public float DropPosX;
        public float DropPosY;
        public float BaseSpeed;
        public float HighSpeed;
        public int ChangeSpeedCount;

        public static DropSettingData CreateDefault()
        {
            DropSettingData data = new DropSettingData();
            data.DropPosX = 0;
            data.DropPosY = 65;
            data.BaseSpeed = 3;
            data.HighSpeed = 10;
            data.ChangeSpeedCount = 20;
            return data;
        }
    }
}
