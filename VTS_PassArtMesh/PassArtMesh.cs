using BepInEx;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using VTS_XYPlugin;
using VTS_XYPlugin_Common;
using HarmonyLib;
using Live2D.Cubism.Framework.Raycasting;

namespace VTS_PassArtMesh
{
    [BepInDependency("me.xiaoye97.plugin.VTubeStudio.VTS_XYPlugin", "2.0.0")]
    [BepInPlugin(GUID, PluginName, VERSION)]
    [ExScript(PluginName, PluginDescription, "宵夜97", VERSION)]
    public class PassArtMesh : BaseUnityPlugin
    {
        public const string GUID = "me.xiaoye97.plugin.VTubeStudio.PassArtMesh";
        public const string PluginName = "PassArtMesh[屏蔽指定ArtMesh]";
        public const string PluginDescription = "有些模型因为特效等需要，会有一个或者多个巨大的透明图层挡在最前面，导致VTS的挂件无法准确挂载到想要的身体部位上，而是挂到了透明图层上。通过此扩展可以将这些透明图层排除在挂件的挂载范围之外[需要在配置文件中设置相关数据]";
        public const string VERSION = "1.0.0";
        private static PassArtMeshConfig nowConfig;
        private static string nowControlModel = "";

        private void Start()
        {
            Harmony.CreateAndPatchAll(typeof(PassArtMesh));
        }

        private void Update()
        {
            // 如果模型为空，则清空配置
            if (XYModelManager.Instance.NowModel == null)
            {
                nowConfig = null;
                nowControlModel = "";
                return;
            }
            // 如果当前模型不为空但是配置为空，则创建配置
            if (nowConfig == null || nowControlModel != XYModelManager.Instance.NowModelDef.Name)
            {
                nowControlModel = XYModelManager.Instance.NowModelDef.Name;
                LoadConfig();
            }
        }

        public void LoadConfig()
        {
            string path = XYModelManager.Instance.NowModelDef.FilePath.Replace(".vtube.json", ".PassArtMesh.json");
            FileInfo file = new FileInfo(path);
            if (file.Exists)
            {
                string json = FileHelper.ReadAllText(file.FullName);
                try
                {
                    var con = JsonConvert.DeserializeObject<PassArtMeshConfig>(json);
                    nowConfig = con;
                }
                catch (Exception ex)
                {
                    XYLog.LogError($"PassArtMesh解析配置文件异常 {ex}");
                }
            }
            else
            {
                //PassArtMeshConfig config = new PassArtMeshConfig();
                //config.ArtMeshList = new List<string>() { "ArtMesh9999" };
                //nowConfig = config;
                //var json = JsonConvert.SerializeObject(nowConfig, Formatting.Indented);
                //FileHelper.WriteAllText(path, json);
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(SceneItem), "raycastModel")]
        public static void SceneItem_raycastModel(ref List<CubismRaycastHit> __result)
        {
            if (__result.Count == 0) return;
            if (nowConfig != null && !string.IsNullOrWhiteSpace(nowControlModel))
            {
                if (nowConfig.ArtMeshList!= null &&  nowConfig.ArtMeshList.Count > 0)
                {
                    for (int i = __result.Count - 1; i >= 0; i--)
                    {
                        // 如果结果中包含屏蔽的artmesh，则在结果中删除此artmesh
                        if (nowConfig.ArtMeshList.Contains(__result[i].Drawable.name))
                        {
                            __result.RemoveAt(i);
                        }
                    }
                }
            }
        }
    }
}