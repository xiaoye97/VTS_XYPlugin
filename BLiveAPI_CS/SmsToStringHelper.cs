using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLiveAPI
{
    public static class SmsToStringHelper
    {
        public static string DANMU_MSG(string uid, JObject sms)
        {
            var 弹幕内容 = sms["info"][1].ToString();
            var 用户UID = sms["info"][2][0].ToString();
            var 用户名 = sms["info"][2][1].ToString();
            var 是否房管 = sms["info"][2][2].ToString() == "1"; // 不包括主播
            var 是否主播 = uid == 用户UID;
            int 当前佩戴牌子等级;
            string 当前佩戴牌子名;
            if (sms["info"][3].HasValues)
            {
                当前佩戴牌子等级 = sms["info"][3][0].ToObject<int>();
                当前佩戴牌子名 = sms["info"][3][1].ToString();
            }
            else
            {
                当前佩戴牌子等级 = 0;
                当前佩戴牌子名 = "_";
            }
            var 舰长类型 = sms["info"][7].ToObject<int>(); // 0无舰长 1总督 2提督 3舰长
            var protoData = Convert.FromBase64String(sms["dm_v2"].ToString());
            var 头像链接 = Encoding.UTF8.GetString(BLiveApi.GetChildFromProtoData(BLiveApi.GetChildFromProtoData(protoData, 20), 4));
            return $"D$#**#${用户UID}$#**#${用户名}$#**#${是否房管}$#**#${舰长类型}$#**#${当前佩戴牌子名}$#**#${当前佩戴牌子等级}$#**#${弹幕内容}";
        }

        public static string SEND_GIFT(JObject sms)
        {
            var 礼物名 = sms["data"]["giftName"].ToString();
            var 用户名 = sms["data"]["uname"].ToString();
            var 用户UID = sms["data"]["uid"].ToString();
            // Giftrcost = obj["data"]["rcost"].ToString();
            var 礼物数量 = sms["data"]["num"].ToObject<int>();
            var 瓜子类型 = sms["data"]["coin_type"].ToString();
            // 此数值单位为瓜子
            var 瓜子数量 = sms["data"]["total_coin"].ToObject<int>();
            var 头像链接 = sms["data"]["face"].ToString();
            return $"G$#**#${用户UID}$#**#${用户名}$#**#${礼物名}$#**#${礼物数量}$#**#${瓜子类型}$#**#${瓜子数量}$#**#${头像链接}";
        }

        public static string GUARD_BUY(JObject sms)
        {
            var UserID = sms["data"]["uid"].ToString();
            var UserName = sms["data"]["username"].ToString();
            var UserGuardLevel = sms["data"]["guard_level"].ToObject<int>();
            var GiftName = UserGuardLevel == 3 ? "舰长" :
                UserGuardLevel == 2 ? "提督" :
                UserGuardLevel == 1 ? "总督" : "";
            var GiftCount = sms["data"]["num"].ToObject<int>();
            return $"J$#**#${UserID}$#**#${UserName}$#**#${UserGuardLevel}$#**#${GiftName}$#**#${GiftCount}"; 
        }

        public static string SUPER_CHAT_MESSAGE(JObject sms)
        {
            var CommentText = sms["data"]["message"]?.ToString();
            var UserID = sms["data"]["uid"].ToString();
            var UserName = sms["data"]["user_info"]["uname"].ToString();
            var Price = sms["data"]["price"].ToObject<decimal>();
            var SCKeepTime = sms["data"]["time"].ToObject<int>();
            return $"S$#**#${UserID}$#**#${UserName}$#**#${Price}$#**#${SCKeepTime}$#**#${CommentText}";
        }

        public static string WATCHED_CHANGE(JObject sms)
        {
            var WatchedCount = sms["data"]["num"].ToObject<int>();
            return $"P$#**#${WatchedCount}";
        }

        public static string WARNING(JObject sms)
        {
            var CommentText = sms["msg"]?.ToString();
            return $"W$#**#${CommentText}";
        }
    }
}
