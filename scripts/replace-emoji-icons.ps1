param(
    [string]$Path = (Join-Path $PSScriptRoot "..\src\SistemaGVP.WPF"),
    [switch]$DryRun
)

$emojiMap = @{
    "📦" = '<iconPacks:PackIconMaterial Kind="PackageVariant" />'
    "🛒" = '<iconPacks:PackIconMaterial Kind="Cart" />'
    "👤" = '<iconPacks:PackIconMaterial Kind="Account" />'
    "👥" = '<iconPacks:PackIconMaterial Kind="AccountGroup" />'
    "⚙️" = '<iconPacks:PackIconMaterial Kind="Cog" />'
    "📊" = '<iconPacks:PackIconMaterial Kind="ChartBar" />'
    "📈" = '<iconPacks:PackIconMaterial Kind="ChartLine" />'
    "🔐" = '<iconPacks:PackIconMaterial Kind="Lock" />'
    "💾" = '<iconPacks:PackIconMaterial Kind="ContentSave" />'
    "📋" = '<iconPacks:PackIconMaterial Kind="ClipboardText" />'
    "🏭" = '<iconPacks:PackIconMaterial Kind="Factory" />'
    "➕" = '<iconPacks:PackIconMaterial Kind="Plus" />'
    "✏️" = '<iconPacks:PackIconMaterial Kind="Pencil" />'
    "🗑️" = '<iconPacks:PackIconMaterial Kind="Delete" />'
    "🔍" = '<iconPacks:PackIconMaterial Kind="Magnify" />'
    "⚠️" = '<iconPacks:PackIconMaterial Kind="Alert" />'
    "✅" = '<iconPacks:PackIconMaterial Kind="CheckCircle" />'
    "❌" = '<iconPacks:PackIconMaterial Kind="CloseCircle" />'
    "☀️" = '<iconPacks:PackIconMaterial Kind="WhiteBalanceSunny" />'
    "🌙" = '<iconPacks:PackIconMaterial Kind="WeatherNight" />'
    "🧾" = '<iconPacks:PackIconMaterial Kind="Receipt" />'
    "🔎" = '<iconPacks:PackIconMaterial Kind="Magnify" />'
    "🗂️" = '<iconPacks:PackIconMaterial Kind="Folder" />'
    "💰" = '<iconPacks:PackIconMaterial Kind="Cash" />'
    "💵" = '<iconPacks:PackIconMaterial Kind="CurrencyUsd" />'
    "🏛" = '<iconPacks:PackIconMaterial Kind="Bank" />'
    "🏆" = '<iconPacks:PackIconMaterial Kind="Trophy" />'
    "🏷️" = '<iconPacks:PackIconMaterial Kind="Tag" />'
    "⚡" = '<iconPacks:PackIconMaterial Kind="LightningBolt" />'
    "📷" = '<iconPacks:PackIconMaterial Kind="Camera" />'
    "⏸️" = '<iconPacks:PackIconMaterial Kind="Pause" />'
    "💳" = '<iconPacks:PackIconMaterial Kind="CreditCard" />'
    "⏹️" = '<iconPacks:PackIconMaterial Kind="Stop" />'
    "✕" = '<iconPacks:PackIconMaterial Kind="Close" />'
    "✓" = '<iconPacks:PackIconMaterial Kind="Check" />'
    "▶️" = '<iconPacks:PackIconMaterial Kind="Play" />'
    "🔄" = '<iconPacks:PackIconMaterial Kind="Refresh" />'
    "🔒" = '<iconPacks:PackIconMaterial Kind="Lock" />'
    "🔓" = '<iconPacks:PackIconMaterial Kind="LockOpenVariant" />'
}

$nsToAdd = 'xmlns:iconPacks="http://metro.mahapps.com/winfx/xaml/iconpacks"'
$nsPattern = 'xmlns:iconPacks="http://metro\.mahapps\.com/winfx/xaml/iconpacks"'

$xamlFiles = Get-ChildItem -Path $Path -Recurse -Filter "*.xaml" | Where-Object { $_.FullName -notmatch "\\bin\\|\\obj\\" }
$csFiles = Get-ChildItem -Path $Path -Recurse -Filter "*.cs" | Where-Object { $_.FullName -notmatch "\\bin\\|\\obj\\" }

$totalChanges = 0
$changedFiles = @()

