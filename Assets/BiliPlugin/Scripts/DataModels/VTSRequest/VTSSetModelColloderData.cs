using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VTS.Models;

[System.Serializable]
public class VTSSetModelColliderData : VTSMessageData
{
    public VTSSetModelColliderData()
    {
        this.messageType = "XYSetModelColliderRequest";
        this.data = new Data();
    }
    public Data data;

    [System.Serializable]
    public class Data
    {
        public bool Enable;
        public float Radius;
        public float OffsetX;
        public float OffsetY;

        public static Data CreateDefault()
        {
            Data data = new Data();
            data.Enable = true;
            data.Radius = 8;
            data.OffsetX = 0;
            data.OffsetY = 25;
            return data;
        }
    }
}
