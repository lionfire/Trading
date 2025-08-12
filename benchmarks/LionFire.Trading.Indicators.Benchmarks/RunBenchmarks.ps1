#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Runs the LionFire Trading Indicators benchmarks and generates reports
.DESCRIPTION
    This script builds the project, runs benchmarks, and generates consolidated reports
.PARAMETER Configuration
    Build configuration (Debug/Release). Default is Release.
.PARAMETER Filter
    Filter for specific benchmarks to run
.PARAMETER OutputFormat
    Output formats for reports (Html, Csv, Json, Markdown)
.EXAMPLE
    ./RunBenchmarks.ps1
.EXAMPLE
    ./RunBenchmarks.ps1 -Filter "*Sma*" -OutputFormat Html,Csv
#>

param(
    [string]$Configuration = "Release",
    [string]$Filter = "*",
    [string[]]$OutputFormat = @("Html", "Csv", "Json", "Markdown"),
    [switch]$SkipBuild,
    [switch]$QuickRun,
    [switch]$UseSimpleRunner
)

$ErrorActionPreference = "Stop"

# Colors for output
function Write-Header {
    param([string]$Message)
    Write-Host "`n================================" -ForegroundColor Cyan
    Write-Host $Message -ForegroundColor Cyan
    Write-Host "================================" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "✓ $Message" -ForegroundColor Green
}

function Write-Info {
    param([string]$Message)
    Write-Host "→ $Message" -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "✗ $Message" -ForegroundColor Red
}

# Setup paths
$ProjectPath = $PSScriptRoot
$ProjectFile = Join-Path $ProjectPath "LionFire.Trading.Indicators.Benchmarks.csproj"
$OutputPath = Join-Path $ProjectPath "bin" $Configuration "net9.0"
$ExecutableName = "LionFire.Trading.Indicators.Benchmarks"
if ($IsWindows) { $ExecutableName += ".exe" }
$ExecutablePath = Join-Path $OutputPath $ExecutableName

$ReportsPath = Join-Path $ProjectPath "Reports"
$ResultsPath = Join-Path $ProjectPath "BenchmarkDotNet.Artifacts" "results"

# Create Reports directory if it doesn't exist
if (!(Test-Path $ReportsPath)) {
    New-Item -ItemType Directory -Path $ReportsPath | Out-Null
    Write-Success "Created Reports directory"
}

# Timestamp for report naming
$Timestamp = Get-Date -Format "yyyyMMdd_HHmmss"

