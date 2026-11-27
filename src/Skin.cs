using UnityEngine;

namespace RecipeBook
{
    // The game's look for the window (the user, 2026-11-26: "fix the Recipe Book's HUD", step 1): the game's own
    // font (AveriaSerifLibre, as Warband and Sea Legs use) and a search box and scrollbar in the window's colours
    // instead of Unity's grey ones. The skin is shared with every other mod's window, so ours is put in only while
    // the book draws (Push) and the old one goes back after (Pop).
    // The same file is in Tidekeeper (Ægir's Log): change both together.
    internal static class Skin
    {
        private static Font _font;
        private static float _nextLook;

        internal static Font Font()
        {
            if (_font != null || Time.realtimeSinceStartup < _nextLook) return _font;
            _nextLook = Time.realtimeSinceStartup + 2f;
            foreach (Font f in Resources.FindObjectsOfTypeAll<Font>())
                if (f.name == "AveriaSerifLibre-Bold") { _font = f; break; }
            return _font;
        }

        // the game's font on these styles, once it is loaded; true when done
        internal static bool Fonts(params GUIStyle[] styles)
        {
            Font f = Font();
            if (f == null) return false;
            foreach (GUIStyle s in styles) if (s != null) s.font = f;
            return true;
        }

        private static GUIStyle _field, _bar, _thumb, _oldField, _oldBar, _oldThumb;

        private static void Make()
        {
            if (_field != null && _field.normal.background != null) return;
            Color gold = new Color(0.95f, 0.72f, 0.3f);
            _field = new GUIStyle(GUI.skin.textField)
            {
                fontSize = 15, padding = new RectOffset(8, 8, 5, 5), border = new RectOffset(2, 2, 2, 2),
                normal = { background = Framed(new Color(0f, 0f, 0f, 0.55f), new Color(gold.r, gold.g, gold.b, 0.35f)), textColor = new Color(0.92f, 0.9f, 0.85f) },
                hover = { background = Framed(new Color(0f, 0f, 0f, 0.6f), new Color(gold.r, gold.g, gold.b, 0.6f)), textColor = Color.white },
                focused = { background = Framed(new Color(0f, 0f, 0f, 0.7f), new Color(gold.r, gold.g, gold.b, 0.95f)), textColor = Color.white }
            };
            _field.active = _field.focused;
            _field.onNormal = _field.normal; _field.onHover = _field.hover; _field.onFocused = _field.focused; _field.onActive = _field.focused;
            _bar = new GUIStyle(GUI.skin.verticalScrollbar)
            {
                fixedWidth = 10f, margin = new RectOffset(4, 0, 0, 0), padding = new RectOffset(0, 0, 0, 0), border = new RectOffset(0, 0, 0, 0),
                normal = { background = Flat(new Color(0f, 0f, 0f, 0.35f)) }
            };
            _thumb = new GUIStyle(GUI.skin.verticalScrollbarThumb)
            {
                fixedWidth = 10f, padding = new RectOffset(0, 0, 0, 0), border = new RectOffset(0, 0, 0, 0),
                normal = { background = Flat(new Color(gold.r, gold.g, gold.b, 0.55f)) },
                hover = { background = Flat(new Color(gold.r, gold.g, gold.b, 0.8f)) },
                active = { background = Flat(new Color(gold.r, gold.g, gold.b, 0.95f)) }
            };
        }

        internal static void Push()
        {
            Make();
            Fonts(_field);
            GUISkin k = GUI.skin;
            _oldField = k.textField; _oldBar = k.verticalScrollbar; _oldThumb = k.verticalScrollbarThumb;
            k.textField = _field; k.verticalScrollbar = _bar; k.verticalScrollbarThumb = _thumb;
        }

        internal static void Pop()
        {
            GUISkin k = GUI.skin;
            if (_oldField != null) k.textField = _oldField;
            if (_oldBar != null) k.verticalScrollbar = _oldBar;
            if (_oldThumb != null) k.verticalScrollbarThumb = _oldThumb;
        }

        internal static GUIStyle Bar { get { Make(); return _bar; } }

        internal static Texture2D Flat(Color c)
        {
            Texture2D t = new Texture2D(1, 1);
            t.SetPixel(0, 0, c);
            t.Apply();
            t.hideFlags = HideFlags.HideAndDontSave;   // else the game's clean-up of unused textures deletes it
            return t;
        }

        // step 2 (borders): the gold of the window, a line, and a framed slot behind an icon
        internal static readonly Color Gold = new Color(0.95f, 0.72f, 0.3f);
        private static Texture2D _white, _slot;

        internal static void Line(Rect r, Color c)
        {
            if (_white == null) _white = Flat(Color.white);
            Color keep = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(r, _white);
            GUI.color = keep;
        }

        internal static void Slot(Rect r)
        {
            if (_slot == null) _slot = Framed(new Color(0f, 0f, 0f, 0.45f), new Color(Gold.r, Gold.g, Gold.b, 0.3f));
            GUI.Box(r, GUIContent.none, SlotStyle());
        }

        private static GUIStyle _slotStyle;
        private static GUIStyle SlotStyle()
        {
            if (_slotStyle == null || _slotStyle.normal.background == null)
                _slotStyle = new GUIStyle { border = new RectOffset(2, 2, 2, 2), normal = { background = _slot } };
            return _slotStyle;
        }

        // a fill with a thin edge (drawn 9-sliced: the edge stays 2 px whatever the size; the style needs border 2)
        internal static Texture2D Framed(Color fill, Color edge)
        {
            const int n = 6;
            Texture2D t = new Texture2D(n, n);
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                    t.SetPixel(x, y, x < 2 || y < 2 || x >= n - 2 || y >= n - 2 ? edge : fill);
            t.Apply();
            t.filterMode = FilterMode.Point;
            t.hideFlags = HideFlags.HideAndDontSave;
            return t;
        }
    }
}
