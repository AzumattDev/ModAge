﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using BepInEx.Bootstrap;
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
        ModAgePlugin.modAgeUIAsset = assetBundle.LoadAsset<GameObject>("ModAgeUI");
        ModAgePlugin.modAgeUIAsset.AddComponent<Localize>();
        ModAgePlugin.modAgeUIcomp = ModAgePlugin.modAgeUIAsset.GetComponent<ModAgeUI>();
        assetBundle?.Unload(false);
    }

    internal static IEnumerator LoadSpriteFromURL(string? imageURL, Action<Sprite> callback)
    {
        if (imageURL == null)
        {
            callback(null);
            yield break;
        }

        UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageURL);
        yield return www.SendWebRequest();

        if (www.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error while fetching image: " + www.error);
            callback(null);
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
                Debug.LogError(webRequest.error);
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
        if (ModAgePlugin.allPackagesInfo == null)
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
        }
    }

    internal static void CompareLocalModsToThunderstore()
    {
        var plugins = Chainloader.PluginInfos;
        foreach (var pluginInfo in plugins.Values)
        {
            string modNameInThunderstoreFormat = Utilities.ConvertToThunderstoreFormat(pluginInfo.Metadata.Name);

            // Find the mod using the converted name.
            PackageInfo? matchedMod = ModAgePlugin.allPackagesInfo?.FirstOrDefault(x => x.name.Equals(modNameInThunderstoreFormat, StringComparison.OrdinalIgnoreCase));
            ModAgePlugin.ModAgeLogger.LogDebug($"Original Mod Name: {pluginInfo.Metadata.Name}");
            ModAgePlugin.ModAgeLogger.LogDebug($"Converted Mod Name: {modNameInThunderstoreFormat}");

            // If the mod is found and has versions, get the latest one.
            if (matchedMod is { versions.Count: > 0 })
            {
                string? latestVersion = matchedMod.versions[0].version_number; // Assuming versions are sorted with latest first.
                UpdateUI(pluginInfo.Metadata.Name, pluginInfo.Metadata.Version.ToString(), latestVersion, matchedMod);
                //Debug.Log($"Match found for {modNameInThunderstoreFormat}");
                ModAgePlugin.ModAgeLogger.LogDebug($"Match found for {modNameInThunderstoreFormat}");
            }
            else
            {
                ModAgePlugin.ModAgeLogger.LogWarning($"No match found for {modNameInThunderstoreFormat}");
            }
        }
        ModAgePlugin.modAgeUIFinal.SetActive(false);
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
        RectTransform? item = Object.Instantiate(ModAgePlugin.modAgeUIcomp.ModRowPlaceholder, ModAgePlugin.modAgeUIcomp.contentList.transform, false);
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
            Debug.LogError($"No version info found for {packageInfo.name}");
            return;
        }


        DateTime dt = DateTime.Parse(versionInfo.date_created, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        string formattedDate = dt.ToString("MMMM dd, yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);

        // If dt is less than August 22nd 2023 at 8:30AM, then the mod placeholder text should say it's older than the Hildir Update.
        inputPlaceholder.text = dt < new DateTime(2023, 8, 22, 8, 30, 0)
            ? $"Last Updated:\n {formattedDate}\n<color=red>This mod is older than the Hildir Update!</color>"
            : $"Last Updated:\n {formattedDate}\n<color=green>This mod is newer than the Hildir Update!</color>";
        ModAgePlugin.Instance.StartCoroutine(Utilities.LoadSpriteFromURL(packageInfo.versions?[0].icon, (sprite) =>
        {
            if (sprite != null)
            {
                modIcon.sprite = sprite;
            }
        }));
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