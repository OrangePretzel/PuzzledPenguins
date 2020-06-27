$coverageRunnerPath = "$env:USERPROFILE\.nuget\packages\opencover\4.6.519\tools\OpenCover.Console.exe"
$reportGeneratorPath = "$env:USERPROFILE\.nuget\packages\reportgenerator\3.1.2\tools\ReportGenerator.exe"

$testDLLPath = "$PSScriptRoot\..\SlideCore\SlideCore.Tests\bin\Release\netcoreapp2.0\SlideCore.Tests.dll"
$outputPath = "$PSScriptRoot\..\temp\coverage"

New-Item -Type Directory -Path $outputPath -Force
Invoke-Expression "$coverageRunnerPath -oldStyle -register:user -target:`"dotnet.exe`" -targetargs:`"$testDLLPath --result=$outputPath\TestResult.xml`" -filter:+[Slide*]* -output:$outputPath\CoverageResult.xml"
Invoke-Expression "$reportGeneratorPath -reports:$outputPath\CoverageResult.xml -targetdir:$outputPath\CoverageResult\"