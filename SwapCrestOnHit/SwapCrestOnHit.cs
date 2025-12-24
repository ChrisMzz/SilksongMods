using BepInEx;
using GlobalSettings;
using HarmonyLib;
using System;
using System.Collections.Generic;

[BepInPlugin("com.chrismzz.swapcrestonhit", "Swap Crest On Hit", "1.0.0")]

public class SwapCrestOnHit : BaseUnityPlugin
{
    public static readonly Random rnd = new Random();
    private void Awake()
    {

        Logger.LogInfo("SwapCrestOnHit loaded and initialized.");
        


        Harmony.CreateAndPatchAll(typeof(SwapCrestOnHit), null);
    }

    [HarmonyPatch(typeof(HeroController), nameof(HeroController.TakeDamage))]
    [HarmonyPostfix]
    private static void TakeDamagePostfix(HeroController __instance)
    {
        List<ToolCrest> allCrests = ToolItemManager.GetAllCrests();
        List<ToolCrest> availableCrests = new List<ToolCrest>();
        int crestCount = ToolItemManager.GetUnlockedCrestsCount();
        foreach (ToolCrest crest in allCrests)
        {
            if (!crest.IsHidden && crest.IsBaseVersion && crest.IsUnlocked)
                availableCrests.Add(crest);
        }

        if (crestCount > 1 && __instance.playerData.CurrentCrestID != Gameplay.CursedCrest.name && __instance.playerData.CurrentCrestID != Gameplay.CloaklessCrest.name)
        {
            __instance.ResetAllCrestState();
            int crestIdx = rnd.Next(0, crestCount);
            ToolCrest newCrest = availableCrests[crestIdx];
            while (__instance.playerData.CurrentCrestID == newCrest.name)
            {
                crestIdx = rnd.Next(0, crestCount);
                newCrest = availableCrests[crestIdx];
            }
            __instance.playerData.CurrentCrestID = newCrest.name;
            ToolItemManager.SendEquippedChangedEvent(force: true);
        }
    }

 }

