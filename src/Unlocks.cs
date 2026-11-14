using System.Collections.Generic;
using System.Text;

namespace RecipeBook
{
    // The order recipes were unlocked in, and the in-game day, saved on the character (recipes are per character).
    // Recipes known from before the mod have no date: they sort after the dated ones, newest stage first.
    internal static class Unlocks
    {
        private const string SaveKey = "RecipeBook_unlocks";

        internal class Info { public int Order; public int Day; }

        internal static Dictionary<string, Info> Load(Player p)
        {
            Dictionary<string, Info> d = new Dictionary<string, Info>();
            string v;
            if (p == null || !p.m_customData.TryGetValue(SaveKey, out v) || string.IsNullOrEmpty(v)) return d;
            int order = 0;
            foreach (string line in v.Split('\n'))
            {
                int tab = line.LastIndexOf('\t');
                if (tab <= 0) continue;
                int day;
                if (!int.TryParse(line.Substring(tab + 1), out day)) day = -1;
                d[line.Substring(0, tab)] = new Info { Order = ++order, Day = day };
            }
            return d;
        }

        // Backup for the unlock hook: every few seconds, if the number of known recipes changed, any recipe known now
        // that isn't in the baseline (what you knew when the book first looked) or in the list gets today's date.
        private const string BaseKey = "RecipeBook_base";
        private static readonly System.Reflection.FieldInfo KnownField = HarmonyLib.AccessTools.Field(typeof(Player), "m_knownRecipes");
        private static int _lastCount = -1;
        private static Player _lastPlayer;
        private static float _next;

        internal static void Watch(Player p)
        {
            if (p == null || UnityEngine.Time.time < _next || ObjectDB.instance == null) return;
            _next = UnityEngine.Time.time + 3f;
            HashSet<string> known = KnownField != null ? KnownField.GetValue(p) as HashSet<string> : null;
            if (known == null) return;
            if (p == _lastPlayer && known.Count == _lastCount) return;
            _lastPlayer = p;
            _lastCount = known.Count;
            Check(p);
        }

        internal static void Check(Player p)
        {
            List<string> now = new List<string>();
            foreach (Recipe r in ObjectDB.instance.m_recipes)
                if (r != null && r.m_item != null && p.IsRecipeKnown(r.m_item.m_itemData.m_shared.m_name)) now.Add(r.m_item.m_itemData.m_shared.m_name);
            string b;
            if (!p.m_customData.TryGetValue(BaseKey, out b))
            {
                // first look: everything already known (and not dated by the hook) is the baseline
                Dictionary<string, Info> dated = Load(p);
                List<string> baseList = new List<string>();
                foreach (string n in now) if (!dated.ContainsKey(n) && !baseList.Contains(n)) baseList.Add(n);
                p.m_customData[BaseKey] = string.Join("|", baseList.ToArray());
                return;
            }
            HashSet<string> baseSet = new HashSet<string>(b.Split('|'));
            Dictionary<string, Info> d = Load(p);
            foreach (string n in now)
                if (!baseSet.Contains(n) && !d.ContainsKey(n)) { Add(p, n); d = Load(p); }
        }

        internal static void Add(Player p, string name)
        {
            if (p == null || string.IsNullOrEmpty(name)) return;
            Dictionary<string, Info> d = Load(p);
            if (d.ContainsKey(name)) return;
            int day = EnvMan.instance != null ? EnvMan.instance.GetDay() : -1;
            string v;
            p.m_customData.TryGetValue(SaveKey, out v);
            StringBuilder sb = new StringBuilder(v ?? "");
            if (sb.Length > 0) sb.Append('\n');
            sb.Append(name.Replace("\n", "").Replace("\t", "")).Append('\t').Append(day);
            p.m_customData[SaveKey] = sb.ToString();
        }
    }
}