function Replace-EmojisInFile {
    param($File, $IsCsFile = $false)
    $content = Get-Content -Path $File.FullName -Raw
    $original = $content
    $fileChanged = $false

    if (-not $IsCsFile) {
        # Check if iconPacks namespace is missing and content has emojis
        $hasEmoji = $false
        foreach ($emoji in $emojiMap.Keys) {
            if ($content.Contains($emoji)) {
                $hasEmoji = $true
                break
            }
        }

        if ($hasEmoji -and $content -notmatch $nsPattern) {
            # Add namespace to root element
            if ($content -match '(<\w+[^>]*xmlns="[^"]+")') {
                $insertPoint = $matches[0].Length + $matches[0].IndexOf(" ")
                if ($insertPoint -gt 0) {
                    $content = $content.Substring(0, $matches[0].Length) + "`n             $nsToAdd" + $content.Substring($matches[0].Length)
                    $fileChanged = $true
                }
            }
        }
    }

    # Replace emojis in XAML text blocks (but not inside code comments)
    $localChanges = 0
    foreach ($emoji in $emojiMap.Keys) {
        if ($IsCsFile) {
            # In .cs files, replace emojis in string literals with text-only versions
            $replacement = $emojiMap[$emoji] -replace '<iconPacks:PackIconMaterial Kind="([^"]+)" />', '$1'
            $textReplacement = "[$replacement]"
            $countRef = [Ref]$localChanges
            $content = $content -replace [regex]::Escape($emoji), { $countRef.Value++; $textReplacement }
            $localChanges = $countRef.Value
        }
        else {
            # In XAML, replace standalone emoji text elements with proper icon elements
            $emojiEscaped = [regex]::Escape($emoji)
            $replacement = $emojiMap[$emoji]

            # Replace emoji used as TextBlock text content (in simple patterns)
            $pattern = '(?s)(Text=")' + $emojiEscaped + '(")'
            if ($content -match $pattern) {
                $content = $content -replace $pattern, '${1}${2}'
                $localChanges++
                $fileChanged = $true
            }

            # Replace emoji inside specific patterns: TextBlock.Text='📦' style
            $countRef = [Ref]$localChanges
            $content = $content -replace [regex]::Escape($emoji), { $countRef.Value++; $replacement }
            $localChanges = $countRef.Value
        }
        if ($localChanges -gt 0) { $fileChanged = $true }
    }

    if ($fileChanged) {
        if (-not $DryRun) {
            Set-Content -Path $File.FullName -Value $content -NoNewline
        }
        return @{ File = $File.FullName; Changes = $localChanges }
    }
    return $null
}

Write-Host "=== Reemplazo de emojis por PackIconMaterial ===" -ForegroundColor Cyan
Write-Host "Ruta: $Path"
if ($DryRun) { Write-Host "MODO SIMULACIÓN (DryRun) - no se realizarán cambios" -ForegroundColor Yellow }
Write-Host ""

# Process XAML files
Write-Host "Procesando archivos XAML..." -ForegroundColor Yellow
foreach ($file in $xamlFiles) {
    $result = Replace-EmojisInFile -File $file -IsCsFile $false
    if ($result) {
        $totalChanges += $result.Changes
        $changedFiles += $result.File
        Write-Host "  $($file.Name): $($result.Changes) cambio(s)" -ForegroundColor Green
    }
}

# Process CS files
Write-Host "Procesando archivos CS..." -ForegroundColor Yellow
foreach ($file in $csFiles) {
    $result = Replace-EmojisInFile -File $file -IsCsFile $true
    if ($result) {
        $totalChanges += $result.Changes
        $changedFiles += $result.File
        Write-Host "  $($file.Name): $($result.Changes) cambio(s)" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "=== Reporte Final ===" -ForegroundColor Cyan
Write-Host "Archivos modificados: $($changedFiles.Count)"
Write-Host "Cambios totales: $totalChanges"
if ($totalChanges -gt 0 -and -not $DryRun) {
    Write-Host "Se realizaron los cambios. Revise los archivos modificados y compile el proyecto." -ForegroundColor Green
}
elseif ($totalChanges -gt 0 -and $DryRun) {
    Write-Host "Simulación completada. Use -DryRun:`$false para aplicar cambios." -ForegroundColor Yellow
}
