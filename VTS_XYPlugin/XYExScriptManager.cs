using System;
using System.Linq;
using System.Reflection;
using VTS_XYPlugin_Common;
using System.Collections.Generic;
using BepInEx;

namespace VTS_XYPlugin
{
    public class XYExScriptManager : MonoSingleton<XYExScriptManager>
    {
        public List<ExScriptAttribute> AllExScripts = new List<ExScriptAttribute>();
        public List<Assembly> AllAssemblies = new List<Assembly>();
        public override void Init()
        {
            LoadAllExScript();
        }

        public void LoadAllExScript()
        {
            try
            {
                XYLog.LogMessage($"开始搜索扩展脚本");
                var plugins = typeof(BaseUnityPlugin).GetAllChildClass();
                foreach (var plugin in plugins)
                {
                    XYLog.LogMessage($"搜索到BepInEx插件:{plugin.Name}");
                    var attrs = plugin.GetCustomAttributes(false);
                    foreach (var attr in attrs)
                    {
                        if (attr is ExScriptAttribute)
                        {
                            var script = attr as ExScriptAttribute;
                            if (script.Name == "XYPlugin")
                            {
                                AllExScripts.Insert(0, script);
                            }
                            else
                            {
                                AllExScripts.Add(script);
                            }
                            if (!AllAssemblies.Contains(plugin.Assembly))
                            {
                                AllAssemblies.Add(plugin.Assembly);
                            }
                            XYLog.LogMessage($"搜索到扩展脚本:{script.Name}");
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                XYLog.LogError($"加载扩展脚本信息出错:{ex}");
            }
        }
    }
}
