using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VTS.Models;

[System.Serializable]
public class VTSRefreshDropItemData : VTSMessageData
{
    public VTSRefreshDropItemData()
    {
        this.messageType = "XYRefreshDropItemRequest";
    }
}
