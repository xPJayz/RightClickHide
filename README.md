RightClickHide
================

A small RimWorld mod that toggles visibility of the bottom UI (main buttons), architect menu, and overlay/time controls with a right-click. The mod hides the UI elements visually but does not change the actual overlay state.

Build
-----

This project targets .NET Framework 4.7.2 and can be built with MSBuild or dotnet (dotnet will work in this workspace as used previously):

Install / Test
--------------

- Copy `Assemblies/RightClickHide.dll` into the mod folder.
- Start RimWorld with the mod enabled and right-click in-game to toggle the bottom bar and related UI elements.

Notes
-----
- The mod uses Harmony to patch RimWorld at runtime. It attempts to patch time controls dynamically to avoid crashes across different RimWorld builds.
- If you want me to create a GitHub repo and push for you, provide the repository URL and credentials (or grant access via a PAT) — otherwise follow the steps below to push yourself.

License
-------
Add your preferred license file if you want others to reuse this code.
