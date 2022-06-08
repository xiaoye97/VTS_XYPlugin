using HarmonyLib;

namespace VTS_XYPlugin
{
    public static class XYPatch
    {
        [HarmonyPostfix, HarmonyPatch(typeof(SceneItem), "Initialize")]
        public static void SceneItemPatch(SceneItem __instance)
        {
            XYLog.LogMessage($"初始化了场景物品:{__instance.ItemInfo.ItemName}");
            __instance.gameObject.AddComponent<SceneItemBehaviour>();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(HotkeyConfigItem), "ModelConfigChanged")]
        public static void HotkeyConfigItemModelConfigChanged()
        {
            XYCache.Instance.PluginCache.HasData = true;
        }
    }
}