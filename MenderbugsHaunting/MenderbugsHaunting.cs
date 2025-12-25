using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

[BepInPlugin("com.chrismzz.menderbugshaunting", "Menderbug's Haunting", "1.0.0")]

public class MenderbugsHaunting : BaseUnityPlugin
{
    internal static ConfigEntry<int> damageOnBreak;

    private void Awake()
    {

        Logger.LogInfo("MenderbugsHaunting loaded and initialized.");
        damageOnBreak = Config.Bind(
            "General",
            "Damage On Break",
            100,
            "Amount of damage to deal to Hornet when she breaks something."
        );

        Harmony.CreateAndPatchAll(typeof(MenderbugsHaunting), null);
    }

    [HarmonyPatch(typeof(Breakable), nameof(Breakable.Break))]
    [HarmonyPostfix]
    private static void Die()
    {
        // I can add exceptions by using breakable.gameObject if ever this gets too game breaking
        HeroController.instance.TakeDamage(null, GlobalEnums.CollisionSide.bottom, damageOnBreak.Value, GlobalEnums.HazardType.EXPLOSION);
    }


}