# Epic 02-002: Dashboard Tab Integration

**Phase**: 02 - Widget Wrappers & Integration
**Status**: ✅ Complete
**Priority**: High
**Depends On**: epic-02-001

## Overview

Add Dashboard 1 and Dashboard 2 tabs to the Results section of OneShotOptimize.razor, wiring up the OptimizationDashboard component with the ViewModel.

## Tasks

### Task 1: Add Dashboard Tabs
- [x] Open `OneShotOptimize.razor`
- [x] Locate Results MudTabs section
- [x] Add "Dashboard 1" MudTabPanel after existing tabs
- [x] Add "Dashboard 2" MudTabPanel after Dashboard 1
- [x] Use Dashboard icon: `Icons.Material.Filled.Dashboard`

### Task 2: Wire Up Dashboard Component
- [x] Add `<OptimizationDashboard>` inside each tab panel
- [x] Pass `ViewModel="ViewModel"` parameter
- [x] Pass `DashboardName="dashboard1"` / `"dashboard2"` parameter

### Task 3: Add Using Directive
- [x] Add `@using LionFire.Trading.Automation.Blazor.Optimization.Dashboard`

## Implementation

```razor
@* Add after existing tabs like "Statistics" *@
<MudTabPanel Text="Dashboard 1" Icon="@Icons.Material.Filled.Dashboard">
    <OptimizationDashboard ViewModel="ViewModel" DashboardName="dashboard1" />
</MudTabPanel>
<MudTabPanel Text="Dashboard 2" Icon="@Icons.Material.Filled.Dashboard">
    <OptimizationDashboard ViewModel="ViewModel" DashboardName="dashboard2" />
</MudTabPanel>
```

## Acceptance Criteria

- [ ] Dashboard 1 tab visible in Results section
- [ ] Dashboard 2 tab visible in Results section
- [ ] Dashboard loads with default widgets
- [ ] Can add/remove/resize widgets
- [ ] Layout saves automatically
- [ ] Each dashboard has independent layout

## Technical Notes

- File: `/src/LionFire.Trading.Automation.Blazor/Optimization/OneShotOptimize.razor`
- Layout keys: `optimization:dashboard1`, `optimization:dashboard2`
