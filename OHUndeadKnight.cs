using System;
using System.Collections.Generic;
using System.Collections;
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
        internal const string ModVersion = "0.0.3";
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


            BuildPiece OH_Undead_Repair_Station = new("undeadknight", "OH_Undead_Repair_Station");
            OH_Undead_Repair_Station.Name.English("Undead Knight Sword Alter");
            OH_Undead_Repair_Station.Description.English("An alter for repairing and building Undead Knight Swords");
            OH_Undead_Repair_Station.RequiredItems.Add("Stone", 5, true);
            OH_Undead_Repair_Station.RequiredItems.Add("TrophySkeleton", 1, true);
            OH_Undead_Repair_Station.RequiredItems.Add("OH_Broken_Sword", 1, true);
            OH_Undead_Repair_Station.Category.Set(BuildPieceCategory.Misc);

            BuildPiece OH_Broken_Sword_Alter = new("undeadknight", "OH_Broken_Sword_Alter");
            OH_Broken_Sword_Alter.Name.English("Broken Sword Totem");
            OH_Broken_Sword_Alter.Description.English("An alter that contains the spirits of skeleton minions.");
            OH_Broken_Sword_Alter.RequiredItems.Add("Stone", 5, true);
            OH_Broken_Sword_Alter.RequiredItems.Add("OH_Broken_Sword", 1, true);
            OH_Broken_Sword_Alter.Category.Set(BuildPieceCategory.Misc);
            OH_Broken_Sword_Alter.Extension.Set("OH_Undead_Repair_Station", 8);
            OH_Broken_Sword_Alter.Crafting.Set("OH_Undead_Repair_Station");

            BuildPiece OH_Knight_Soul_Alter = new("undeadknight", "OH_Knight_Soul_Alter");
            OH_Knight_Soul_Alter.Name.English("Undead Soul Dais");
            OH_Knight_Soul_Alter.Description.English("An alter that contains the soul of an Undead Knight.");
            OH_Knight_Soul_Alter.RequiredItems.Add("Stone", 5, true);
            OH_Knight_Soul_Alter.RequiredItems.Add("OH_Knights_Spirit_Sword", 1, true);
            OH_Knight_Soul_Alter.Category.Set(BuildPieceCategory.Misc);
            OH_Knight_Soul_Alter.Extension.Set("OH_Undead_Repair_Station", 8);
            OH_Knight_Soul_Alter.Crafting.Set("OH_Undead_Repair_Station");


            #endregion


            #region Items

            Item OH_Broken_Sword = new("undeadknight", "OH_Broken_Sword");
            OH_Broken_Sword.Name.English("Knights Broken Sword");
            OH_Broken_Sword.Description.English("A sword of a once great knight, now rusted and broken.");
            OH_Broken_Sword.DropsFrom.Add("Skeleton", .1f, 1, 1);

            Item OH_Knights_Spirit_Sword = new("undeadknight", "OH_Knights_Spirit_Sword");
            OH_Knights_Spirit_Sword.Name.English("Knights Spirit Sword");
            OH_Knights_Spirit_Sword.Description.English("A sword spirit of a fallen knight.");


            Item OH_Undead_Knight_Attack = new("undeadknight", "OH_Undead_Knight_Attack");
            OH_Undead_Knight_Attack.Configurable = Configurability.Full;
            Item OH_Undead_Knight_Attack_Chop = new("undeadknight", "OH_Undead_Knight_Attack_Chop");
            OH_Undead_Knight_Attack_Chop.Configurable = Configurability.Full;
            Item OH_Undead_Knight_Attack_Slash = new("undeadknight", "OH_Undead_Knight_Attack_Slash");
            OH_Undead_Knight_Attack_Slash.Configurable = Configurability.Full;



            Item OH_Undead_Knight_Sword = new("undeadknight", "OH_Undead_Knight_Sword");
            OH_Undead_Knight_Sword.Name.English("Stormreaver"); // You can use this to fix the display name in code
            OH_Undead_Knight_Sword.Description.English("A sharp blade owned by a fallen knight.");
            OH_Undead_Knight_Sword.Configurable = Configurability.Full;
            OH_Undead_Knight_Sword.Crafting.Add("OH_Undead_Repair_Station", 1);
            OH_Undead_Knight_Sword.RequiredItems.Add("OH_Knights_Spirit_Sword", 1);
            OH_Undead_Knight_Sword.RequiredUpgradeItems.Add("OH_Broken_Sword", 5);

            Item OH_Undead_Knight_Sword1 = new("undeadknight", "OH_Undead_Knight_Sword1");
            OH_Undead_Knight_Sword1.Name.English("Shadowcleaver"); // You can use this to fix the display name in code
            OH_Undead_Knight_Sword1.Description.English("A sharp blade owned by a fallen knight.");
            OH_Undead_Knight_Sword1.Configurable = Configurability.Full;
            OH_Undead_Knight_Sword1.Crafting.Add("OH_Undead_Repair_Station", 1);
            OH_Undead_Knight_Sword1.RequiredItems.Add("OH_Knights_Spirit_Sword", 1);
            OH_Undead_Knight_Sword1.RequiredUpgradeItems.Add("OH_Broken_Sword", 5);

            Item OH_Undead_Knight_Sword2 = new("undeadknight", "OH_Undead_Knight_Sword2");
            OH_Undead_Knight_Sword2.Name.English("Wyrmbane"); // You can use this to fix the display name in code
            OH_Undead_Knight_Sword2.Description.English("A sharp blade owned by a fallen knight.");
            OH_Undead_Knight_Sword2.Configurable = Configurability.Full;
            OH_Undead_Knight_Sword2.Crafting.Add("OH_Undead_Repair_Station", 1);
            OH_Undead_Knight_Sword2.RequiredItems.Add("OH_Knights_Spirit_Sword", 1);
            OH_Undead_Knight_Sword2.RequiredUpgradeItems.Add("OH_Broken_Sword", 5);

            Item OH_Undead_Knight_Sword3 = new("undeadknight", "OH_Undead_Knight_Sword3");
            OH_Undead_Knight_Sword3.Name.English("Nightfall"); // You can use this to fix the display name in code
            OH_Undead_Knight_Sword3.Description.English("A sharp blade owned by a fallen knight.");
            OH_Undead_Knight_Sword3.Configurable = Configurability.Full;
            OH_Undead_Knight_Sword3.Crafting.Add("OH_Undead_Repair_Station", 1);
            OH_Undead_Knight_Sword3.RequiredItems.Add("OH_Knights_Spirit_Sword", 1);
            OH_Undead_Knight_Sword3.RequiredUpgradeItems.Add("OH_Broken_Sword", 5);

            Item OH_Undead_Knight_Sword4 = new("undeadknight", "OH_Undead_Knight_Sword4");
            OH_Undead_Knight_Sword4.Name.English("Ironclaw"); // You can use this to fix the display name in code
            OH_Undead_Knight_Sword4.Description.English("A sharp blade owned by a fallen knight.");
            OH_Undead_Knight_Sword4.Configurable = Configurability.Full;
            OH_Undead_Knight_Sword4.Crafting.Add("OH_Undead_Repair_Station", 1);
            OH_Undead_Knight_Sword4.RequiredItems.Add("OH_Knights_Spirit_Sword", 1);
            OH_Undead_Knight_Sword4.RequiredUpgradeItems.Add("OH_Broken_Sword", 5);

            Item OH_Undead_Knight_Sword5 = new("undeadknight", "OH_Undead_Knight_Sword5");
            OH_Undead_Knight_Sword5.Name.English("Bloodsong"); // You can use this to fix the display name in code
            OH_Undead_Knight_Sword5.Description.English("A sharp blade owned by a fallen knight.");
            OH_Undead_Knight_Sword5.Configurable = Configurability.Full;
            OH_Undead_Knight_Sword5.Crafting.Add("OH_Undead_Repair_Station", 1);
            OH_Undead_Knight_Sword5.RequiredItems.Add("OH_Knights_Spirit_Sword", 1);
            OH_Undead_Knight_Sword5.RequiredUpgradeItems.Add("OH_Broken_Sword", 5);

            Item OH_Undead_Knight_Sword6 = new("undeadknight", "OH_Undead_Knight_Sword6");
            OH_Undead_Knight_Sword6.Name.English("Hearthguard"); // You can use this to fix the display name in code
            OH_Undead_Knight_Sword6.Description.English("A sharp blade owned by a fallen knight.");
            OH_Undead_Knight_Sword6.Configurable = Configurability.Full;
            OH_Undead_Knight_Sword6.Crafting.Add("OH_Undead_Repair_Station", 1);
            OH_Undead_Knight_Sword6.RequiredItems.Add("OH_Knights_Spirit_Sword", 1);
            OH_Undead_Knight_Sword6.RequiredUpgradeItems.Add("OH_Broken_Sword", 5);

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
            OH_Undead_Knight.Drops["OH_Knights_Spirit_Sword"].Amount = new CreatureManager.Range(1, 1);
            OH_Undead_Knight.Drops["OH_Knights_Spirit_Sword"].DropChance = 0.3f;

            #endregion


            #region End

            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();
        }

        [HarmonyPatch(typeof(ZNetScene), "Awake")]
        public static class ObjectDB_Awake_Patch
        {
            [HarmonyPriority(Priority.Low)]
            public static void Postfix(ZNetScene __instance)
            {
                if (ObjectDB.instance.m_items.Count == 0) return;

                // Retrieve the prefabs for OH_Broken_Sword, Bronze, and OH_Undead_Repair_Station
                var brokenSwordPrefab = ObjectDB.instance.GetItemPrefab("OH_Broken_Sword");
                var bronzeItemPrefab = ObjectDB.instance.GetItemPrefab("Bronze");
                var undeadRepairStationPrefab = ZNetScene.instance.GetPrefab("OH_Undead_Repair_Station");

                // Ensure all items are found before proceeding
                if (brokenSwordPrefab != null && bronzeItemPrefab != null && undeadRepairStationPrefab != null)
                {
                    // Create a new recipe for converting OH_Broken_Sword into Bronze
                    Recipe recipe = ScriptableObject.CreateInstance<Recipe>();
                    recipe.m_item = bronzeItemPrefab.GetComponent<ItemDrop>(); // Resulting item: Bronze
                    recipe.m_amount = 1; // Amount of Bronze produced

                    // Define required materials
                    recipe.m_resources = new Piece.Requirement[]
                    {
                    new Piece.Requirement
                    {
                        m_resItem = brokenSwordPrefab.GetComponent<ItemDrop>(), // Input item: OH_Broken_Sword
                        m_amount = 1 // Amount of OH_Broken_Sword needed
                    }
                    };

                    // Assign the custom workbench as the crafting station
                    recipe.m_craftingStation = undeadRepairStationPrefab.GetComponent<CraftingStation>();

                    // Add the recipe to ObjectDB
                    ObjectDB.instance.m_recipes.Add(recipe);
                }
            }
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