using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;
using SPT_AdvancedModdingLights.Utils;
using EFT.UI;
using EFT.UI.WeaponModding;
using HarmonyLib;
using System.Collections.Generic;

namespace SPT_AdvancedModdingLights.Patches
{
    public static class WeaponModdingPatch
    {
        private static readonly Dictionary<Transform, (bool Active, float Intensity, LightShadows Shadows, float Range, Vector3 Rotation)> 
            originalLightSettings = new Dictionary<Transform, (bool, float, LightShadows, float, Vector3)>();
        
        private static readonly HashSet<GameObject> activeScreens = new HashSet<GameObject>();
        
        public static void Enable()
        {
            new EditBuildScreenAwakePatch().Enable();
            new WeaponModdingScreenAwakePatch().Enable();
            new EditBuildScreenDestroyPatch().Enable();
            new WeaponModdingScreenDestroyPatch().Enable();
        }

        public static void ApplySettings()
        {
            int processedCount = 0;
            var screensToProcess = new List<GameObject>(activeScreens);
            
            foreach (var screen in screensToProcess)
            {
                if (screen != null)
                {
                    if (screen.gameObject == null)
                    {
                        activeScreens.Remove(screen);
                        continue;
                    }
                    
                    HandleScreen(screen, GetScreenType(screen));
                    processedCount++;
                }
                else
                {
                    activeScreens.Remove(screen);
                }
            }
        }
        
        private static string GetScreenType(GameObject screen)
        {
            if (screen.GetComponent<EditBuildScreen>() != null)
                return "EditBuildScreen";
            if (screen.GetComponent<WeaponModdingScreen>() != null)
                return "WeaponModdingScreen";
            return "Unknown";
        }
        
        public static void RegisterActiveScreen(GameObject screen, string screenType)
        {
            if (activeScreens.Add(screen))
            {
                HandleScreen(screen, screenType);
            }
        }
        
        public static void UnregisterActiveScreen(GameObject screen, string screenType)
        {
            if (activeScreens.Remove(screen))
            {
                var keysToRemove = new List<(GameObject, string)>();
                foreach (var key in lightCache.Keys)
                {
                    if (key.Item1 == screen)
                        keysToRemove.Add(key);
                }
                foreach (var key in keysToRemove)
                {
                    lightCache.Remove(key);
                }
            }
        }
        
        private static void HandleScreen(GameObject screen, string screenType)
        {
            if (screen == null) return;
            
            ApplyLightsToScreen(screen, screenType);
        }
        
        private static void ApplyLightsToScreen(GameObject screen, string screenType)
        {
            try
            {
                if (Settings.EnableModdingLights.Value)
                    EnableLights(screen);
                else
                    DisableLights(screen);
            }
            catch (System.Exception ex)
            {
                Plugin.LogSource.LogError($"Error applying lights to {screenType}: {ex.Message}");
            }
        }

        #region Light Handling
        
        private static void EnableLights(GameObject screen)
        {
            if (screen == null) return;
            
            var dirLight = FindLight(screen, "Directional light");
            if (dirLight != null)
            {
                StoreOriginalSettings(dirLight);
                dirLight.gameObject.SetActive(true);
                
                var light = dirLight.GetComponent<Light>();
                if (light != null)
                {
                    light.intensity = Settings.DirectionalLightBrightness.Value;
                }
                
                var rotationX = Settings.DirectionalLightRotationX.Value;
                var rotationY = Settings.DirectionalLightRotationY.Value;
                dirLight.rotation = Quaternion.Euler(rotationX, rotationY, 0f);
            }
            
            var lights = FindLight(screen, "Lights");
            if (lights != null)
            {
                foreach (Transform child in lights)
                {
                    StoreOriginalSettings(child);
                    UpdateLight(child);
                }
            }
        }
        
        private static void DisableLights(GameObject screen)
        {
            if (screen == null) return;
            
            var dirLight = FindLight(screen, "Directional light");
            if (dirLight != null)
            {
                RestoreOriginalSettings(dirLight);
            }
                
            var lights = FindLight(screen, "Lights");
            if (lights != null)
            {
                foreach (Transform child in lights)
                {
                    RestoreOriginalSettings(child);
                }
            }
        }
        
