# Phase 02: Widget Wrappers & Integration

**Status**: 🔄 In Progress
**Goal**: Create widget wrapper components and integrate dashboard tabs into OneShotOptimize

## Deliverables

### Widget Wrappers (5 widgets)

| Widget | Wraps | Settings |
|--------|-------|----------|
| `StatisticsHistogramWidget.razor` | StatisticsHistogram | SelectedMetric, BucketSize |
| `EquityCurvesWidget.razor` | BacktestEquityChart | (none) |
| `ResultsFilterWidget.razor` | ResultsFilter | (none) |
| `BacktestDataGridWidget.razor` | MudDataGrid extraction | HiddenColumns, SortColumn |
| `ResultsSummaryWidget.razor` | ResultsSummary | (none) |

### Integration

1. Add "Dashboard 1" tab to `OneShotOptimize.razor` Results section
2. Add "Dashboard 2" tab to `OneShotOptimize.razor` Results section
3. Wire up ViewModel to dashboard components

## Success Criteria

- [ ] All 5 widget wrappers created and functional
- [ ] Widgets inherit from `OptimizationWidgetBase`
- [ ] Widget settings persist and restore correctly
- [ ] Dashboard tabs appear in Results section
- [ ] Both dashboards function independently
- [ ] Build succeeds without errors
