using System;
using System.Collections.Generic;
using UnityEngine;

namespace RecipeBook
{
    // The window. Left: tabs (All / Recent), search, stage and metal filters, the recipe list.
    // Right: the chosen recipe: what it costs, where it's made, its upgrades. Click any material to list
    // the recipes (of yours) that use it, and the recipe that makes it if you know one.
    internal static class Book
    {
        internal class Entry
        {
            public Recipe R;
            public string Name, Key, Station, Tag;
            public Sprite Icon;
            public int Stage, Order, Day;
        }

        private static readonly List<Entry> All = new List<Entry>();
        private static readonly List<Entry> Shown = new List<Entry>();
        private static bool _recent;
        private static int _stage;          // 0 = every stage
        private static string _tag;         // item category (same as the Storage window's tags); null = all
        private static bool _tagPop;
        private static string _material;    // shared name (e.g. $item_iron); null = none
        private static string _materialLabel;
        private static Sprite _materialIcon;
        private static string _search = "", _lastSearch = "";
        private static Entry _sel;
        private static Vector2 _listScroll, _detailScroll;
        internal static bool SearchFocused;
        private static bool _dirty = true;
        private static readonly List<KeyValuePair<string, Sprite>> Metals = new List<KeyValuePair<string, Sprite>>();
        private static readonly List<string> MetalNames = new List<string>();

        // ------------------------------------------------------------ data

        internal static void Refresh(Player p)
        {
            All.Clear();
            Stages.Reset();
            if (ObjectDB.instance == null) return;
            Unlocks.Check(p);
            Dictionary<string, Unlocks.Info> unl = Unlocks.Load(p);
            foreach (Recipe r in ObjectDB.instance.m_recipes)
            {
                if (r == null || !r.m_enabled || r.m_item == null || r.m_resources == null || r.m_resources.Length == 0) continue;
                string key = r.m_item.m_itemData.m_shared.m_name;
                if (!p.IsRecipeKnown(key)) continue;
                bool hasRes = false;
                foreach (Piece.Requirement q in r.m_resources) if (q != null && q.m_resItem != null) { hasRes = true; break; }
                if (!hasRes) continue;
                Entry e = new Entry { R = r, Key = key, Name = L(key), Icon = Icon(r.m_item), Stage = Stages.Of(r) };
                e.Station = StationText(r);
                e.Tag = Tags.Category(r.m_item);
                Unlocks.Info u;
                if (unl.TryGetValue(key, out u)) { e.Order = u.Order; e.Day = u.Day; } else { e.Order = 0; e.Day = -1; }
                All.Add(e);
            }
            // the metals your recipes use, in the order you meet them
            Metals.Clear();
            foreach (string m in new[] { "Copper", "Tin", "Bronze", "Iron", "Silver", "BlackMetal", "Eitr", "Flametal", "FlametalNew" })
            {
                GameObject g = ObjectDB.instance.GetItemPrefab(m);
                if (g == null) continue;
                ItemDrop id = g.GetComponent<ItemDrop>();
                if (id == null || !UsedBy(id.m_itemData.m_shared.m_name)) continue;
                bool dup = false;
                foreach (KeyValuePair<string, Sprite> kv in Metals) if (kv.Key == id.m_itemData.m_shared.m_name) dup = true;
                if (!dup) Metals.Add(new KeyValuePair<string, Sprite>(id.m_itemData.m_shared.m_name, Icon(id)));
            }
            MetalNames.Clear();
            foreach (KeyValuePair<string, Sprite> kv in Metals) MetalNames.Add(L(kv.Key));
            if (_sel != null) { Entry again = null; foreach (Entry e in All) if (e.R == _sel.R) again = e; _sel = again; }
            _dirty = true;
        }

        private static bool UsedBy(string material)
        {
            foreach (Entry e in All)
                foreach (Piece.Requirement q in e.R.m_resources)
                    if (q != null && q.m_resItem != null && q.m_resItem.m_itemData.m_shared.m_name == material) return true;
            return false;
        }

