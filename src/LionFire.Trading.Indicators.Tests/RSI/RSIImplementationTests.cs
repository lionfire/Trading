using LionFire.Trading.Indicators;
using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.QuantConnect_;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace LionFire.Trading.Indicators.Tests.RSI;

public class RSIImplementationTests
{
    private static List<decimal> GenerateTestPrices(int count, decimal startPrice = 100m)
    {
        var prices = new List<decimal>();
        var random = new Random(42); // Fixed seed for reproducibility
        var price = startPrice;
        
        for (int i = 0; i < count; i++)
        {
            // Generate price movements between -2% and +2%
            var change = (decimal)(random.NextDouble() * 0.04 - 0.02);
            price = price * (1 + change);
            prices.Add(price);
        }
        
        return prices;
    }
    
    [Fact]
    public void AllImplementationsProduceSimilarResults()
    {
        // Arrange
        var parameters = new PRSI<decimal, decimal>
        {
            Period = 14,
            OverboughtLevel = 70,
            OversoldLevel = 30
        };
        
        var testPrices = GenerateTestPrices(100);
        
        var qcRSI = new RSI_QC<decimal, decimal>(parameters);
        var fpRSI = new RSI_FP<decimal, decimal>(parameters);
        var defaultRSI = new RSI<decimal, decimal>(parameters);
        
        var qcResults = new List<decimal>();
        var fpResults = new List<decimal>();
        var defaultResults = new List<decimal>();
        
        qcRSI.Subscribe(results => qcResults.AddRange(results));
        fpRSI.Subscribe(results => fpResults.AddRange(results));
        defaultRSI.Subscribe(results => defaultResults.AddRange(results));
        
        // Act
        qcRSI.OnNext(testPrices);
        fpRSI.OnNext(testPrices);
        defaultRSI.OnNext(testPrices);
        
        // Assert - Allow for small floating point differences
        // Skip the initial values where RSI is not ready
        var skipCount = parameters.Period + 1;
        
        Assert.Equal(qcResults.Count, fpResults.Count);
        Assert.Equal(qcResults.Count, defaultResults.Count);
        
        for (int i = skipCount; i < qcResults.Count; i++)
        {
            // QuantConnect and First-Party should produce very similar results
            Assert.True(Math.Abs(qcResults[i] - fpResults[i]) < 0.1m,
                $"Mismatch at index {i}: QC={qcResults[i]:F4}, FP={fpResults[i]:F4}");
                
            // Default should match QuantConnect exactly (since it inherits from it)
            Assert.Equal(qcResults[i], defaultResults[i]);
        }
    }
    
    [Fact]
    public void RSI_HandlesOverboughtOversold()
    {
        // Arrange
        var parameters = new PRSI<decimal, decimal>
        {
            Period = 14,
            OverboughtLevel = 70,
            OversoldLevel = 30
        };
        
        var rsi = new RSI_FP<decimal, decimal>(parameters);
        
        // Generate trending prices
        var prices = new List<decimal>();
        decimal price = 100;
        
        // Generate uptrend for overbought
        for (int i = 0; i < 30; i++)
        {
            price *= 1.01m; // 1% daily gain
            prices.Add(price);
        }
        
        // Act
        rsi.OnNext(prices);
        
        // Assert - RSI should be overbought after strong uptrend
        Assert.True(rsi.IsReady);
        Assert.True(rsi.CurrentValue > 70, $"RSI should be overbought but is {rsi.CurrentValue:F2}");
        Assert.True(rsi.IsOverbought);
        
        // Now test oversold with downtrend
        rsi.Clear();
        prices.Clear();
        price = 100;
        
        // Generate downtrend for oversold
        for (int i = 0; i < 30; i++)
        {
            price *= 0.99m; // 1% daily loss
            prices.Add(price);
        }
        
        rsi.OnNext(prices);
        
        // Assert - RSI should be oversold after strong downtrend
        Assert.True(rsi.IsReady);
        Assert.True(rsi.CurrentValue < 30, $"RSI should be oversold but is {rsi.CurrentValue:F2}");
        Assert.True(rsi.IsOversold);
    }
    
    [Fact]
    public void RSI_ValidatesParameters()
    {
        // Test invalid period
        Assert.Throws<ArgumentException>(() =>
        {
            var parameters = new PRSI<decimal, decimal>
            {
                Period = 1 // Too small
            };
            new RSI_QC<decimal, decimal>(parameters);
        });
        
        // Test invalid overbought/oversold levels
        Assert.Throws<ArgumentException>(() =>
        {
            var parameters = new PRSI<decimal, decimal>
            {
                Period = 14,
                OverboughtLevel = 30,
                OversoldLevel = 70 // Oversold > Overbought
            };
            new RSI_FP<decimal, decimal>(parameters);
        });
    }
    
    [Fact]
    public void RSI_PerformanceComparison()
    {
        // Arrange
        var parameters = new PRSI<double, double>
        {
            Period = 14
        };
        
        var testData = GenerateTestPrices(100_000).Select(d => (double)d).ToList();
        
        // Act & Measure QuantConnect implementation
        var qcStart = DateTime.UtcNow;
        var qcRSI = new RSI_QC<double, double>(parameters);
        var qcOutput = new double[testData.Count];
        qcRSI.OnBarBatch(testData, qcOutput);
        var qcTime = DateTime.UtcNow - qcStart;
        
        // Act & Measure First-Party implementation
        var fpStart = DateTime.UtcNow;
        var fpRSI = new RSI_FP<double, double>(parameters);
        var fpOutput = new double[testData.Count];
        fpRSI.OnBarBatch(testData, fpOutput);
        var fpTime = DateTime.UtcNow - fpStart;
        
        // Log results (would be captured by test output)
        Console.WriteLine($"QuantConnect: {qcTime.TotalMilliseconds:F2}ms");
        Console.WriteLine($"First-Party: {fpTime.TotalMilliseconds:F2}ms");
        Console.WriteLine($"Speedup: {qcTime.TotalMilliseconds / fpTime.TotalMilliseconds:F2}x");
        
        // Assert - both should complete reasonably fast
        Assert.True(qcTime.TotalSeconds < 5, "QuantConnect implementation too slow");
        Assert.True(fpTime.TotalSeconds < 5, "First-Party implementation too slow");
    }
}