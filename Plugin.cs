using BepInEx;
using BepInEx.Logging;
using SPT_AdvancedModdingLights.Utils;
using SPT_AdvancedModdingLights.Patches;

namespace SPT_AdvancedModdingLights
{
    [BepInPlugin("moxopixel.advanced.modding.lights", "MoxoPixel-AdvancedModdingLights", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource LogSource;

        private void Awake()
        {
            LogSource = Logger;
            Settings.Init(Config);
            WeaponModdingPatch.Enable();
            LogSource.LogInfo("Advanced Modding Lights loaded");
        }
    }
}