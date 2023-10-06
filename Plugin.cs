using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LocalizationManager;
using UnityEngine;

namespace ModAge
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class ModAgePlugin : BaseUnityPlugin
    {
        internal const string ModName = "ModAge";
        internal const string ModVersion = "1.0.2";
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
        internal static List<PackageInfo>? allPackagesInfo = null;
        internal static Dictionary<string, PreparedPackageInfo>? allPreparedPackagesInfo = null;
        internal static bool CanCompareMods = false;

        public void Awake()
        {
            Instance = this;
            Localizer.Load();
            Utilities.LoadAssets();
            
            var instantiatedObject = Instantiate(modAgeUIAsset);
            modAgeUIFinal = instantiatedObject;
            modAgeUIcomp = instantiatedObject.GetComponent<ModAgeUI>();
            DontDestroyOnLoad(modAgeUIFinal);
            ModAgePlugin.modAgeUIFinal.SetActive(false);
            modcheckerCoroutine = StartCoroutine(Utilities.CheckModUpdates());

            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
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


        private void OnDestroy()
        {
            StopCoroutine(modcheckerCoroutine);
        }
    }
}