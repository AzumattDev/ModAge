using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using LocalizationManager;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using YamlDotNet.Serialization;
using Object = UnityEngine.Object;

namespace ModAge;

public class Utilities
{
    public static AssetBundle GetAssetBundleFromResources(string filename)
    {
        Assembly execAssembly = Assembly.GetExecutingAssembly();
        string resourceName = execAssembly.GetManifestResourceNames().Single(str => str.EndsWith(filename));
        using (Stream? stream = execAssembly.GetManifestResourceStream(resourceName))
        {
            return AssetBundle.LoadFromStream(stream);
        }
    }

    public static void LoadAssets()
    {
        AssetBundle? assetBundle = GetAssetBundleFromResources("modage");
        ModAgePlugin.modAgeUIAsset = assetBundle.LoadAsset<GameObject>("ModAgeUI");
        ModAgePlugin.modAgeUIAsset.AddComponent<Localize>();
        ModAgePlugin.modAgeUIcomp = ModAgePlugin.modAgeUIAsset.GetComponent<ModAgeUI>();
        assetBundle?.Unload(false);
    }

    internal static IEnumerator LoadSpriteFromURL(string? imageURL, Action<Sprite> callback)
    {
        if (imageURL == null)
        {
            callback(null!);
            yield break;
        }

        UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageURL);
        yield return www.SendWebRequest();

