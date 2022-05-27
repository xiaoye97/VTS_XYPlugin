// By 宵夜97
using UnityEngine;
using System.IO;

public static class XYPaths
{
    public static string XYDirPath
    {
        get
        {
#if UNITY_EDITOR
            string path = $"C:/Program Files (x86)/Steam/steamapps/common/VTube Studio/XYPluginConfig";
#else
            string path = $"{Application.dataPath}/../../../../../XYPluginConfig";
#endif
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }
    }

    public static string DropItemImageDirPath
    {
        get
        {
            string path = $"{XYDirPath}/DropItem";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }
    }

    public static string GlobalConfigPath
    {
        get
        {
            string path = $"{XYDirPath}/GlobalConfig.json";
            return path;
        }
    }

    public static string DropItemConfigPath
    {
        get
        {
            string path = $"{DropItemImageDirPath}/DropItemConfig.json";
            return path;
        }
    }
}