<#
Upload helper for Steam Workshop (RimWorld mod)

Usage notes:
- Install SteamCMD: https://developer.valvesoftware.com/wiki/SteamCMD
- You must own RimWorld on the Steam account you use to upload, and have permission to upload (Steam Guard may require code entry).
- This script generates a temporary .vdf file and calls steamcmd +workshop_build to upload.
- For a new item, leave WorkshopId empty. For updating an existing item, supply the WorkshopId.

Run from PowerShell:
    pwsh .\tools\upload_workshop.ps1

The script will prompt for missing values.
#>

param(
    [string]$SteamCmdPath = "C:\\Program Files (x86)\\Steam\\steamcmd.exe",
    [string]$WorkshopContentRoot = "$env:USERPROFILE\\OneDrive\\Desktop\\RightClickHide_Workshop",
    [string]$PreviewFile = "$env:USERPROFILE\\OneDrive\\Desktop\\RightClickHide_Workshop\\About\\preview.png",
    [string]$Title = "Right Click Hide UI",
    [string]$Description = "A simple mod that lets you right-click (or Ctrl+right-click) to toggle UI visibility for RimWorld.",
    [string]$ChangeNote = "Updated for RimWorld 1.6 and added Ctrl+right-click option.",
    [string]$WorkshopId = ""  # leave empty to create new item
)

function PromptIfEmpty([string]$varName, [string]$currentValue) {
    if ([string]::IsNullOrWhiteSpace($currentValue)) {
        return Read-Host "$varName"
    }
    return $currentValue
}

$SteamCmdPath = PromptIfEmpty "SteamCMD path" $SteamCmdPath
if (-not (Test-Path $SteamCmdPath)) {
    Write-Error "steamcmd.exe not found at $SteamCmdPath. Install SteamCMD and provide the correct path."; exit 1
}

$WorkshopContentRoot = PromptIfEmpty "Workshop content root (folder)" $WorkshopContentRoot
if (-not (Test-Path $WorkshopContentRoot)) {
    Write-Error "Content root not found: $WorkshopContentRoot"; exit 1
}

$PreviewFile = PromptIfEmpty "Preview image path" $PreviewFile
if (-not (Test-Path $PreviewFile)) {
    Write-Warning "Preview file not found: $PreviewFile. The upload can proceed without a preview but Steam will warn you.";
}

$Title = PromptIfEmpty "Workshop title" $Title
$Description = PromptIfEmpty "Workshop description" $Description
$ChangeNote = PromptIfEmpty "Change note" $ChangeNote
$WorkshopId = PromptIfEmpty "Workshop item ID (leave blank for new item)" $WorkshopId

# Ask for credentials. If user chooses to leave password blank, steamcmd will prompt interactively.
$steamUser = Read-Host "Steam username for upload"
$steamPassword = Read-Host "Steam password (leave blank to prompt in steamcmd)" -AsSecureString
if ($steamPassword.Length -eq 0) {
    $passwordArg = $null
} else {
    $ptr = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($steamPassword)
    $plainPass = [System.Runtime.InteropServices.Marshal]::PtrToStringBSTR($ptr)
    [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($ptr)
    $passwordArg = $plainPass
}

# Build VDF content
$vdf = @{
    "workshopitem" = @{
        "appid" = "294100"
        "contentroot" = $WorkshopContentRoot
        "previewfile" = $PreviewFile
        "title" = $Title
        "description" = $Description
        "changenote" = $ChangeNote
    }
}

# If updating an existing item, include the item id
if (-not [string]::IsNullOrWhiteSpace($WorkshopId)) {
    $vdf.workshopitem["workshopitemid"] = $WorkshopId
}

# Convert the small hashtable to a VDF string. We'll build it manually so formatting is correct.
function HashtreeToVDF([hashtable]$h, [int]$indent = 0) {
    $pad = ' ' * $indent
    $s = ""
    foreach ($k in $h.Keys) {
        $v = $h[$k]
        if ($v -is [hashtable]) {
            $s += "$pad\"$k\"\n$pad{\n"
            $s += HashtreeToVDF $v ($indent + 4)
            $s += "$pad}\n"
        } else {
            # Quote and escape backslashes
            $val = $v -replace '\\', '\\\\'
            $s += "$pad\"$k\" \"$val\"\n"
        }
    }
    return $s
}

$vdfString = "workshopitem\n{\n" + (HashtreeToVDF $vdf.workshopitem 4) + "}\n"

$tempVdf = Join-Path $env:TEMP "RightClickHide_workshop.vdf"
Set-Content -Path $tempVdf -Value $vdfString -Encoding UTF8
Write-Output "Wrote temporary vdf to: $tempVdf"

# Build steamcmd args
$args = @()
if ($passwordArg) { $args += "+login"; $args += $steamUser; $args += $passwordArg } else { $args += "+login"; $args += $steamUser }
$args += "+workshop_build"; $args += $tempVdf; $args += "+quit"

Write-Output "Running steamcmd..."
try {
    & $SteamCmdPath @args
} catch {
    Write-Error "steamcmd failed: $_"
    exit 1
}

Write-Output "Done. If upload succeeded, Steam will have created/updated the workshop item."

# Clean up temp VDF (optional)
# Remove-Item $tempVdf -Force
