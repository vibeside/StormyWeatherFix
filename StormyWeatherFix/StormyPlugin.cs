using System;
using System.Collections;
using BepInEx;
using HarmonyLib;
using MonoMod.RuntimeDetour;
using UnityEngine;
namespace StormyWeatherFix
{
    [BepInPlugin(modGUID,modName,modVersion)]
    public class StormyPlugin : BaseUnityPlugin
    {
        public const string modGUID = "grug.lethalcompany.stormsrock";
        public const string modName = "StormyWeather Fix";
        public const string modVersion = "37.87.37";
        public static Hook? fixHook;
        public static Hook? stormyHook;
        public static ILHook?
        public static StormyWeather? fixedWeather;
        public void Awake()
        {
            fixHook = new(typeof(GrabbableObject).GetMethod(nameof(GrabbableObject.Start),AccessTools.allDeclared),FixPatch);
            stormyHook = new(typeof(StormyWeather).GetMethod(nameof(StormyWeather.OnEnable),AccessTools.allDeclared),WeatherPatch);
        }
        public static void FixPatch(Action<GrabbableObject> orig, GrabbableObject self)
        {
            if (fixedWeather != null)
            {
                if (self.itemProperties.isConductiveMetal)
                {
                    fixedWeather.metalObjects.Add(self);
                }
            }
        }
        public static void WeatherPatch(Action<StormyWeather> orig, StormyWeather self)
        {
            orig(self);
            fixedWeather = self;
        }
    }
}
