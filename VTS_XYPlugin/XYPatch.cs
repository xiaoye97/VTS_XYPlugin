using HarmonyLib;

namespace VTS_XYPlugin
{
    public static class XYPatch
    {
        [HarmonyPostfix, HarmonyPatch(typeof(SceneItem), "Initialize")]
        public static void SceneItemInitializePatch(SceneItem __instance)
        {
            XYLog.LogMessage($"初始化了场景物品:{__instance.ItemInfo.ItemName}");
            __instance.gameObject.AddComponent<SceneItemBehaviour>();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(SceneItem), "ToggleLock")]
        public static void SceneItemToggleLockPatch(SceneItem __instance)
        {
            // 用于防止礼物碰撞体在锁定的状态下也移动
            __instance.GestureTransformer.enabled = !__instance.ItemInfo.IsLocked;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(HotkeyConfigItem), "ModelConfigChanged")]
        public static void HotkeyConfigItemModelConfigChanged()
        {
            XYCache.Instance.PluginCache.HasData = true;
        }
    }
}