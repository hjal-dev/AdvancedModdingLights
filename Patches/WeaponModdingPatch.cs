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
        }

        public static void ApplySettings()
        {
            PruneDestroyedObjects();

            List<GameObject> screensToProcess = new List<GameObject>(activeScreens);
            foreach (GameObject screen in screensToProcess)
            {
                HandleScreen(screen, GetScreenType(screen));
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
            PruneDestroyedObjects();

            if (activeScreens.Add(screen))
            {
                HandleScreen(screen, screenType);
            }
        }

        private static void PruneDestroyedObjects()
        {
            List<GameObject> deadScreens = new List<GameObject>();
            foreach (GameObject screen in activeScreens)
            {
                if (screen == null)
                    deadScreens.Add(screen);
            }
            foreach (GameObject screen in deadScreens)
            {
                activeScreens.Remove(screen);
            }

            List<(GameObject, string)> deadCacheKeys = new List<(GameObject, string)>();
            foreach (KeyValuePair<(GameObject, string), Transform> entry in lightCache)
            {
                if (entry.Key.Item1 == null || entry.Value == null)
                    deadCacheKeys.Add(entry.Key);
            }
            foreach ((GameObject, string) key in deadCacheKeys)
            {
                lightCache.Remove(key);
            }

            List<Transform> deadLights = new List<Transform>();
            foreach (Transform lightTransform in originalLightSettings.Keys)
            {
                if (lightTransform == null)
                    deadLights.Add(lightTransform);
            }
            foreach (Transform lightTransform in deadLights)
            {
                originalLightSettings.Remove(lightTransform);
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

            Transform dirLight = FindLight(screen, "Directional light");
            if (dirLight != null)
            {
                StoreOriginalSettings(dirLight);
                dirLight.gameObject.SetActive(true);

                Light light = dirLight.GetComponent<Light>();
                if (light != null)
                {
                    light.intensity = Settings.DirectionalLightBrightness.Value;
                }

                float rotationX = Settings.DirectionalLightRotationX.Value;
                float rotationY = Settings.DirectionalLightRotationY.Value;
                dirLight.rotation = Quaternion.Euler(rotationX, rotationY, 0f);
            }

            Transform lights = FindLight(screen, "Lights");
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

            Transform dirLight = FindLight(screen, "Directional light");
            if (dirLight != null)
            {
                RestoreOriginalSettings(dirLight);
            }

            Transform lights = FindLight(screen, "Lights");
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
            Transform light = screen.transform.Find($"Preview Panel/{name}");
            if (light != null) return light;

            light = screen.transform.Find(name);
            if (light != null) return light;

            return FindInChildren(screen.transform, name);
        }

        private static readonly Dictionary<(GameObject, string), Transform> lightCache =
            new Dictionary<(GameObject, string), Transform>();

        private static Transform FindInChildren(Transform parent, string name)
        {
            (GameObject, string) cacheKey = (parent.gameObject, name);
            if (lightCache.TryGetValue(cacheKey, out Transform cached) && cached != null)
            {
                return cached;
            }

            Transform result = FindInChildrenRecursive(parent, name);
            if (result != null)
            {
                lightCache[cacheKey] = result;
            }
            return result;
        }

        private static Transform FindInChildrenRecursive(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                    return child;

                Transform found = FindInChildrenRecursive(child, name);
                if (found != null)
                    return found;
            }
            return null;
        }

        private static void StoreOriginalSettings(Transform lightTransform)
        {
            if (!originalLightSettings.ContainsKey(lightTransform))
            {
                Light light = lightTransform.GetComponent<Light>();
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
            if (originalLightSettings.TryGetValue(lightTransform, out (bool Active, float Intensity, LightShadows Shadows, float Range, Vector3 Rotation) settings))
            {
                lightTransform.gameObject.SetActive(settings.Active);
                Light light = lightTransform.GetComponent<Light>();
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
            Light[] lights = light.GetComponentsInChildren<Light>();
            switch (light.name)
            {
                case "Cold Light (9)":
                    foreach (Light l in lights)
                        l.shadows = LightShadows.None;
                    break;

                case "AngleLight_2":
                    foreach (Light l in lights)
                    {
                        l.shadows = LightShadows.None;
                        l.intensity = 1f;
                        l.range = 2f;
                    }
                    break;

                case "Cold Light (4)":
                    foreach (Light l in lights)
                    {
                        l.shadows = LightShadows.None;
                        l.intensity = 1.7f;
                    }
                    break;

                case "Cold Light (6)":
                    foreach (Light l in lights)
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
                return AccessTools.DeclaredMethod(typeof(EditBuildScreen), nameof(EditBuildScreen.Awake));
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
                return AccessTools.DeclaredMethod(typeof(WeaponModdingScreen), nameof(WeaponModdingScreen.Awake));
            }

            [PatchPostfix]
            public static void Postfix(WeaponModdingScreen __instance)
            {
                RegisterActiveScreen(__instance.gameObject, "WeaponModdingScreen");
            }
        }

        #endregion
    }
}
