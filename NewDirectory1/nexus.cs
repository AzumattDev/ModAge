using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Linq;
using ModAge;

public class NexusModFetcher
{
    private static readonly HttpClient httpClient = new HttpClient();

    private static string ModRoute(int modId) => $"https://api.nexusmods.com/v1/games/valheim/mods/{modId}.json";
    private static string UpdatedRoute() => "https://api.nexusmods.com/v1/games/valheim/mods/updated.json?period=1m";
    private static readonly string appVersion = "YOUR_APP_VERSION_HERE"; // Replace with your app's version
    private static readonly string NEXUS_API_KEY = "YOUR_API_KEY_HERE"; // Replace with your actual key
    private static readonly string filePath = "data/nexus_mods.json";

    private static Dictionary<string, string> defaultHeaders = new Dictionary<string, string>
    {
        { "apikey", NEXUS_API_KEY },
        { "Application-Name", "Valheim Mod Version Check" },
        { "Application-Version", appVersion }
    };

    private static void FetchMod(Dictionary<string, object> mods, int modId, bool force = false)
    {
        string idStr = modId.ToString();

        if (!force && mods.ContainsKey(idStr))
            return;

        if (!mods.ContainsKey(idStr))
            ModAgePlugin.ModAgeLogger.LogInfo($"Found new mod from Nexus with id {modId}");
        else
            ModAgePlugin.ModAgeLogger.LogInfo($"Updating mod from Nexus with id {modId}");

        var response = httpClient.GetAsync(ModRoute(modId)).Result;

        if (response.IsSuccessStatusCode)
            mods[idStr] = JsonSerializer.Deserialize<Dictionary<string, object>>(response.Content.ReadAsStringAsync().Result);
        else
            mods[idStr] = null;

        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        File.WriteAllText(filePath, JsonSerializer.Serialize(mods, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static int? GetHighestIdOfUpdatedMods()
    {
        try
        {
            var response = httpClient.GetAsync(UpdatedRoute()).Result;
            if (response.IsSuccessStatusCode)
            {
                var jsonContent = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(response.Content.ReadAsStringAsync().Result);
                return Convert.ToInt32(jsonContent.OrderByDescending(x => Convert.ToInt32(x["mod_id"])).First()["mod_id"]);
            }
            else
            {
                ModAgePlugin.ModAgeLogger.LogInfo($"Failed to fetch updated mods with status code {response.StatusCode}");
                return null;
            }
        }
        catch (Exception e)
        {
            ModAgePlugin.ModAgeLogger.LogInfo($"Failed to fetch updated nexus mods: {e}");
            return null;
        }
    }

    private static void AddNewMods(Dictionary<string, object> mods)
    {
        int? highestId = GetHighestIdOfUpdatedMods();

        if (highestId.HasValue)
            for (int modId = 0; modId < highestId; modId++)
                FetchMod(mods, modId + 1);
    }

    private static void UpdateMods(Dictionary<string, object> mods)
    {
        try
        {
            var response = httpClient.GetAsync(UpdatedRoute()).Result;
            if (response.IsSuccessStatusCode)
            {
                var updated = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(response.Content.ReadAsStringAsync().Result);
                foreach (var mod in updated)
                {
                    string modId = mod["mod_id"].ToString();
                    if (Convert.ToInt32(mod["latest_file_update"]) > Convert.ToInt32(mods[modId]["updated_timestamp"]))
                        FetchMod(mods, Convert.ToInt32(modId), true);
                }
            }
            else
            {
                ModAgePlugin.ModAgeLogger.LogInfo($"Failed to fetch updated mods with status code {response.StatusCode}");
            }
        }
        catch (Exception e)
        {
            ModAgePlugin.ModAgeLogger.LogInfo($"Failed to fetch updated nexus mods: {e}");
        }
    }

    public static Dictionary<string, object> FetchOnline()
    {
        if (string.IsNullOrEmpty(NEXUS_API_KEY))
            return new Dictionary<string, object>();

        var mods = new Dictionary<string, object>();
        if (File.Exists(filePath))
            mods = JsonSerializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(filePath));

        AddNewMods(mods);
        UpdateMods(mods);

        return mods;
    }
}