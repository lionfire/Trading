// Temporarily disabled due to complex generic type system
#if false
using BenchmarkDotNet.Attributes;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.Indicators.Native;
using System.Collections.Generic;
using System.Linq;

namespace LionFire.Trading.Indicators.Benchmarks.Indicators;

/// <summary>
/// Benchmarks comparing multiple indicators running simultaneously (realistic scenario)
/// </summary>
[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class CompositeBenchmark : IndicatorBenchmarkBase
{
    // QuantConnect indicators
    private SMA_QC<decimal, decimal> _qcSma20 = null!;
    private SMA_QC<decimal, decimal> _qcSma50 = null!;
    private EMA_QC<decimal, decimal> _qcEma12 = null!;
    private EMA_QC<decimal, decimal> _qcEma26 = null!;
    private RSI_QC<decimal, decimal> _qcRsi = null!;
    private BollingerBands_QC<decimal, decimal> _qcBB = null!;
    
    // Native (FP) indicators
    private SMA_FP<decimal, decimal> _fpSma20 = null!;
    private SMA_FP<decimal, decimal> _fpSma50 = null!;
    private EMA_FP<decimal, decimal> _fpEma12 = null!;
    private EMA_FP<decimal, decimal> _fpEma26 = null!;
    private RSI_FP<decimal, decimal> _fpRsi = null!;
    private BollingerBands_FP<decimal, decimal> _fpBB = null!;

    [Params(14)] // Fixed period for RSI
    public new int Period { get; set; }

    [GlobalSetup]
    public override void GlobalSetup()
    {
        base.GlobalSetup();
        
        // Initialize QuantConnect indicators
        _qcSma20 = new QCSmaIndicator("SMA_20", 20);
        _qcSma50 = new QCSmaIndicator("SMA_50", 50);
        _qcEma12 = new QCEmaIndicator("EMA_12", 12);
        _qcEma26 = new QCEmaIndicator("EMA_26", 26);
        _qcRsi = new QCRsiIndicator("RSI_14", 14);
        _qcBB = new QCBollingerBandsIndicator("BB_20", 20, 2);
        
        // Initialize FinancialPython indicators
        _fpSma20 = new FPSmaIndicator("SMA_20", 20);
        _fpSma50 = new FPSmaIndicator("SMA_50", 50);
        _fpEma12 = new FPEmaIndicator("EMA_12", 12);
        _fpEma26 = new FPEmaIndicator("EMA_26", 26);
        _fpRsi = new FPRsiIndicator("RSI_14", 14);
        _fpBB = new FPBollingerBandsIndicator("BB_20", 20, 2);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Composite", "QuantConnect")]
    public CompositeResults Composite_QuantConnect_AllIndicators()
    {
        // Reset all indicators
        _qcSma20.Reset();
        _qcSma50.Reset();
        _qcEma12.Reset();
        _qcEma26.Reset();
        _qcRsi.Reset();
        _qcBB.Reset();
        
        var results = new CompositeResults
        {
            Sma20 = new List<decimal?>(DataSize),
            Sma50 = new List<decimal?>(DataSize),
            Ema12 = new List<decimal?>(DataSize),
            Ema26 = new List<decimal?>(DataSize),
            Rsi = new List<decimal?>(DataSize),
            BBUpper = new List<decimal?>(DataSize),
            BBMiddle = new List<decimal?>(DataSize),
            BBLower = new List<decimal?>(DataSize)
        };
        
        // Process all data points through all indicators
        for (int i = 0; i < PriceData.Length; i++)
        {
            var timestamp = Timestamps[i];
            var price = PriceData[i];
            
            _qcSma20.Update(timestamp, price);
            _qcSma50.Update(timestamp, price);
            _qcEma12.Update(timestamp, price);
            _qcEma26.Update(timestamp, price);
            _qcRsi.Update(timestamp, price);
            _qcBB.Update(timestamp, price);
            
            results.Sma20.Add(_qcSma20.Current?.Value);
            results.Sma50.Add(_qcSma50.Current?.Value);
            results.Ema12.Add(_qcEma12.Current?.Value);
            results.Ema26.Add(_qcEma26.Current?.Value);
            results.Rsi.Add(_qcRsi.Current?.Value);
            results.BBUpper.Add(_qcBB.UpperBand?.Value);
            results.BBMiddle.Add(_qcBB.Current?.Value);
            results.BBLower.Add(_qcBB.LowerBand?.Value);
        }
        
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("Composite", "FinancialPython")]
    public CompositeResults Composite_FinancialPython_AllIndicators()
    {
        // Reset all indicators
        _fpSma20.Reset();
        _fpSma50.Reset();
        _fpEma12.Reset();
        _fpEma26.Reset();
        _fpRsi.Reset();
        _fpBB.Reset();
        
        var results = new CompositeResults
        {
            Sma20 = new List<decimal?>(DataSize),
            Sma50 = new List<decimal?>(DataSize),
            Ema12 = new List<decimal?>(DataSize),
            Ema26 = new List<decimal?>(DataSize),
            Rsi = new List<decimal?>(DataSize),
            BBUpper = new List<decimal?>(DataSize),
            BBMiddle = new List<decimal?>(DataSize),
            BBLower = new List<decimal?>(DataSize)
        };
        
        // Process all data points through all indicators
        for (int i = 0; i < PriceData.Length; i++)
        {
            var timestamp = Timestamps[i];
            var price = PriceData[i];
            
            _fpSma20.Update(timestamp, price);
            _fpSma50.Update(timestamp, price);
            _fpEma12.Update(timestamp, price);
            _fpEma26.Update(timestamp, price);
            _fpRsi.Update(timestamp, price);
            _fpBB.Update(timestamp, price);
            
            results.Sma20.Add(_fpSma20.Current?.Value);
            results.Sma50.Add(_fpSma50.Current?.Value);
            results.Ema12.Add(_fpEma12.Current?.Value);
            results.Ema26.Add(_fpEma26.Current?.Value);
            results.Rsi.Add(_fpRsi.Current?.Value);
            results.BBUpper.Add(_fpBB.UpperBand?.Value);
            results.BBMiddle.Add(_fpBB.Current?.Value);
            results.BBLower.Add(_fpBB.LowerBand?.Value);
        }
        
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("Composite", "TradingStrategy")]
    public List<TradeSignal> Composite_QuantConnect_TradingStrategy()
    {
        // Simulate a real trading strategy using multiple indicators
        _qcSma20.Reset();
        _qcSma50.Reset();
        _qcRsi.Reset();
        _qcBB.Reset();
        
        var signals = new List<TradeSignal>();
        
        for (int i = 0; i < PriceData.Length; i++)
        {
            var timestamp = Timestamps[i];
            var price = PriceData[i];
            
            _qcSma20.Update(timestamp, price);
            _qcSma50.Update(timestamp, price);
            _qcRsi.Update(timestamp, price);
            _qcBB.Update(timestamp, price);
            
            if (_qcSma20.IsReady && _qcSma50.IsReady && _qcRsi.IsReady && _qcBB.IsReady)
            {
                // Simple strategy logic
                var sma20 = _qcSma20.Current!.Value;
                var sma50 = _qcSma50.Current!.Value;
                var rsi = _qcRsi.Current!.Value;
                var bbUpper = _qcBB.UpperBand!.Value;
                var bbLower = _qcBB.LowerBand!.Value;
                
                if (sma20 > sma50 && rsi < 30 && price <= bbLower)
                {
                    signals.Add(new TradeSignal { Timestamp = timestamp, Type = SignalType.Buy, Price = price });
                }
                else if (sma20 < sma50 && rsi > 70 && price >= bbUpper)
                {
                    signals.Add(new TradeSignal { Timestamp = timestamp, Type = SignalType.Sell, Price = price });
                }
            }
        }
        
        return signals;
    }

    [Benchmark]
    [BenchmarkCategory("Composite", "TradingStrategy")]
    public List<TradeSignal> Composite_FinancialPython_TradingStrategy()
    {
        // Simulate a real trading strategy using multiple indicators
        _fpSma20.Reset();
        _fpSma50.Reset();
        _fpRsi.Reset();
        _fpBB.Reset();
        
        var signals = new List<TradeSignal>();
        
        for (int i = 0; i < PriceData.Length; i++)
        {
            var timestamp = Timestamps[i];
            var price = PriceData[i];
            
            _fpSma20.Update(timestamp, price);
            _fpSma50.Update(timestamp, price);
            _fpRsi.Update(timestamp, price);
            _fpBB.Update(timestamp, price);
            
            if (_fpSma20.IsReady && _fpSma50.IsReady && _fpRsi.IsReady && _fpBB.IsReady)
            {
                // Simple strategy logic
                var sma20 = _fpSma20.Current!.Value;
                var sma50 = _fpSma50.Current!.Value;
                var rsi = _fpRsi.Current!.Value;
                var bbUpper = _fpBB.UpperBand!.Value;
                var bbLower = _fpBB.LowerBand!.Value;
                
                if (sma20 > sma50 && rsi < 30 && price <= bbLower)
                {
                    signals.Add(new TradeSignal { Timestamp = timestamp, Type = SignalType.Buy, Price = price });
                }
                else if (sma20 < sma50 && rsi > 70 && price >= bbUpper)
                {
                    signals.Add(new TradeSignal { Timestamp = timestamp, Type = SignalType.Sell, Price = price });
                }
            }
        }
        
        return signals;
    }

    [GlobalCleanup]
    public override void GlobalCleanup()
    {
        // Dispose QuantConnect indicators
        _qcSma20?.Dispose();
        _qcSma50?.Dispose();
        _qcEma12?.Dispose();
        _qcEma26?.Dispose();
        _qcRsi?.Dispose();
        _qcBB?.Dispose();
        
        // Dispose FinancialPython indicators
        _fpSma20?.Dispose();
        _fpSma50?.Dispose();
        _fpEma12?.Dispose();
        _fpEma26?.Dispose();
        _fpRsi?.Dispose();
        _fpBB?.Dispose();
        
        base.GlobalCleanup();
    }
    
    public class CompositeResults
    {
        public List<decimal?> Sma20 { get; set; } = null!;
        public List<decimal?> Sma50 { get; set; } = null!;
        public List<decimal?> Ema12 { get; set; } = null!;
        public List<decimal?> Ema26 { get; set; } = null!;
        public List<decimal?> Rsi { get; set; } = null!;
        public List<decimal?> BBUpper { get; set; } = null!;
        public List<decimal?> BBMiddle { get; set; } = null!;
        public List<decimal?> BBLower { get; set; } = null!;
    }
    
    public class TradeSignal
    {
        public DateTime Timestamp { get; set; }
        public SignalType Type { get; set; }
        public decimal Price { get; set; }
    }
    
    public enum SignalType
    {
        Buy,
        Sell,
        Hold
    }
}
#endif
