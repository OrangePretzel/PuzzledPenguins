param (
	[Parameter(Mandatory=$true)]
	[string] $levelID
)

& "dotnet" ("$PSScriptRoot\..\SlideCore\SlideCLI\bin\Debug\netcoreapp2.0\SlideCLI.dll solve -b $PSScriptRoot\..\SlideUnity\Assets\Resources\Level\ -l `"$levelID`"").Split(" ")