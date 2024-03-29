﻿using MoonSharp.Interpreter;
using System.Collections.Generic;

namespace VTS_XYPlugin
{
    public static class XYLuaHelper
    {
        /// <summary>
        /// 运行lua计算数值
        /// </summary>
        /// <param name="luaScript"></param>
        /// <returns></returns>
        public static double RunLua(string luaScript, Dictionary<string, object> globals = null)
        {
            Script script = new Script();
            if (globals != null)
            {
                foreach (var kv in globals)
                {
                    script.Globals[kv.Key] = kv.Value;
                }
            }
            DynValue res = script.DoString(luaScript);
            return res.Number;
        }
    }
}