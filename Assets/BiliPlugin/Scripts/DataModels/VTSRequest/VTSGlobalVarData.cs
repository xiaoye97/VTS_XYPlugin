using VTS.Models;

[System.Serializable]
public class VTSGlobalVarData : VTSMessageData
{
    public VTSGlobalVarData()
    {
        this.messageType = "XYGlobalVarRequest";
        this.data = new Data();
    }
    public Data data;

    [System.Serializable]
    public class Data
    {
        public string Key;
        public string Json;
    }
}
