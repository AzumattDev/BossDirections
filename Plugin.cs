using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using ServerSync;
using UnityEngine;

namespace BossDirections
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class BossDirectionsPlugin : BaseUnityPlugin
    {
        internal const string ModName = "BossDirections";
        internal const string ModVersion = "1.0.4";
        internal const string Author = "Azumatt";
        private const string ModGUID = $"{Author}.{ModName}";
        private static string ConfigFileName = $"{ModGUID}.cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        private readonly Harmony _harmony = new(ModGUID);
        public static readonly ManualLogSource BossDirectionsLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);
        private static readonly ConfigSync ConfigSync = new(ModGUID) { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };
        public const string OfferingsFileName = $"{ModGUID}.Offerings.yml";
        private static string YmlConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + OfferingsFileName;
        internal static readonly CustomSyncedValue<string> bdData = new(ConfigSync, "bddata", "");

        private FileSystemWatcher _watcher = null!;
        private readonly object _reloadLock = new();
        private DateTime _lastConfigReloadTime;

        private FileSystemWatcher _ymlwatcher;
        private readonly object _ymlReloadLock = new();
        private DateTime _ymllastConfigReloadTime;
        private const long ReloadDelay = 10000000; // One second

        public enum Toggle
        {
            Off,
            On
        }

        private static ConfigEntry<Toggle> _serverConfigLocked = null!;
        internal static ConfigEntry<Toggle> AddPin = null!;
        internal static ConfigEntry<Toggle> ShowDistanceToBoss = null!;

        public void Awake()
        {
            bool saveOnSet = Config.SaveOnConfigSet;
            Config.SaveOnConfigSet = false;

            _serverConfigLocked = config("1 - General", "Lock Configuration", Toggle.On, "If on, the configuration is locked and can be changed by server admins only.");
            _ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);

            AddPin = config("1 - Behavior Changes", "Add Pin To Map", Toggle.Off, "If off, the pin will not be added to the map. If on, the pin will be added to the map.", false);
            ShowDistanceToBoss = config("1 - Behavior Changes", "Show Distance to Boss", Toggle.Off, "If on, the distance to the boss will be shown in the pin tooltip.", false);

            // Load offerings.yaml
            OfferManager.Initialize(YamlLoader.LoadYamlFile<OfferingsYaml>(OfferingsFileName));

            bdData.ValueChanged += YmlChangeUpdateOfferings; // check for file changes
            bdData.AssignLocalValue(File.ReadAllText(YmlConfigFileFullPath));

            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();

            Config.Save();
            if (saveOnSet)
            {
                Config.SaveOnConfigSet = saveOnSet;
            }
        }

        public void OnDestroy()
        {
            bdData.ValueChanged -= YmlChangeUpdateOfferings;
            SaveWithRespectToConfigSet();
            _watcher?.Dispose();
            _ymlwatcher?.Dispose();
        }

        private void SetupWatcher()
        {
            _watcher = new(Paths.ConfigPath, ConfigFileName);
            _watcher.Changed += ReadConfigValues;
            _watcher.Created += ReadConfigValues;
            _watcher.Renamed += ReadConfigValues;
            _watcher.IncludeSubdirectories = true;
            _watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            _watcher.EnableRaisingEvents = true;

            _ymlwatcher = new FileSystemWatcher(Paths.ConfigPath, OfferingsFileName);
            _ymlwatcher.Changed += ReadYamlConfigValues;
            _ymlwatcher.Created += ReadYamlConfigValues;
            _ymlwatcher.Renamed += ReadYamlConfigValues;
            _ymlwatcher.IncludeSubdirectories = true;
            _ymlwatcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            _ymlwatcher.EnableRaisingEvents = true;
        }

        private void ReadYamlConfigValues(object sender, FileSystemEventArgs e)
        {
            DateTime now = DateTime.Now;
            long time = now.Ticks - _ymllastConfigReloadTime.Ticks;
            if (time < ReloadDelay)
            {
                return;
            }

            lock (_ymlReloadLock)
            {
                if (!File.Exists(YmlConfigFileFullPath))
                {
                    BossDirectionsLogger.LogWarning("Config file does not exist. Skipping reload.");
                    return;
                }

                try
                {
                    BossDirectionsLogger.LogDebug("Reloading configuration...");
                    bdData.AssignLocalValue(File.ReadAllText(YmlConfigFileFullPath));
                    BossDirectionsLogger.LogInfo("Configuration reload complete.");
                }
                catch (Exception ex)
                {
                    BossDirectionsLogger.LogError($"Error reloading configuration: {ex.Message}");
                }
            }

            _ymllastConfigReloadTime = now;
        }

        private static void YmlChangeUpdateOfferings()
        {
            BossDirectionsLogger.LogDebug("YmlChangeUpdateOfferings called");
            try
            {
                OfferManager.Initialize(YamlLoader.DeserializeFromString<OfferingsYaml>(bdData.Value));
            }
            catch (Exception e)
            {
                BossDirectionsLogger.LogError($"Failed to deserialize {OfferingsFileName}: {e}");
            }
        }

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            DateTime now = DateTime.Now;
            long time = now.Ticks - _lastConfigReloadTime.Ticks;
            if (time < ReloadDelay)
            {
                return;
            }

            lock (_reloadLock)
            {
                if (!File.Exists(ConfigFileFullPath))
                {
                    BossDirectionsLogger.LogWarning("Config file does not exist. Skipping reload.");
                    return;
                }

                try
                {
                    BossDirectionsLogger.LogDebug("Reloading configuration...");
                    SaveWithRespectToConfigSet(true);
                    BossDirectionsLogger.LogInfo("Configuration reload complete.");
                }
                catch (Exception ex)
                {
                    BossDirectionsLogger.LogError($"Error reloading configuration: {ex.Message}");
                }
            }

            _lastConfigReloadTime = now;
        }

        private void SaveWithRespectToConfigSet(bool reload = false)
        {
            bool originalSaveOnSet = Config.SaveOnConfigSet;
            Config.SaveOnConfigSet = false;
            if (reload)
                Config.Reload();
            Config.Save();
            if (originalSaveOnSet)
            {
                Config.SaveOnConfigSet = originalSaveOnSet;
            }
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description, bool synchronizedSetting = true)
        {
            ConfigDescription extendedDescription = new(description.Description + (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"), description.AcceptableValues, description.Tags);
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, extendedDescription);
            SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, string description, bool synchronizedSetting = true)
        {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }
    }

    public static class ToggleExtensions
    {
        public static bool IsOn(this BossDirectionsPlugin.Toggle toggle)
        {
            return toggle == BossDirectionsPlugin.Toggle.On;
        }

        public static bool IsOff(this BossDirectionsPlugin.Toggle toggle)
        {
            return toggle == BossDirectionsPlugin.Toggle.Off;
        }
    }

    [HarmonyPatch(typeof(Game), nameof(Game.RPC_DiscoverLocationResponse))]
    static class GameRPCDiscoverLocationResponsePatch
    {
        static bool Prefix(Vegvisir __instance, long sender, string pinName, int pinType, Vector3 pos, bool showMap)
        {
            if (BossDirectionsPlugin.AddPin.Value.IsOn())
            {
                Minimap.instance.DiscoverLocation(pos, (Minimap.PinType)pinType, pinName, showMap);
            }

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

    [HarmonyPatch(typeof(Fireplace), nameof(Fireplace.UseItem))]
    internal static class Fireplace_UseItem_Patch
    {
        // Prefix runs *before* the original. 
        // If it returns false, the original method is not called.
        static bool Prefix(Fireplace __instance, Humanoid user, ItemDrop.ItemData item, ref bool __result) // capture/override the return value
        {
            // 1) Null‑check: if no item, let vanilla handle it.
            if (item == null)
                return true;

            // 2) Call your offering logic.
            //    You may want to overload TryOffer to take the fireplace instance:
            //    `public static bool TryOffer(ItemData item, Vector3 firePos)`
            if (OfferManager.TryOffer(item, __instance.transform.position))
            {
                // We handled it: tell the game “yes, we used the item”,
                // and *don’t* run the original UseItem code.
                __result = true;
                return false;
            }

            // 3) Not one of ours? Let the game run its normal logic.
            return true;
        }
    }
}