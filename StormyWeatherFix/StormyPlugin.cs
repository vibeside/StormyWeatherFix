using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LethalLib;
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
        public static Hook? GrabbableObjectDestroyHook;
        public static Hook? stormyWeatherUpdateHook;
        public static ILHook? stormyWeatherILHook;
        public static List<GrabbableObject> metalObjects = [];
        public static StormyWeather? fixedWeather;
        public void Awake()
        {
            mls = base.Logger;
            GrabbableObjectStartHook = new(typeof(GrabbableObject).GetMethod(nameof(GrabbableObject.Start),AccessTools.allDeclared),GOBjectStartPatch);
            stormyWeatherUpdateHook = new(typeof(StormyWeather).GetMethod(nameof(StormyWeather.Update), AccessTools.allDeclared), WeatherPatch);
            stormyWeatherILHook = new(typeof(StormyWeather).GetMethod(nameof(StormyWeather.OnEnable), AccessTools.allDeclared), StormyILHook);
            GrabbableObjectDestroyHook = new(typeof(RoundManager).GetMethod(nameof(RoundManager.DetectElevatorIsRunning), AccessTools.allDeclared),GOBjectDestroyPatch);
        }
        public static void GOBjectDestroyPatch(Action<RoundManager> orig,RoundManager self)
        {
            orig(self);
            for(int i = 0; i < metalObjects.Count; i++)
            {
                if (metalObjects[i] == null)
                {
                    metalObjects.RemoveAt(i);
                }
            }
        }
        public static void GOBjectStartPatch(Action<GrabbableObject> orig, GrabbableObject self)
        {
                if (self.itemProperties.isConductiveMetal)
                {
                    metalObjects.Add(self);
                }
            orig(self);
        }
        public static void WeatherPatch(Action<StormyWeather> orig, StormyWeather self)
        {
            orig(self);
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
