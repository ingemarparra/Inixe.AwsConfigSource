#------------------------------------------------------------------------------
#
# Copyright Inixe S.A.
#
# File: unit-test.ps1
# Purpose: Testing Script for Unit Test
#------------------------------------------------------------------------------
param(
    [string]$TestResultsDir,
    [Parameter(Mandatory=$true)]
    [string]$CoverageReportsDir,
    [Parameter(Mandatory=$false)]
    [int]$Threshold = 80
)

dotnet test --configuration release `
    --logger 'trx' `
    --logger 'console;verbosity=normal' `
    --results-directory $TestResultsDir `
    /p:CollectCoverage=true `
    /p:Threshold=$Threshold