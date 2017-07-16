$ErrorActionPreference = "SilentlyContinue";

$repoRoot = (Split-Path $PSScriptRoot -Parent)
$helpFile = "$repoRoot\help.md"
$command = "vsts"
$currentDirectory = (Get-Location).path

$commands = @(
    "-h",
    "browse -h",
    "builds -h",
    "builds logs -h",
    "builds queue -h",
    "code -h",
    "pullrequests -h",
    "pullrequests create -h",
    "workitems -h"
    "workitems add -h"
)

New-Item -ItemType File $helpFile -Force | Out-Null
Write-Output "## Current Supported Commands" | Out-File -Append -FilePath $helpFile
$commands | ForEach-Object {
    $commandOutput = Invoke-CommandString -command $command -commandArgs $_
    Write-Output "### $command $_" | Out-File -Append -FilePath $helpFile
    Write-Output '```' | Out-File -Append -FilePath $helpFile
    $commandOutput | `
        Select-String -NotMatch -SimpleMatch $currentDirectory | `
        Out-File -Append -FilePath $helpFile
    Write-Output '```' | Out-File -Append -FilePath $helpFile
}

(Get-Content $helpFile -Raw) -replace "(\r\n){3,}", "`r`n" | Out-File -Encoding ascii -FilePath $helpFile

$readme = "$repoRoot\Readme.md"
Get-Content "$repoRoot\Readme-Header.md" | Out-File -FilePath $readme -Encoding ascii
Get-Content $helpFile | Out-File -Append -FilePath $readme -Encoding ascii
Remove-Item $helpFile