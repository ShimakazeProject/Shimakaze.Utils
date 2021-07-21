#!/usr/bin/env pwsh
#Requires -Version 5

param($input,$output=$null,$buffer=1024,$version=0)
if([string]::IsNullOrEmpty($input)){
    #   printf("<input> [<output>] [-v <byte>] [-b <int>]");
    # printf("");
    # printf("<input>  input file");
    # printf("<output>  output file (Default is <inputWithoutExtension>.Extension)");
    # printf("-b <int> Set Buffer Length (Default: 1024)");
    # printf("-v <byte> Set Version (Default use the latest)");
    Write-Host "Usage: $PSScriptRoot\ConvertTo.ps1 <input> [<output>] [<buffer length>] [<version>]"
    Write-Host "ConvertTo.ps1 is a tool to convert file to another format."
    Write-Host "The default version is 0, which means the latest version."
    Write-Host "The default buffer length is 1024."
    Write-Host "The default output file is <inputWithoutExtension>.Extension."
    Write-Host "this usage guide is by copilot."
    exit
}

Import-Module Shimakaze.Utils.Csf.dll -Force
using namespace Shimakaze.Utils.Csf;
if ($version -eq 0){
    CsfUtils.Convert($input, $output, $buffer).Wait();
}
else{
    CsfUtils.Convert($input, $output, $buffer, $version).Wait();
}