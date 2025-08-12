# PowerShell script to run Lorentzian Classification benchmarks
# Usage: ./RunLorentzianBenchmarks.ps1 [-Category <category>] [-Format <format>] [-Quick]

param(
    [string]$Category = "",
    [string]$Format = "html,csv,md",
    [switch]$Quick = $false,
    [switch]$Memory = $false,
    [switch]$Profile = $false,
    [string]$OutputDir = "./BenchmarkDotNet.Artifacts"
)

Write-Host "=== Lorentzian Classification Benchmarks ===" -ForegroundColor Green
Write-Host "Starting comprehensive benchmark suite..." -ForegroundColor Yellow
Write-Host ""

# Base command
$baseCmd = "dotnet run -c Release --"

# Filter for Lorentzian Classification benchmarks
$filter = "--filter '*LorentzianClassification*'"

# Add category filter if specified
if ($Category -ne "") {
    $filter += " --categories '$Category'"
}

# Configure iterations based on mode
if ($Quick) {
    $iterations = "--warmupCount 1 --iterationCount 3 --launchCount 1"
    Write-Host "Running in QUICK mode (reduced iterations)" -ForegroundColor Yellow
} else {
    $iterations = "--warmupCount 3 --iterationCount 10 --launchCount 1"
    Write-Host "Running in FULL mode (complete iterations)" -ForegroundColor Yellow
}

# Configure exporters
$exporters = "--exporters $Format"

# Configure profiling
$profiling = ""
if ($Profile) {
    $profiling = "--profiler ETW"
    Write-Host "ETW profiling enabled" -ForegroundColor Cyan
}

# Configure memory diagnostics
$memory = ""
if ($Memory) {
    $memory = "--memory"
    Write-Host "Enhanced memory diagnostics enabled" -ForegroundColor Cyan
}

# Build the full command
$fullCmd = "$baseCmd $filter $iterations $exporters $profiling $memory"

Write-Host "Command: $fullCmd" -ForegroundColor Gray
Write-Host ""

# Create output directory if it doesn't exist
if (!(Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force
}

# Set environment variables for better performance
$env:DOTNET_TieredCompilation = "1"
$env:DOTNET_ReadyToRun = "1"

Write-Host "=== Benchmark Execution ===" -ForegroundColor Green

# Execute the benchmark
try {
    Invoke-Expression $fullCmd
    $exitCode = $LASTEXITCODE
    
    if ($exitCode -eq 0) {
        Write-Host ""
        Write-Host "=== Benchmark Completed Successfully ===" -ForegroundColor Green
        
        # Show generated files
        Write-Host ""
        Write-Host "Generated files:" -ForegroundColor Yellow
        Get-ChildItem -Path "./BenchmarkDotNet.Artifacts" -Recurse -File | ForEach-Object {
            Write-Host "  $($_.FullName)" -ForegroundColor Gray
        }
        
        # Quick summary of key files
        $htmlFiles = Get-ChildItem -Path "./BenchmarkDotNet.Artifacts" -Filter "*.html" -Recurse
        $csvFiles = Get-ChildItem -Path "./BenchmarkDotNet.Artifacts" -Filter "*.csv" -Recurse
        $mdFiles = Get-ChildItem -Path "./BenchmarkDotNet.Artifacts" -Filter "*.md" -Recurse
        
        Write-Host ""
        Write-Host "Quick access:" -ForegroundColor Yellow
        if ($htmlFiles.Count -gt 0) {
            Write-Host "  HTML Report: $($htmlFiles[0].FullName)" -ForegroundColor Cyan
        }
        if ($csvFiles.Count -gt 0) {
            Write-Host "  CSV Data: $($csvFiles[0].FullName)" -ForegroundColor Cyan
        }
        if ($mdFiles.Count -gt 0) {
            Write-Host "  Markdown: $($mdFiles[0].FullName)" -ForegroundColor Cyan
        }
        
    } else {
        Write-Host ""
        Write-Host "=== Benchmark Failed ===" -ForegroundColor Red
        Write-Host "Exit code: $exitCode" -ForegroundColor Red
    }
} catch {
    Write-Host ""
    Write-Host "=== Benchmark Error ===" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Provide category-specific suggestions
Write-Host ""
Write-Host "=== Available Benchmark Categories ===" -ForegroundColor Green
Write-Host "Run with -Category parameter to focus on specific areas:" -ForegroundColor Yellow
Write-Host ""
Write-Host "  Performance Categories:" -ForegroundColor White
Write-Host "    Initialization    - Startup and configuration performance" -ForegroundColor Gray
Write-Host "    SingleUpdate     - Real-time streaming latency" -ForegroundColor Gray
Write-Host "    BatchProcessing  - Backtesting throughput" -ForegroundColor Gray
Write-Host "    FeatureExtraction - Technical indicator overhead" -ForegroundColor Gray
Write-Host "    KNN              - k-NN search algorithm performance" -ForegroundColor Gray
Write-Host ""
Write-Host "  Resource Categories:" -ForegroundColor White
Write-Host "    Memory           - Memory allocation and usage" -ForegroundColor Gray
Write-Host ""
Write-Host "  Scenario Categories:" -ForegroundColor White
Write-Host "    MarketCondition  - Performance across market types" -ForegroundColor Gray
Write-Host "    Usage            - Streaming vs backtesting patterns" -ForegroundColor Gray
Write-Host ""
Write-Host "  Implementation Categories:" -ForegroundColor White
Write-Host "    Double           - Double precision arithmetic" -ForegroundColor Gray
Write-Host "    Float            - Single precision arithmetic" -ForegroundColor Gray
Write-Host "    Decimal          - Decimal precision arithmetic" -ForegroundColor Gray

Write-Host ""
Write-Host "Examples:" -ForegroundColor Yellow
Write-Host "  ./RunLorentzianBenchmarks.ps1 -Category 'SingleUpdate' -Quick" -ForegroundColor Gray
Write-Host "  ./RunLorentzianBenchmarks.ps1 -Category 'Memory' -Memory" -ForegroundColor Gray
Write-Host "  ./RunLorentzianBenchmarks.ps1 -Category 'Initialization' -Profile" -ForegroundColor Gray
Write-Host "  ./RunLorentzianBenchmarks.ps1 -Format 'json,csv'" -ForegroundColor Gray

Write-Host ""
Write-Host "For detailed documentation, see:" -ForegroundColor Cyan
Write-Host "  ./Indicators/README_LorentzianClassification.md" -ForegroundColor Gray