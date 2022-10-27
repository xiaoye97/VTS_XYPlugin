using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VTS_XYPlugin_Common;

namespace VTS_XYPlugin
{
    public static class FileHelper
    {
        public static string ReadAllText(string path)
        {
            try
            {
                return File.ReadAllText(path);
            }
            catch (Exception ex)
            {
                XYLog.LogError($"读取文件异常，路径:{path}，异常:{ex}");
            }
            return "";
        }

        public static void WriteAllText(string path, string data)
        {
            try
            {
                FileInfo info = new FileInfo(path);
                if (!info.Directory.Exists)
                {
                    info.Directory.Create();
                }
                File.WriteAllText(path, data);
            }
            catch (Exception ex)
            {
                XYLog.LogError($"写入文件异常，路径:{path}，异常:{ex}");
            }
        }

        public static void LoadGlobalConfig()
        {
            try
            {
                string path = XYPaths.GlobalConfigPath;
                XYLog.LogMessage($"加载全局配置文件:{path}");
                if (File.Exists(path))
                {
                    string json = FileHelper.ReadAllText(path);
                    XYPlugin.Instance.GlobalConfig = JsonConvert.DeserializeObject<XYGlobalConfig>(json);
                }
                if (XYPlugin.Instance.GlobalConfig == null)
                {
                    XYPlugin.Instance.GlobalConfig = new XYGlobalConfig();
                    SaveGlobalConfig();
                }
            }
            catch (Exception ex)
            {
                XYLog.LogError($"{ex}");
            }
        }

        public static void SaveGlobalConfig()
        {
            try
            {
                XYLog.LogMessage("开始保存GlobalConfig");
                if (XYPlugin.Instance.GlobalConfig == null)
                {
                    XYLog.LogWarning($"GlobalConfig为空，无法保存");
                    return;
                }
                string path = XYPaths.GlobalConfigPath;
                string json = JsonConvert.SerializeObject(XYPlugin.Instance.GlobalConfig, Formatting.Indented);
                //XYLog.LogMessage($"保存全局配置文件:{path}\n{json}");
                XYPlugin.Instance.GlobalConfigWatcher.IgnoreOnceModify = true;
                WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                XYLog.LogError($"{ex}");
            }
        }

        public static void LoadDropItemConfig()
        {
            try
            {
                string path = XYPaths.DropItemConfigPath;
                XYLog.LogMessage($"加载掉落物配置文件:{path}");
                if (File.Exists(path))
                {
                    string json = FileHelper.ReadAllText(path);
                    XYPlugin.Instance.DropItemDataBase = JsonConvert.DeserializeObject<DropItemDataBase>(json);
                }
                if (XYPlugin.Instance.DropItemDataBase == null)
                {
                    XYPlugin.Instance.DropItemDataBase = new DropItemDataBase();
                    SaveDropItemConfig();
                }
            }
            catch (Exception ex)
            {
                XYLog.LogError($"{ex}");
            }
        }

        public static void SaveDropItemConfig()
        {
            try
            {
                XYLog.LogMessage("开始保存DropItemDataBase");
                if (XYPlugin.Instance.DropItemDataBase == null)
                {
                    XYLog.LogWarning($"DropItemDataBase为空，无法保存");
                    return;
                }
                string path = XYPaths.DropItemConfigPath;
                string json = JsonConvert.SerializeObject(XYPlugin.Instance.DropItemDataBase, Formatting.Indented);
                //XYPlugin.Instance.DropItemDataBaseWatcher.IgnoreOnceModify = true;
                WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                XYLog.LogError($"{ex}");
            }
        }

