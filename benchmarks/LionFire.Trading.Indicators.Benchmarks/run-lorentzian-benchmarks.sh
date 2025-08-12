#!/bin/bash

# Bash script to run Lorentzian Classification benchmarks
# Usage: ./run-lorentzian-benchmarks.sh [options]

# Default values
CATEGORY=""
FORMAT="html,csv,md"
QUICK=false
MEMORY=false
PROFILE=false
OUTPUT_DIR="./BenchmarkDotNet.Artifacts"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
WHITE='\033[1;37m'
GRAY='\033[0;37m'
NC='\033[0m' # No Color

# Function to print colored output
print_color() {
    local color=$1
    local message=$2
    echo -e "${color}${message}${NC}"
}

# Function to show usage
show_usage() {
    echo "Usage: $0 [options]"
    echo ""
    echo "Options:"
    echo "  -c, --category CATEGORY    Filter by benchmark category"
    echo "  -f, --format FORMAT        Output formats (default: html,csv,md)"
    echo "  -q, --quick               Quick mode with reduced iterations"
    echo "  -m, --memory              Enable enhanced memory diagnostics"
    echo "  -p, --profile             Enable ETW profiling"
    echo "  -o, --output DIR          Output directory"
    echo "  -h, --help                Show this help message"
    echo ""
    echo "Categories:"
    echo "  Initialization, SingleUpdate, BatchProcessing, FeatureExtraction"
    echo "  KNN, Memory, MarketCondition, Usage, Double, Float, Decimal"
    echo ""
    echo "Examples:"
    echo "  $0 --category SingleUpdate --quick"
    echo "  $0 --category Memory --memory"
    echo "  $0 --profile --format json,csv"
}

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -c|--category)
            CATEGORY="$2"
            shift 2
            ;;
        -f|--format)
            FORMAT="$2"
            shift 2
            ;;
        -q|--quick)
            QUICK=true
            shift
            ;;
        -m|--memory)
            MEMORY=true
            shift
            ;;
        -p|--profile)
            PROFILE=true
            shift
            ;;
        -o|--output)
            OUTPUT_DIR="$2"
            shift 2
            ;;
        -h|--help)
            show_usage
            exit 0
            ;;
        *)
            echo "Unknown option: $1"
            show_usage
            exit 1
            ;;
    esac
done

print_color $GREEN "=== Lorentzian Classification Benchmarks ==="
print_color $YELLOW "Starting comprehensive benchmark suite..."
echo ""

# Base command
BASE_CMD="dotnet run -c Release --"

# Filter for Lorentzian Classification benchmarks
FILTER="--filter '*LorentzianClassification*'"

# Add category filter if specified
if [[ -n "$CATEGORY" ]]; then
    FILTER="$FILTER --categories '$CATEGORY'"
fi

# Configure iterations based on mode
if [[ "$QUICK" == true ]]; then
    ITERATIONS="--warmupCount 1 --iterationCount 3 --launchCount 1"
    print_color $YELLOW "Running in QUICK mode (reduced iterations)"
else
    ITERATIONS="--warmupCount 3 --iterationCount 10 --launchCount 1"
    print_color $YELLOW "Running in FULL mode (complete iterations)"
fi

# Configure exporters
EXPORTERS="--exporters $FORMAT"

# Configure profiling
PROFILING=""
if [[ "$PROFILE" == true ]]; then
    PROFILING="--profiler ETW"
    print_color $CYAN "ETW profiling enabled"
fi

# Configure memory diagnostics
MEMORY_FLAG=""
if [[ "$MEMORY" == true ]]; then
    MEMORY_FLAG="--memory"
    print_color $CYAN "Enhanced memory diagnostics enabled"
fi

# Build the full command
FULL_CMD="$BASE_CMD $FILTER $ITERATIONS $EXPORTERS $PROFILING $MEMORY_FLAG"

print_color $GRAY "Command: $FULL_CMD"
echo ""

# Create output directory if it doesn't exist
mkdir -p "$OUTPUT_DIR"

# Set environment variables for better performance
export DOTNET_TieredCompilation=1
export DOTNET_ReadyToRun=1

print_color $GREEN "=== Benchmark Execution ==="

# Execute the benchmark
if eval $FULL_CMD; then
    EXIT_CODE=$?
    
    if [[ $EXIT_CODE -eq 0 ]]; then
        echo ""
        print_color $GREEN "=== Benchmark Completed Successfully ==="
        
        # Show generated files
        echo ""
        print_color $YELLOW "Generated files:"
        find "./BenchmarkDotNet.Artifacts" -type f 2>/dev/null | while read file; do
            print_color $GRAY "  $file"
        done
        
        # Quick summary of key files
        echo ""
        print_color $YELLOW "Quick access:"
        
        HTML_FILE=$(find "./BenchmarkDotNet.Artifacts" -name "*.html" -type f 2>/dev/null | head -1)
        CSV_FILE=$(find "./BenchmarkDotNet.Artifacts" -name "*.csv" -type f 2>/dev/null | head -1)
        MD_FILE=$(find "./BenchmarkDotNet.Artifacts" -name "*.md" -type f 2>/dev/null | head -1)
        
        [[ -n "$HTML_FILE" ]] && print_color $CYAN "  HTML Report: $HTML_FILE"
        [[ -n "$CSV_FILE" ]] && print_color $CYAN "  CSV Data: $CSV_FILE"
        [[ -n "$MD_FILE" ]] && print_color $CYAN "  Markdown: $MD_FILE"
        
    else
        echo ""
        print_color $RED "=== Benchmark Failed ==="
        print_color $RED "Exit code: $EXIT_CODE"
    fi
else
    echo ""
    print_color $RED "=== Benchmark Error ==="
    print_color $RED "Failed to execute benchmark command"
fi

# Provide category-specific suggestions
echo ""
print_color $GREEN "=== Available Benchmark Categories ==="
print_color $YELLOW "Run with --category parameter to focus on specific areas:"
echo ""
print_color $WHITE "  Performance Categories:"
print_color $GRAY "    Initialization    - Startup and configuration performance"
print_color $GRAY "    SingleUpdate     - Real-time streaming latency"
print_color $GRAY "    BatchProcessing  - Backtesting throughput"
print_color $GRAY "    FeatureExtraction - Technical indicator overhead"
print_color $GRAY "    KNN              - k-NN search algorithm performance"
echo ""
print_color $WHITE "  Resource Categories:"
print_color $GRAY "    Memory           - Memory allocation and usage"
echo ""
print_color $WHITE "  Scenario Categories:"
print_color $GRAY "    MarketCondition  - Performance across market types"
print_color $GRAY "    Usage            - Streaming vs backtesting patterns"
echo ""
print_color $WHITE "  Implementation Categories:"
print_color $GRAY "    Double           - Double precision arithmetic"
print_color $GRAY "    Float            - Single precision arithmetic"
print_color $GRAY "    Decimal          - Decimal precision arithmetic"

echo ""
print_color $YELLOW "Examples:"
print_color $GRAY "  $0 --category SingleUpdate --quick"
print_color $GRAY "  $0 --category Memory --memory"
print_color $GRAY "  $0 --category Initialization --profile"
print_color $GRAY "  $0 --format json,csv"

echo ""
print_color $CYAN "For detailed documentation, see:"
print_color $GRAY "  ./Indicators/README_LorentzianClassification.md"