using BepInEx;
using BepInEx.Configuration;
using GlobalSettings;
using HarmonyLib;
using System;
using System.Collections.Generic;

[BepInPlugin("com.chrismzz.swapcrestonhit", "Swap Crest On Hit", "1.1.0")]

public class SwapCrestOnHit : BaseUnityPlugin
{
    public static readonly Random rnd = new Random();
    internal static ConfigEntry<bool> crestSanity;
    private void Awake()
    {

        Logger.LogInfo("SwapCrestOnHit loaded and initialized.");
        crestSanity = Config.Bind(
            "General",
            "Crest Sanity",
            true,
            "Only use unlocked and available crests. Set to false if you want pure chaos."
        );


        Harmony.CreateAndPatchAll(typeof(SwapCrestOnHit), null);
    }

    [HarmonyPatch(typeof(HeroController), nameof(HeroController.TakeDamage))]
    [HarmonyPostfix]
    private static void TakeDamagePostfix(HeroController __instance)
    {
        int currentSilk = __instance.playerData.silk;
        List<ToolCrest> allCrests = ToolItemManager.GetAllCrests();
        List<ToolCrest> availableCrests = new List<ToolCrest>();
        int crestCount = ToolItemManager.GetUnlockedCrestsCount();
        foreach (ToolCrest crest in allCrests)
        {
            if (!crest.IsHidden && crest.IsBaseVersion && crest.IsUnlocked)
                availableCrests.Add(crest);
        }
        bool canSwap = (crestCount > 1 && __instance.playerData.CurrentCrestID != Gameplay.CursedCrest.name && __instance.playerData.CurrentCrestID != Gameplay.CloaklessCrest.name);
        List<ToolCrest> crestsList = crestSanity.Value ? availableCrests : allCrests;
        int count = crestSanity.Value ? crestCount : allCrests.Count;
        if (!crestSanity.Value)
            crestsList.Add(Gameplay.CursedCrest);
        if (canSwap || !crestSanity.Value)
        {
            __instance.ResetAllCrestState();
            int crestIdx = rnd.Next(0, count);
            ToolCrest newCrest = crestsList[crestIdx];
            while (__instance.playerData.CurrentCrestID == newCrest.name)
            {
                crestIdx = rnd.Next(0, count);
                newCrest = crestsList[crestIdx];
            }
            __instance.playerData.CurrentCrestID = newCrest.name;
            ToolItemManager.SendEquippedChangedEvent(force: true);
        }
        // sometimes ResetAllCrestState seems to remove silk from the player
        __instance.playerData.silk = currentSilk;

    }

 }

