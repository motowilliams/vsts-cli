$repoRoot = (Split-Path $PSScriptRoot -Parent)
$helpFile = "$repoRoot\help.md"
$command = "vsts"
$currentDirectory = (Get-Location).path

$commands = @(
    "",
    "-h",
    "browse -h",
    "builds -h",
    "builds log -h",
    "code -h",
    "pullrequests -h",
    "workitems -h"
    "workitems add -h"
)

Remove-Item -Path $helpFile -ErrorAction Ignore
$commands | ForEach-Object {
    $commandOutput = Invoke-CommandString -command $command -commandArgs $_
    Write-Output "### $command $_" | Out-File -Append -FilePath $helpFile
    Write-Output '```' | Out-File -Append -FilePath $helpFile
    $commandOutput | `
     Select-String -NotMatch -SimpleMatch $currentDirectory | `
     Out-File -Append -FilePath $helpFile
    Write-Output '```' | Out-File -Append -FilePath $helpFile
}