        public static void LoadNowModelConfig()
        {
            try
            {
                // 如果当前没有加载模型，则直接忽略
                if (XYModelManager.Instance.NowModelDef == null)
                {
                    return;
                }
                string configFilePath = XYModelManager.Instance.NowModelDef.FilePath.Replace(".vtube.json", ".xyplugin.json");
                XYLog.LogMessage($"加载模型配置文件:{configFilePath}");
                if (File.Exists(configFilePath))
                {
                    string json = FileHelper.ReadAllText(configFilePath);
                    XYModelManager.Instance.NowModelConfig = JsonConvert.DeserializeObject<XYModelConfig>(json);
                }
                if (XYModelManager.Instance.NowModelConfig == null)
                {
                    XYModelManager.Instance.NowModelConfig = new XYModelConfig();
                    SaveNowModelConfig();
                }
                for (int i = 0; i < XYModelManager.Instance.NowModelConfig.TriggerActionData.Count; i++)
                {
                    XYModelManager.Instance.NowModelConfig.TriggerActionData[i].ID = i;
                }
                XYHotkeyManager.Instance.ClearAllCD();
            }
            catch (Exception ex)
            {
                XYLog.LogError($"{ex}");
            }
        }

        public static void SaveNowModelConfig()
        {
            try
            {
                XYLog.LogMessage("开始保存模型配置");
                // 如果当前没有加载模型，则直接忽略
                if (XYModelManager.Instance.NowModelDef == null || XYModelManager.Instance.NowModelConfig == null)
                {
                    XYLog.LogWarning($"模型配置为空，无法保存");
                    return;
                }
                string configFilePath = XYModelManager.Instance.NowModelDef.FilePath.Replace(".vtube.json", ".xyplugin.json");
                string json = JsonConvert.SerializeObject(XYModelManager.Instance.NowModelConfig, Formatting.Indented);
                if (XYModelManager.Instance.modelConfigWatcher != null)
                {
                    XYModelManager.Instance.modelConfigWatcher.IgnoreOnceModify = true;
                }
                XYLog.LogMessage($"保存模型配置文件:{configFilePath}\n{json}");
                WriteAllText(configFilePath, json);
            }
            catch (Exception ex)
            {
                XYLog.LogError($"{ex}");
            }
        }

        public static void LoadBiliHeadCache()
        {
            try
            {
                string path = XYPaths.BiliHeadCacheConfigPath;
                XYLog.LogMessage($"加载B站头像缓存配置文件:{path}");
                if (File.Exists(path))
                {
                    string json = FileHelper.ReadAllText(path);
                    BilibiliHeadCache.Instance.HeadLinkDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                }
                if (BilibiliHeadCache.Instance.HeadLinkDict == null)
                {
                    BilibiliHeadCache.Instance.HeadLinkDict = new Dictionary<string, string>();
                    SaveBiliHeadCache();
                }
            }
            catch (Exception ex)
            {
                XYLog.LogError($"{ex}");
            }
        }

        public static void SaveBiliHeadCache()
        {
            try
            {
                XYLog.LogMessage("开始保存BiliHeadCache");
                if (BilibiliHeadCache.Instance.HeadLinkDict == null)
                {
                    XYLog.LogWarning($"HeadLinkDict为空，无法保存");
                    return;
                }
                string path = XYPaths.BiliHeadCacheConfigPath;
                string json = JsonConvert.SerializeObject(BilibiliHeadCache.Instance.HeadLinkDict, Formatting.Indented);
                WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                XYLog.LogError($"{ex}");
            }
        }

        public static Texture2D LoadTexture2D(string path)
        {
            Texture2D tex = new Texture2D(2, 2);
            var bytes = File.ReadAllBytes(path);
            tex.LoadImage(bytes);
            return tex;
        }

        public static Sprite LoadSprite(string path, bool setPerUnit = false)
        {
            Sprite sprite = null;
            Texture2D tex = LoadTexture2D(path);
            try
            {
                if (setPerUnit)
                {
                    sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), tex.width);
                }
                else
                {
                    sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                }
            }
            catch (Exception ex)
            {
                XYLog.LogError($"{ex}");
            }
            return sprite;
        }
    }
}