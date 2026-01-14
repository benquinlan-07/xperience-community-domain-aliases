<#
.SYNOPSIS
    Sets up a Kentico Xperience Dancing Goat sample project with community packages.

.DESCRIPTION
    This script creates a new Dancing Goat project using the latest Kentico Xperience templates,
    sets up the database, and integrates community packages from the ../src/ directory.

.PARAMETER ProjectName
    The name of the project to create (default: "DancingGoat")

.PARAMETER SqlServerName
    The SQL Server instance name (default: "localhost")

.EXAMPLE
    .\Init-DancingGoat.ps1
    .\Init-DancingGoat.ps1 -ProjectName "MyDancingGoat" -SqlServerName ".\SQLEXPRESS"
#>

[CmdletBinding()]
param(
    [string]$ProjectName = "DancingGoat",
    [string]$SqlServerName = "localhost"
)

#region Configuration
$Config = @{
    ProjectName = $ProjectName
    TemplateName = "kentico-xperience-sample-mvc"
    SqlServerName = $SqlServerName
    DatabaseNamePrefix = "DancingGoat-"
    HashStringSalt = "1f528aa5-cbae-42a4-93d0-1a490c5e951a"
    LicenseFilePath = "C:\Temp\xbyk-license.txt"
    PackageName = "Kentico.Xperience.Templates"
    SourcePath = "..\src\"
}
#endregion

#region Functions
function Get-CsprojFiles {
    <#
    .SYNOPSIS
        Finds all .csproj files in the specified directory.
    .PARAMETER SourcePath
        The path to search for .csproj files (default: "..\src\")
    #>
    param (
        [string]$SourcePath = $Config.SourcePath
    )
    
    $scriptDirectory = Split-Path -Parent $MyInvocation.ScriptName
    $fullSourcePath = Join-Path $scriptDirectory $SourcePath
    
    if (Test-Path $fullSourcePath) {
        $csprojFiles = Get-ChildItem -Path $fullSourcePath -Filter "*.csproj" -Recurse
        Write-Host "Found $($csprojFiles.Count) .csproj file(s) in $fullSourcePath" -ForegroundColor Green
        return $csprojFiles
    } else {
        Write-Warning "Source directory not found: $fullSourcePath"
        return @()
    }
}

function Get-LatestPackageVersion {
    <#
    .SYNOPSIS
        Gets the latest version of a NuGet package.
    .PARAMETER PackageName
        The name of the NuGet package
    #>
    param (
        [string]$PackageName
    )
    
    try {
        Write-Host "Fetching latest version of $PackageName..." -ForegroundColor Cyan
        $nugetUrl = "https://api.nuget.org/v3-flatcontainer/$PackageName/index.json"
        $response = Invoke-WebRequest -Uri $nugetUrl -ErrorAction Stop
        $versions = $response.Content | ConvertFrom-Json
        $latestVersion = $versions.versions[-1]
        Write-Host "Latest version: $latestVersion" -ForegroundColor Green
        return $latestVersion
    }
    catch {
        Write-Error "Failed to fetch package version: $_"
        throw
    }
}

function Initialize-ProjectDirectory {
    <#
    .SYNOPSIS
        Creates and initializes the project directory.
    .PARAMETER ProjectDirectory
        The path to the project directory
    #>
    param (
        [string]$ProjectDirectory
    )
    
    if (Test-Path $ProjectDirectory) {
        Write-Host "Removing existing project directory..." -ForegroundColor Yellow
        Remove-Item $ProjectDirectory -Recurse -Force
    }
    
    Write-Host "Creating project directory: $ProjectDirectory" -ForegroundColor Cyan
    New-Item -ItemType "Directory" -Path $ProjectDirectory -Force | Out-Null
    Set-Location -Path $ProjectDirectory
    New-Item -ItemType "Directory" -Path $Config.ProjectName -Force | Out-Null
    Set-Location -Path $Config.ProjectName
}

function Install-ProjectTemplate {
    <#
    .SYNOPSIS
        Installs the Kentico Xperience project template and creates the project.
    #>
    Write-Host "Installing project templates..." -ForegroundColor Cyan
    dotnet new install $Config.PackageName
    
    Write-Host "Creating project from template..." -ForegroundColor Cyan
    dotnet new $Config.TemplateName -n $Config.ProjectName --no-restore --allow-scripts Yes
    
    Write-Host "Restoring packages..." -ForegroundColor Cyan
    dotnet restore
    dotnet tool restore
}

function Initialize-Database {
    <#
    .SYNOPSIS
        Creates and initializes the project database.
    .PARAMETER DatabaseName
        The name of the database to create
    #>
    param (
        [string]$DatabaseName
    )
    
    Write-Host "Creating database: $DatabaseName" -ForegroundColor Cyan
    dotnet kentico-xperience-dbmanager -- `
        -s $Config.SqlServerName `
        -d $DatabaseName `
        -a "xperience" `
        --hash-string-salt $Config.HashStringSalt `
        --license-file $Config.LicenseFilePath `
        --recreate-existing-database
}

function Add-CommunityPackages {
    <#
    .SYNOPSIS
        Adds community packages to the solution and creates references.
    #>
    Write-Host "Setting up solution and adding community packages..." -ForegroundColor Cyan
    
    # Move up a directory
    Set-Location -Path "..\"

    # Create solution
    dotnet new sln -n $Config.ProjectName
    dotnet sln add "$($Config.ProjectName)\$($Config.ProjectName).csproj"
    
    # Get and add community packages
    $packageProjectFiles = Get-CsprojFiles
    
    foreach ($file in $packageProjectFiles) {
        Write-Host "  Adding: $($file.Name)" -ForegroundColor Yellow
        dotnet sln add $file.FullName
        Write-Host "    Adding reference to: $($file.Name)" -ForegroundColor Gray
        dotnet add "$($Config.ProjectName)\$($Config.ProjectName).csproj" reference $file.FullName
    }
}

function Build-Solution {
    <#
    .SYNOPSIS
        Builds the complete solution.
    #>
    Write-Host "Building solution..." -ForegroundColor Cyan
    dotnet build
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Build completed successfully!" -ForegroundColor Green
    } else {
        Write-Error "❌ Build failed with exit code: $LASTEXITCODE"
        throw "Build failed"
    }
}

