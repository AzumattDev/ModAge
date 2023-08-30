using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using LocalizationManager;
using ServerSync;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;
using YamlDotNet.Serialization;
using Object = UnityEngine.Object;

namespace ModAge
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class ModAgePlugin : BaseUnityPlugin
    {
        internal const string ModName = "ModAge";
        internal const string ModVersion = "1.0.0";
        internal const string Author = "Azumatt";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

        internal static string ConnectionError = "";

        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource ModAgeLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);

        private static readonly ConfigSync ConfigSync = new(ModGUID) { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };

        public static GameObject modAgeUI = null!;
        public static ModAgeUI modAgeUIcomp = null!;
        public static GameObject modAgeUIObject = null!;
        internal static ModAgePlugin Instance;
        private Coroutine? modcheckerCoroutine;
        private List<PackageInfo>? allPackagesInfo = null;

        public enum Toggle
        {
            On = 1,
            Off = 0
        }

        public void Awake()
        {
            Instance = this;
            Localizer.Load();

            _serverConfigLocked = config("1 - General", "Lock Configuration", Toggle.On, "If on, the configuration is locked and can be changed by server admins only.");
            _ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);


            LoadAssets();

            modAgeUIObject = new GameObject("ModAgeUI");
            var instantiatedObject = Instantiate(modAgeUI);
            modAgeUI = instantiatedObject;
            modAgeUIcomp = instantiatedObject.GetComponent<ModAgeUI>();
            modAgeUI.transform.SetParent(modAgeUIObject.transform);
            DontDestroyOnLoad(modAgeUIObject);


            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();
        }

        public void Start()
        {
            modcheckerCoroutine = StartCoroutine(CheckModUpdates());
        }


        private static AssetBundle GetAssetBundleFromResources(string filename)
        {
            var execAssembly = Assembly.GetExecutingAssembly();
            var resourceName = execAssembly.GetManifestResourceNames()
                .Single(str => str.EndsWith(filename));

            using (var stream = execAssembly.GetManifestResourceStream(resourceName))
            {
                return AssetBundle.LoadFromStream(stream);
            }
        }

        public static void LoadAssets()
        {
            var assetBundle = GetAssetBundleFromResources("modage");
            modAgeUI = assetBundle.LoadAsset<GameObject>("ModAgeUI");
            modAgeUI.AddComponent<Localize>();
            modAgeUIcomp = modAgeUI.GetComponent<ModAgeUI>();
            assetBundle?.Unload(false);
        }

        private bool dataFetched = false;

        private IEnumerator CheckModUpdates()
        {
            if (allPackagesInfo == null)
            {
                yield return GetAllModsFromThunderstore((result) =>
                {
                    if (result != null)
                    {
                        CompareLocalModsToThunderstore();
                    }
                });
            }
            else
            {
                CompareLocalModsToThunderstore();
            }
        }

        private void CompareLocalModsToThunderstore()
        {
            var plugins = Chainloader.PluginInfos;
            foreach (var pluginInfo in plugins.Values)
            {
                string modNameInThunderstoreFormat = ConvertToThunderstoreFormat(pluginInfo.Metadata.Name);
                var matchedMod = allPackagesInfo?.FirstOrDefault(x => x.name == modNameInThunderstoreFormat);
                var latestVersion = matchedMod?.versions?.FirstOrDefault()?.version_number;

                if (latestVersion != null)
                {
                    UpdateUI(pluginInfo.Metadata.Name, pluginInfo.Metadata.Version.ToString(), latestVersion);
                }
            }
        }

        private string ConvertToThunderstoreFormat(string modName)
        {
            // Replace spaces with underscores
            modName = modName.Replace(" ", "_");

            // Retain only allowed characters
            modName = new string(modName.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());

            return modName;
        }


        private void UpdateUI(string modName, string localVersion, string latestVersion)
        {
            // Instantiate the item without setting the parent
            var item = Instantiate(modAgeUIcomp.ModRowPlaceholder);

            // Set the parent after instantiation
            item.transform.SetParent(modAgeUIcomp.contentList.transform, false); // The 'false' argument ensures the object's local coordinates aren't modified during parenting

            var naming = Utils.FindChild(item.transform, "Naming");

            var modNameText = Utils.FindChild(naming.transform, "ModName").GetComponent<TextMeshProUGUI>();
            var modVersionText = Utils.FindChild(naming.transform, "ModVersion").GetComponent<TextMeshProUGUI>();
            var modStatusText = Utils.FindChild(naming.transform, "ModStatus").GetComponent<TextMeshProUGUI>();

            modNameText.text = modName;
            modVersionText.text = localVersion;
            modStatusText.text = (localVersion == latestVersion) ? "Up to date" : "Update available";
        }


        private IEnumerator GetAllModsFromThunderstore(System.Action<List<PackageInfo>?> callback)
        {
            string url = $"https://thunderstore.io/api/v1/package/";
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                webRequest.SetRequestHeader("Accept-Encoding", "gzip");
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(webRequest.error);
                    callback(null);
                }
                else
                {
                    string jsonData = webRequest.downloadHandler.text;

                    IDeserializer deserializer = new DeserializerBuilder().Build();
                    allPackagesInfo = deserializer.Deserialize<List<PackageInfo>>(jsonData);
                    callback(allPackagesInfo);
                }
            }
        }


        private void OnDestroy()
        {
            StopCoroutine(modcheckerCoroutine);
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
                ModAgeLogger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                ModAgeLogger.LogError($"There was an issue loading your {ConfigFileName}");
                ModAgeLogger.LogError("Please check your config entries for spelling and format!");
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
            [UsedImplicitly] public int? Order = null!;
            [UsedImplicitly] public bool? Browsable = null!;
            [UsedImplicitly] public string? Category = null!;
            [UsedImplicitly] public Action<ConfigEntryBase>? CustomDrawer = null!;
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
    }

    /// <summary>
    ///     Add default localization and instantiate the mod settings button in Fejd.
    /// </summary>
    [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.SetupGui))]
    static class FejdStartupSetupGuiPatch
    {
        static void Postfix(FejdStartup __instance)
        {
            // Set the modageUIgameobject to be a child of the main menu
            ModAgePlugin.modAgeUIObject.transform.SetParent(__instance.m_mainMenu.transform, false);
            try
            {
                var menuList = __instance.m_mainMenu.transform.Find("MenuList");
                CreateMenu(menuList);
            }
            catch (Exception ex)
            {
                ModAgePlugin.ModAgeLogger.LogWarning($"Exception caught while creating the Mod Settings: {ex}");
            }
        }

        /// <summary>
        ///     Create our own menu list entry when mod config is available
        /// </summary>
        /// <param name="menuList"></param>
        private static void CreateMenu(Transform menuList)
        {
            ModAgePlugin.ModAgeLogger.LogDebug("Instantiating Mod Age");

            var settingsFound = false;
            var mainMenuButtons = new List<Button>();
            for (int i = 0; i < menuList.childCount; i++)
            {
                if (menuList.GetChild(i).gameObject.activeInHierarchy &&
                    menuList.GetChild(i).name != "ModAge" &&
                    menuList.GetChild(i).TryGetComponent<Button>(out var menuButton))
                {
                    mainMenuButtons.Add(menuButton);
                }

                if (menuList.GetChild(i).name == "Settings")
                {
                    Transform modSettings = Object.Instantiate(menuList.GetChild(i), menuList);
                    modSettings.name = "ModAge";
                    modSettings.GetComponentInChildren<Text>().text = Localization.instance.Localize("$menu_title");
                    Button modSettingsButton = modSettings.GetComponent<Button>();
                    for (int j = 0; j < modSettingsButton.onClick.GetPersistentEventCount(); ++j)
                    {
                        modSettingsButton.onClick.SetPersistentListenerState(j, UnityEventCallState.Off);
                    }

                    modSettingsButton.onClick.RemoveAllListeners();
                    modSettingsButton.onClick.AddListener(() =>
                    {
                        try
                        {
                            // ModAgePlugin.Instance.StartCoroutine(CreateWindow(menuList));
                        }
                        catch (Exception ex)
                        {
                            ModAgePlugin.ModAgeLogger.LogWarning($"Exception caught while showing the Mod Age window: {ex}");
                        }
                    });
                    mainMenuButtons.Add(modSettingsButton);

                    Transform left = modSettings.Find("LeftKnot");
                    if (left != null)
                    {
                        var localPosition = left.localPosition;
                        localPosition = new Vector2(localPosition.x - 10f, localPosition.y);
                        left.localPosition = localPosition;
                    }

                    Transform right = modSettings.Find("RightKnot");
                    if (right != null)
                    {
                        var localPosition = right.localPosition;
                        localPosition = new Vector2(localPosition.x + 10f, localPosition.y);
                        right.localPosition = localPosition;
                    }

                    settingsFound = true;
                }
                else if (settingsFound)
                {
                    RectTransform rectTransform = menuList.GetChild(i).GetComponent<RectTransform>();
                    var anchoredPosition = rectTransform.anchoredPosition;
                    anchoredPosition = new Vector2(anchoredPosition.x, anchoredPosition.y - 40);
                    rectTransform.anchoredPosition = anchoredPosition;
                }
            }

            if (FejdStartup.instance != null)
            {
                FejdStartup.instance.m_menuButtons = mainMenuButtons.ToArray();
            }
        }
    }

    public class VersionInfo
    {
        public string? name { get; set; }
        public string? full_name { get; set; }
        public string? description { get; set; }
        public string? icon { get; set; }
        public string? version_number { get; set; }
        public List<string>? dependencies { get; set; }
        public string? download_url { get; set; }
        public int downloads { get; set; }
        public string? date_created { get; set; }
        public string? website_url { get; set; }
        public bool is_active { get; set; }
        public string? uuid4 { get; set; }
        public int file_size { get; set; }
    }

    public class PackageInfo
    {
        public string? name { get; set; }
        public string? full_name { get; set; } = string.Empty;
        public string? owner { get; set; }
        public string? package_url { get; set; }
        public string? donation_link { get; set; }
        public string? date_created { get; set; }
        public string? date_updated { get; set; }
        public string? uuid4 { get; set; }
        public int rating_score { get; set; }
        public bool is_pinned { get; set; }
        public bool is_deprecated { get; set; }
        public bool has_nsfw_content { get; set; }
        public List<string>? categories { get; set; }
        public List<VersionInfo>? versions { get; set; }
    }
}