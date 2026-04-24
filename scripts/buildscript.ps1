#!/usr/bin/env pwsh

# Build standalone executables for Windows, macOS, and Linux
# Covers the all-in-one CLI and each per-database CLI variant

Write-Host "Building standalone Journey CLI executables..." -ForegroundColor Green

$version = "1.1.6"

$outputDir = "publish"
if (Test-Path $outputDir) {
    Remove-Item $outputDir -Recurse -Force
}
New-Item -ItemType Directory -Path $outputDir | Out-Null

$variants = @(
    @{ Project = "Journey.Command/Journey.Command.csproj";           Name = "journey" },
    @{ Project = "Journey.Command.Postgres/Journey.Command.Postgres.csproj"; Name = "journey-postgres" },
    @{ Project = "Journey.Command.MySql/Journey.Command.MySql.csproj";       Name = "journey-mysql" },
    @{ Project = "Journey.Command.Sqlite/Journey.Command.Sqlite.csproj";     Name = "journey-sqlite" },
    @{ Project = "Journey.Command.Mssql/Journey.Command.Mssql.csproj";       Name = "journey-mssql" },
    @{ Project = "Journey.Command.Cassandra/Journey.Command.Cassandra.csproj"; Name = "journey-cassandra" }
)

$platforms = @("win-x64", "win-arm64", "osx-x64", "osx-arm64", "linux-x64", "linux-arm64")

foreach ($variant in $variants) {
    Write-Host "`nBuilding $($variant.Name)..." -ForegroundColor Cyan
    foreach ($platform in $platforms) {
        $dir = "$outputDir/$($variant.Name)-$platform"
        Write-Host "  [$platform]" -ForegroundColor Yellow
        dotnet publish $variant.Project -c Release -r $platform -f net9.0 -o $dir --self-contained true -p:PublishSingleFile=true -p:Version=$version --nologo -v q

        if ($platform -like "win-*") {
            $archive = "$outputDir/$($variant.Name)_${version}_$platform.zip"
            Compress-Archive -Path $dir -DestinationPath $archive -Force
        } else {
            $archive = "$outputDir/$($variant.Name)_${version}_$platform.tgz"
            tar -czf $archive -C $outputDir "$($variant.Name)-$platform"
        }
        Write-Host "    -> $archive" -ForegroundColor DarkGray
    }
}

Write-Host "`nDone. Archives in '$outputDir/':" -ForegroundColor Green
Get-ChildItem $outputDir -File | Select-Object Name, @{N="Size";E={"{0:N0} KB" -f ($_.Length / 1KB)}} | Format-Table -AutoSize
