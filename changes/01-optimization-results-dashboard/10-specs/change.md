# Change 01: Optimization Results Dashboard with BlazorGridStack

**Type**: feature
**Created**: 2026-01-18
**Status**: Active
**Related Spec**: None (from plan/optimization-dashboard.md)

## Description

Add customizable dashboard tabs to the Results section using BlazorGridStack, allowing users to create widget-based layouts with optimization result visualizations. Initially add two dashboard tabs ("Dashboard 1" and "Dashboard 2") with the ability to add/arrange widgets.

## Scope

- [x] Add `Settings` property to `WidgetLayoutItem.cs` for per-widget configuration
- [x] Create Dashboard core infrastructure (base class, catalog, instance)
- [x] Create `OptimizationDashboard.razor` with BlazorGridStack
- [ ] Create widget wrappers for existing components
- [ ] Add Dashboard tabs to `OneShotOptimize.razor`
- [ ] Build and verify implementation

## Available Widgets

1. **Statistics Histogram** - Distribution chart with metric/bucket size settings
2. **Equity Curves** - Backtest equity curve chart
3. **Results Filter** - Filter controls for optimization results
4. **Backtest Data Grid** - Sortable/filterable results table
5. **Results Summary** - Summary statistics panel

## Success Criteria

- [ ] Dashboard tabs appear in Results section
- [ ] Default layouts load with widgets
- [ ] Widgets display data from optimization
- [ ] Filter widget affects other widgets
- [ ] Can drag/resize widgets
- [ ] Add Widget button shows picker dialog
- [ ] Layout persists on page refresh
- [ ] Dashboard 2 has separate layout from Dashboard 1

## Files Already Modified (Before Change Creation)

### Modified Files:
1. `/mnt/c/src/Trading.Proprietary/src/LionFire.Trading.Workbench.Abstractions/ViewModes/WidgetLayoutItem.cs`
   - Added `Settings` property with `[Id(13)]` attribute

### Created Files:
1. `/mnt/c/src/Trading/src/LionFire.Trading.Automation.Blazor/Optimization/Dashboard/OptimizationWidgetBase.cs`
2. `/mnt/c/src/Trading/src/LionFire.Trading.Automation.Blazor/Optimization/Dashboard/OptimizationWidgetCatalog.cs`
3. `/mnt/c/src/Trading/src/LionFire.Trading.Automation.Blazor/Optimization/Dashboard/OptimizationWidgetInstance.cs`
4. `/mnt/c/src/Trading/src/LionFire.Trading.Automation.Blazor/Optimization/Dashboard/OptimizationDashboard.razor`
5. `/mnt/c/src/Trading/src/LionFire.Trading.Automation.Blazor/Optimization/Dashboard/OptimizationDashboard.razor.cs`

### Directories Created:
1. `/mnt/c/src/Trading/src/LionFire.Trading.Automation.Blazor/Optimization/Dashboard/`
2. `/mnt/c/src/Trading/src/LionFire.Trading.Automation.Blazor/Optimization/Dashboard/Widgets/`

## Remaining Work

### Phase 3: Widget Wrappers (In Progress)
- [ ] `StatisticsHistogramWidget.razor` - Wraps StatisticsHistogram with settings
- [ ] `EquityCurvesWidget.razor` - Wraps BacktestEquityChart
- [ ] `ResultsFilterWidget.razor` - Wraps ResultsFilter
- [ ] `BacktestDataGridWidget.razor` - Extracts data grid from BacktestResultsPanel
- [ ] `ResultsSummaryWidget.razor` - Wraps ResultsSummary

### Phase 5: Integration
- [ ] Add Dashboard 1 and Dashboard 2 tabs to `OneShotOptimize.razor`

### Verification
- [ ] Build: `dotnet-win build /mnt/c/src/Internal/LionFire.All.Trading.slnf`
- [ ] Test dashboard functionality

## Notes

Implementation started before change structure was created. Files listed above represent work already completed as part of Phase 1, 2, and partial Phase 4.
