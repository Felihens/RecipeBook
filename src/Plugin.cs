using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace RecipeBook
{
    // Recipe Book: press I to see every recipe you have unlocked, what each one costs, and the newest ones first.
    // Read only: nothing is crafted from here. Filter by stage (before / after each boss) or by material;
    // click a material to see which of your recipes use it (and which recipe makes it).
    [HarmonyPatch]
    [BepInPlugin("felih.RecipeBook", "Recipe Book", "1.1.1")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ConfigEntry<KeyboardShortcut> Key;
        internal static ConfigEntry<float> Scale;
        internal static ConfigEntry<float> ChestRange;
        internal static BepInEx.Logging.ManualLogSource Log;
        internal static bool Open;

        private void Awake()
        {
            Log = Logger;
            Key = Config.Bind("General", "Key", new KeyboardShortcut(KeyCode.I), "Opens and closes the recipe book.");
            Scale = Config.Bind("General", "Scale", 1f, new ConfigDescription("Size of the recipe book window (1 = normal).", new AcceptableValueRange<float>(0.6f, 2f)));
            ChestRange = Config.Bind("General", "ChestRange", 20f, new ConfigDescription("Metres around you where chests count toward 'you have' (0 = only your bag).", new AcceptableValueRange<float>(0f, 60f)));
            new Harmony("felih.RecipeBook").PatchAll(typeof(Plugin).Assembly);
        }

        private void Update()
        {
            Player p = Player.m_localPlayer;
            if (p == null) { Open = false; return; }
            Unlocks.Watch(p);
            if (Open && (InventoryGui.IsVisible() || Minimap.IsOpen() || Menu.IsVisible() || StoreGui.IsVisible() || p.IsDead())) Open = false;
            if (Open && Book.SearchFocused) return;   // typing in the search box
            if (Key.Value.MainKey == KeyCode.None || !Key.Value.IsDown()) return;
            if (!Open && ((Chat.instance != null && Chat.instance.HasFocus()) || Console.IsVisible() || TextInput.IsVisible()
                || InventoryGui.IsVisible() || Minimap.IsOpen() || Menu.IsVisible() || StoreGui.IsVisible()
                || Hud.IsPieceSelectionVisible() || Typing())) return;
            Open = !Open;
            if (Open) Nearby.Forget();
            if (Open) Book.Refresh(p);
        }

        // a text box has the keyboard (a search box in the build or craft window, a sign, another mod's window):
        // the key is a letter typed there, not a request for the book
        private static bool Typing()
        {
            UnityEngine.EventSystems.EventSystem es = UnityEngine.EventSystems.EventSystem.current;
            GameObject sel = es != null ? es.currentSelectedGameObject : null;
            if (sel == null) return false;
            TMPro.TMP_InputField tmp = sel.GetComponent<TMPro.TMP_InputField>();
            if (tmp != null && tmp.isFocused) return true;
            UnityEngine.UI.InputField ui = sel.GetComponent<UnityEngine.UI.InputField>();
            return ui != null && ui.isFocused;
        }

        private void OnGUI()
        {
            if (Open && Player.m_localPlayer != null) Book.Draw();
        }

        // ---- while the book is open: free mouse, no walking, looking, zooming or hotkeys; Esc closes it

        [HarmonyPatch(typeof(GameCamera), "UpdateMouseCapture"), HarmonyPostfix]
        private static void FreeCursor()
        {
            if (!Open) return;
            ZCursor.LockState = CursorLockMode.None;
            ZCursor.Show();
        }

        [HarmonyPatch(typeof(PlayerController), "TakeInput"), HarmonyPrefix]
        private static bool NoControl(ref bool __result)
        {
            if (!Open) return true;
            __result = false;
            return false;
        }

        [HarmonyPatch(typeof(Player), "TakeInput"), HarmonyPrefix]
        private static bool NoHotkeys(ref bool __result)
        {
            if (!Open) return true;
            __result = false;
            return false;
        }

        // the mouse wheel scrolls the book, not the camera (like the game's own windows)
        [HarmonyPatch(typeof(ZInput), "GetMouseScrollWheel"), HarmonyPrefix]
        private static bool NoZoom(ref float __result)
        {
            if (!Open) return true;
            __result = 0f;
            return false;
        }

        [HarmonyPatch(typeof(ZInput), "Internal_GetMouseScrollWheel"), HarmonyPrefix]
        private static bool NoZoomInner(ref float __result) { return NoZoom(ref __result); }

        [HarmonyPatch(typeof(Minimap), "Update"), HarmonyPrefix]
        private static bool NoMapWhileTyping() { return !(Open && Book.SearchFocused); }

        [HarmonyPatch(typeof(Menu), "Update"), HarmonyPrefix]
        private static bool EscCloses()
        {
            if (Open && ZInput.GetKeyDown(KeyCode.Escape)) { Open = false; return false; }
            return true;
        }

        // ---- remember when each recipe was unlocked (the game doesn't), for the Recent tab

        [HarmonyPatch(typeof(Player), "AddKnownRecipe"), HarmonyPrefix]
        private static void BeforeLearn(Player __instance, Recipe recipe, out bool __state)
        {
            __state = recipe != null && recipe.m_item != null && !__instance.IsRecipeKnown(recipe.m_item.m_itemData.m_shared.m_name);
        }

        [HarmonyPatch(typeof(Player), "AddKnownRecipe"), HarmonyPostfix]
        private static void AfterLearn(Player __instance, Recipe recipe, bool __state)
        {
            if (__state && __instance == Player.m_localPlayer) Unlocks.Add(__instance, recipe.m_item.m_itemData.m_shared.m_name);
        }
    }
}
