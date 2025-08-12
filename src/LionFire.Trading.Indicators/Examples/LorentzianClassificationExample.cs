using LionFire.Trading.Indicators.Defaults;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading;
using System;

namespace LionFire.Trading.Indicators.Examples;

/// <summary>
/// Example demonstrating how to use the Lorentzian Classification indicator.
/// This indicator uses machine learning techniques to classify market patterns.
/// </summary>
public static class LorentzianClassificationExample
{
    /// <summary>
    /// Basic usage example with default parameters
    /// </summary>
    public static void BasicUsage()
    {
        // Create indicator with default parameters
        var indicator = LorentzianClassification.CreateDouble();
        
        // Subscribe to results
        indicator.Subscribe(results =>
        {
            Console.WriteLine($"Classification Results: Signal={results[0]}, Confidence={results[1]}");
        });

        // Example market data
        var marketData = new[]
        {
            new OHLC<double> { Open = 100.0, High = 102.0, Low = 99.0, Close = 101.0 },
            new OHLC<double> { Open = 101.0, High = 103.0, Low = 100.0, Close = 102.5 },
            new OHLC<double> { Open = 102.5, High = 104.0, Low = 101.0, Close = 103.0 },
            // ... more data
        };

        // Process market data
        indicator.OnNext(marketData);

        // Check current signal and confidence
        Console.WriteLine($"Current Signal: {indicator.Signal}");
        Console.WriteLine($"Current Confidence: {indicator.Confidence}");
        Console.WriteLine($"Is Ready: {indicator.IsReady}");
        Console.WriteLine($"Historical Patterns: {indicator.HistoricalPatternsCount}");
    }

    /// <summary>
    /// Advanced usage example with custom parameters
    /// </summary>
    public static void AdvancedUsage()
    {
        // Create custom parameters for backtesting
        var parameters = new PLorentzianClassification<double, double>
        {
            NeighborsCount = 10,          // More neighbors for stability
            LookbackPeriod = 200,         // Longer history
            NormalizationWindow = 30,     // Larger normalization window
            MinConfidence = 0.75,         // Higher confidence threshold
            RSIPeriod = 21,               // Custom RSI period
            CCIPeriod = 28,               // Custom CCI period
            ADXPeriod = 21,               // Custom ADX period
            LabelThreshold = 0.02,        // 2% threshold for labeling
            LabelLookahead = 7            // Look 7 bars ahead for labeling
        };

        // Create indicator with custom parameters
        var indicator = LorentzianClassification.Create(parameters);

        // Subscribe to detailed results
        indicator.Subscribe(results =>
        {
            var signal = results[0];
            var confidence = results[1];
            
            string signalText = signal switch
            {
                > 0.5 => "STRONG BUY",
                > 0 => "BUY", 
                < -0.5 => "STRONG SELL",
                < 0 => "SELL",
                _ => "NEUTRAL"
            };

            Console.WriteLine($"Signal: {signalText} (Value: {signal:F3}, Confidence: {confidence:F3})");
            
            // Print current features for analysis
            var features = indicator.CurrentFeatures;
            Console.WriteLine($"Features: [RSI: {features[0]:F2}, CCI_Change: {features[1]:F2}, " +
                             $"ADX: {features[2]:F2}, Returns: {features[3]:F4}, " +
                             $"Volatility: {features[4]:F4}, Momentum: {features[5]:F4}]");
        });

        // Simulate processing real market data
        ProcessMarketDataStream(indicator);
    }

    /// <summary>
    /// Example optimized for live trading
    /// </summary>
    public static void LiveTradingExample()
    {
        // Create indicator optimized for live trading
        var indicator = LorentzianClassification.CreateForLiveTrading<double, double>(
            neighborsCount: 5,    // Fewer neighbors for faster response
            lookbackPeriod: 100,  // Shorter lookback
            minConfidence: 0.6    // Lower confidence threshold for more signals
        );

        // Subscribe to trading signals
        indicator.Subscribe(results =>
        {
            var signal = results[0];
            var confidence = results[1];

            // Only act on high-confidence signals
            if (Math.Abs(signal) > 0 && confidence > 0.6)
            {
                string action = signal > 0 ? "BUY" : "SELL";
                Console.WriteLine($"TRADING SIGNAL: {action} (Confidence: {confidence:P1})");
                
                // Here you would place actual trades
                // PlaceTrade(action, confidence);
            }
        });

        // In a real application, you'd connect to a live data feed
        Console.WriteLine("Live trading indicator ready. Connect to data feed...");
    }

