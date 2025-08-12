#!/bin/bash

# Check if bunx and cloc are available
if ! command -v bunx &> /dev/null; then
    echo "Error: bunx is not installed or not found in PATH."
    exit 1
fi

# Check if inside a git repository
if ! git rev-parse --is-inside-work-tree &> /dev/null; then
    echo "Error: This script must be run inside a Git repository."
    exit 1
fi

# Temporary file to store untracked files/directories
TEMP_FILE=$(mktemp)

# Get untracked files and directories from git status
# Use porcelain to get machine-readable output, filter untracked (??)
git status --porcelain | grep '^??' | cut -d ' ' -f 2- | tr -d '"' > "$TEMP_FILE"

# Check if there are any untracked files
if [ ! -s "$TEMP_FILE" ]; then
    echo "No untracked files or directories found."
    rm "$TEMP_FILE"
    exit 0
fi

# Run cloc on the untracked files/directories
echo "Counting lines of code for untracked files and directories..."
bunx cloc --quiet $(cat "$TEMP_FILE")

# Clean up
rm "$TEMP_FILE"

exit 0


