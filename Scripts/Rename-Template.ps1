[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [string]$NewName,
    [string]$OldName = "AselDevBlazor",
    [switch]$SkipClean
)

$ErrorActionPreference = "Stop"

function Test-ProjectName {
    param([string]$Value)

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return "Project name is required."
    }

    if ($Value -notmatch '^[A-Za-z_][A-Za-z0-9_]*$') {
        return "Use a valid C# namespace style name. Example: CompanyDxPortal"
    }

    if ($Value -match '\s') {
        return "Project name cannot contain spaces."
    }

    return $null
}

function Rename-PathPart {
    param(
        [string]$Path,
        [string]$OldValue,
        [string]$NewValue
    )

    $parent = Split-Path -Parent $Path
    $leaf = Split-Path -Leaf $Path
    $newLeaf = $leaf.Replace($OldValue, $NewValue)

    if ($leaf -eq $newLeaf) {
        return
    }

    $target = Join-Path $parent $newLeaf

    if (Test-Path -LiteralPath $target) {
        throw "Cannot rename '$Path' to '$target' because the target already exists."
    }

    if ($PSCmdlet.ShouldProcess($Path, "Rename to $newLeaf")) {
        Rename-Item -LiteralPath $Path -NewName $newLeaf
        Write-Host "Renamed: $leaf -> $newLeaf"
    }
}

if ([string]::IsNullOrWhiteSpace($NewName)) {
    $NewName = Read-Host "Enter the new project base name"
}

$validationMessage = Test-ProjectName -Value $NewName
if ($validationMessage) {
    throw $validationMessage
}

if ($NewName -eq $OldName) {
    Write-Host "New name is the same as old name. Nothing to do."
    exit 0
}

$root = (Resolve-Path ".").Path
Write-Host "Template root: $root"
Write-Host "Old name: $OldName"
Write-Host "New name: $NewName"

$excludedDirectories = @(
    ".git",
    ".vs",
    "bin",
    "obj"
)

$textExtensions = @(
    ".cs",
    ".razor",
    ".csproj",
    ".sln",
    ".slnx",
    ".json",
    ".md",
    ".props",
    ".targets",
    ".config",
    ".xml",
    ".yml",
    ".yaml",
    ".cshtml",
    ".css",
    ".js",
    ".ts"
)

Write-Host "Updating text references..."
$failedFiles = New-Object System.Collections.Generic.List[string]
$files = Get-ChildItem -LiteralPath $root -Recurse -File |
    Where-Object {
        $pathParts = $_.FullName.Substring($root.Length).TrimStart('\') -split '[\\/]'
        -not ($pathParts | Where-Object { $excludedDirectories -contains $_ }) -and
        $textExtensions -contains $_.Extension
    }

foreach ($file in $files) {
    try {
        $content = [System.IO.File]::ReadAllText($file.FullName)

        if ([string]::IsNullOrEmpty($content)) {
            continue
        }

        if ($content.Contains($OldName)) {
            $updated = $content.Replace($OldName, $NewName)
            if ($PSCmdlet.ShouldProcess($file.FullName, "Replace $OldName with $NewName")) {
                [System.IO.File]::WriteAllText($file.FullName, $updated)
                Write-Host "Updated: $($file.FullName.Substring($root.Length + 1))"
            }
        }
    }
    catch {
        $failedFiles.Add($file.FullName)
        Write-Warning "Could not update '$($file.FullName)'. Close any app using this file, then run this script again. $($_.Exception.Message)"
    }
}

if ($failedFiles.Count -gt 0) {
    Write-Host ""
    Write-Warning "Rename paused before file/folder renames because $($failedFiles.Count) file(s) could not be updated."
    Write-Host "Close Visual Studio, VS Code, running dotnet processes, or file preview tools, then rerun:"
    Write-Host "  .\Scripts\Rename-Template.ps1 -NewName $NewName"
    exit 1
}

Write-Host "Renaming files..."
Get-ChildItem -LiteralPath $root -Recurse -File |
    Where-Object {
        $pathParts = $_.FullName.Substring($root.Length).TrimStart('\') -split '[\\/]'
        -not ($pathParts | Where-Object { $excludedDirectories -contains $_ }) -and
        $_.Name.Contains($OldName)
    } |
    Sort-Object { $_.FullName.Length } -Descending |
    ForEach-Object {
        Rename-PathPart -Path $_.FullName -OldValue $OldName -NewValue $NewName
    }

Write-Host "Renaming directories..."
Get-ChildItem -LiteralPath $root -Recurse -Directory |
    Where-Object {
        $pathParts = $_.FullName.Substring($root.Length).TrimStart('\') -split '[\\/]'
        -not ($pathParts | Where-Object { $excludedDirectories -contains $_ }) -and
        $_.Name.Contains($OldName)
    } |
    Sort-Object { $_.FullName.Length } -Descending |
    ForEach-Object {
        Rename-PathPart -Path $_.FullName -OldValue $OldName -NewValue $NewName
    }

if (-not $SkipClean) {
    Write-Host "Removing bin/obj folders..."
    Get-ChildItem -LiteralPath $root -Recurse -Directory |
        Where-Object { $_.Name -in @("bin", "obj") } |
        Sort-Object { $_.FullName.Length } -Descending |
        ForEach-Object {
            if ($PSCmdlet.ShouldProcess($_.FullName, "Remove generated build folder")) {
                Remove-Item -LiteralPath $_.FullName -Recurse -Force
                Write-Host "Removed: $($_.FullName.Substring($root.Length + 1))"
            }
        }
}

Write-Host ""
Write-Host "Rename complete."
Write-Host "Next commands:"
Write-Host "  dotnet restore $NewName.slnx"
Write-Host "  dotnet build $NewName.slnx"