        private static Transform FindLight(GameObject screen, string name)
        {
            var light = screen.transform.Find($"Preview Panel/{name}");
            if (light != null) return light;
            
            light = screen.transform.Find(name);
            if (light != null) return light;
            
            return FindInChildren(screen.transform, name);
        }
        
        private static readonly Dictionary<(GameObject, string), Transform> lightCache = 
            new Dictionary<(GameObject, string), Transform>();
            
        private static Transform FindInChildren(Transform parent, string name)
        {
            var cacheKey = (parent.gameObject, name);
            if (lightCache.TryGetValue(cacheKey, out var cached))
            {
                return cached != null && cached.gameObject != null ? cached : null;
            }
            
            var result = FindInChildrenRecursive(parent, name);
            lightCache[cacheKey] = result;
            return result;
        }
        
        private static Transform FindInChildrenRecursive(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                    return child;
                    
                var found = FindInChildrenRecursive(child, name);
                if (found != null)
                    return found;
            }
            return null;
        }
        
        private static void StoreOriginalSettings(Transform lightTransform)
        {
            if (!originalLightSettings.ContainsKey(lightTransform))
            {
                var light = lightTransform.GetComponent<Light>();
                if (light != null)
                {
                    originalLightSettings[lightTransform] = (
                        lightTransform.gameObject.activeSelf,
                        light.intensity,
                        light.shadows,
                        light.range,
                        lightTransform.rotation.eulerAngles
                    );
                }
            }
        }
        
        private static void RestoreOriginalSettings(Transform lightTransform)
        {
            if (originalLightSettings.TryGetValue(lightTransform, out var settings))
            {
                lightTransform.gameObject.SetActive(settings.Active);
                var light = lightTransform.GetComponent<Light>();
                if (light != null)
                {
                    light.intensity = settings.Intensity;
                    light.shadows = settings.Shadows;
                    light.range = settings.Range;
                }
                lightTransform.rotation = Quaternion.Euler(settings.Rotation);
            }
        }
        
        private static void UpdateLight(Transform light)
        {
            var lights = light.GetComponentsInChildren<Light>();
            switch (light.name)
            {
                case "Cold Light (9)":
                    foreach (var l in lights)
                        l.shadows = LightShadows.None;
                    break;
                    
                case "AngleLight_2":
                    foreach (var l in lights)
                    {
                        l.shadows = LightShadows.None;
                        l.intensity = 1f;
                        l.range = 2f;
                    }
                    break;
                    
                case "Cold Light (4)":
                    foreach (var l in lights)
                    {
                        l.shadows = LightShadows.None;
                        l.intensity = 1.7f;
                    }
                    break;
                    
                case "Cold Light (6)":
                    foreach (var l in lights)
                    {
                        l.shadows = LightShadows.None;
                        l.intensity = 0.5f;
                        l.range = 0.8f;
                    }
                    break;
                    
                default:
                    light.gameObject.SetActive(false);
                    break;
            }
        }
        
        #endregion
        
        #region Patch Classes
        
        internal class EditBuildScreenAwakePatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(EditBuildScreen), nameof(EditBuildScreen.Awake));
            }

            [PatchPostfix]
            public static void Postfix(EditBuildScreen __instance)
            {
                RegisterActiveScreen(__instance.gameObject, "EditBuildScreen");
            }
        }

        internal class WeaponModdingScreenAwakePatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(WeaponModdingScreen), nameof(WeaponModdingScreen.Awake));
            }

            [PatchPostfix]
            public static void Postfix(WeaponModdingScreen __instance)
            {
                RegisterActiveScreen(__instance.gameObject, "WeaponModdingScreen");
            }
        }

        internal class EditBuildScreenDestroyPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(EditBuildScreen), nameof(EditBuildScreen.OnDestroy));
            }

            [PatchPrefix]
            public static void Prefix(EditBuildScreen __instance)
            {
                UnregisterActiveScreen(__instance.gameObject, "EditBuildScreen");
            }
        }

        internal class WeaponModdingScreenDestroyPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(WeaponModdingScreen), nameof(WeaponModdingScreen.OnDestroy));
            }

            [PatchPrefix]
            public static void Prefix(WeaponModdingScreen __instance)
            {
                UnregisterActiveScreen(__instance.gameObject, "WeaponModdingScreen");
            }
        }

        #endregion
    }
}