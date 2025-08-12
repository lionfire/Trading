#!/bin/bash

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Functions for colored output
write_header() {
    echo -e "\n${CYAN}================================${NC}"
    echo -e "${CYAN}$1${NC}"
    echo -e "${CYAN}================================${NC}"
}

write_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

write_info() {
    echo -e "${YELLOW}→ $1${NC}"
}

write_error() {
    echo -e "${RED}✗ $1${NC}"
}

# Default values
CONFIGURATION="Release"
FILTER="*"
OUTPUT_FORMATS="Html,Csv,Json,Markdown"
SKIP_BUILD=false
QUICK_RUN=false
USE_SIMPLE_RUNNER=false

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -c|--configuration)
            CONFIGURATION="$2"
            shift 2
            ;;
        -f|--filter)
            FILTER="$2"
            shift 2
            ;;
        -o|--output)
            OUTPUT_FORMATS="$2"
            shift 2
            ;;
        --skip-build)
            SKIP_BUILD=true
            shift
            ;;
        --quick)
            QUICK_RUN=true
            shift
            ;;
        --simple)
            USE_SIMPLE_RUNNER=true
            shift
            ;;
        -h|--help)
            echo "Usage: $0 [options]"
            echo "Options:"
            echo "  -c, --configuration <config>  Build configuration (Debug/Release). Default: Release"
            echo "  -f, --filter <filter>         Filter for specific benchmarks. Default: *"
            echo "  -o, --output <formats>        Output formats (Html,Csv,Json,Markdown). Default: Html,Csv,Json,Markdown"
            echo "  --skip-build                  Skip building the project"
            echo "  --quick                       Run quick benchmarks (single iteration)"
            echo "  --simple                      Use simple console runner"
            echo "  -h, --help                    Show this help message"
            exit 0
            ;;
        *)
            echo "Unknown option: $1"
            echo "Use -h or --help for usage information"
            exit 1
            ;;
    esac
done

# Setup paths
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT_PATH="$SCRIPT_DIR"
PROJECT_FILE="${PROJECT_PATH}/LionFire.Trading.Indicators.Benchmarks.csproj"
OUTPUT_PATH="${PROJECT_PATH}/bin/${CONFIGURATION}/net9.0"
EXECUTABLE_NAME="LionFire.Trading.Indicators.Benchmarks"

# Check if running on Windows WSL
if [[ -f /proc/sys/fs/binfmt_misc/WSLInterop ]]; then
    DOTNET_CMD="dotnet-win"
    write_info "Detected WSL environment, using dotnet-win"
else
    DOTNET_CMD="dotnet"
fi

# Check if dotnet command exists
if ! command -v $DOTNET_CMD &> /dev/null; then
    write_error "$DOTNET_CMD command not found"
    if [[ "$DOTNET_CMD" == "dotnet-win" ]]; then
        write_info "Falling back to dotnet"
        DOTNET_CMD="dotnet"
    else
        exit 1
    fi
fi

EXECUTABLE_PATH="${OUTPUT_PATH}/${EXECUTABLE_NAME}"
REPORTS_PATH="${PROJECT_PATH}/Reports"
RESULTS_PATH="${PROJECT_PATH}/BenchmarkDotNet.Artifacts/results"

# Create Reports directory if it doesn't exist
if [ ! -d "$REPORTS_PATH" ]; then
    mkdir -p "$REPORTS_PATH"
    write_success "Created Reports directory"
fi

# Timestamp for report naming
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")

# Error handling
set -e
trap 'write_error "Script failed at line $LINENO"' ERR

