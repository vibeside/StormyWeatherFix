using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MonoMod.Cil;
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
        public static ManualLogSource mls = null!;
        public static Hook? GrabbableObjectStartHook;
        public static Hook? stormyWeatherHook;
        public static Hook? stormyWeatherUpdateHook;
        public static ILHook? stormyWeatherILHook;
        public static List<GrabbableObject> metalObjects = [];
        public static StormyWeather? fixedWeather;
        public void Awake()
        {
            mls = base.Logger;
            GrabbableObjectStartHook = new(typeof(GrabbableObject).GetMethod(nameof(GrabbableObject.Start),AccessTools.allDeclared),FixPatch);
            //stormyWeatherUpdateHook = new(typeof(StormyWeather).GetMethod(nameof(StormyWeather.OnEnable),AccessTools.allDeclared),WeatherPatch);
            stormyWeatherILHook = new(typeof(StormyWeather).GetMethod(nameof(StormyWeather.OnEnable),AccessTools.allDeclared),StormyILHook);
            stormyWeatherUpdateHook = new(typeof(StormyWeather).GetMethod(nameof(StormyWeather.OnEnable),AccessTools.allDeclared),WeatherPatch);
        }
        public static void FixPatch(Action<GrabbableObject> orig, GrabbableObject self)
        {
                if (self.itemProperties.isConductiveMetal)
                {
                    metalObjects.Add(self);
                }
        }
        public static void WeatherPatch(Action<StormyWeather> orig, StormyWeather self)
        {
            orig(self);
            foreach(GrabbableObject obj in metalObjects)
            {
                if (obj == null)
                {
                    mls.LogInfo("null"); 
                    continue;
                }
                mls.LogInfo(obj);
            }
            self.metalObjects = metalObjects;
        }
        public static void StormyILHook(ILContext il)
        {
            ILCursor c = new(il);

            c.GotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdarg(0),
                x => x.MatchCall<StormyWeather>("GetMetalObjectsAfterDelay")
                );
            c.RemoveRange(5);
        }
    }
}
