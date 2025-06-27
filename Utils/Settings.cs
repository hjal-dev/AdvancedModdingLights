using System.Collections.Generic;
using BepInEx.Configuration;
using SPT_AdvancedModdingLights.Patches;

namespace SPT_AdvancedModdingLights.Utils
{
    internal class Settings
    {
        private const string GeneralSection = "General";
        private const string DirectionalLightSection = "Directional Light";
        
        public static ConfigEntry<bool> EnableModdingLights { get; set; }
        public static ConfigEntry<float> DirectionalLightBrightness { get; set; }
        public static ConfigEntry<float> DirectionalLightRotationX { get; set; }
        public static ConfigEntry<float> DirectionalLightRotationY { get; set; }
        
        public static void Init(ConfigFile config)
        {
            var configEntries = new List<ConfigEntryBase>();
            
            configEntries.Add(EnableModdingLights = config.Bind(
                GeneralSection,
                "Enable Custom Lighting",
                true,
                new ConfigDescription(
                    "Enables enhanced lighting in weapon modding and build screens",
                    null,
                    new ConfigurationManagerAttributes { Order = 10 })
            ));
            
            configEntries.Add(DirectionalLightBrightness = config.Bind(
                DirectionalLightSection,
                "Brightness",
                0.85f,
                new ConfigDescription(
                    "Controls the brightness/intensity of the directional light (0.1 to 5.0)",
                    new AcceptableValueRange<float>(0.1f, 5.0f),
                    new ConfigurationManagerAttributes { Order = 9 })
            ));
            
            configEntries.Add(DirectionalLightRotationX = config.Bind(
                DirectionalLightSection,
                "Rotation X",
                0f,
                new ConfigDescription(
                    "X-axis rotation of the directional light in degrees (-180 to 180)",
                    new AcceptableValueRange<float>(-180f, 180f),
                    new ConfigurationManagerAttributes { Order = 8 })
            ));
            
            configEntries.Add(DirectionalLightRotationY = config.Bind(
                DirectionalLightSection,
                "Rotation Y",
                0f,
                new ConfigDescription(
                    "Y-axis rotation of the directional light in degrees (-180 to 180)",
                    new AcceptableValueRange<float>(-180f, 180f),
                    new ConfigurationManagerAttributes { Order = 7 })
            ));
            
            EnableModdingLights.SettingChanged += OnSettingChanged;
            DirectionalLightBrightness.SettingChanged += OnSettingChanged;
            DirectionalLightRotationX.SettingChanged += OnSettingChanged;
            DirectionalLightRotationY.SettingChanged += OnSettingChanged;
            
            WeaponModdingPatch.ApplySettings();
        }
        
        private static void OnSettingChanged(object sender, System.EventArgs e)
        {
            WeaponModdingPatch.ApplySettings();
        }
    }
}