function Apply-FileReplacements {
    <#
    .SYNOPSIS
        Applies file replacements from template.file-replacements.json to the generated project.
    #>
    Write-Host "Applying file replacements..." -ForegroundColor Cyan
    
    $scriptDirectory = Split-Path -Parent $MyInvocation.ScriptName
    $templatePath = Join-Path $scriptDirectory "template.file-replacements.json"
    
    if (-not (Test-Path $templatePath)) {
        Write-Host "No template.file-replacements.json found - skipping file replacements" -ForegroundColor Gray
        return
    }
    
    try {
        $template = Get-Content -Path $templatePath -Raw | ConvertFrom-Json
        $replacementCount = 0
        
        foreach ($replacement in $template.replacements) {
            # Skip if explicitly disabled
            if ($replacement.PSObject.Properties.Name -contains "enabled" -and $replacement.enabled -eq $false) {
                Write-Host "  Skipping (disabled): $($replacement.filePath)" -ForegroundColor Gray
                continue
            }
            
            $targetFile = $replacement.filePath
            
            if (-not (Test-Path $targetFile)) {
                Write-Warning "  File not found: $targetFile - skipping"
                continue
            }
            
            $description = if ($replacement.PSObject.Properties.Name -contains "description") { 
                " - $($replacement.description)" 
            } else { 
                "" 
            }
            
            Write-Host "  Processing: $targetFile$description" -ForegroundColor Yellow
            
            if ($replacement.type -eq "full") {
                # Replace entire file content
                Set-Content -Path $targetFile -Value $replacement.content -Encoding UTF8
                Write-Host "    ✅ Replaced entire file content" -ForegroundColor Green
                $replacementCount++
            }
            elseif ($replacement.type -eq "search-replace") {
                # Apply search-replace operations
                $fileContent = Get-Content -Path $targetFile -Raw
                $modified = $false
                
                foreach ($operation in $replacement.replacements) {
                    if ($fileContent -match [regex]::Escape($operation.search)) {
                        $fileContent = $fileContent -replace [regex]::Escape($operation.search), $operation.replace
                        Write-Host "    ✅ Replaced: $($operation.search.Substring(0, [Math]::Min(50, $operation.search.Length)))..." -ForegroundColor Green
                        $modified = $true
                        $replacementCount++
                    }
                    else {
                        Write-Warning "    Search string not found: $($operation.search.Substring(0, [Math]::Min(50, $operation.search.Length)))..."
                    }
                }
                
                if ($modified) {
                    Set-Content -Path $targetFile -Value $fileContent -Encoding UTF8 -NoNewline
                }
            }
            else {
                Write-Warning "  Unknown replacement type: $($replacement.type)"
            }
        }
        
        if ($replacementCount -eq 0) {
            Write-Host "No file replacements applied (all disabled or no matches found)" -ForegroundColor Gray
        }
        else {
            Write-Host "✅ Applied $replacementCount file replacement(s)" -ForegroundColor Green
        }
    }
    catch {
        Write-Warning "Failed to apply file replacements: $_"
    }
}
#endregion

#region Main Execution
try {
    Write-Host "🚀 Starting Kentico Xperience Dancing Goat setup..." -ForegroundColor Magenta
    Write-Host "Project: $($Config.ProjectName)" -ForegroundColor White
    Write-Host "SQL Server: $($Config.SqlServerName)" -ForegroundColor White
    Write-Host ""
    
    # Get latest version
    $latestVersion = Get-LatestPackageVersion -PackageName $Config.PackageName
    $projectDirectory = ".\DancingGoat\$latestVersion"
    $databaseName = $Config.DatabaseNamePrefix + $latestVersion
    
    # Store original location
    $originalLocation = Get-Location
    
    # Initialize project
    Initialize-ProjectDirectory -ProjectDirectory $projectDirectory
    
    # Install template and create project
    Install-ProjectTemplate
    
    # Initialize database
    Initialize-Database -DatabaseName $databaseName
    
    # Build initial project
    Build-Solution
    
    # Add community packages
    Add-CommunityPackages
    
    # Apply file replacements from template (after moving to solution directory)
    Apply-FileReplacements
    
    # Final build
    Build-Solution
    
    Write-Host ""
    Write-Host "🎉 Xperience by Kentico Dancing Goat solution setup completed successfully!" -ForegroundColor Green
    Write-Host "📁 Project location: $(Get-Location)" -ForegroundColor Cyan
    Write-Host "🗄️  Database: $databaseName" -ForegroundColor Cyan
}
catch {
    Write-Error "❌ Setup failed: $_"
    exit 1
}
finally {
    # Return to original location
    if ($originalLocation) {
        Set-Location -Path $originalLocation
    }
}
#endregion
