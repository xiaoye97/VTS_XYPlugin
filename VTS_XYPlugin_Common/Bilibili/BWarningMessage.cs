namespace VTS_XYPlugin_Common
{
    [System.Serializable]
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