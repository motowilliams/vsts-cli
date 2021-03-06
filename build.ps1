param (
    [switch]$test
)

$scriptroot = "$PSScriptRoot"
$artifacts = "$scriptroot\artifacts"

Remove-Item -Force -Recurse $artifacts -ErrorAction Ignore

$sourcedirectory = "$PSScriptRoot\src"
$solutionName = "vsts.cli"
$solutionFile = "$sourcedirectory\$solutionName.sln"

dotnet restore $solutionFile
dotnet publish $solutionFile -r win10-x64 -o $artifacts;

if ($test) {
    Get-ChildItem $artifacts\*tests.dll | ForEach-Object { dotnet vstest $_  }
}