RightClickHide
================

A small RimWorld mod that toggles visibility of the bottom UI (main buttons), architect menu, and overlay/time controls with a right-click. The mod hides the UI elements visually but does not change the actual overlay state.

Build
-----

This project targets .NET Framework 4.7.2 and can be built with MSBuild or dotnet (dotnet will work in this workspace as used previously):

```powershell
# from the Source directory
dotnet build "G:\SteamLibrary\steamapps\common\RimWorld\Mods\RightClickHide\Source\RightClickHide.csproj"
```

Install / Test
--------------

- Copy `Assemblies/RightClickHide.dll` into the mod folder (the build already outputs there).
- Start RimWorld with the mod enabled and right-click in-game to toggle the bottom bar and related UI elements.

Notes
-----
- The mod uses Harmony to patch RimWorld at runtime. It attempts to patch time controls dynamically to avoid crashes across different RimWorld builds.
- If you want me to create a GitHub repo and push for you, provide the repository URL and credentials (or grant access via a PAT) — otherwise follow the steps below to push yourself.

How to upload to GitHub (commands)
---------------------------------

1. Create a repo on GitHub.
2. In PowerShell (from the mod root directory):

```powershell
# initialize repo locally (if not already done)
git init
git add .
git commit -m "Initial commit: RightClickHide RimWorld mod"

# add your GitHub repo as 'origin' (replace URL below)
git remote add origin https://github.com/<your-username>/<your-repo>.git

# push to GitHub (main branch)
git branch -M main
git push -u origin main
```

If you use SSH, replace the `remote add` URL with the SSH URL.

License
-------
Add your preferred license file if you want others to reuse this code.
