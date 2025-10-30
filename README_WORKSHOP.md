Uploading the mod to Steam Workshop

This repository includes a helper script to upload your RimWorld mod to the Steam Workshop using SteamCMD.

Quick manual steps (Steam client):
1. Open RimWorld > Mods > Open Mods folder (or open the Steam game's Mods folder).
2. Copy the `RightClickHide_Workshop` folder (About + Assemblies) into that folder and use the Steam client to upload via the Workshop UI if available.

Automated upload (SteamCMD) — prerequisites:
- SteamCMD installed: https://developer.valvesoftware.com/wiki/SteamCMD
- A Steam account with permission to upload for AppID 294100 (RimWorld)
- Steam Guard may require a code during login

Using the included script:
1. Open PowerShell and run:
   pwsh .\tools\upload_workshop.ps1

2. The script will prompt for:
   - SteamCMD path (e.g. C:\\Program Files (x86)\\Steam\\steamcmd.exe)
   - Workshop content root folder (default: your Desktop RightClickHide_Workshop)
   - Preview image path (default: About/preview.png inside the workshop folder)
   - Title, description, changenote
   - Workshop item ID (leave blank to create a new item)
   - Steam username and password (password may be left blank to allow steamcmd to prompt securely)

3. The script generates a temporary VDF, calls steamcmd +workshop_build <vdf>, and exits.

Notes:
- The script does NOT store your password.
- If you run into Steam Guard issues, run steamcmd separately and authorize the account first.
- If you prefer, you can edit `tools/workshop_build.vdf.template` and run steamcmd manually:
  steamcmd +login <user> +workshop_build <path-to-vdf> +quit

If you want, I can:
- Prepare the VDF already filled with default values for your mod.
- Create a GitHub release with the workshop zip attached.
- Walk through a live upload (you provide SteamCMD path and will enter credentials locally).
