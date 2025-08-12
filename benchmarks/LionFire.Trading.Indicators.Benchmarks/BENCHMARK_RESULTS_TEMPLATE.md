# Lorentzian Classification Benchmark Results

**Date:** [YYYY-MM-DD]  
**Version:** [Indicator Version]  
**Environment:** [OS, CPU, Memory]  
**.NET Version:** [Runtime Version]  
**Test Duration:** [HH:MM:SS]

## Executive Summary

### Key Performance Metrics

| Metric | Target | Achieved | Status | Delta |
|--------|--------|----------|---------|-------|
| Single Update Latency | < 500μs | [XXX]μs | ✅/❌ | [+/-]XX% |
| Batch Processing Rate | > 200k bars/sec | [XXX]k bars/sec | ✅/❌ | [+/-]XX% |
| Memory per Pattern | < 500B | [XXX]B | ✅/❌ | [+/-]XX% |
| Initialization Time | < 5ms | [XXX]ms | ✅/❌ | [+/-]XX% |
| Feature Extraction | < 50μs | [XXX]μs | ✅/❌ | [+/-]XX% |

### Overall Assessment

**Performance Rating:** [Excellent/Good/Acceptable/Needs Improvement]

**Key Findings:**
- [Major finding 1]
- [Major finding 2]
- [Major finding 3]

**Recommendations:**
- [Recommendation 1]
- [Recommendation 2]

## Detailed Results

### 1. Initialization Performance

#### K-Value Scaling
```
K=4:   [X.XX]ms ± [X.XX]ms
K=8:   [X.XX]ms ± [X.XX]ms  (baseline)
K=16:  [X.XX]ms ± [X.XX]ms  ([+X.X]x slower)
K=32:  [X.XX]ms ± [X.XX]ms  ([+X.X]x slower)
```

#### Lookback Period Impact
```
Lookback=50:   [X.XX]ms
Lookback=100:  [X.XX]ms  (baseline)
Lookback=500:  [X.XX]ms  ([+X.X]x slower)
```

**Analysis:**
- [Key observations about initialization performance]
- [Scaling behavior analysis]
- [Memory allocation patterns during initialization]

### 2. Single Update Performance (Real-Time Trading)

#### Latency Distribution
```
Mean:     [XXX]μs
Median:   [XXX]μs
P95:      [XXX]μs
P99:      [XXX]μs
P99.9:    [XXX]μs
Max:      [XXX]μs
StdDev:   [XXX]μs
```

#### Performance by Configuration
| K | Lookback | Mean Latency | P95 Latency | Memory/Update |
|---|----------|--------------|-------------|---------------|
| 4 | 50 | [XXX]μs | [XXX]μs | [XXX]B |
| 8 | 100 | [XXX]μs | [XXX]μs | [XXX]B |
| 16 | 100 | [XXX]μs | [XXX]μs | [XXX]B |
| 32 | 500 | [XXX]μs | [XXX]μs | [XXX]B |

**Real-Time Trading Assessment:**
- ✅/❌ Meets sub-millisecond requirement
- ✅/❌ Consistent latency (low jitter)
- ✅/❌ Scalable with parameter increases

### 3. Batch Processing Performance (Backtesting)

#### Throughput Results
```
Small Dataset (1k bars):   [XXX] bars/sec
Medium Dataset (10k bars): [XXX] bars/sec
Large Dataset (100k bars): [XXX] bars/sec
```

#### Memory Efficiency
```
Peak Memory Usage: [XXX] MB
Memory Growth Rate: [XXX] MB per 10k bars
GC Pressure: [Low/Medium/High]
```

**Backtesting Assessment:**
- Throughput suitable for historical analysis: ✅/❌
- Memory usage bounded (not linear): ✅/❌
- GC pressure acceptable: ✅/❌

### 4. Feature Extraction Performance

#### Individual Feature Costs
```
RSI Calculation:      [XX]μs ([XX]% of total)
CCI Calculation:      [XX]μs ([XX]% of total)  
ADX Approximation:    [XX]μs ([XX]% of total)
Price Returns:        [XX]μs ([XX]% of total)
Volatility:           [XX]μs ([XX]% of total)
Momentum:             [XX]μs ([XX]% of total)
Normalization:        [XX]μs ([XX]% of total)
```

**Feature Analysis:**
- Most expensive feature: [Feature Name] ([XX]μs)
- Optimization opportunities: [Details]
- Normalization overhead: [Acceptable/High/Critical]

### 5. k-NN Search Performance

#### Search Time by K Value
```
K=4:  [XX]μs  (baseline)
K=8:  [XX]μs  ([+X.X]x)
K=16: [XX]μs  ([+X.X]x)
K=32: [XX]μs  ([+X.X]x)
```

