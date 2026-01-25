#!/bin/bash
# Migration script to rename backtest folders from old format to new format
# Old: 2024-01-01 - 2025-01-01 (hyphens in dates, space-hyphen-space separator)
# New: 2024.01.01-2025.01.01 (dots in dates, single hyphen separator)

set -e

BACKTEST_DIR="${1:-/mnt/g/Trading/Backtesting}"

if [ ! -d "$BACKTEST_DIR" ]; then
    echo "Error: Directory not found: $BACKTEST_DIR"
    echo "Usage: $0 [backtest_directory]"
    exit 1
fi

echo "Scanning for folders to migrate in: $BACKTEST_DIR"
echo ""

# Find all date range folders (4 levels deep: Bot/Symbol/TimeFrame/DateRange)
# Pattern: yyyy-MM-dd - yyyy-MM-dd (old format with spaces)
count=0
renamed=0

while IFS= read -r -d '' dir; do
    folder_name=$(basename "$dir")

    # Check if it matches the old pattern (has " - " separator)
    if [[ "$folder_name" =~ ^([0-9]{4})-([0-9]{2})-([0-9]{2})\ -\ ([0-9]{4})-([0-9]{2})-([0-9]{2})$ ]]; then
        # Extract date parts
        start_year="${BASH_REMATCH[1]}"
        start_month="${BASH_REMATCH[2]}"
        start_day="${BASH_REMATCH[3]}"
        end_year="${BASH_REMATCH[4]}"
        end_month="${BASH_REMATCH[5]}"
        end_day="${BASH_REMATCH[6]}"

        # Build new folder name: yyyy.MM.dd-yyyy.MM.dd
        new_name="${start_year}.${start_month}.${start_day}-${end_year}.${end_month}.${end_day}"
        parent_dir=$(dirname "$dir")
        new_path="${parent_dir}/${new_name}"

        count=$((count + 1))

        if [ -d "$new_path" ]; then
            echo "SKIP (target exists): $folder_name -> $new_name"
        else
            echo "RENAME: $folder_name -> $new_name"
            mv "$dir" "$new_path"
            renamed=$((renamed + 1))
        fi
    fi
done < <(find "$BACKTEST_DIR" -mindepth 4 -maxdepth 4 -type d -print0 2>/dev/null)

echo ""
echo "Done. Found $count old-format folders, renamed $renamed."
