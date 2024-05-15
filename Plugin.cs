using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace BossDirections
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class BossDirectionsPlugin : BaseUnityPlugin
    {
        internal const string ModName = "BossDirections";
        internal const string ModVersion = "1.0.2";
        internal const string Author = "Azumatt";
        private const string ModGUID = Author + "." + ModName;

        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource BossDirectionsLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        internal static EffectList activeEffect = new();
        internal static GameObject activeObject = new();

        public void Awake()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
        }
    }

    [HarmonyPatch(typeof(Game), nameof(Game.RPC_DiscoverLocationResponse))]
    static class GameRPCDiscoverLocationResponsePatch
    {
        static bool Prefix(Vegvisir __instance, long sender, string pinName, int pinType, Vector3 pos, bool showMap)
        {
            if ((Minimap.PinType)pinType != Minimap.PinType.Boss || !Player.m_localPlayer)
            {
                return true;
            }
#if DEBUG
            BossDirectionsPlugin.BossDirectionsLogger.LogWarning("BossDirections: Boss pin detected, setting look direction. This is a debug message and can safely be ignored.");
#endif
            Player.m_localPlayer.SetLookDir(pos - Player.m_localPlayer.transform.position, 3.5f);
            return false;
        }
    }

    /*[HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    static class ZNetSceneAwakePatch
    {
        static void Postfix(ZNetScene __instance)
        {
            GameObject waystone = __instance.GetPrefab("Waystone");
            if (waystone != null)
            {
                WayStone waystoneComp = waystone.GetComponent<WayStone>();
                // Clone and store the waystone's m_activeEffect and m_activeObject
                BossDirectionsPlugin.activeEffect = waystoneComp.m_activeEffect;
                BossDirectionsPlugin.activeObject = waystoneComp.m_activeObject;
            }
        }
    }*/
}