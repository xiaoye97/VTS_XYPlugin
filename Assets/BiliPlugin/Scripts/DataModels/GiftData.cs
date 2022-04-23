using System.Collections;
using System.Collections.Generic;
using System;

public class GiftData
{
    public DateTime Time;
    public string UserName;
    public string GiftName;
    public int GiftCount;
    public float Money;
    public string ActionName;
    public GiftType GiftType;
    public string GiftMsg;

    public GiftData()
    {
        Time = DateTime.Now;
    }

    public GiftData(string personName, string giftName, string actionName)
    {
        Time = DateTime.Now;
        UserName = personName;
        GiftName = giftName;
        ActionName = actionName;
    }
}

public enum GiftType
{
    Gift,
    SC,
    Jian
}