using BepInEx;
using System.IO;

namespace VTS_XYPlugin
{
    public static class XYPaths
    {
        public static string XYDirPath
        {
            get
            {
                string path = $"{Paths.GameRootPath}/XYPluginConfig";
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

        public static string GUIExePath
        {
            get
            {
                string path = $"{Paths.PluginPath}/VTS_XYPlugin/VTS_XYPluginGUI/VTS_XYPluginGUI.exe";
                return path;
            }
        }

        public static string BiliHeadDirPath
        {
            get
            {
                string path = $"{XYDirPath}/BiliHeadCache";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return path;
            }
        }

        public static string BiliHeadCacheConfigPath
        {
            get
            {
                string path = $"{BiliHeadDirPath}/HeadCacheConfig.json";
                return path;
            }
        }

        public static string AssetsDirPath
        {
            get
            {
                string path = $"{Paths.PluginPath}/VTS_XYPlugin/Assets";
                return path;
            }
        }
    }
}