        private static void Filter()
        {
            Shown.Clear();
            string s = (_search ?? "").Trim().ToLowerInvariant();
            foreach (Entry e in All)
            {
                if (_stage != 0 && e.Stage != _stage) continue;
                if (_tag != null && e.Tag != _tag) continue;
                if (_material != null && !Uses(e.R, _material)) continue;
                if (s.Length > 0 && e.Name.ToLowerInvariant().IndexOf(s, StringComparison.Ordinal) < 0 && !UsesNamed(e.R, s)) continue;
                Shown.Add(e);
            }
            if (_recent)
                Shown.Sort((a, b) =>
                {
                    if (a.Order != b.Order) return b.Order.CompareTo(a.Order);       // newest unlocked first
                    if (a.Stage != b.Stage) return b.Stage.CompareTo(a.Stage);       // undated: later stages first
                    return string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
                });
            else
                Shown.Sort((a, b) =>
                {
                    if (a.Stage != b.Stage) return a.Stage.CompareTo(b.Stage);
                    return string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
                });
            _dirty = false;
        }

        private static bool Uses(Recipe r, string material)
        {
            foreach (Piece.Requirement q in r.m_resources)
                if (q != null && q.m_resItem != null && q.m_resItem.m_itemData.m_shared.m_name == material) return true;
            return false;
        }

        private static bool UsesNamed(Recipe r, string lower)
        {
            foreach (Piece.Requirement q in r.m_resources)
                if (q != null && q.m_resItem != null && L(q.m_resItem.m_itemData.m_shared.m_name).ToLowerInvariant().IndexOf(lower, StringComparison.Ordinal) >= 0) return true;
            return false;
        }

        private static void PickMaterial(ItemDrop item)
        {
            _material = item.m_itemData.m_shared.m_name;
            _materialLabel = L(_material);
            _materialIcon = Icon(item);
            _listScroll = Vector2.zero;
            _dirty = true;
        }

        private static void ClearMaterial() { _material = null; _dirty = true; }

        // ------------------------------------------------------------ drawing

        private static GUIStyle _bg, _panel, _title, _text, _small, _row, _rowSel, _tab, _tabOn, _chip, _head, _link;
        private const float W = 1120f, H = 700f;

