namespace VTS_XYPlugin_Common
{
    [System.Serializable]
    public class BWatchPeopleMessage : XYMessage
    {
        public int 看过人数;

        public BWatchPeopleMessage() : base()
        {
        }

        public BWatchPeopleMessage(string[] data) : base()
        {
            看过人数 = data[1].ToInt();
        }
    }
}