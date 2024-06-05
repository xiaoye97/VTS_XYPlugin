namespace VTS_XYPlugin_Common
{
    public static class StringHelper
    {
        public static int ToInt(this string str)
        {
            int result = 0;
            int.TryParse(str, out result);
            return result;
        }

        public static float ToFloat(this string str)
        {
            float result = 0;
            float.TryParse(str, out result);
            return result;
        }

        public static bool ToBool(this string str)
        {
            bool result = false;
            bool.TryParse(str, out result);
            return result;
        }

        public static BJianDuiType ToJianDuiType(this string str)
        {
            int J = str.ToInt();
            return J.ToJianDuiType();
        }

        public static BJianDuiType ToJianDuiType(this int level)
        {
            BJianDuiType result = BJianDuiType.无;
            if (level == 1) result = BJianDuiType.总督;
            if (level == 2) result = BJianDuiType.提督;
            if (level == 3) result = BJianDuiType.舰长;
            return result;
        }

        public static BGiftCoinType ToGiftCoinType(this string str)
        {
            BGiftCoinType result = BGiftCoinType.银瓜子;
            if (str == "gold") result = BGiftCoinType.金瓜子;
            return result;
        }
    }
}