try {
    # Step 1: Clean previous results (optional)
    Write-Header "Cleaning Previous Results"
    $ArtifactsPath = Join-Path $ProjectPath "BenchmarkDotNet.Artifacts"
    if (Test-Path $ArtifactsPath) {
        Write-Info "Backing up previous results..."
        $BackupPath = Join-Path $ReportsPath "Archive_$Timestamp"
        New-Item -ItemType Directory -Path $BackupPath -Force | Out-Null
        
        if (Test-Path $ResultsPath) {
            Copy-Item -Path "$ResultsPath\*" -Destination $BackupPath -Recurse -Force
            Write-Success "Previous results backed up to $BackupPath"
        }
        
        Remove-Item -Path $ArtifactsPath -Recurse -Force
        Write-Success "Cleaned artifacts directory"
    }

    # Step 2: Build the project
    if (!$SkipBuild) {
        Write-Header "Building Project"
        Write-Info "Configuration: $Configuration"
        
        # Use dotnet-win for Windows filesystem
        $dotnetCommand = "dotnet-win"
        if (!(Get-Command $dotnetCommand -ErrorAction SilentlyContinue)) {
            $dotnetCommand = "dotnet"
        }
        
        $buildArgs = @("build", $ProjectFile, "-c", $Configuration, "--verbosity", "minimal")
        Write-Info "Executing: $dotnetCommand $($buildArgs -join ' ')"
        
        & $dotnetCommand $buildArgs
        if ($LASTEXITCODE -ne 0) {
            throw "Build failed with exit code $LASTEXITCODE"
        }
        Write-Success "Build completed successfully"
    }

    # Step 3: Run benchmarks
    Write-Header "Running Benchmarks"
    
    if ($UseSimpleRunner) {
        # Use the simple console runner as fallback
        Write-Info "Using simple console runner..."
        $runnerArgs = @("run", "--project", $ProjectFile, "-c", $Configuration, "--", "--simple")
        
        if ($Filter -ne "*") {
            $runnerArgs += "--filter"
            $runnerArgs += $Filter
        }
        
        & $dotnetCommand $runnerArgs
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Simple runner failed with exit code $LASTEXITCODE"
        }
    }
    else {
        # Check if executable exists
        if (!(Test-Path $ExecutablePath)) {
            throw "Executable not found at: $ExecutablePath"
        }
        
        # Prepare benchmark arguments
        $benchmarkArgs = @()
        
        if ($Filter -ne "*") {
            $benchmarkArgs += "--filter"
            $benchmarkArgs += $Filter
        }
        
        if ($QuickRun) {
            $benchmarkArgs += "--runOncePerIteration"
        }
        
        # Add export options
        foreach ($format in $OutputFormat) {
            $benchmarkArgs += "--exporters"
            switch ($format.ToLower()) {
                "html" { $benchmarkArgs += "html" }
                "csv" { $benchmarkArgs += "csv" }
                "json" { $benchmarkArgs += "json" }
                "markdown" { $benchmarkArgs += "github" }
            }
        }
        
        Write-Info "Executing: $ExecutablePath $($benchmarkArgs -join ' ')"
        & $ExecutablePath $benchmarkArgs
        
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Benchmarks failed with exit code $LASTEXITCODE"
            Write-Info "Trying simple runner as fallback..."
            $UseSimpleRunner = $true
        }
    }
    
    # Step 4: Generate consolidated report
    Write-Header "Generating Consolidated Report"
    
    if (Test-Path $ResultsPath) {
        $reportFiles = Get-ChildItem -Path $ResultsPath -Filter "*-report.*" -File
        
        if ($reportFiles.Count -gt 0) {
            Write-Info "Found $($reportFiles.Count) report files"
            
            # Copy reports to Reports directory with timestamp
            foreach ($file in $reportFiles) {
                $newName = [System.IO.Path]::GetFileNameWithoutExtension($file.Name) + "_$Timestamp" + $file.Extension
                $destination = Join-Path $ReportsPath $newName
                Copy-Item -Path $file.FullName -Destination $destination
                Write-Success "Copied: $newName"
            }
            
            # Generate summary report
            $summaryPath = Join-Path $ReportsPath "Summary_$Timestamp.md"
            $summaryContent = @"
# Benchmark Summary Report
Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
Configuration: $Configuration
Filter: $Filter

## Results Location
- Reports Directory: $ReportsPath
- Timestamp: $Timestamp

## Generated Reports
"@
            
            foreach ($file in $reportFiles) {
                $summaryContent += "`n- $($file.Name)"
            }
            
            $summaryContent | Out-File -FilePath $summaryPath -Encoding UTF8
            Write-Success "Generated summary report: Summary_$Timestamp.md"
            
            # Open HTML report if available
            $htmlReport = $reportFiles | Where-Object { $_.Extension -eq ".html" } | Select-Object -First 1
            if ($htmlReport -and $IsWindows) {
                Write-Info "Opening HTML report in browser..."
                Start-Process $htmlReport.FullName
            }
        }
        else {
            Write-Error "No report files found in results directory"
        }
    }
    else {
        Write-Error "Results directory not found. Benchmarks may not have completed successfully."
    }
    
    # Step 5: Run custom report generator if available
    $reportGeneratorPath = Join-Path $OutputPath "ReportGenerator.dll"
    if (Test-Path $reportGeneratorPath) {
        Write-Header "Running Custom Report Generator"
        & $dotnetCommand $reportGeneratorPath $ReportsPath $Timestamp
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Custom report generation completed"
        }
    }
    
    Write-Header "Benchmark Run Complete"
    Write-Success "All reports saved to: $ReportsPath"
    
    # Display quick summary
    if (Test-Path $ResultsPath) {
        $csvFiles = Get-ChildItem -Path $ResultsPath -Filter "*.csv" -File
        if ($csvFiles.Count -gt 0) {
            Write-Header "Quick Performance Summary"
            foreach ($csvFile in $csvFiles) {
                Write-Info "Processing $($csvFile.Name)..."
                $csv = Import-Csv $csvFile.FullName | Select-Object -First 5
                $csv | Format-Table -AutoSize | Out-String | Write-Host
            }
        }
    }
}
catch {
    Write-Error "Benchmark run failed: $_"
    exit 1
}