using System;
using System.Collections.Generic;
using UnityEngine;

namespace RecipeBook
{
    // Which stage of the game a recipe belongs to (before / after each boss), from the materials it needs:
    // the latest material decides. Materials not in the list take the stage of the recipe that makes them,
    // else Meadows. Modded materials land where their own ingredients put them.
    internal static class Stages
    {
        internal const int Count = 7;
        internal static readonly string[] Short = { "", "Meadows", "Black Forest", "Swamp", "Mountain", "Plains", "Mistlands", "Ashlands" };
        internal static readonly string[] Long =
        {
            "", "Meadows · start", "Black Forest · after Eikthyr", "Swamp · after The Elder", "Mountain · after Bonemass",
            "Plains · after Moder", "Mistlands · after Yagluth", "Ashlands · after The Queen",
        };

        private static readonly Dictionary<string, int> Table = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, int> Cache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        static Stages()
        {
            Put(1, "Wood", "Stone", "Flint", "LeatherScraps", "DeerHide", "Resin", "Feathers", "BoneFragments", "Raspberry", "Mushroom",
                "Honey", "NeckTail", "RawMeat", "Dandelion", "DeerMeat", "MushroomYellow", "TrophyDeer", "TrophyBoar", "TrophyNeck", "QueenBee");
            Put(2, "HardAntler", "TrophyEikthyr", "Copper", "CopperOre", "Tin", "TinOre", "Bronze", "BronzeNails", "FineWood", "RoundLog",
                "TrollHide", "GreydwarfEye", "SurtlingCore", "AncientSeed", "Coal", "Blueberries", "Carrot", "CarrotSeeds", "Thistle",
                "TrophyGreydwarf", "TrophyFrostTroll", "TrophySkeleton", "BeechSeeds", "PineCone", "FirCone", "CopperScrap", "Bjorn", "BjornHide", "BjornPaw");
            Put(3, "TrophyTheElder", "CryptKey", "Iron", "IronScrap", "IronOre", "IronNails", "ElderBark", "Guck", "Chain", "Bloodbag", "Entrails",
                "Ooze", "Root", "Chitin", "Turnip", "TurnipSeeds", "WitheredBone", "SerpentScale", "SerpentMeat", "TrophyDraugr", "TrophyAbomination", "TrophyLeech");
            Put(4, "TrophyBonemass", "Wishbone", "Silver", "SilverOre", "WolfPelt", "WolfFang", "WolfClaw", "WolfHairBundle", "WolfMeat", "Obsidian",
                "FreezeGland", "Crystal", "DragonEgg", "JuteRed", "Onion", "OnionSeeds", "TrophyWolf", "TrophyHatchling", "TrophyFenring", "TrophyUlv",
                "TrophySGolem", "TrophyCultist", "BearMeat", "Bjorn_sleeping");
            Put(5, "TrophyDragonQueen", "DragonTear", "BlackMetal", "BlackMetalScrap", "LoxPelt", "LoxMeat", "Needle", "Barley", "BarleyFlour", "Flax",
                "LinenThread", "Tar", "GoblinTotem", "Cloudberry", "TrophyLox", "TrophyDeathsquito", "TrophyGoblin", "TrophyGrowth", "TrophyGoblinShaman", "TrophyGoblinBrute");
            Put(6, "TrophyGoblinKing", "YagluthDrop", "Eitr", "BlackCore", "Carapace", "Mandible", "Softtissue", "YggdrasilWood", "ScaleHide", "BlackMarble",
                "JuteBlue", "Sap", "Wisp", "RoyalJelly", "GiantBloodSack", "Bilebag", "BugMeat", "HareMeat", "MushroomJotunPuffs", "MushroomMagecap",
                "TrophyGjall", "TrophySeeker", "TrophySeekerBrute", "TrophyTick", "TrophyHare", "TrophyDvergr", "DvergrNeedle", "DvergrKeyFragment",
                "ChickenMeat", "ChickenEgg", "Feaster", "BlackSoup");
            Put(7, "TrophySeekerQueen", "QueenDrop", "Flametal", "FlametalNew", "FlametalOre", "FlametalOreNew", "CharredBone", "AskHide", "AsksvinMeat",
                "Blackwood", "Grausten", "CelestialFeather", "MorgenSinew", "MorgenHeart", "CharcoalResin", "BellFragment", "CharredCogwheel", "MoltenCore",
                "ProustitePowder", "SulfurStone", "Fiddleheadfern", "Vineberry", "VineberrySeeds", "MushroomSmokePuff", "BonemawSerpentTooth",
                "BonemawSerpentScale", "VoltureEgg", "VoltureMeat", "AsksvinEgg", "PowderedDragonEgg", "TrophyAsksvin", "TrophyCharredMelee",
                "TrophyMorgen", "TrophyVolture", "TrophyFallenValkyrie", "TrophyBonemawSerpent", "FaderDrop", "TrophyFader");
        }

        private static void Put(int stage, params string[] names) { foreach (string n in names) Table[n] = stage; }

        internal static void Reset() { Cache.Clear(); }

        internal static int Of(Recipe r) { return Of(r, 0); }

        private static int Of(Recipe r, int depth)
        {
            int best = 1;
            if (r == null || r.m_resources == null) return best;
            foreach (Piece.Requirement q in r.m_resources)
                if (q != null && q.m_resItem != null) best = Math.Max(best, Material(q.m_resItem, depth));
            return best;
        }

        private static int Material(ItemDrop item, int depth)
        {
            string prefab = Utils.GetPrefabName(item.gameObject);
            int s;
            if (Table.TryGetValue(prefab, out s)) return s;
            if (Cache.TryGetValue(prefab, out s)) return s;
            s = 1;
            if (depth < 3 && ObjectDB.instance != null)
            {
                string key = item.m_itemData.m_shared.m_name;
                foreach (Recipe r in ObjectDB.instance.m_recipes)
                    if (r != null && r.m_item != null && r.m_item.m_itemData.m_shared.m_name == key) { s = Math.Max(s, Of(r, depth + 1)); break; }
            }
            Cache[prefab] = s;
            return s;
        }

        internal static bool Present(int stage, List<Book.Entry> all)
        {
            foreach (Book.Entry e in all) if (e.Stage == stage) return true;
            return false;
        }
    }
}