        private static void Styles()
        {
            if (_bg != null) return;
            _bg = new GUIStyle(GUI.skin.box) { normal = { background = Tex(new Color(0.08f, 0.07f, 0.06f, 0.94f)) } };
            _panel = new GUIStyle(GUI.skin.box) { normal = { background = Tex(new Color(0f, 0f, 0f, 0.35f)) }, padding = new RectOffset(8, 8, 8, 8) };
            _title = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold, normal = { textColor = new Color(1f, 0.82f, 0.45f) } };
            _head = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold, normal = { textColor = new Color(1f, 0.82f, 0.45f) } };
            _text = new GUIStyle(GUI.skin.label) { fontSize = 15, wordWrap = true, normal = { textColor = new Color(0.92f, 0.9f, 0.85f) } };
            _small = new GUIStyle(GUI.skin.label) { fontSize = 12, normal = { textColor = new Color(0.7f, 0.68f, 0.62f) } };
            _row = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft, fontSize = 15, padding = new RectOffset(44, 6, 2, 2),
                normal = { background = Tex(new Color(1f, 1f, 1f, 0.03f)), textColor = new Color(0.92f, 0.9f, 0.85f) },
                hover = { background = Tex(new Color(1f, 0.8f, 0.4f, 0.12f)), textColor = Color.white } };
            _rowSel = new GUIStyle(_row) { normal = { background = Tex(new Color(1f, 0.75f, 0.3f, 0.3f)), textColor = Color.white } };
            _tab = new GUIStyle(GUI.skin.button) { fontSize = 15, normal = { background = Tex(new Color(0f, 0f, 0f, 0.5f)), textColor = new Color(0.85f, 0.83f, 0.78f) },
                hover = { background = Tex(new Color(1f, 0.8f, 0.4f, 0.2f)), textColor = Color.white } };
            _tabOn = new GUIStyle(_tab) { normal = { background = Tex(new Color(0.95f, 0.72f, 0.3f, 0.85f)), textColor = Color.black }, fontStyle = FontStyle.Bold };
            _chip = new GUIStyle(_tab) { fontSize = 13, padding = new RectOffset(4, 4, 2, 2) };
            _link = new GUIStyle(_row) { fontSize = 15, padding = new RectOffset(36, 6, 2, 2) };
        }

        internal static void Draw()
        {
            Styles();
            float s = Screen.height / 1080f * Plugin.Scale.Value;
            Matrix4x4 old = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(s, s, 1f));
            float sw = Screen.width / s, sh = Screen.height / s;
            Rect win = new Rect((sw - W) / 2f, (sh - H) / 2f, W, H);
            GUI.Box(win, GUIContent.none, _bg);
            GUI.Box(win, GUIContent.none, _bg);

            GUILayout.BeginArea(new Rect(win.x + 14, win.y + 10, W - 28, H - 20));
            GUILayout.BeginHorizontal();
            GUILayout.Label("Recipe Book", _title);
            GUILayout.FlexibleSpace();
            GUILayout.Label(All.Count + " recipes known   ·   " + Plugin.Key.Value + " or Esc to close", _small);
            if (GUILayout.Button("✕", _tab, GUILayout.Width(32))) Plugin.Open = false;
            GUILayout.EndHorizontal();
            GUILayout.Space(6);

            GUILayout.BeginHorizontal();
            // ---- left column
            GUILayout.BeginVertical(GUILayout.Width(470));
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("All", _recent ? _tab : _tabOn, GUILayout.Height(30))) { _recent = false; _dirty = true; _listScroll = Vector2.zero; }
            if (GUILayout.Button("Recent", _recent ? _tabOn : _tab, GUILayout.Height(30))) { _recent = true; _dirty = true; _listScroll = Vector2.zero; }
            GUILayout.EndHorizontal();
            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Search", _small, GUILayout.Width(46));
            GUI.SetNextControlName("rb_search");
            _search = GUILayout.TextField(_search ?? "", 40);
            GUILayout.EndHorizontal();
            SearchFocused = GUI.GetNameOfFocusedControl() == "rb_search";
            GUILayout.Space(4);
            if (GUILayout.Button(_tag == null ? "Filter: All" : "Filter: " + _tag, _tag == null ? _chip : _tabOn, GUILayout.Height(26))) _tagPop = !_tagPop;
            if (_tagPop)
            {
                List<string> present = new List<string> { "All" };
                foreach (string t in Tags.All) foreach (Entry e in All) if (e.Tag == t) { present.Add(t); break; }
                for (int i = 0; i < present.Count; i++)
                {
                    if (i % 5 == 0) { if (i > 0) GUILayout.EndHorizontal(); GUILayout.BeginHorizontal(); }
                    string t = present[i];
                    bool on = t == "All" ? _tag == null : t == _tag;
                    if (GUILayout.Button(t, on ? _tabOn : _chip, GUILayout.Width(88))) { _tag = t == "All" ? null : t; _tagPop = false; _dirty = true; _listScroll = Vector2.zero; }
                }
                GUILayout.EndHorizontal();
            }
            if (_search != _lastSearch) { _lastSearch = _search; _dirty = true; }

            // stage chips: before / after each boss
            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Any stage", _stage == 0 ? _tabOn : _chip)) { _stage = 0; _dirty = true; }
            for (int i = 1; i <= Stages.Count; i++)
            {
                if (i == 5) { GUILayout.EndHorizontal(); GUILayout.BeginHorizontal(); }
                if (!Stages.Present(i, All)) continue;
                if (GUILayout.Button(new GUIContent(Stages.Short[i], Stages.Long[i]), _stage == i ? _tabOn : _chip)) { _stage = _stage == i ? 0 : i; _dirty = true; }
            }
            GUILayout.EndHorizontal();

            // metal chips
            if (Metals.Count > 0)
            {
                GUILayout.BeginHorizontal();
                for (int i = 0; i < Metals.Count; i++)
                {
                    bool on = _material == Metals[i].Key;
                    if (GUILayout.Button(MetalNames[i], on ? _tabOn : _chip))
                    {
                        if (on) ClearMaterial();
                        else
                        {
                            GameObject g = FindItem(Metals[i].Key);
                            if (g != null) PickMaterial(g.GetComponent<ItemDrop>());
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }
            if (_material != null)
            {
                GUILayout.BeginHorizontal();
                Rect ir = GUILayoutUtility.GetRect(22, 22, GUILayout.Width(22));
                DrawSprite(ir, _materialIcon);
                GUILayout.Label("Recipes that use " + _materialLabel, _head);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("clear", _chip)) ClearMaterial();
                GUILayout.EndHorizontal();
                Entry maker = MakerOf(_material);
                if (maker != null)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Made by:", _small, GUILayout.Width(56));
                    if (GUILayout.Button(maker.Name + "  (" + maker.Station + ")", _chip)) { _sel = maker; _detailScroll = Vector2.zero; }
                    GUILayout.EndHorizontal();
                }
            }
            if (_dirty) Filter();

            GUILayout.Space(4);
            _listScroll = GUILayout.BeginScrollView(_listScroll, false, true, GUIStyle.none, GUI.skin.verticalScrollbar, _panel);
            if (Shown.Count == 0) GUILayout.Label("Nothing here.", _small);
            int lastStage = -1;
            foreach (Entry e in Shown)
            {
                if (!_recent && e.Stage != lastStage) { lastStage = e.Stage; GUILayout.Label(Stages.Long[e.Stage], _small); }
                string right = _recent ? (e.Day >= 0 ? "day " + e.Day : "") : e.Station;
                GUIContent c = new GUIContent(e.Name);
                Rect r = GUILayoutUtility.GetRect(c, _row, GUILayout.Height(36));
                if (GUI.Button(r, c, e == _sel ? _rowSel : _row)) { _sel = e; _detailScroll = Vector2.zero; GUI.FocusControl(null); }
                DrawSprite(new Rect(r.x + 6, r.y + 3, 30, 30), e.Icon);
                if (right.Length > 0) GUI.Label(new Rect(r.xMax - 170, r.y + 9, 164, 20), right, new GUIStyle(_small) { alignment = TextAnchor.MiddleRight });
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.Space(12);
            // ---- right column: the chosen recipe
            GUILayout.BeginVertical(_panel);
            _detailScroll = GUILayout.BeginScrollView(_detailScroll);
            if (_sel == null) GUILayout.Label("Pick a recipe on the left.\n\nClick a material in a recipe to see every recipe of yours that uses it.", _text);
            else Detail(_sel);
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            if (!string.IsNullOrEmpty(GUI.tooltip))
            {
                Vector2 m = Event.current.mousePosition;
                GUI.Label(new Rect(m.x + 14, m.y + 14, 260, 24), GUI.tooltip, new GUIStyle(_small) { normal = { background = Tex(new Color(0, 0, 0, 0.85f)), textColor = Color.white }, padding = new RectOffset(6, 6, 3, 3) });
            }
            GUI.matrix = old;
        }

        private static void Detail(Entry e)
        {
            Recipe r = e.R;
            ItemDrop.ItemData.SharedData sh = r.m_item.m_itemData.m_shared;
            Player p = Player.m_localPlayer;
            GUILayout.BeginHorizontal();
            Rect ir = GUILayoutUtility.GetRect(64, 64, GUILayout.Width(64), GUILayout.Height(64));
            DrawSprite(ir, e.Icon);
            GUILayout.Space(8);
            GUILayout.BeginVertical();
            GUILayout.Label(e.Name + (r.m_amount > 1 ? "  ×" + r.m_amount : ""), _title);
            GUILayout.Label(e.Tag + "   ·   " + e.Station + "   ·   " + Stages.Long[e.Stage], _small);
            if (e.Day >= 0) GUILayout.Label("Unlocked on day " + e.Day, _small);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            string desc = L(sh.m_description);
            if (!string.IsNullOrEmpty(desc) && !desc.StartsWith("[")) GUILayout.Label(desc, _text);

            GUILayout.Space(8);
            GUILayout.Label(r.m_requireOnlyOneIngredient ? "Needs one of" : "Needs", _head);
            Materials(r, 1, p);

            if (sh.m_maxQuality > 1)
            {
                bool any = false;
                foreach (Piece.Requirement q in r.m_resources) if (q != null && q.m_resItem != null && q.m_amountPerLevel > 0) any = true;
                if (any)
                {
                    GUILayout.Space(10);
                    GUILayout.Label("Upgrades (up to quality " + sh.m_maxQuality + ")", _head);
                    for (int lv = 2; lv <= sh.m_maxQuality; lv++)
                    {
                        string st = r.m_craftingStation != null ? L(r.m_craftingStation.m_name) + " level " + r.GetRequiredStationLevel(lv) : "";
                        GUILayout.Label("Quality " + lv + (st.Length > 0 ? "  ·  " + st : ""), _small);
                        Materials(r, lv, p);
                    }
                }
            }
        }

        private static void Materials(Recipe r, int quality, Player p)
        {
            foreach (Piece.Requirement q in r.m_resources)
            {
                if (q == null || q.m_resItem == null) continue;
                int n = quality <= 1 ? q.m_amount : q.m_amountPerLevel * (quality - 1);
                if (n <= 0) continue;
                string name = L(q.m_resItem.m_itemData.m_shared.m_name);
                int have = p != null ? p.GetInventory().CountItems(q.m_resItem.m_itemData.m_shared.m_name) : 0;
                int near = Nearby.Count(q.m_resItem.m_itemData.m_shared.m_name);
                GUIContent c = new GUIContent(n + " × " + name, "Show the recipes that use " + name);
                Rect rr = GUILayoutUtility.GetRect(c, _link, GUILayout.Height(30));
                bool on = _material == q.m_resItem.m_itemData.m_shared.m_name;
                if (GUI.Button(rr, c, on ? _rowSel : _link)) { if (on) ClearMaterial(); else PickMaterial(q.m_resItem); }
                DrawSprite(new Rect(rr.x + 5, rr.y + 3, 24, 24), Icon(q.m_resItem));
                // green: the bag has enough; amber: enough counting the chests around you; grey: not enough
                Color col = have >= n ? new Color(0.6f, 0.85f, 0.5f) : have + near >= n ? new Color(0.95f, 0.75f, 0.35f) : new Color(0.7f, 0.68f, 0.62f);
                string txt = "you have " + have + (near > 0 ? " + " + near + " nearby" : "");
                GUI.Label(new Rect(rr.xMax - 230, rr.y + 6, 224, 20), txt, new GUIStyle(_small) { alignment = TextAnchor.MiddleRight, normal = { textColor = col } });
            }
        }

        // the recipe (of yours) that makes this material, if any
        private static Entry MakerOf(string material)
        {
            foreach (Entry e in All) if (e.Key == material) return e;
            return null;
        }

        private static GameObject FindItem(string sharedName)
        {
            foreach (GameObject g in ObjectDB.instance.m_items)
            {
                ItemDrop d = g != null ? g.GetComponent<ItemDrop>() : null;
                if (d != null && d.m_itemData.m_shared.m_name == sharedName) return g;
            }
            return null;
        }

        // ------------------------------------------------------------ helpers

        private static string StationText(Recipe r)
        {
            if (r.m_craftingStation != null)
                return L(r.m_craftingStation.m_name) + (r.m_minStationLevel > 1 ? " lv " + r.m_minStationLevel : "");
            if (r.m_repairStation != null) return L(r.m_repairStation.m_name);
            return "By hand";
        }

        internal static string L(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return Localization.instance != null ? Localization.instance.Localize(s) : s;
        }

        private static Sprite Icon(ItemDrop d)
        {
            if (d == null) return null;
            Sprite[] ic = d.m_itemData.m_shared.m_icons;
            return ic != null && ic.Length > 0 ? ic[0] : null;
        }

        private static void DrawSprite(Rect r, Sprite s)
        {
            if (s == null || s.texture == null) return;
            Texture2D t = s.texture;
            Rect tr = s.textureRect;
            GUI.DrawTextureWithTexCoords(r, t, new Rect(tr.x / t.width, tr.y / t.height, tr.width / t.width, tr.height / t.height));
        }

        private static Texture2D Tex(Color c)
        {
            Texture2D t = new Texture2D(1, 1);
            t.SetPixel(0, 0, c);
            t.Apply();
            UnityEngine.Object.DontDestroyOnLoad(t);
            return t;
        }
    }
}
