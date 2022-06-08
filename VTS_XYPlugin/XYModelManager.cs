using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VTS_XYPlugin_Common;

namespace VTS_XYPlugin
{
    public class XYModelManager : MonoSingleton<XYModelManager>
    {
        private VTubeStudioModelLoader modelLoader;
        public VTubeStudioModel NowModel;
        public ModelDefinitionJSON NowModelDef;
        public XYModelConfig NowModelConfig;
        public XYFileWatcher modelConfigWatcher;
        public XYModelBehaviour XYModelBehaviour;
        public GameObject ModelRoot;
        public Dictionary<string, XYCustomBehaviour> CustomBehaviours = new Dictionary<string, XYCustomBehaviour>();

        private void Update()
        {
            if (modelConfigWatcher != null)
            {
                modelConfigWatcher.Update(Time.deltaTime);
            }
        }

        public override void Init()
        {
            ModelRoot = GameObject.Find("Live2DModel/ModelScaleTranslateRotate/ModeRotationPivot");
            modelLoader = GameObject.FindObjectOfType<VTubeStudioModelLoader>();
            if (modelLoader == null)
            {
                XYLog.LogError("未找到模型加载器");
                return;
            }
            modelLoader.modelLoadStarted.AddListener(OnModelLoadStarted);
            modelLoader.modelLoadingFinished.AddListener(OnModelLoadingFinished);
        }

        /// <summary>
        /// 重载模型自定义脚本
        /// </summary>
        public void ReloadXYCustomBehaviour()
        {
            // 销毁已加载的自定义脚本
            List<string> keys = new List<string>();
            foreach (var key in CustomBehaviours.Keys)
            {
                keys.Add(key);
            }
            for (int i = 0; i < keys.Count; i++)
            {
                GameObject.Destroy(CustomBehaviours[keys[i]]);
            }
            CustomBehaviours.Clear();
            if (NowModelDef == null) return;
            // 查找扩展脚本程序集，只有写明了扩展脚本信息的程序集才会参与搜索
            // 从目标程序集中找到继承于XYCustomBehaviour的类
            List<Type> customBehaviourTypeList = new List<Type>();
            foreach (var assembly in XYExScriptManager.Instance.AllAssemblies)
            {
                var types = assembly.GetTypes().Where(t => t.BaseType == typeof(XYCustomBehaviour));
                customBehaviourTypeList.AddRange(types);
            }
            XYLog.LogMessage($"查找到{customBehaviourTypeList.Count}个自定义脚本");
            // 移除场景内已有的自定义脚本并添加新脚本
            foreach (var type in customBehaviourTypeList)
            {
                XYLog.LogMessage($"处理类型 {type.Name} ...");
                bool needAdd = true;
                var attrs = type.GetCustomAttributes(false);
                List<BindModelAttribute> binds = new List<BindModelAttribute>();
                foreach (var obj in attrs)
                {
                    // 如果当前类型含有绑定模型特性，则检查当前模型是否允许被绑定
                    if (obj is BindModelAttribute)
                    {
                        binds.Add((BindModelAttribute)obj);
                        needAdd = false;
                    }
                }
                if (binds.Count > 0)
                {
                    foreach (var bind in binds)
                    {
                        if (bind.ModelName == NowModelDef.Name)
                        {
                            needAdd = true;
                        }
                    }
                }
                // 如果需要添加，则添加到当前模型上
                if (needAdd)
                {
                    var script = ModelRoot.AddComponent(type);
                    CustomBehaviours[type.Name] = (XYCustomBehaviour)script;
                }
            }
        }

        private void OnModelLoadStarted(ModelDefinitionJSON modelDef)
        {
            NowModel = null;
            NowModelConfig = null;
            NowModelDef = modelDef;
            modelConfigWatcher = null;
            XYCache.Instance.PluginCache.NowModelConfigFilePath = null;
            if (modelDef != null)
            {
                FileHelper.LoadNowModelConfig();
                string path = NowModelDef.FilePath.Replace(".vtube.json", ".xyplugin.json");
                XYCache.Instance.PluginCache.NowModelConfigFilePath = path;
                XYCache.Instance.PluginCache.HasData = true;
                modelConfigWatcher = new XYFileWatcher(path);
                modelConfigWatcher.OnFileModified += OnModelConfigFileModified;
            }
            else
            {
                XYCache.Instance.PluginCache.NowModelConfigFilePath = "";
                XYCache.Instance.PluginCache.HasData = true;
            }
            ReloadXYCustomBehaviour();
        }

        private void OnModelLoadingFinished(VTubeStudioModel model)
        {
            NowModel = model;
        }

        private void OnModelConfigFileModified()
        {
            FileHelper.LoadNowModelConfig();
        }
    }
}