        if (www.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
        {
            ModAgePlugin.ModAgeLogger.LogError("Error while fetching image: " + www.error);
            callback(null!);
        }
        else
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(www);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            callback(sprite);
        }
    }

    private static IEnumerator GetAllModsFromThunderstore(System.Action<List<PackageInfo>?> callback)
    {
        string url = $"https://thunderstore.io/c/valheim/api/v1/package/";
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            webRequest.SetRequestHeader("Accept-Encoding", "gzip");
            yield return webRequest.SendWebRequest();

            if (webRequest.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
            {
                ModAgePlugin.ModAgeLogger.LogError(webRequest.error);
                callback(null);
            }
            else
            {
                string jsonData = webRequest.downloadHandler.text;

                IDeserializer deserializer = new DeserializerBuilder().Build();
                ModAgePlugin.allPackagesInfo = deserializer.Deserialize<List<PackageInfo>>(jsonData);
                callback(ModAgePlugin.allPackagesInfo);
            }
        }
    }

    private static IEnumerator GetAllModsModVersionCheckBot(System.Action<Dictionary<string, PreparedPackageInfo>?> callback)
    {
        string url = $"https://mod-version-check.eu/api/experimental/prepared-mods";
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            webRequest.SetRequestHeader("Accept-Encoding", "gzip");
            yield return webRequest.SendWebRequest();

            if (webRequest.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
            {
                ModAgePlugin.ModAgeLogger.LogError(webRequest.error);
                callback(null);
            }
            else
            {
                string jsonData = webRequest.downloadHandler.text;

                IDeserializer deserializer = new DeserializerBuilder().Build();
                ModAgePlugin.allPreparedPackagesInfo = deserializer.Deserialize<Dictionary<string, PreparedPackageInfo>>(jsonData);
                callback(ModAgePlugin.allPreparedPackagesInfo);
            }
        }
    }

    internal static string ConvertToThunderstoreFormat(string modName)
    {
        // Replace spaces with underscores
        modName = modName.Replace(" ", "_");

        // Retain only allowed characters
        modName = new string(modName.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());

        return modName;
    }

    internal static IEnumerator CheckModUpdates()
    {
        /*if (ModAgePlugin.allPackagesInfo == null)
        {
            yield return GetAllModsFromThunderstore((result) =>
            {
                if (result != null)
                {
                    ModAgePlugin.CanCompareMods = true;
                }
            });
        }
        else
        {
            CompareLocalModsToThunderstore();
        }*/

        if (ModAgePlugin.allPreparedPackagesInfo == null)
        {
            yield return GetAllModsModVersionCheckBot((result) =>
            {
                if (result != null)
                {
                    ModAgePlugin.ModAgeLogger.LogDebug("Got prepared mods from mod-version-check.eu");
                    ModAgePlugin.CanCompareMods = true;
                }
                else
                {
                    ModAgePlugin.ModAgeLogger.LogError("Error: Could not get prepared mods from mod-version-check.eu");
                }
            });
        }
        else
        {
            CompareLocalModsToPreparedPackage();
        }
    }

    internal static void CompareLocalModsToThunderstore()
    {
        Dictionary<string, PluginInfo>? plugins = Chainloader.PluginInfos;
        foreach (PluginInfo? pluginInfo in plugins.Values)
        {
            string modNameInThunderstoreFormat = Utilities.ConvertToThunderstoreFormat(pluginInfo.Metadata.Name);

            // Find the mod using the converted name.
            PackageInfo? matchedMod = ModAgePlugin.allPackagesInfo?.FirstOrDefault(x => x.name != null && x.name.Equals(modNameInThunderstoreFormat, StringComparison.OrdinalIgnoreCase));
            ModAgePlugin.ModAgeLogger.LogDebug($"Original Mod Name: {pluginInfo.Metadata.Name}");
            ModAgePlugin.ModAgeLogger.LogDebug($"Converted Mod Name: {modNameInThunderstoreFormat}");

            // If the mod is found and has versions, get the latest one.
            if (matchedMod is { versions.Count: > 0 })
            {
                string? latestVersion = matchedMod.versions[0].version_number; // Assuming versions are sorted with latest first.
                UpdateUI(pluginInfo.Metadata.Name, pluginInfo.Metadata.Version.ToString(), latestVersion, matchedMod);
                //ModAgePlugin.ModAgeLogger.Log($"Match found for {modNameInThunderstoreFormat}");
                ModAgePlugin.ModAgeLogger.LogDebug($"Match found for {modNameInThunderstoreFormat}");
            }
            else
            {
                ModAgePlugin.ModAgeLogger.LogWarning($"No match found for {modNameInThunderstoreFormat}");
            }
        }

        ModAgePlugin.modAgeUIFinal.SetActive(false);
    }

    internal static void CompareLocalModsToPreparedPackage()
    {
        Dictionary<string, PluginInfo>? plugins = Chainloader.PluginInfos;
        foreach (PluginInfo? pluginInfo in plugins.Values)
        {
            // Find the mod using the converted name.
            KeyValuePair<string, PreparedPackageInfo>? matchedMod = ModAgePlugin.allPreparedPackagesInfo?.FirstOrDefault(
                x => x.Value.clean_name != null && x.Value.clean_name.Equals(
                    pluginInfo.Metadata.Name.ToLower(), StringComparison.OrdinalIgnoreCase));

            // If the mod is found and has versions, get the latest one.
            if (matchedMod?.Value is not { version.Length: > 0 }) continue;
            string? latestVersion = matchedMod.Value.Value.version; // Assuming versions are sorted with latest first.
            UpdateUIPrepackaged(pluginInfo.Metadata.Name, pluginInfo.Metadata.Version.ToString(), latestVersion, matchedMod);
        }
    }


    private static void UpdateUI(string modName, string? localVersion, string? latestVersion, PackageInfo packageInfo)
    {
        System.Version localVer = ParseVersion(localVersion);
        System.Version onlineVer = ParseVersion(latestVersion);
        if (onlineVer == localVer || ParseVersion(localVersion + ".0") == onlineVer) // If both versions are the same, do nothing.
        {
            return;
        }

        // Instantiate the item without setting the parent
        RectTransform? item = Object.Instantiate(ModAgePlugin.modAgeUIcomp.Placeholder, ModAgePlugin.modAgeUIcomp.contentList.transform, false);
        item.gameObject.SetActive(true);

        Transform? naming = Utils.FindChild(item.transform, "Naming");
        Transform? rightCol = Utils.FindChild(item.transform, "Right column");

        TextMeshProUGUI? modNameText = Utils.FindChild(naming.transform, "ModName").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI? modVersionText = Utils.FindChild(naming.transform, "ModVersion").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI? modStatusText = Utils.FindChild(naming.transform, "ModStatus").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI? modLinkButtonText = Utils.FindChild(rightCol.transform, "ModLinkButtonText").GetComponent<TextMeshProUGUI>();
        Button? modLinkButton = Utils.FindChild(rightCol.transform, "ModLinkButton").GetComponent<Button>();
        TextMeshProUGUI? inputPlaceholder = Utils.FindChild(rightCol.transform, "Placeholder").GetComponent<TextMeshProUGUI>();
        Image? modIcon = Utils.FindChild(item.transform, "ModIcon").GetComponent<Image>();
        
        modNameText.text = packageInfo.full_name;
        modVersionText.text = $"Installed ({localVersion})";
        modStatusText.text = onlineVer > localVer
            ? $"<color=red>Update available: {onlineVer}</color>"
            : $"Test version: Live Version is {onlineVer}"; // onlineVer < localVer
        modLinkButtonText.text = $"{packageInfo.name} on Thunderstore";
        modLinkButton.onClick.AddListener(() => Application.OpenURL(packageInfo.package_url));

        // Get the creation date where the packageInfo version is equal to the local version. If it's not found, use the first version.
        VersionInfo? versionInfo = packageInfo.versions?.FirstOrDefault(x => x.version_number == localVersion) ?? packageInfo.versions?[0];
        if (versionInfo == null)
        {
            ModAgePlugin.ModAgeLogger.LogError($"No version info found for {packageInfo.name}");
            return;
        }


        DateTime dt = DateTime.Parse(versionInfo.date_created, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        string formattedDate = dt.ToString("MMMM dd, yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);

        // If dt is less than the configured date, then the mod placeholder text should say it's older than the whatever Update.
        inputPlaceholder.text = dt < new DateTime(ModAgePlugin.yearConfig.Value, ModAgePlugin.monthConfig.Value, ModAgePlugin.dayConfig.Value, ModAgePlugin.hourConfig.Value, ModAgePlugin.minuteConfig.Value, ModAgePlugin.secondConfig.Value)
            ? $"Last Updated:\n {formattedDate}\n<color=red>This mod is older than the target Update!</color>"
            : $"Last Updated:\n {formattedDate}\n<color=green>This mod is newer than the target Update!</color>";
        ModAgePlugin.modcheckerImageCoroutine = ModAgePlugin.Instance.StartCoroutine(Utilities.LoadSpriteFromURL(packageInfo.versions?[0].icon, (sprite) =>
        {
            if (sprite != null)
            {
                modIcon.sprite = sprite;
            }
        }));
    }

    private static void UpdateUIPrepackaged(string modName, string? localVersion, string? latestVersion, KeyValuePair<string, PreparedPackageInfo>? packageInfo)
    {
        System.Version localVer = ParseVersion(localVersion);
        System.Version onlineVer = ParseVersion(latestVersion);

        DateTime dt = DateTime.Parse(packageInfo?.Value.updated, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        DateTime dtLocal = dt.ToLocalTime();
        string formattedDate = dtLocal.ToString("MMMM dd, yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
        DateTime targetDateLocal = new DateTime(ModAgePlugin.yearConfig.Value, ModAgePlugin.monthConfig.Value, ModAgePlugin.dayConfig.Value, ModAgePlugin.hourConfig.Value, ModAgePlugin.minuteConfig.Value, ModAgePlugin.secondConfig.Value, DateTimeKind.Local);
        bool isNotUpdated = dtLocal < targetDateLocal;
        bool shouldShow = ModAgePlugin.showAllMods.Value == ModAgePlugin.Toggle.On || isNotUpdated || onlineVer > localVer;
        if (shouldShow)
        {
            RectTransform? item = Object.Instantiate(ModAgePlugin.modAgeUIcomp.Placeholder, ModAgePlugin.modAgeUIcomp.contentList.transform, false);
            item.gameObject.SetActive(true);
            item.TryGetComponent<ModAgeUIPlaceholder>(out ModAgeUIPlaceholder? placeholder);
            if (!placeholder || packageInfo == null) return;
            placeholder.PlaceholderModName.text = $"{packageInfo?.Value.name} <color=#DB8000>by {packageInfo?.Value.icon_url?.Split('/').Last().Split('-')[0]}</color>";
            if (onlineVer > localVer)
            {
                placeholder.PlaceholderVersionInstalled.text = "$modage_anewversionavailable";
                placeholder.PlaceholderVersionAvailable.text = $"<color=#CC5500>{onlineVer}</color>";
            }
            else
            {
                placeholder.PlaceholderVersionInstalled.text = $"$modage_versioninstalled: {localVer}";
                placeholder.PlaceholderVersionAvailable.text = $"$modage_versionavailable: {onlineVer}";
            }

            placeholder.PlaceholderLastUpdated.text = $"$modage_lastupdated: {formattedDate}";

            placeholder.PlaceholderGameUpdatedBool.text = isNotUpdated
                ? $"$modage_gameupdatemessage: <color=#CC5500>$menu_no</color>"
                : $"$modage_gameupdatemessage: <color=#2b932e>$menu_yes</color>";
            placeholder.PlaceholderMoreInfoButton.GetComponentInChildren<TextMeshProUGUI>().text = $"$modage_moreinformation";
            placeholder.PlaceholderMoreInfoButton.onClick.AddListener(() => Application.OpenURL(packageInfo?.Value.urls?[0]));
            ModAgePlugin.modcheckerImageCoroutine =  ModAgePlugin.Instance.StartCoroutine(Utilities.LoadSpriteFromURL(packageInfo?.Value.icon_url, (sprite) =>
            {
                if (sprite != null)
                {
                    placeholder.PlaceholderModImage.sprite = sprite;
                }
            }));
            
            if (item.gameObject.TryGetComponent<Localize>(out Localize? localize))
            {
                localize.RelocalizeAllUponChange();
            }
            else
            {
                item.gameObject.AddComponent<Localize>().RelocalizeAllUponChange();
            }
        }
    }

    internal static System.Version ParseVersion(string? input)
    {
        try
        {
            if (input != null)
            {
                System.Version ver = System.Version.Parse(input);
                return ver;
            }
        }
        catch (ArgumentNullException)
        {
            ModAgePlugin.ModAgeLogger.LogError("Error: String to be parsed is null.");
        }
        catch (ArgumentOutOfRangeException)
        {
            ModAgePlugin.ModAgeLogger.LogError($"Error: Negative value in '{input}'.");
        }
        catch (ArgumentException)
        {
            ModAgePlugin.ModAgeLogger.LogError($"Error: Bad number of components in '{input}'.");
        }
        catch (FormatException)
        {
            ModAgePlugin.ModAgeLogger.LogError($"Error: Non-integer value in '{input}'.");
        }
        catch (OverflowException)
        {
            ModAgePlugin.ModAgeLogger.LogError($"Error: Number out of range in '{input}'.");
        }

        return System.Version.Parse(ModAgePlugin.ModVersion);
    }
}