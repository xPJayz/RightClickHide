using RimWorld;
using Verse;
using UnityEngine;
using HarmonyLib;
using System.Reflection;
using UnityEngine.Events;

namespace RightClickHide
{
    public class RightClickHideMod : Mod
    {
        public static bool IsUIHidden = false;

        public RightClickHideMod(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("com.righthide.rimworld");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            // Defer reflective patching of time controls until after the game finishes loading.
            // Some RimWorld builds load types later; queue a long event so the Type exists when we search for it.
            try
            {
                LongEventHandler.QueueLongEvent(() =>
                {
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
                                Log.Message("RightClickHide: Successfully patched TimeControls.DoTimeControls to follow UI hide/show.");
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
                        Log.Error("RightClickHide: Exception while attempting deferred patch of TimeControls: " + ex);
                    }
                }, "RightClickHide - patching UI", false, null);
            }
            catch (System.Exception ex)
            {
                Log.Error("RightClickHide: Failed to queue deferred TimeControls patch: " + ex);
            }
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