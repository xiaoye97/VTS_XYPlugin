using System;
using UnityEngine;
using VTS_XYPlugin_Common;

namespace VTS_XYPlugin
{
    public class WaitDropItemData
    {
        public int WaitCount = 1;
        public bool HighSpeed = false;
        public Vector2 StartDropPoint = new Vector3(0, 65, 100);
        public DropItemData ItemData;
        // 在掉落头像时使用
        public int UserID;
    }
}
