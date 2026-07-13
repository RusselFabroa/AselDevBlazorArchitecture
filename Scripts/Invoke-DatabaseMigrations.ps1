[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateSet("PostgreSql", "MySql")]
    [string]$Provider,

    [Parameter(Mandatory)]
    [ValidateSet("Add", "List", "Script", "Update", "Remove")]
    [string]$Action,

    [string]$MigrationName,
    [string]$ConnectionString,
    [string]$Output
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$projectName = "AselDevBlazorArchitecture.Migrations.$Provider"
$project = Join-Path $root "$projectName\$projectName.csproj"
$context = "AppDbContext"

if (-not (Test-Path -LiteralPath $project)) {
    throw "Migration project not found: $project"
}

Push-Location $root
try {
    dotnet tool restore
    if ($LASTEXITCODE -ne 0) { throw "Unable to restore the repository-local EF tool." }

    if (-not [string]::IsNullOrWhiteSpace($ConnectionString)) {
        $env:DATABASE_CONNECTION_STRING = $ConnectionString
    }

    dotnet build $project --nologo --verbosity minimal --maxcpucount:1 --nodeReuse:false
    if ($LASTEXITCODE -ne 0) { throw "Unable to build migration project: $project" }

    $common = @(
        "--project", $project,
        "--startup-project", $project,
        "--context", $context,
        "--no-build"
    )

    switch ($Action) {
        "Add" {
            if ([string]::IsNullOrWhiteSpace($MigrationName)) {
                throw "MigrationName is required for the Add action."
            }

            dotnet tool run dotnet-ef -- migrations add $MigrationName @common --output-dir Migrations
        }
        "List" {
            dotnet tool run dotnet-ef -- migrations list @common --no-connect
        }
        "Script" {
            if ([string]::IsNullOrWhiteSpace($Output)) {
                $outputDirectory = Join-Path $root "artifacts\migrations"
                New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
                $Output = Join-Path $outputDirectory "$($Provider.ToLowerInvariant()).sql"
            }

            dotnet tool run dotnet-ef -- migrations script @common --idempotent --output $Output
            if ($LASTEXITCODE -eq 0) { Write-Host "Migration script created: $Output" }
        }
        "Update" {
            if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
                throw "ConnectionString is required for the Update action."
            }

            dotnet tool run dotnet-ef -- database update @common --connection $ConnectionString
        }
        "Remove" {
            dotnet tool run dotnet-ef -- migrations remove @common
        }
    }

    if ($LASTEXITCODE -ne 0) {
        throw "EF migration action '$Action' failed for provider '$Provider'."
    }
}
finally {
    Remove-Item Env:DATABASE_CONNECTION_STRING -ErrorAction SilentlyContinue
    Pop-Location
}
