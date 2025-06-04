# Auto publish script
param(
    [switch]$IncrementVersion
)

# 设置脚本编码为 UTF-8
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$PSDefaultParameterValues['*:Encoding'] = 'utf8'

# Set working directory to project root
Set-Location (Split-Path $PSScriptRoot)

# Read version information
$versionJson = Get-Content -Path "version.json" -Encoding UTF8 -Raw | ConvertFrom-Json
$version = $versionJson.version

# Increment version if specified
if ($IncrementVersion) {
    $versionParts = $version.Split('.')
    $versionParts[2] = ([int]$versionParts[2] + 1).ToString()
    $version = $versionParts -join '.'
    $versionJson.version = $version
    $versionJson.releaseDate = (Get-Date).ToString("yyyy-MM-dd")
    $versionJson | ConvertTo-Json -Depth 10 | Set-Content -Path "version.json" -Encoding UTF8
}

# Update AssemblyInfo.cs version
$assemblyInfoContent = Get-Content -Path "Properties\AssemblyInfo.cs" -Encoding UTF8
$assemblyInfoContent = $assemblyInfoContent -replace '\[assembly: AssemblyVersion\(".*?"\)\]', "[assembly: AssemblyVersion(`"$version.0`")]"
$assemblyInfoContent = $assemblyInfoContent -replace '\[assembly: AssemblyFileVersion\(".*?"\)\]', "[assembly: AssemblyFileVersion(`"$version.0`")]"
Set-Content -Path "Properties\AssemblyInfo.cs" -Value $assemblyInfoContent -Encoding UTF8
Write-Host "Updated AssemblyInfo.cs version to $version"

# Create release directory
$releaseDir = "Release"
New-Item -ItemType Directory -Force -Path $releaseDir

# Clean up old release files
Remove-Item -Path "$releaseDir\DesktopPet-v*" -Force -Recurse
Remove-Item -Path "bin\Release" -Force -Recurse -ErrorAction SilentlyContinue

Write-Host "Building full version (with runtime)..."
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
$fullVersionDir = "Release-v$version-full"
New-Item -ItemType Directory -Force -Path $fullVersionDir
Copy-Item "bin\Release\net9.0-windows\win-x64\publish\DesktopPet.exe" $fullVersionDir
Copy-Item -Recurse "dog_gifs" $fullVersionDir -Exclude "dog_gifs_backup.zip"

Write-Host "Building framework-dependent version..."
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true
$frameworkVersionDir = "Release-v$version-framework"
New-Item -ItemType Directory -Force -Path $frameworkVersionDir
Copy-Item "bin\Release\net9.0-windows\win-x64\publish\DesktopPet.exe" $frameworkVersionDir
Copy-Item -Recurse "dog_gifs" $frameworkVersionDir -Exclude "dog_gifs_backup.zip"

# Create README.txt
$readmeContent = @"
Desktop Pet v$version

[Version Information]
Release Date: $($versionJson.releaseDate)

Updates:
$(($versionJson.changes | ForEach-Object { "- $_" }) -join "`n")

[Available Versions]
1. Full Version (approx. 60MB):
   - File: DesktopPet-v$version-full.zip
   - Features: Includes all runtime components
   - Best for: Users who want a standalone installation

2. Framework Version (approx. 1MB):
   - File: DesktopPet-v$version-framework.zip
   - Features: Smaller size, requires .NET Runtime
   - Best for: Users with .NET 9.0 already installed
   - Requirement: .NET 9.0 Desktop Runtime must be installed

[Installation Guide]
1. Extract the zip file to any directory
2. Run DesktopPet.exe
3. The pet will appear in the bottom right of your screen
4. Right-click to access the settings menu

[File Structure]
DesktopPet.exe - Main application
dog_gifs/      - Animation files (required)
"@

Set-Content -Path "$fullVersionDir\README.txt" -Value $readmeContent -Encoding UTF8
Set-Content -Path "$frameworkVersionDir\README.txt" -Value $readmeContent -Encoding UTF8

# Create ZIP packages
Write-Host "Creating ZIP packages..."
Compress-Archive -Path "$fullVersionDir\*" -DestinationPath "$releaseDir\DesktopPet-v$version-full.zip" -Force
Compress-Archive -Path "$frameworkVersionDir\*" -DestinationPath "$releaseDir\DesktopPet-v$version-framework.zip" -Force

# Clean up temporary files
Remove-Item -Path $fullVersionDir -Recurse -Force
Remove-Item -Path $frameworkVersionDir -Recurse -Force

# Clean up build directories
Write-Host "Cleaning up build directories..."
Remove-Item -Path "bin" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "obj" -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "Release completed!"
Write-Host ("Full version: {0}\DesktopPet-v{1}-full.zip" -f $releaseDir, $version)
Write-Host ("Framework version: {0}\DesktopPet-v{1}-framework.zip" -f $releaseDir, $version)
