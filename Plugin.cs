using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using LocalizationManager;
using UnityEngine;
using UnityEngine.Rendering;

namespace ModAge
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class ModAgePlugin : BaseUnityPlugin
    {
        internal const string ModName = "ModAge";
        internal const string ModVersion = "1.0.5";
        internal const string Author = "Azumatt";
        private const string ModGUID = Author + "." + ModName;
        private readonly Harmony _harmony = new(ModGUID);
        public static readonly ManualLogSource ModAgeLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);
        public static GameObject modAgeUIAsset = null!;
        public static GameObject modAgeUIFinal = null!;
        public static ModAgeUI modAgeUIcomp = null!;
        public static GameObject modAgeUIObject = null!;
        internal static ModAgePlugin Instance = null!;
        private Coroutine? modcheckerCoroutine;
        internal static Coroutine? modcheckerImageCoroutine;
        internal static List<PackageInfo>? allPackagesInfo = null;
        internal static Dictionary<string, PreparedPackageInfo>? allPreparedPackagesInfo = null;
        internal static bool CanCompareMods = false;
        internal Action RefreshModListDelegate = null!;


        public enum Toggle
        {
            Off,
            On
        }

        public void Awake()
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            {
                ModAgeLogger.LogWarning("ModAge does not work in headless mode as it's a client only mod. ModAge will not be loaded.");
                return;
            }
            var configOnSet = Config.SaveOnConfigSet;
            Config.SaveOnConfigSet = false;
            showAllMods = config("1 - General", "ShowAllMods", Toggle.Off, new ConfigDescription("Show all mods, not just outdated ones.", null, new ConfigurationManagerAttributes() { Order = 6 }));
            RefreshModListDelegate = RefreshModList;
            showAllMods.SettingChanged += OnShowAllModsChanged;
            yearConfig = config("1 - General", "YearToTarget", 2024, new ConfigDescription("Year to target. This is the year that the mods will be compared to.", null, new ConfigurationManagerAttributes() { Order = 5 }));
            yearConfig.SettingChanged += OnShowAllModsChanged;
            monthConfig = config("1 - General", "MonthToTarget", 4, new ConfigDescription("Month to target. This is the month that the mods will be compared to.", null, new ConfigurationManagerAttributes() { Order = 4 }));
            monthConfig.SettingChanged += OnShowAllModsChanged;
            dayConfig = config("1 - General", "DayToTarget", 10, new ConfigDescription("Day to target. This is the day that the mods will be compared to.", null, new ConfigurationManagerAttributes() { Order = 3 }));
            dayConfig.SettingChanged += OnShowAllModsChanged;
            hourConfig = config("1 - General", "HourToTarget", 8, new ConfigDescription("Hour to target. It's advised to make this slightly after the update to be the most accurate. This is the hour that the mods will be compared to.", null, new ConfigurationManagerAttributes() { Order = 2 }));
            hourConfig.SettingChanged += OnShowAllModsChanged;
            minuteConfig = config("1 - General", "MinuteToTarget", 0, new ConfigDescription("Minute to target. This is the minute that the mods will be compared to. It's advised to set this slightly after the expected update time for accuracy.", null, new ConfigurationManagerAttributes() { Order = 1 }));
            minuteConfig.SettingChanged += OnShowAllModsChanged;
            secondConfig = config("1 - General", "SecondToTarget", 0, new ConfigDescription("Second to target. This is the second that the mods will be compared to. It's advised to set this slightly after the expected update time for accuracy.", null, new ConfigurationManagerAttributes() { Order = 0 }));
            secondConfig.SettingChanged += OnShowAllModsChanged;
            Instance = this;
            Localizer.Load();
            Utilities.LoadAssets();

            GameObject? instantiatedObject = Instantiate(modAgeUIAsset);
            modAgeUIFinal = instantiatedObject;
            modAgeUIcomp = instantiatedObject.GetComponent<ModAgeUI>();
            DontDestroyOnLoad(modAgeUIFinal);
            ModAgePlugin.modAgeUIFinal.SetActive(false);
            modcheckerCoroutine = StartCoroutine(Utilities.CheckModUpdates());

            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);

            if (configOnSet)
            {
                Config.SaveOnConfigSet = true;
                Config.Save();
            }
        }

        public void Start()
        {
            if (CanCompareMods)
            {
                ModAgePlugin.ModAgeLogger.LogDebug("Comparing local mods to prepared packages");
                Utilities.CompareLocalModsToPreparedPackage();
            }
            else
            {
                ModAgePlugin.ModAgeLogger.LogWarning("CanCompareMods is false, not comparing local mods to prepared packages");

                // Check again in 5 seconds
                Invoke(nameof(Utilities.CompareLocalModsToPreparedPackage), 10f);
            }
        }

        private void OnShowAllModsChanged(object sender, EventArgs e)
        {
            RefreshModListDelegate?.Invoke();
        }

        public void RefreshModList()
        {
            if (modcheckerCoroutine != null)
            {
                StopCoroutine(modcheckerCoroutine);
            }

            if (modcheckerImageCoroutine != null)
            {
                StopCoroutine(modcheckerImageCoroutine);
            }

            ClearUI();
            modcheckerCoroutine = StartCoroutine(Utilities.CheckModUpdates());
        }

        private void ClearUI()
        {
            if (modAgeUIcomp != null && modAgeUIcomp.contentList != null)
            {
                foreach (Transform child in modAgeUIcomp.contentList.transform)
                {
                    if (child.gameObject != modAgeUIcomp.Placeholder.gameObject)
                    {
                        Destroy(child.gameObject);
                    }
                }
            }
        }


        private void OnDestroy()
        {
            if (modcheckerCoroutine != null)
            {
                StopCoroutine(modcheckerCoroutine);
            }

            if (modcheckerImageCoroutine != null)
            {
                StopCoroutine(modcheckerImageCoroutine);
            }

            Config.Save();
        }

        internal static ConfigEntry<Toggle> showAllMods = null!;
        internal static ConfigEntry<int> yearConfig = null!;
        internal static ConfigEntry<int> monthConfig = null!;
        internal static ConfigEntry<int> dayConfig = null!;
        internal static ConfigEntry<int> hourConfig = null!;
        internal static ConfigEntry<int> minuteConfig = null!;
        internal static ConfigEntry<int> secondConfig = null!;


        private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description)
        {
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, description);

            return configEntry;
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, string description)
        {
            return config(group, name, value, new ConfigDescription(description));
        }

        private class ConfigurationManagerAttributes
        {
            [UsedImplicitly] public int? Order = null!;
            [UsedImplicitly] public bool? Browsable = null!;
            [UsedImplicitly] public string? Category = null!;
            [UsedImplicitly] public Action<ConfigEntryBase>? CustomDrawer = null!;
        }
    }
}