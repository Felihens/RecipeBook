namespace RecipeBook
{
    // The same item categories as the Storage window's chest tags (Hoardkeeper), so the filters match.
    // A recipe's tag is the category of the item it makes.
    internal static class Tags
    {
        internal static readonly string[] All =
        {
            "Weapons", "Ammo", "Armor", "Tools", "Food", "Meads", "Meat", "Fish",
            "Crops", "Seeds", "Ores", "Metals", "Wood", "Stone", "Hides", "Creature",
            "Valuables", "Trophies", "Materials", "Misc"
        };

        private static bool Has(string name, params string[] parts)
        {
            foreach (string s in parts) if (name.Contains(s)) return true;
            return false;
        }

        // which category an item belongs to (by type, then by its internal name for materials)
        internal static string Category(ItemDrop item)
        {
            ItemDrop.ItemData.SharedData d = item.m_itemData.m_shared;
            string n = (d.m_name ?? "").ToLowerInvariant();
            string id = Utils.GetPrefabName(item.gameObject);
            // by the item's prefab name first: things the rules below would get wrong
            if (id.StartsWith("Beastkeeper_")) return "Tools";
            if (id.StartsWith("Catapult") || id.StartsWith("BombSiege")) return "Ammo";
            if (id.StartsWith("MeadBase") || id == "BarleyWineBase") return "Meads";
            if ((id.StartsWith("Feast") && id.EndsWith("_Material")) || id.StartsWith("Spice") || id.EndsWith("Flour") || id == "BreadDough") return "Food";
            if (id.EndsWith("Uncooked") && !Gear(id)) return "Food";
            if (d.m_itemType == ItemDrop.ItemData.ItemType.Material && (id.StartsWith("Mold") || id.StartsWith("Orb") || id.StartsWith("Staff") || id.EndsWith("Uncooked"))) return "Materials";
            switch (d.m_itemType)
            {
                case ItemDrop.ItemData.ItemType.OneHandedWeapon:
                case ItemDrop.ItemData.ItemType.TwoHandedWeapon:
                case ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft:
                case ItemDrop.ItemData.ItemType.Bow:
                case ItemDrop.ItemData.ItemType.Shield:
                case ItemDrop.ItemData.ItemType.Attach_Atgeir: return "Weapons";
                case ItemDrop.ItemData.ItemType.Ammo:
                case ItemDrop.ItemData.ItemType.AmmoNonEquipable: return "Ammo";
                case ItemDrop.ItemData.ItemType.Helmet:
                case ItemDrop.ItemData.ItemType.Chest:
                case ItemDrop.ItemData.ItemType.Legs:
                case ItemDrop.ItemData.ItemType.Hands:
                case ItemDrop.ItemData.ItemType.Shoulder:
                case ItemDrop.ItemData.ItemType.Utility:
                case ItemDrop.ItemData.ItemType.Trinket: return "Armor";
                case ItemDrop.ItemData.ItemType.Tool:
                case ItemDrop.ItemData.ItemType.Torch: return "Tools";
                case ItemDrop.ItemData.ItemType.Trophy: return "Trophies";
                case ItemDrop.ItemData.ItemType.Fish: return "Fish";
                case ItemDrop.ItemData.ItemType.Consumable:
                    if (d.m_food > 0f || d.m_foodStamina > 0f || d.m_foodEitr > 0f) return "Food";
                    return "Meads";
                case ItemDrop.ItemData.ItemType.Material:
                    // order matters: the first match wins (e.g. leather scraps are Hides, not Ores; chitin isn't a metal)
                    if (Has(n, "coin", "amber", "ruby", "pearl", "necklace", "gemstone", "jewel")) return "Valuables";
                    if (Has(n, "seed", "sapling", "cone", "acorn")) return "Seeds";
                    if (n.StartsWith("$item_fish")) return "Fish";
                    if (Has(n, "hide", "pelt", "leather")) return "Hides";
                    if (Has(n, "meat", "entrails", "necktail", "neck_tail", "chicken")) return "Meat";
                    if (Has(n, "feather", "gland", "bone", "claw", "fang", "tooth", "horn", "antler", "carapace", "mandible",
                            "chitin", "tissue", "ooze", "guck", "bile", "ectoplasm", "scale", "blood", "wing", "hair", "core",
                            "eye", "needle", "skull", "tail", "bladder", "carrion", "paw", "sinew", "heart", "blubber", "ribcage",
                            "larva")) return "Creature";
                    if (Has(n, "barley", "flax", "turnip", "onion", "thistle", "dandelion", "fiddlehead", "mushroom", "berr",
                            "carrot", "smokepuff", "jotunpuff", "magecap", "vineberry", "seaweed")) return "Crops";
                    if (Has(n, "ore", "scrap")) return "Ores";
                    if (Has(n, "copper", "tin", "bronze", "iron", "silver", "blackmetal", "flametal", "ingot", "metal")) return "Metals";
                    if (Has(n, "wood", "roundlog", "elderbark", "log")) return "Wood";
                    if (Has(n, "stone", "marble", "obsidian", "flint", "grausten", "crystal")) return "Stone";
                    return "Materials";
                default: return "Misc";
            }
        }

        // the gold gear and staffs still to be finished at a forge ("...Uncooked") aren't food
        private static bool Gear(string id)
        {
            foreach (string g in new[] { "Armor", "Atgeir", "Axe", "Battleaxe", "Bow", "Crossbow", "Fist", "Keys", "Knife", "Mace",
                "Shield", "Sledge", "SmallParts", "Spear", "Staff", "Sword", "THSword", "Generic", "Mold" })
                if (id.StartsWith(g)) return true;
            return false;
        }
    }
}
