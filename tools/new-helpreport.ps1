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

$readme = "$repoRoot\readme.md"
$helpOutputSection = "## Current Supported Commands"

$readmelines = (Get-Content $readme).Split([Environment]::NewLine)
New-Item -ItemType File $readme -Force | Out-Null
foreach ($line in $readmelines) {
    if ($line -eq $helpOutputSection) {
        break;
    }
    $line | Add-Content -Encoding Ascii -Path $readme
}

Write-Output $helpOutputSection | Out-File -Encoding ascii -Append -FilePath $readme

$commands | ForEach-Object {
    $commandOutput = Invoke-CommandString -command $command -commandArgs $_
    Write-Output "### $command $_" | Out-File -Encoding ascii -Append -FilePath $readme
    Write-Output '```' | Out-File -Encoding ascii -Append -FilePath $readme
    $commandOutput | `
        Select-String -NotMatch -SimpleMatch $currentDirectory | `
        Out-File -Encoding ascii -Append -FilePath $readme
    Write-Output '```' | Out-File -Encoding ascii -Append -FilePath $readme
}

(Get-Content $readme -Raw) -replace "(\r\n){3,}", "`r`n" | Out-File -Encoding ascii -FilePath $readme