    /// <summary>
    /// Example optimized for backtesting
    /// </summary>
    public static void BacktestingExample()
    {
        // Create indicator optimized for backtesting
        var indicator = LorentzianClassification.CreateForBacktesting<double, double>(
            neighborsCount: 8,      // Balanced neighbors count
            lookbackPeriod: 300,    // Long lookback for stability
            minConfidence: 0.7,     // High confidence for quality signals
            labelThreshold: 0.015   // 1.5% labeling threshold
        );

        var trades = new List<(DateTime time, string action, double confidence)>();

        // Subscribe to backtest signals
        indicator.Subscribe(results =>
        {
            var signal = results[0];
            var confidence = results[1];

            if (Math.Abs(signal) > 0 && confidence >= 0.7)
            {
                string action = signal > 0 ? "BUY" : "SELL";
                trades.Add((DateTime.Now, action, confidence));
                Console.WriteLine($"Backtest Signal: {action} (Confidence: {confidence:P1})");
            }
        });

        Console.WriteLine($"Backtesting completed. Generated {trades.Count} signals.");
        
        // Analyze backtest results
        var buySignals = trades.Count(t => t.action == "BUY");
        var sellSignals = trades.Count(t => t.action == "SELL");
        var avgConfidence = trades.Average(t => t.confidence);
        
        Console.WriteLine($"Buy Signals: {buySignals}, Sell Signals: {sellSignals}");
        Console.WriteLine($"Average Confidence: {avgConfidence:P1}");
    }

    /// <summary>
    /// Simulates processing a stream of market data
    /// </summary>
    private static void ProcessMarketDataStream(ILorentzianClassification<OHLC<double>, double> indicator)
    {
        // Simulate realistic market data with some trend and noise
        var random = new Random(42); // Fixed seed for reproducible results
        double price = 100.0;
        
        for (int i = 0; i < 500; i++) // Process 500 bars
        {
            // Simulate price movement with trend and noise
            var trend = Math.Sin(i * 0.02) * 0.5; // Long-term trend
            var noise = (random.NextDouble() - 0.5) * 2.0; // Random noise
            var change = trend + noise;
            
            price += change;
            
            // Create OHLC bar with realistic spreads
            var open = price;
            var high = price + Math.Abs(random.NextDouble() * 1.5);
            var low = price - Math.Abs(random.NextDouble() * 1.5);
            var close = price + (random.NextDouble() - 0.5) * 1.0;
            
            var ohlc = new OHLC<double> 
            { 
                Open = open, 
                High = high, 
                Low = low, 
                Close = close 
            };

            // Process single bar
            indicator.OnNext(ohlc);
            
            // Print status every 50 bars
            if (i % 50 == 0 && indicator.IsReady)
            {
                Console.WriteLine($"Bar {i}: Price={close:F2}, Signal={indicator.Signal:F3}, " +
                                $"Confidence={indicator.Confidence:F3}, Patterns={indicator.HistoricalPatternsCount}");
            }
        }
    }

    /// <summary>
    /// Example showing how to analyze feature importance
    /// </summary>
    public static void FeatureAnalysisExample()
    {
        var indicator = LorentzianClassification.CreateDouble();
        
        // Track feature statistics
        var featureStats = new double[6][];
        for (int i = 0; i < 6; i++)
        {
            featureStats[i] = new double[100]; // Store last 100 values
        }
        int featureIndex = 0;

        indicator.Subscribe(results =>
        {
            if (indicator.IsReady)
            {
                var features = indicator.CurrentFeatures;
                
                // Store features for analysis
                for (int i = 0; i < features.Length; i++)
                {
                    featureStats[i][featureIndex % 100] = Convert.ToDouble(features[i]);
                }
                featureIndex++;

                // Print feature analysis every 100 bars
                if (featureIndex % 100 == 0)
                {
                    Console.WriteLine("\nFeature Analysis:");
                    string[] featureNames = { "RSI", "CCI_Change", "ADX", "Returns", "Volatility", "Momentum" };
                    
                    for (int i = 0; i < featureNames.Length; i++)
                    {
                        var values = featureStats[i];
                        var mean = values.Average();
                        var std = Math.Sqrt(values.Select(x => Math.Pow(x - mean, 2)).Average());
                        Console.WriteLine($"{featureNames[i]}: Mean={mean:F4}, StdDev={std:F4}");
                    }
                }
            }
        });

        // Process test data for feature analysis
        ProcessMarketDataStream(indicator);
    }
}