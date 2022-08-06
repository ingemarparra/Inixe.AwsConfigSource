#------------------------------------------------------------------------------
#
# Copyright Inixe S.A.
#
# File: pre-push.ps1
# Purpose: Pre Push GitHook script
#------------------------------------------------------------------------------
$HasTestResults = Test-Path "$($PSScriptRoot)../../TestResults"
if (!$HasTestResults){
    New-Item -Path "$($PSScriptRoot)../../" -Name "TestResults" -ItemType "directory"
}

Write-Host "Executing unit tests before push..."

$TestResultsPath = Resolve-Path "$($PSScriptRoot)../../TestResults"
$UnitTestsScript = Resolve-Path "$($PSScriptRoot)../../build/unit-test.ps1"
$UnitTestsCommand = "$($UnitTestsScript) -TestResultsDir '$($TestResultsPath)'"
Invoke-Expression $UnitTestsCommand
