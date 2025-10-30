using RimWorld;
using Verse;
using UnityEngine;
using HarmonyLib;
using System.Reflection;
using UnityEngine.Events;

namespace RightClickHide
{
    public class RightClickHideSettings : ModSettings
    {
        public bool requireCtrlKey = false;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref requireCtrlKey, "requireCtrlKey", false);
            base.ExposeData();
        }
    }

    public class RightClickHideMod : Mod
    {
        public static bool IsUIHidden = false;
        public static RightClickHideSettings settings;

        public RightClickHideMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<RightClickHideSettings>();
            var harmony = new Harmony("com.righthide.rimworld");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            // Try to patch TimeControls.DoTimeControls at runtime safely.
            // Some RimWorld builds may have different method names/signatures, so attempt reflectively.
            try
            {
                var asm = System.AppDomain.CurrentDomain.GetAssemblies();
                System.Type timeControlsType = null;
                // Try to find the TimeControls type by name across loaded assemblies
                foreach (var a in asm)
                {
                    timeControlsType = a.GetType("TimeControls");
                    if (timeControlsType != null) break;
                }

                if (timeControlsType == null)
                {
                    // Try full name variations
                    foreach (var a in asm)
                    {
                        foreach (var t in a.GetTypes())
                        {
                            if (t.Name == "TimeControls")
                            {
                                timeControlsType = t;
                                break;
                            }
                        }
                        if (timeControlsType != null) break;
                    }
                }

                if (timeControlsType != null)
                {
                    var method = timeControlsType.GetMethod("DoTimeControls", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                    if (method != null)
                    {
                        var prefix = new HarmonyMethod(typeof(RightClickHideMod).GetMethod(nameof(TimeControls_Prefix), BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public));
                        harmony.Patch(method, prefix: prefix);
                    }
                    else
                    {
                        Log.Message("RightClickHide: TimeControls type found but DoTimeControls method not located; skipping time controls patch.");
                    }
                }
                else
                {
                    Log.Message("RightClickHide: TimeControls type not found; skipping time controls patch.");
                }
            }
            catch (System.Exception ex)
            {
                Log.Error("RightClickHide: Exception while attempting to patch TimeControls: " + ex);
            }
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);
            
            listing.CheckboxLabeled("Require Ctrl + Right Click", 
                ref settings.requireCtrlKey, 
                "If enabled, you must hold Ctrl while right-clicking to toggle the UI. If disabled, just right-click.");
            
            listing.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Right Click Hide UI";
        }

        // Prefix used to skip drawing time controls when UI is hidden
        public static bool TimeControls_Prefix()
        {
            return !IsUIHidden;
        }
    }

    [HarmonyPatch(typeof(PlaySettings))]
    [HarmonyPatch("DoPlaySettingsGlobalControls")]
    public static class PlaySettings_DoPlaySettingsGlobalControls_Patch
    {
        public static bool Prefix()
        {
            return !RightClickHideMod.IsUIHidden;
        }
    }

    [HarmonyPatch(typeof(MainButtonsRoot))]
    [HarmonyPatch("MainButtonsOnGUI")]
    public static class MainButtonsRoot_MainButtonsOnGUI_Patch
    {
        public static bool Prefix()
        {
            if (Current.ProgramState != ProgramState.Playing) return true;

            if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
            {
                if (Find.WindowStack?.Windows?.Count > 0) return true;

                // Check if we need Ctrl key and if it's pressed
                if (RightClickHideMod.settings.requireCtrlKey && !Event.current.control)
                    return true;

                RightClickHideMod.IsUIHidden = !RightClickHideMod.IsUIHidden;

                if (Find.MainTabsRoot != null)
                {
                        // Make Architect tab follow bottom bar visibility:
                        // - If UI is being hidden, close Architect if it's open
                        // - If UI is being shown, open Architect
                        MainButtonDef architectTab = DefDatabase<MainButtonDef>.GetNamed("Architect");
                        if (RightClickHideMod.IsUIHidden)
                        {
                            if (Find.MainTabsRoot.OpenTab == architectTab)
                            {
                                Find.MainTabsRoot.SetCurrentTab(null);
                            }
                        }
                        else
                        {
                            // Open the Architect tab visually when showing the bottom bar
                            Find.MainTabsRoot.SetCurrentTab(architectTab);
                        }
                }

                Event.current.Use();
            }

            return !RightClickHideMod.IsUIHidden;
        }
    }
}