#### Distance Calculation Efficiency
```
Lorentzian Distance Calculation: [XX]ns per pair
Feature Vector Comparison: [XX]ns per feature
Sorting K Neighbors: [XX]ns
```

**k-NN Analysis:**
- Search scales as expected: ✅/❌
- Distance calculation efficiency: [Excellent/Good/Needs Improvement]
- Memory access patterns: [Optimal/Acceptable/Cache Unfriendly]

### 6. Memory Usage Analysis

#### Allocation Patterns
```
Static Allocation (initialization): [XXX] KB
Dynamic Allocation (per update): [XXX] B
Peak Working Set: [XXX] MB
```

#### Garbage Collection Impact
```
Gen0 Collections: [XXX] per 10k updates
Gen1 Collections: [XXX] per 10k updates  
Gen2 Collections: [XXX] per 10k updates
Total GC Time: [XXX]ms per 10k updates
```

**Memory Assessment:**
- Memory leaks detected: ❌/⚠️
- GC pressure: [Low/Medium/High]
- Long-running stability: ✅/❌

### 7. Numeric Type Comparison

#### Performance by Type
| Type | Initialization | Single Update | Batch Processing | Memory Usage |
|------|----------------|---------------|------------------|--------------|
| double | [X.XX]ms | [XXX]μs | [XXX] bars/sec | [XXX]KB |
| float | [X.XX]ms | [XXX]μs | [XXX] bars/sec | [XXX]KB |
| decimal | [X.XX]ms | [XXX]μs | [XXX] bars/sec | [XXX]KB |

**Type Recommendations:**
- **Real-time trading:** [Recommended type and rationale]
- **Backtesting:** [Recommended type and rationale]
- **High-precision scenarios:** [Recommended type and rationale]

### 8. Market Condition Performance

#### Consistency Across Market Types
| Market Type | Mean Latency | StdDev | Throughput | Signal Quality |
|-------------|--------------|--------|------------|----------------|
| Trending | [XXX]μs | [XX]μs | [XXX] bars/sec | [High/Med/Low] |
| Sideways | [XXX]μs | [XX]μs | [XXX] bars/sec | [High/Med/Low] |
| Volatile | [XXX]μs | [XX]μs | [XXX] bars/sec | [High/Med/Low] |

**Market Condition Analysis:**
- Performance consistency: ✅/❌
- Adapts well to volatility: ✅/❌
- Signal stability maintained: ✅/❌

## Performance Trends

### Historical Comparison
[Include chart or table showing performance over time]

### Regression Analysis
- Performance change from last version: [+/-]XX%
- Memory usage change: [+/-]XX%
- Significant improvements: [List]
- Performance regressions: [List]

## Environment Details

### Hardware Configuration
```
CPU: [Processor details]
Memory: [RAM details]
Storage: [Disk type and specs]
OS: [Operating system and version]
```

### Software Environment
```
.NET Runtime: [Version]
JIT Compiler: [Details]
GC Mode: [Server/Workstation, Concurrent/Non-concurrent]
Optimization: [Enabled/Disabled]
```

### Benchmark Configuration
```
Warmup Iterations: [X]
Test Iterations: [X]  
Launch Count: [X]
Timeout: [XX] minutes
```

## Recommendations

### For Real-Time Trading
1. **Optimal Configuration:** K=[X], Lookback=[X], Window=[X]
2. **Numeric Type:** [Type] for best latency
3. **Memory Management:** [Specific recommendations]
4. **Monitoring:** Watch for [specific metrics]

### For Backtesting  
1. **Optimal Configuration:** K=[X], Lookback=[X], Window=[X]
2. **Numeric Type:** [Type] for best accuracy/performance balance
3. **Batch Size:** [Recommended batch size]
4. **Memory Limits:** Stay under [X]GB for large datasets

### Performance Optimization Opportunities
1. **[High Priority]** [Optimization 1]: Expected improvement [XX%]
2. **[Medium Priority]** [Optimization 2]: Expected improvement [XX%]
3. **[Low Priority]** [Optimization 3]: Expected improvement [XX%]

## Appendices

### A. Raw Benchmark Data
[Link to detailed CSV/JSON results]

### B. Profiling Reports  
[Links to ETW/profiler outputs]

### C. Memory Analysis
[Heap dump analysis, allocation traces]

### D. Statistical Analysis
[Confidence intervals, significance tests]

---

**Report Generated:** [Timestamp]  
**Generated By:** [Automated/Manual]  
**Tool Version:** BenchmarkDotNet v[X.X.X]  
**Next Review Date:** [YYYY-MM-DD]