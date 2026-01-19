# Epic 02-003: Build and Verification

**Phase**: 02 - Widget Wrappers & Integration
**Status**: ✅ Complete (Build verified)
**Priority**: High
**Depends On**: epic-02-002

## Overview

Build the solution, fix any compilation errors, and verify the dashboard functionality works as expected.

## Tasks

### Task 1: Build Solution
- [x] Run: `dotnet-win build /mnt/c/src/Internal/LionFire.All.Trading.slnf`
- [x] Fix any compilation errors
- [ ] Ensure no warnings in new code (existing warnings in project, new code clean)

### Task 2: Runtime Verification
- [ ] Start the application
- [ ] Navigate to `/optimize/one-shot`
- [ ] Run an optimization to populate results
- [ ] Switch to Results section

### Task 3: Dashboard 1 Verification
- [ ] Verify Dashboard 1 tab is visible
- [ ] Verify default layout loads (Equity Curves, Filter, Data Grid)
- [ ] Verify widgets display optimization data
- [ ] Verify can drag/resize widgets
- [ ] Verify Add Widget button works
- [ ] Verify layout persists on page refresh

### Task 4: Dashboard 2 Verification
- [ ] Verify Dashboard 2 tab is visible
- [ ] Verify different default layout (2 Histograms, Summary, Filter, Data Grid)
- [ ] Verify Histogram settings (AD vs Fitness) persist
- [ ] Verify independent layout from Dashboard 1

### Task 5: Filter Integration
- [ ] Apply a filter in one widget
- [ ] Verify other widgets update accordingly
- [ ] Verify filter state shared across all widgets

## Acceptance Criteria

- [ ] Build succeeds with no errors
- [ ] All success criteria from change.md are met
- [ ] No console errors during operation
- [ ] Layouts persist correctly

## Success Criteria Checklist (from change.md)

- [ ] Dashboard tabs appear in Results section
- [ ] Default layouts load with widgets
- [ ] Widgets display data from optimization
- [ ] Filter widget affects other widgets
- [ ] Can drag/resize widgets
- [ ] Add Widget button shows picker dialog
- [ ] Layout persists on page refresh
- [ ] Dashboard 2 has separate layout from Dashboard 1
