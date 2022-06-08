using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace VTS_XYPlugin
{
    public static class MiscHelper
    {
        /// <summary>
        /// （批量）截取字符串中开始和结束字符串中间的字符串
        /// </summary>
        /// <param name="source">源字符串</param>
        /// <param name="startStr">开始字符串</param>
        /// <param name="endStr">结束字符串</param>
        /// <returns>中间字符串</returns>
        public static List<string> SubstringMultiple(string source, string startStr, string endStr)
        {
            Regex rg = new Regex("(?<=(" + startStr + "))[.\\s\\S]*?(?=(" + endStr + "))", RegexOptions.Multiline | RegexOptions.Singleline);

            MatchCollection matches = rg.Matches(source);

            List<string> resList = new List<string>();

            foreach (Match item in matches)
                resList.Add(item.Value);

            return resList;
        }

        /// <summary>
        /// 字符串中是否含有中文
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool HasChinese(this string str)
        {
            return Regex.IsMatch(str, @"[\u4e00-\u9fa5]");
        }

        /// <summary>
        /// 在所有程序集查找类的所有子类
        /// </summary>
        /// <param name="baseClass"></param>
        /// <returns></returns>
        public static List<Type> GetAllChildClass(this Type baseClass, bool allAssemblies = true)
        {
            List<Type> typeList = new List<Type>();
            List<Assembly> all = new List<Assembly>();
            if (allAssemblies)
            {
                all.AddRange(AppDomain.CurrentDomain.GetAssemblies());
            }
            else
            {
                all.Add(baseClass.Assembly);
            }
            foreach (var assembly in all)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (baseClass.IsInterface)
                    {
                        if (type.GetInterface(baseClass.Name) != null && !type.IsAbstract)
                        {
                            typeList.Add(type);
                        }
                    }
                    else
                    {
                        if (type != baseClass && type.BaseType == baseClass)
                        {
                            typeList.Add(type);
                        }
                    }
                }
            }
            return typeList;
        }
    }
}