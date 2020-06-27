# Convert Levels (via NodeJS Script)
pushd $PSScriptRoot\LevelToJSONConverter
& npm install
popd
& node $PSScriptRoot\LevelToJSONConverter\convertAll.js;

# Build Solution
$projects = @(
	"$PSScriptRoot\..\SlideCore\SlideCore.Tests\SlideCore.Tests.csproj" # Builds SlideCore.csproj as a dependency
) | Resolve-Path
foreach($proj in $projects)
{
	& dotnet build $proj -c Release
}

# Copy Output
$source = Resolve-Path "$PSScriptRoot\..\SlideCore\SlideCore\bin\Release\netstandard2.0\*"
$dest = Resolve-Path "$PSScriptRoot\..\SlideUnity\Assets\Scripts\SlideCore\"
$contentSource = Resolve-Path "$PSScriptRoot\..\SlideCore\SlideCore\bin\Release\netstandard2.0\Content\*"
$contentDest = Resolve-Path "$PSScriptRoot\..\SlideUnity\Assets\Resources\"

Write-Host "Copying SlideCore.dll from [$source] to [$dest]"
Copy-Item -Path $source -Destination $dest -Force
Remove-Item -Path (Join-Path $dest "Content")
Copy-Item -Path $contentSource -Destination $contentDest -Force -Recurse
Write-Host "Successfully copied SlideCore.dll"

# Run Coverage
& $PSScriptRoot\cover.ps1