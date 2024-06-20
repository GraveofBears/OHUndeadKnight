using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CreatureManager;
using HarmonyLib;
using ItemManager;
using PieceManager;
using ServerSync;
using UnityEngine;



namespace OHUndeadKnight
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [BepInDependency("org.bepinex.plugins.odinshollow")]
    public class OHUndeadKnightPlugin : BaseUnityPlugin
    {
        internal const string ModName = "OHUndeadKnight";
        internal const string ModVersion = "0.0.2";
        internal const string Author = "OdinPlus";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        internal static string ConnectionError = "";
        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource OHUndeadKnightLogger =
            BepInEx.Logging.Logger.CreateLogSource(ModName);

        private static readonly ConfigSync ConfigSync = new(ModGUID)
            { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };
        

        public enum Toggle
        {
            On = 1,
            Off = 0
        }

        public void Awake()
        {
            _serverConfigLocked = config("1 - General", "Lock Configuration", Toggle.On,
                "If on, the configuration is locked and can be changed by server admins only.");
            _ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);


            #region Pieces

            BuildPiece OH_Undead_Knight_Alter = new("undeadknight", "OH_Undead_Knight_Alter");
            OH_Undead_Knight_Alter.Name.English("Undead Knight Alter");
            OH_Undead_Knight_Alter.Description.English("An alter for summoning the knight");
            OH_Undead_Knight_Alter.RequiredItems.Add("SwordCheat", 1, false);
            OH_Undead_Knight_Alter.Tool.Add("OdinsHollowWand");
            OH_Undead_Knight_Alter.Category.Set("Hollow Pieces");
            #endregion


            #region Items

            Item OH_Broken_Sword = new("undeadknight", "OH_Broken_Sword");
            OH_Broken_Sword.Name.English("Knights Broken Sword");
            OH_Broken_Sword.Description.English("A sword of a once great knight, now rusted and broken.");
            OH_Broken_Sword.DropsFrom.Add("Skeleton", .1f, 1, 1);
            
            Item OH_Undead_Knight_Attack = new("undeadknight", "OH_Undead_Knight_Attack");
            OH_Undead_Knight_Attack.Configurable = Configurability.Full;
            Item OH_Undead_Knight_Attack_Chop = new("undeadknight", "OH_Undead_Knight_Attack_Chop");
            OH_Undead_Knight_Attack_Chop.Configurable = Configurability.Full;
            Item OH_Undead_Knight_Attack_Slash = new("undeadknight", "OH_Undead_Knight_Attack_Slash");
            OH_Undead_Knight_Attack_Slash.Configurable = Configurability.Full;



            Item OH_Undead_Knight_Sword = new("undeadknight", "OH_Undead_Knight_Sword");
            OH_Undead_Knight_Sword.Configurable = Configurability.Full;

            #endregion

            #region Creatures

            Creature OH_Undead_Knight = new("undeadknight", "OH_Undead_Knight")
            {
                Biome = Heightmap.Biome.None,
                CanSpawn = true,
                SpawnChance = 100,
                GroupSize = new CreatureManager.Range(1, 1),
                Maximum = 1

            };
            OH_Undead_Knight.Localize()
               .English("Undead Hollow Knight");
            OH_Undead_Knight.Drops["BoneFragments"].Amount = new CreatureManager.Range(1, 2);
            OH_Undead_Knight.Drops["BoneFragments"].DropChance = 50f;
            OH_Undead_Knight.Drops["Coins"].Amount = new CreatureManager.Range(25, 50);
            OH_Undead_Knight.Drops["Coins"].DropChance = 50f;
            OH_Undead_Knight.Drops["OH_Undead_Knight_Sword"].Amount = new CreatureManager.Range(1, 1);
            OH_Undead_Knight.Drops["OH_Undead_Knight_Sword"].DropChance = 0.3f;

            #endregion


            #region End

            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();
        }


        private void OnDestroy()
        {
            Config.Save();
        }

        private void SetupWatcher()
        {
            FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                OHUndeadKnightLogger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                OHUndeadKnightLogger.LogError($"There was an issue loading your {ConfigFileName}");
                OHUndeadKnightLogger.LogError("Please check your config entries for spelling and format!");
            }
        }


        #region ConfigOptions

        private static ConfigEntry<Toggle> _serverConfigLocked = null!;

        private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
            bool synchronizedSetting = true)
        {
            ConfigDescription extendedDescription =
                new(
                    description.Description +
                    (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                    description.AcceptableValues, description.Tags);
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, extendedDescription);
            //var configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, string description,
            bool synchronizedSetting = true)
        {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }

        private class ConfigurationManagerAttributes
        {
            public bool? Browsable = false;
        }
        class AcceptableShortcuts : AcceptableValueBase
        {
            public AcceptableShortcuts() : base(typeof(KeyboardShortcut))
            {
            }

            public override object Clamp(object value) => value;
            public override bool IsValid(object value) => true;

            public override string ToDescriptionString() =>
                "# Acceptable values: " + string.Join(", ", UnityInput.Current.SupportedKeyCodes); 
        }

        #endregion
        #endregion
    }
}