$time = [int64](Get-Date -UFormat %s)
$buildPath = "\bin\temp\$time\"

$project = "Huragok.csproj"
$config = "Release"

$json = dotnet msbuild $project `
  -property:Configuration=$config `
  -getProperty:TargetName,VersionPrefix,VersionSuffix,BlamVariant

$data = $json | ConvertFrom-Json

$props = $data.Properties

$outFileName = "{0}-{1}+{2}_$time.zip" -f `
    $props.TargetName,
    $props.VersionPrefix,
    $props.VersionSuffix

dotnet publish -c Release -p:OutputPath=$buildPath

New-Item -ItemType Directory -Force -Path .\build 1>$null
Compress-Archive -Path $buildPath\publish\* -DestinationPath .\build\$outFileName -Force

Remove-Item -Recurse $buildPath

Write-Output "built: build\$outFileName"