using Live2D.Cubism.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VTS_XYPlugin;
using VTS_XYPlugin_Common;
using MoonSharp.Interpreter;

namespace VTS_TimeParameter
{
    public class TimeParameterBehaviour : XYCustomBehaviour
    {
        public static TimeParameterBehaviour Inst;
        private bool inited;
        public List<TimeParameterConfig> configs = new List<TimeParameterConfig>();
        public static Dictionary<CubismParameter, float> ParameterDict = new Dictionary<CubismParameter, float>();

        public Script script = new Script();

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
                DynValue result = script.DoString(config.Expression);
                CubismParameter cubismParameter;
                bool hasParam = XYModelManager.Instance.NowModel.NamedParams.TryGetValue(config.Parameter, out cubismParameter);
                if (hasParam)
                {
                    ParameterDict[cubismParameter] = (float)result.Number;
                }
            }
        }

        public void SetLuaParameters()
        {
            script.Globals["GameTime"] = Time.time;
            DateTime now = DateTime.Now;
            script.Globals["Year"] = now.Year;
            script.Globals["Month"] = now.Month;
            script.Globals["Day"] = now.Day;
            script.Globals["Hour"] = now.Hour;
            script.Globals["Minute"] = now.Minute;
            script.Globals["Second"] = now.Second;
            script.Globals["Millisecond"] = now.Millisecond;
            if (now.DayOfWeek == DayOfWeek.Sunday)
            {
                script.Globals["DayOfWeek"] = 7;
            }
            else
            {
                script.Globals["DayOfWeek"] = (int)now.DayOfWeek;
            }
            script.Globals["DayOfYear"] = now.DayOfYear;
            script.Globals["TimeOfDayTotalMilliseconds"] = now.TimeOfDay.TotalMilliseconds;
            script.Globals["TimeOfDayTotalSeconds"] = now.TimeOfDay.TotalSeconds;
            script.Globals["TimeOfDayTotalMinutes"] = now.TimeOfDay.TotalMinutes;
            script.Globals["TimeOfDayTotalHours"] = now.TimeOfDay.TotalHours;
            script.Globals["TimeOfDayTotalDays"] = now.TimeOfDay.TotalDays;
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
                    configs = JsonConvert.DeserializeObject<List<TimeParameterConfig>>(json);
                    foreach (var config in configs)
                    {
                        config.Expression = $"return {config.Expression}";
                    }
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