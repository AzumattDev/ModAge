namespace ModAge;

public class modnames
{
    public static string CleanName(string name)
    {
        string cleaned = name.Trim().Replace(" ", "").Replace("_", "").Replace("-", "");

        switch (cleaned)
        {
            case "CreatureLevel&LootControl":
                return "CreatureLevelAndLootControl";
            case "SpawnThat!":
                return "SpawnThat";
            case "DropThat!":
                return "DropThat";
            case "PotionsPlus":
                return "PotionPlus";
            case "Detalhes.EraSystem":
                return "ValheimEraSystemVAS";
            case "kgladder":
                return "BetterLadders";
            case "OdinPlusQOL":
                return "OdinsQOL";
            case "Friendlies":
            case "FriendliesAssets":
            case "FriendliesAI":
                return "FriendliesReloaded";
            case "SkillInjectorMod":
                return "SkillInjector";
            case "SmartContainersMod":
                return "SmartContainers";
            case "UsefulTrophiesMod":
                return "UsefulTrophies";
            case "BlacksmithTools":
                return "BlacksmithsTools";
            case "Basements":
                return "BasementJVLedition";
            case "SpeedyPathsMod":
                return "SpeedyPaths";
            case "EpicValheimsAdditionsbyHuntard":
                return "EpicValheimsAdditions";
            case "AzuMarketplaceSigns":
                return "MarketplaceSigns";
            case "DigitalrootMaxDungeonRooms":
                return "DigitalrootValheimMaxDungeonRooms";
            case "MorePlayerClothColliders":
                return "MoreandModifiedPlayerClothColliders";
            case "CraftyCarts":
                return "CraftyCartsRemake";
            case "BronzeStoneworking":
                return "BronzeStonecutting";
            case "RagnarsRökareMobAI":
            case "RagnarsRÃ¶kareMobAI":
                return "MobAILib";
            case "MossBuild":
                return "BalrondMossBuilds";
            case "AFeedBalrondTrough":
                return "BalrondTrough";
            case "BalrondBarrell":
                return "BalrondBarrel";
            case "BalrondMetalLocker":
                return "BalrondMetalShelf";
            case "AllTameableOverhaul":
                return "AllTameableTamingOverhaul";
            case "KrumpacZMonsters":
                return "Monsters";
            case "MrSerjiConstruction":
                return "Construction";
            case "TrashItemsMod":
                return "TrashItems";
            case "JotunntheValheimLibrary":
                return "Jotunn";
            default:
                if (cleaned.Contains("RainbowTrollArmor"))
                    return "RainbowTrollArmor";
                if (cleaned.Contains("Detalhes."))
                    return cleaned.Replace("Detalhes.", "");
                if (cleaned.Contains("ValheimExpanded"))
                    return "ValheimExpanded";
                return cleaned;
        }
    }
}