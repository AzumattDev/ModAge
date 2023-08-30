using System;
using System.Collections.Generic;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = System.Object;

namespace ModAge;

[HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.LoadMainScene))]
static class FejdStartupLoadMainScenePatch
{
    static void Prefix(FejdStartup __instance)
    {
        if (ModAgePlugin.modAgeUIFinal != null)
        {
            // Destroy it
            UnityEngine.Object.Destroy(ModAgePlugin.modAgeUIFinal);
            ModAgePlugin.modAgeUIFinal = null;
        }
    }
}

[HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.SetupGui))]
static class FejdStartupSetupGuiPatch
{
    static void Postfix(FejdStartup __instance)
    {
        try
        {
            if (__instance.m_mainMenu.transform != null && ModAgePlugin.modAgeUIFinal != null)
            {
                ModAgePlugin.modAgeUIFinal.transform.SetParent(__instance.m_mainMenu.transform, false);
                var menuList = Utils.FindChild(__instance.m_mainMenu.transform, "MenuEntries");
                CreateMenu(menuList);
            }
            else if (__instance.m_mainMenu.transform != null && ModAgePlugin.modAgeUIFinal == null)
            {
                var instantiatedObject = UnityEngine.Object.Instantiate(ModAgePlugin.modAgeUIAsset);
                ModAgePlugin.modAgeUIFinal = instantiatedObject;
                ModAgePlugin.modAgeUIcomp = instantiatedObject.GetComponent<ModAgeUI>();
                ModAgePlugin.DontDestroyOnLoad(ModAgePlugin.modAgeUIFinal);
                ModAgePlugin.modAgeUIFinal.transform.SetParent(__instance.m_mainMenu.transform, false);
                var menuList = Utils.FindChild(__instance.m_mainMenu.transform, "MenuEntries");
                FejdStartupSetupGuiPatch.CreateMenu(menuList);
                Utilities.CompareLocalModsToThunderstore();
            }
        }
        catch (Exception ex)
        {
            ModAgePlugin.ModAgeLogger.LogWarning($"Exception caught while creating the Mod Age Menu Option: {ex}");
        }
    }

    internal static void CreateMenu(Transform menuList)
    {
        ModAgePlugin.ModAgeLogger.LogDebug("Instantiating Mod Age");

        var settingsFound = false;
        var mainMenuButtons = new List<Button>();
        for (int i = 0; i < menuList.childCount; ++i)
        {
            if (menuList.GetChild(i).gameObject.activeInHierarchy &&
                menuList.GetChild(i).name != "ModAge" &&
                menuList.GetChild(i).TryGetComponent<Button>(out var menuButton))
            {
                mainMenuButtons.Add(menuButton);
            }

            if (menuList.GetChild(i).name == "Settings")
            {
                Transform modSettings = UnityEngine.Object.Instantiate(menuList.GetChild(i), menuList);
                modSettings.name = "ModAge";
                modSettings.GetComponentInChildren<TextMeshProUGUI>().text = Localization.instance.Localize("$menu_title");
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
                        ModAgePlugin.modAgeUIFinal.SetActive(!ModAgePlugin.modAgeUIFinal.activeSelf);
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