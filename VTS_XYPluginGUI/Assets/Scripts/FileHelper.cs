// By 宵夜97
using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using VTS_XYPlugin_Common;
using ThreeDISevenZeroR.UnityGifDecoder;

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
            Debug.LogError($"读取文件异常，路径:{path}，异常:{ex}");
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
            Debug.LogError($"写入文件异常，路径:{path}，异常:{ex}");
        }
    }

    public static void LoadGlobalConfig()
    {
        try
        {
            string path = XYPaths.GlobalConfigPath;
            Debug.Log($"加载全局配置文件:{path}");
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
            Debug.LogError($"{ex}");
        }
    }

    public static void SaveGlobalConfig()
    {
        try
        {
            Debug.Log("开始保存GlobalConfig");
            if (XYPlugin.Instance.GlobalConfig == null)
            {
                Debug.LogWarning($"GlobalConfig为空，无法保存");
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
            Debug.LogError($"{ex}");
        }
    }

    public static void LoadDropItemConfig()
    {
        try
        {
            string path = XYPaths.DropItemConfigPath;
            Debug.Log($"加载掉落物配置文件:{path}");
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
            Debug.LogError($"{ex}");
        }
    }

    public static void SaveDropItemConfig()
    {
        try
        {
            Debug.Log("开始保存DropItemDataBase");
            if (XYPlugin.Instance.DropItemDataBase == null)
            {
                Debug.LogWarning($"DropItemDataBase为空，无法保存");
                return;
            }
            string path = XYPaths.DropItemConfigPath;
            string json = JsonConvert.SerializeObject(XYPlugin.Instance.DropItemDataBase, Formatting.Indented);
            XYPlugin.Instance.DropItemDataBaseWatcher.IgnoreOnceModify = true;
            WriteAllText(path, json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"{ex}");
        }
    }

    public static void LoadNowModelConfig()
    {
        try
        {
            // 如果当前没有加载模型，则直接忽略
            if (string.IsNullOrWhiteSpace(XYCache.Instance.Cache.NowModelConfigFilePath))
            {
                XYPlugin.Instance.NowModelConfig = null;
                return;
            }
            string configFilePath = XYCache.Instance.Cache.NowModelConfigFilePath;
            Debug.Log($"加载模型配置文件:{configFilePath}");
            if (File.Exists(configFilePath))
            {
                string json = FileHelper.ReadAllText(configFilePath);
                XYPlugin.Instance.NowModelConfig = JsonConvert.DeserializeObject<XYModelConfig>(json);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"{ex}");
        }
    }

    public static void SaveNowModelConfig()
    {
        try
        {
            Debug.Log("开始保存模型配置");
            // 如果当前没有加载模型，则直接忽略
            if (XYPlugin.Instance.NowModelConfig == null || string.IsNullOrWhiteSpace(XYCache.Instance.Cache.NowModelConfigFilePath))
            {
                Debug.LogError($"模型配置为空，无法保存");
                return;
            }
            string configFilePath = XYCache.Instance.Cache.NowModelConfigFilePath;
            string json = JsonConvert.SerializeObject(XYPlugin.Instance.NowModelConfig, Formatting.Indented);
            Debug.Log($"保存模型配置文件:{configFilePath}\n{json}");
            WriteAllText(configFilePath, json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"{ex}");
        }
    }

    public static Texture2D LoadTexture2D(string path)
    {
        Texture2D tex = new Texture2D(2, 2);
        try
        {
            var bytes = File.ReadAllBytes(path);
            tex.LoadImage(bytes);
        }
        catch (Exception ex)
        {
            Debug.LogError($"{ex}");
        }
        return tex;
    }

    public static Sprite LoadSprite(string path)
    {
        Sprite sprite = null;
        Texture2D tex = LoadTexture2D(path);
        try
        {
            sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
        catch (Exception ex)
        {
            Debug.LogError($"{ex}");
        }
        return sprite;
    }

    public static Sprite LoadGifFirstFrame(string path)
    {
        try
        {
            using (var gifStream = new GifStream(path))
            {
                while (gifStream.HasMoreData)
                {
                    if (gifStream.CurrentToken == GifStream.Token.Image)
                    {
                        var image = gifStream.ReadImage();
                        var frame = new Texture2D(gifStream.Header.width, gifStream.Header.height, TextureFormat.ARGB32, false);
                        frame.SetPixels32(image.colors);
                        frame.Apply();
                        var sprite = Sprite.Create(frame, new Rect(0, 0, frame.width, frame.height), new Vector2(0.5f, 0.5f));
                        return sprite;
                    }
                    else
                    {
                        gifStream.SkipToken();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"{ex}");
        }
        return null;
    }
}