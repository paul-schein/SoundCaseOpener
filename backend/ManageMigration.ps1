#!/usr/bin/env pwsh

param(
    [string]$MigrationProject = "./SoundCaseOpener.Persistence/SoundCaseOpener.Persistence.csproj",
    [string]$StartupProject   = "./SoundCaseOpener/SoundCaseOpener.csproj"
)

Write-Host "=================================="
Write-Host "   EF Core Migration Management   "
Write-Host "=================================="
Write-Host "1) Add Migration"
Write-Host "2) Update Database"
Write-Host "----------------------------------"

$choice = Read-Host "Please enter 1 or 2"

switch ($choice) {
    1 {
        $migrationName = Read-Host "Enter the migration name (leave blank for 'Initial')"
        if ([string]::IsNullOrWhiteSpace($migrationName)) {
            $migrationName = "Initial"
        }

        Write-Host "`nAdding migration '$migrationName'..."
        dotnet ef migrations add $migrationName `
            --project $MigrationProject `
            --startup-project $StartupProject
    }

    2 {
        Write-Host "`nUpdating the database..."
        dotnet ef database update `
            --project $MigrationProject `
            --startup-project $StartupProject
    }

    default {
        Write-Host "`nInvalid selection. Exiting..."
    }
}

Write-Host "`nDone!"