using System;
using System.IO;
using UnityEngine;
using VTS_XYPlugin;
using Newtonsoft.Json;
using Live2D.Cubism.Core;
using VTS_XYPlugin_Common;
using System.Collections.Generic;

namespace VTS_TimeParameter
{
    public class TimeParameterBehaviour : XYCustomBehaviour
    {
        public static TimeParameterBehaviour Inst;
        private bool inited;
        public List<TimeParameterConfig> configs = new List<TimeParameterConfig>();

        public Dictionary<string, object> LuaParameters = new Dictionary<string, object>();
        public static Dictionary<CubismParameter, float> ParameterDict = new Dictionary<CubismParameter, float>();

        public void Start()
        {
            Inst = this;
            if (XYModelManager.Instance.NowModelDef != null)
            {
                Init(XYModelManager.Instance.NowModelDef);
            }
            else
            {
                XYLog.LogMessage("当前没有模型，TimeParameter不进行初始化");
            }
        }

        public void Update()
        {
            if (!inited) return;
            if (XYModelManager.Instance.NowModel == null) return;
            SetLuaParameters();
            ParameterDict.Clear();
            foreach (var config in configs)
            {
                var value = XYLuaHelper.RunLua($"return {config.Expression}", LuaParameters);
                //Debug.Log(value);
                CubismParameter cubismParameter;
                bool hasParam = XYModelManager.Instance.NowModel.NamedParams.TryGetValue(config.Parameter, out cubismParameter);
                if (hasParam)
                {
                    ParameterDict[cubismParameter] = (float)value;
                }
            }
        }

        public void SetLuaParameters()
        {
            LuaParameters["GameTime"] = Time.time;
            DateTime now =  DateTime.Now;
            LuaParameters["Year"] = now.Year;
            LuaParameters["Month"] = now.Month;
            LuaParameters["Day"] = now.Day;
            LuaParameters["Hour"] = now.Hour;
            LuaParameters["Minute"] = now.Minute;
            LuaParameters["Second"] = now.Second;
            LuaParameters["Millisecond"] = now.Millisecond;
        }

        public void Init(ModelDefinitionJSON modelDef)
        {
            LoadConfig();
            inited = true;
        }

        public void LoadConfig()
        {
            configs = new List<TimeParameterConfig>();
            string path = XYModelManager.Instance.NowModelDef.FilePath.Replace(".vtube.json", ".TimeParameter.json");
            FileInfo file = new FileInfo(path);
            if (file.Exists)
            {
                string json = FileHelper.ReadAllText(file.FullName);
                try
                {
                    var con = JsonConvert.DeserializeObject<List<TimeParameterConfig>>(json);
                    configs = con;
                }
                catch (Exception ex)
                {
                    XYLog.LogError($"TimeParameter解析配置文件异常 {ex}");
                }
            }
            else
            {
                //TimeParameterConfig config = new TimeParameterConfig();
                //config.Parameter = "FaceAngleX";
                //config.Expression = "GameTime";
                //configs.Add(config);
                //var json = JsonConvert.SerializeObject(configs, Formatting.Indented);
                //FileHelper.WriteAllText(path, json);
            }
        }
    }
}
