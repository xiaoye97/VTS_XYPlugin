using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VTS.Models;

[System.Serializable]
public class VTSDropItemData : VTSMessageData
{
    public VTSDropItemData()
    {
        this.messageType = "XYDropItemRequest";
        this.data = new Data();
    }
    public Data data;

    [System.Serializable]
    public class Data
    {
        public string ImageName;
        public int DropCount;
    }
}