# Step 1: Clean previous results
write_header "Cleaning Previous Results"
ARTIFACTS_PATH="${PROJECT_PATH}/BenchmarkDotNet.Artifacts"
if [ -d "$ARTIFACTS_PATH" ]; then
    write_info "Backing up previous results..."
    BACKUP_PATH="${REPORTS_PATH}/Archive_${TIMESTAMP}"
    mkdir -p "$BACKUP_PATH"
    
    if [ -d "$RESULTS_PATH" ]; then
        cp -r "${RESULTS_PATH}"/* "$BACKUP_PATH" 2>/dev/null || true
        write_success "Previous results backed up to $BACKUP_PATH"
    fi
    
    rm -rf "$ARTIFACTS_PATH"
    write_success "Cleaned artifacts directory"
fi

# Step 2: Build the project
if [ "$SKIP_BUILD" = false ]; then
    write_header "Building Project"
    write_info "Configuration: $CONFIGURATION"
    
    # Convert path format if using dotnet-win
    BUILD_SUCCESS=0
    if [[ "$DOTNET_CMD" == "dotnet-win" ]]; then
        WIN_PROJECT_FILE=$(wslpath -w "$PROJECT_FILE")
        write_info "Executing: $DOTNET_CMD build \"$WIN_PROJECT_FILE\" -c $CONFIGURATION"
        $DOTNET_CMD build "$WIN_PROJECT_FILE" -c "$CONFIGURATION"
        BUILD_SUCCESS=$?
    else
        write_info "Executing: $DOTNET_CMD build \"$PROJECT_FILE\" -c $CONFIGURATION"
        $DOTNET_CMD build "$PROJECT_FILE" -c "$CONFIGURATION"
        BUILD_SUCCESS=$?
    fi
    
    if [ $BUILD_SUCCESS -eq 0 ]; then
        write_success "Build completed successfully"
    else
        write_error "Main project build failed"
        write_info "Falling back to SimpleBenchmark project..."
        
        # Try to run SimpleBenchmark as fallback
        SIMPLE_PROJECT_PATH="${SCRIPT_DIR}/SimpleBenchmark"
        if [ -d "$SIMPLE_PROJECT_PATH" ]; then
            cd "$SIMPLE_PROJECT_PATH"
            if [[ "$DOTNET_CMD" == "dotnet-win" ]]; then
                WIN_SIMPLE_PATH=$(wslpath -w "$SIMPLE_PROJECT_PATH")
                $DOTNET_CMD run -c Release -- --standalone
            else
                $DOTNET_CMD run -c Release -- --standalone
            fi
            if [ $? -eq 0 ]; then
                write_success "SimpleBenchmark completed successfully"
                exit 0
            else
                write_error "SimpleBenchmark also failed"
                exit 1
            fi
        else
            write_error "SimpleBenchmark directory not found"
            exit 1
        fi
    fi
fi

# Step 3: Run benchmarks
write_header "Running Benchmarks"

if [ "$USE_SIMPLE_RUNNER" = true ]; then
    # Use the simple console runner as fallback
    write_info "Using simple console runner..."
    if [[ "$DOTNET_CMD" == "dotnet-win" ]]; then
        WIN_PROJECT_FILE=$(wslpath -w "$PROJECT_FILE")
        RUN_ARGS="run --project \"$WIN_PROJECT_FILE\" -c $CONFIGURATION -- --simple"
    else
        RUN_ARGS="run --project \"$PROJECT_FILE\" -c $CONFIGURATION -- --simple"
    fi
    
    if [ "$FILTER" != "*" ]; then
        RUN_ARGS="$RUN_ARGS --filter \"$FILTER\""
    fi
    
    eval $DOTNET_CMD $RUN_ARGS
    if [ $? -ne 0 ]; then
        write_error "Simple runner failed"
    fi
else
    # Check if executable exists
    if [ ! -f "$EXECUTABLE_PATH" ]; then
        write_error "Executable not found at: $EXECUTABLE_PATH"
        exit 1
    fi
    
    # Prepare benchmark arguments
    BENCHMARK_ARGS=""
    
    if [ "$FILTER" != "*" ]; then
        BENCHMARK_ARGS="$BENCHMARK_ARGS --filter \"$FILTER\""
    fi
    
    if [ "$QUICK_RUN" = true ]; then
        BENCHMARK_ARGS="$BENCHMARK_ARGS --runOncePerIteration"
    fi
    
    # Add export options
    IFS=',' read -ra FORMATS <<< "$OUTPUT_FORMATS"
    for format in "${FORMATS[@]}"; do
        format_lower=$(echo "$format" | tr '[:upper:]' '[:lower:]' | tr -d ' ')
        case $format_lower in
            html)
                BENCHMARK_ARGS="$BENCHMARK_ARGS --exporters html"
                ;;
            csv)
                BENCHMARK_ARGS="$BENCHMARK_ARGS --exporters csv"
                ;;
            json)
                BENCHMARK_ARGS="$BENCHMARK_ARGS --exporters json"
                ;;
            markdown)
                BENCHMARK_ARGS="$BENCHMARK_ARGS --exporters github"
                ;;
        esac
    done
    
    write_info "Executing: $EXECUTABLE_PATH $BENCHMARK_ARGS"
    eval "$EXECUTABLE_PATH" $BENCHMARK_ARGS
    
    if [ $? -ne 0 ]; then
        write_error "Benchmarks failed"
        write_info "Trying simple runner as fallback..."
        USE_SIMPLE_RUNNER=true
    fi
fi

# Step 4: Generate consolidated report
write_header "Generating Consolidated Report"

if [ -d "$RESULTS_PATH" ]; then
    REPORT_FILES=$(find "$RESULTS_PATH" -name "*-report.*" -type f 2>/dev/null)
    
    if [ -n "$REPORT_FILES" ]; then
        FILE_COUNT=$(echo "$REPORT_FILES" | wc -l)
        write_info "Found $FILE_COUNT report files"
        
        # Copy reports to Reports directory with timestamp
        for file in $REPORT_FILES; do
            filename=$(basename "$file")
            extension="${filename##*.}"
            basename="${filename%.*}"
            newname="${basename}_${TIMESTAMP}.${extension}"
            destination="${REPORTS_PATH}/${newname}"
            cp "$file" "$destination"
            write_success "Copied: $newname"
        done
        
        # Generate summary report
        SUMMARY_PATH="${REPORTS_PATH}/Summary_${TIMESTAMP}.md"
        cat > "$SUMMARY_PATH" << EOF
# Benchmark Summary Report
Generated: $(date "+%Y-%m-%d %H:%M:%S")
Configuration: $CONFIGURATION
Filter: $FILTER

## Results Location
- Reports Directory: $REPORTS_PATH
- Timestamp: $TIMESTAMP

## Generated Reports
EOF
        
        for file in $REPORT_FILES; do
            echo "- $(basename "$file")" >> "$SUMMARY_PATH"
        done
        
        write_success "Generated summary report: Summary_${TIMESTAMP}.md"
        
        # Open HTML report if available (on systems with xdg-open)
        HTML_REPORT=$(find "$RESULTS_PATH" -name "*.html" -type f | head -n 1)
        if [ -n "$HTML_REPORT" ] && command -v xdg-open &> /dev/null; then
            write_info "Opening HTML report in browser..."
            xdg-open "$HTML_REPORT" &
        fi
    else
        write_error "No report files found in results directory"
    fi
else
    write_error "Results directory not found. Benchmarks may not have completed successfully."
fi

# Step 5: Run custom report generator if available
REPORT_GENERATOR_PATH="${OUTPUT_PATH}/ReportGenerator.dll"
if [ -f "$REPORT_GENERATOR_PATH" ]; then
    write_header "Running Custom Report Generator"
    $DOTNET_CMD "$REPORT_GENERATOR_PATH" "$REPORTS_PATH" "$TIMESTAMP"
    if [ $? -eq 0 ]; then
        write_success "Custom report generation completed"
    fi
fi

write_header "Benchmark Run Complete"
write_success "All reports saved to: $REPORTS_PATH"

# Display quick summary
if [ -d "$RESULTS_PATH" ]; then
    CSV_FILES=$(find "$RESULTS_PATH" -name "*.csv" -type f 2>/dev/null)
    if [ -n "$CSV_FILES" ]; then
        write_header "Quick Performance Summary"
        for csv_file in $CSV_FILES; do
            write_info "Processing $(basename "$csv_file")..."
            head -n 6 "$csv_file" | column -t -s ','
            echo ""
        done
    fi
fi

exit 0