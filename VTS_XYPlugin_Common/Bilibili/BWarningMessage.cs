namespace VTS_XYPlugin_Common
{
    [System.Serializable]
    /// <summary>
    /// 超管警告消息数据(WIP，没有实验样本)
    /// </summary>
    public class BWarningMessage : XYMessage
    {
        public string 警告信息;

        public BWarningMessage() : base()
        {
        }

        public BWarningMessage(string[] data) : base()
        {
            警告信息 = data[1];
        }
    }
}
