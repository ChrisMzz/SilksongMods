using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GlobalEnums;
using GlobalSettings;
using HarmonyLib;
using System;
using System.Collections.Generic;

[BepInPlugin("com.chrismzz.swapcrestonhit", "Swap Crest On Hit", "1.2.1")]

public class SwapCrestOnHit : BaseUnityPlugin
{
    public static readonly Random rnd = new Random();
    internal static ConfigEntry<bool> crestSanity;
    public static ManualLogSource Logger;
    public static int cooldown;

    private void Awake()
    {
        Logger = base.Logger;

        Logger.LogInfo("SwapCrestOnHit loaded and initialized.");
        crestSanity = Config.Bind(
            "General",
            "Crest Sanity",
            true,
            "Only use unlocked and available crests. Set to false if you want pure chaos."
        );
        cooldown = 100;


        Harmony.CreateAndPatchAll(typeof(SwapCrestOnHit), null);
    }

    [HarmonyPatch(typeof(HeroController), "HeroDamaged")]
    [HarmonyPostfix]
    private static void HeroDamagedPostfix(HeroController __instance)
    {
        if (cooldown < 100) { return;}
        int currentSilk = __instance.playerData.silk;
        List<ToolCrest> allCrests = ToolItemManager.GetAllCrests();
        List<ToolCrest> availableCrests = new List<ToolCrest>();
        foreach (ToolCrest crest in allCrests)
        {
            if (!crest.IsHidden && crest.IsUnlocked)
            {
                if (!crest.IsUpgradedVersionUnlocked)
                    availableCrests.Add(crest);
            } 
        }
        bool canSwap = (availableCrests.Count > 1 && __instance.playerData.CurrentCrestID != Gameplay.CursedCrest.name && __instance.playerData.CurrentCrestID != Gameplay.CloaklessCrest.name);
        List<ToolCrest> crestsList = crestSanity.Value ? availableCrests : allCrests;
        if (canSwap || !crestSanity.Value)
        {
            __instance.ResetAllCrestState();
            int crestIdx = rnd.Next(0, crestsList.Count);
            ToolCrest newCrest = crestsList[crestIdx];
            while (__instance.playerData.CurrentCrestID == newCrest.name)
            {
                crestIdx = rnd.Next(0, crestsList.Count);
                newCrest = crestsList[crestIdx];
            }
            // Logger.LogInfo($"Swapped to {newCrest.name}.");
            ToolItemManager.SetEquippedCrest(newCrest.name);
            ToolItemManager.SendEquippedChangedEvent(force: true);
        }
        // sometimes ResetAllCrestState seems to remove silk from the player
        __instance.playerData.silk = currentSilk;
        //__instance.playerData.AddHealth(1); // for testing
        cooldown = 0;
    }

    [HarmonyPatch(typeof(HeroController), "FixedUpdate")]
    [HarmonyPostfix]
    private static void CanSwapCrest()
    {
        if (cooldown < 100) 
            cooldown++;
    }
    

 }

