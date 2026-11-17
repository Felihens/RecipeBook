using System.Collections.Generic;
using UnityEngine;

namespace RecipeBook
{
    // What the chests around you hold, so a recipe can say "you have 3 + 40 nearby". Only chests you may open
    // (not someone else's private chest, not tombstones), within ChestRange metres. Counted at most every
    // 2 seconds and only while the book is open, since it looks at every loaded container.
    internal static class Nearby
    {
        private static readonly Dictionary<string, int> _counts = new Dictionary<string, int>();
        private static float _next = -1f;

        internal static int Count(string itemName)
        {
            Refresh();
            int n;
            return _counts.TryGetValue(itemName, out n) ? n : 0;
        }

        // the book was just opened: count again right away
        internal static void Forget() { _next = -1f; }

        private static void Refresh()
        {
            if (Time.unscaledTime < _next) return;
            _next = Time.unscaledTime + 2f;
            _counts.Clear();
            Player p = Player.m_localPlayer;
            float range = Plugin.ChestRange.Value;
            if (p == null || range <= 0f) return;
            Vector3 at = p.transform.position;
            float r2 = range * range;
            long me = p.GetPlayerID();
            foreach (Container c in Object.FindObjectsByType<Container>(FindObjectsSortMode.None))
            {
                if (c == null || (c.transform.position - at).sqrMagnitude > r2) continue;
                Piece piece = c.GetComponent<Piece>();
                if (piece == null || c.GetComponent<TombStone>() != null) continue;
                if (c.m_privacy == Container.PrivacySetting.Private && piece.GetCreator() != me) continue;   // someone else's private chest
                Inventory inv = c.GetInventory();
                if (inv == null) continue;
                foreach (ItemDrop.ItemData it in inv.GetAllItems())
                {
                    string name = it.m_shared.m_name;
                    int n;
                    _counts.TryGetValue(name, out n);
                    _counts[name] = n + it.m_stack;
                }
            }
        }
    }
}
