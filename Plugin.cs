using BepInEx;
using BepInEx.Logging;
using SPT_AdvancedModdingLights.Utils;
using SPT_AdvancedModdingLights.Patches;

namespace SPT_AdvancedModdingLights
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "moxopixel.advanced.modding.lights";
        public const string PluginName = "Advanced Modding Lights";
        public const string PluginVersion = "2.0.0";
        public const string PluginAuthors = "Hj, MoxoPixel";

        public static ManualLogSource LogSource;

        private void Awake()
        {
            LogSource = Logger;
            Settings.Init(Config);
            WeaponModdingPatch.Enable();
            LogSource.LogInfo($"{PluginName} {PluginVersion} by {PluginAuthors} loaded");
        }
    }
}
