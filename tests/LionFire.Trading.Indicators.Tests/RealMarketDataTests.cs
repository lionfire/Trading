using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.ValueTypes;
using Xunit;
using Xunit.Abstractions;

namespace LionFire.Trading.Indicators.Tests;

public class RealMarketDataTests
{
    private readonly ITestOutputHelper _output;

    public RealMarketDataTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void RealData_SP500_Bull_Market_Characteristics()
    {
        // Arrange - Simulate S&P 500 bull market data (2009-2021 style)
        var inputs = GenerateBullMarketData(3000); // ~12 years of daily data
        
        var smaParams = new PSMA<double, double> { Period = 200 };
        var rsiParams = new PRSI<double, double> { Period = 14 };
        var macdParams = new PMACD<double, MACDResult> { FastPeriod = 12, SlowPeriod = 26, SignalPeriod = 9 };
        
        var sma200 = new SMA_QC<double, double>(smaParams);
        var rsi = new RSI_QC<double, double>(rsiParams);
        var macd = new MACD_QC<double, MACDResult>(macdParams);
        
        var smaOutputs = new double[inputs.Length];
        var rsiOutputs = new double[inputs.Length];
        var macdOutputs = new MACDResult[inputs.Length];

        // Act
        sma200.OnBarBatch(inputs, smaOutputs);
        rsi.OnBarBatch(inputs, rsiOutputs);
        macd.OnBarBatch(inputs, macdOutputs);

        // Assert - Bull market characteristics
        var finalPrice = inputs[inputs.Length - 1];
        var finalSMA200 = smaOutputs[inputs.Length - 1];
        
        // Price should be above 200-day SMA most of the time in bull market
        var daysAboveSMA200 = 0;
        for (int i = 200; i < inputs.Length; i++)
        {
            if (inputs[i] > smaOutputs[i] && smaOutputs[i] > 0)
                daysAboveSMA200++;
        }
        var percentAboveSMA200 = daysAboveSMA200 / (double)(inputs.Length - 200) * 100;
        
        // RSI should show reasonable distribution (not constantly overbought)
        var validRSI = rsiOutputs.Skip(14).Where(r => r > 0).ToArray();
        var overboughtDays = validRSI.Count(r => r > 70);
        var oversoldDays = validRSI.Count(r => r < 30);
        var neutralDays = validRSI.Count(r => r >= 30 && r <= 70);
        
        Assert.True(percentAboveSMA200 > 60, $"Bull market should be above SMA200 >60% of time: {percentAboveSMA200:F1}%");
        Assert.True(finalPrice > inputs[0] * 2, "Bull market should at least double over period");
        Assert.True(neutralDays > overboughtDays, "RSI should not be constantly overbought");
        
        _output.WriteLine($"Bull Market Analysis:");
        _output.WriteLine($"- Price above SMA200: {percentAboveSMA200:F1}%");
        _output.WriteLine($"- Total return: {(finalPrice / inputs[0] - 1) * 100:F1}%");
        _output.WriteLine($"- RSI distribution: {overboughtDays} overbought, {neutralDays} neutral, {oversoldDays} oversold");
    }

    [Fact]
    public void RealData_Crypto_Volatility_Test()
    {
        // Arrange - Simulate crypto-style high volatility data
        var inputs = GenerateCryptoStyleData(1000); // ~3 years daily
        
        var atrParams = new PATR<HLC, double> { Period = 14 };
        var bbParams = new PBollingerBands<double, BollingerBandsResult> { Period = 20, StandardDeviations = 2 };
        var rsiParams = new PRSI<double, double> { Period = 14 };
        
        var atr = new AverageTrueRange_QC<HLC, double>(atrParams);
        var bb = new BollingerBands_QC<double, BollingerBandsResult>(bbParams);
        var rsi = new RSI_QC<double, double>(rsiParams);
        
        var hlcInputs = inputs.Select(p => new HLC 
        { 
            High = p * 1.03, 
            Low = p * 0.97, 
            Close = p 
        }).ToArray();
        var priceInputs = inputs.Select(hlc => hlc.Close).ToArray();
        
        var atrOutputs = new double[hlcInputs.Length];
        var bbOutputs = new BollingerBandsResult[priceInputs.Length];
        var rsiOutputs = new double[priceInputs.Length];

        // Act
        atr.OnBarBatch(hlcInputs, atrOutputs);
        bb.OnBarBatch(priceInputs, bbOutputs);
        rsi.OnBarBatch(priceInputs, rsiOutputs);

        // Assert - High volatility characteristics
        var validATR = atrOutputs.Skip(14).Where(a => a > 0).ToArray();
        var avgATR = validATR.Average();
        var maxATR = validATR.Max();
        var volatilitySpikes = validATR.Count(a => a > avgATR * 2);
        
        // Bollinger Band width should vary significantly
        var validBB = bbOutputs.Skip(20).Where(b => b != null).ToArray();
        var bandWidths = validBB.Select(b => b.UpperBand - b.LowerBand).ToArray();
        var avgBandWidth = bandWidths.Average();
        var maxBandWidth = bandWidths.Max();
        var minBandWidth = bandWidths.Min();
        
        // RSI should hit extremes frequently in volatile crypto markets
        var validRSI = rsiOutputs.Skip(14).Where(r => r > 0).ToArray();
        var extremeRSI = validRSI.Count(r => r < 20 || r > 80);
        var extremePercent = extremeRSI / (double)validRSI.Length * 100;
        
        Assert.True(maxATR > avgATR * 3, "Crypto should have significant ATR spikes");
        Assert.True(maxBandWidth > minBandWidth * 3, "BB width should vary significantly");
        Assert.True(extremePercent > 15, $"RSI should hit extremes frequently: {extremePercent:F1}%");
        Assert.True(volatilitySpikes > 10, "Should have multiple volatility spikes");
        
        _output.WriteLine($"Crypto Volatility Analysis:");
        _output.WriteLine($"- Avg ATR: {avgATR:F2}, Max ATR: {maxATR:F2}, Spikes: {volatilitySpikes}");
        _output.WriteLine($"- BB width range: {minBandWidth:F2} - {maxBandWidth:F2}");
        _output.WriteLine($"- RSI extremes: {extremePercent:F1}%");
    }

    [Fact]
    public void RealData_ForexMajorPair_RangeBreakout()
    {
        // Arrange - Simulate EUR/USD style data with range and breakout
        var inputs = GenerateForexRangeBreakoutData();
        
        var dcParams = new PDonchianChannels<HL, DonchianChannelsResult> { UpperPeriod = 20, LowerPeriod = 20 };
        var atrParams = new PATR<HLC, double> { Period = 14 };
        var adxParams = new PADX<HLC, double> { Period = 14 };
        
        var dc = new DonchianChannels_QC<HL, DonchianChannelsResult>(dcParams);
        var atr = new AverageTrueRange_QC<HLC, double>(atrParams);
        var adx = new ADX_QC<HLC, double>(adxParams);
        
        var hlInputs = inputs.Select(p => new HL { High = p.High, Low = p.Low }).ToArray();
        var hlcInputs = inputs.ToArray();
        
        var dcOutputs = new DonchianChannelsResult[hlInputs.Length];
        var atrOutputs = new double[hlcInputs.Length];
        var adxOutputs = new double[hlcInputs.Length];

        // Act
        dc.OnBarBatch(hlInputs, dcOutputs);
        atr.OnBarBatch(hlcInputs, atrOutputs);
        adx.OnBarBatch(hlcInputs, adxOutputs);

        // Assert - Range and breakout detection
        var consolidationPeriod = inputs.Take(400).ToArray(); // First 400 bars
        var breakoutPeriod = inputs.Skip(400).Take(100).ToArray(); // Next 100 bars
        
        // ATR should be lower during consolidation
        var consolidationATR = atrOutputs.Skip(14).Take(380).Where(a => a > 0).Average();
        var breakoutATR = atrOutputs.Skip(414).Take(80).Where(a => a > 0).Average();
        
        // ADX should be lower during range, higher during trend
        var consolidationADX = adxOutputs.Skip(28).Take(360).Where(a => a > 0).Average();
        var breakoutADX = adxOutputs.Skip(428).Take(60).Where(a => a > 0).Average();
        
        // Donchian Channel breakouts
        var breakouts = 0;
        for (int i = 420; i < Math.Min(480, inputs.Length); i++)
        {
            if (dcOutputs[i] != null)
            {
                if (inputs[i].High > dcOutputs[i].UpperBand || inputs[i].Low < dcOutputs[i].LowerBand)
                    breakouts++;
            }
        }
        
        Assert.True(breakoutATR > consolidationATR * 1.2, 
            $"Breakout ATR {breakoutATR:F5} should be higher than consolidation {consolidationATR:F5}");
        Assert.True(breakoutADX > consolidationADX + 5, 
            $"Breakout ADX {breakoutADX:F1} should be higher than consolidation {consolidationADX:F1}");
        Assert.True(breakouts > 5, $"Should detect multiple breakouts: {breakouts}");
        
        _output.WriteLine($"Forex Range/Breakout Analysis:");
        _output.WriteLine($"- Consolidation ATR: {consolidationATR:F5}, Breakout ATR: {breakoutATR:F5}");
        _output.WriteLine($"- Consolidation ADX: {consolidationADX:F1}, Breakout ADX: {breakoutADX:F1}");
        _output.WriteLine($"- Donchian breakouts detected: {breakouts}");
    }

    [Fact]
    public void RealData_CommodityTrend_GoldStyleData()
    {
        // Arrange - Simulate gold-style commodity data with strong trends
        var inputs = GenerateGoldStyleData(1500); // ~6 years of daily data
        
        var sma50Params = new PSMA<double, double> { Period = 50 };
        var sma200Params = new PSMA<double, double> { Period = 200 };
        var macdParams = new PMACD<double, MACDResult> { FastPeriod = 12, SlowPeriod = 26, SignalPeriod = 9 };
        var parabolicSARParams = new PParabolicSAR<HLC, double> { AccelerationFactor = 0.02, AccelerationStep = 0.02, MaxAccelerationFactor = 0.2 };
        
        var sma50 = new SMA_QC<double, double>(sma50Params);
        var sma200 = new SMA_QC<double, double>(sma200Params);
        var macd = new MACD_QC<double, MACDResult>(macdParams);
        var sar = new ParabolicSAR_QC<HLC, double>(parabolicSARParams);
        
        var hlcInputs = inputs.Select(p => new HLC { High = p + 5, Low = p - 5, Close = p }).ToArray();
        
        var sma50Outputs = new double[inputs.Length];
        var sma200Outputs = new double[inputs.Length];
        var macdOutputs = new MACDResult[inputs.Length];
        var sarOutputs = new double[hlcInputs.Length];

        // Act
        sma50.OnBarBatch(inputs, sma50Outputs);
        sma200.OnBarBatch(inputs, sma200Outputs);
        macd.OnBarBatch(inputs, macdOutputs);
        sar.OnBarBatch(hlcInputs, sarOutputs);

        // Assert - Trend following characteristics
        // Golden cross (SMA50 above SMA200)
        var goldenCrosses = 0;
        var deathCrosses = 0;
        
        for (int i = 201; i < inputs.Length; i++)
        {
            if (sma50Outputs[i] > 0 && sma200Outputs[i] > 0 && 
                sma50Outputs[i - 1] > 0 && sma200Outputs[i - 1] > 0)
            {
                var prevDiff = sma50Outputs[i - 1] - sma200Outputs[i - 1];
                var currDiff = sma50Outputs[i] - sma200Outputs[i];
                
                if (prevDiff <= 0 && currDiff > 0) goldenCrosses++;
                if (prevDiff >= 0 && currDiff < 0) deathCrosses++;
            }
        }
        
        // MACD trend strength
        var validMACD = macdOutputs.Skip(35).Where(m => m != null).ToArray();
        var strongBullish = validMACD.Count(m => m.MACD > m.Signal && m.MACD > 0);
        var strongBearish = validMACD.Count(m => m.MACD < m.Signal && m.MACD < 0);
        
        // Parabolic SAR trend periods
        var bullishSAR = 0;
        var bearishSAR = 0;
        for (int i = 2; i < sarOutputs.Length; i++)
        {
            if (sarOutputs[i] > 0)
            {
                if (sarOutputs[i] < hlcInputs[i].Close) bullishSAR++;
                else bearishSAR++;
            }
        }
        
        Assert.True(goldenCrosses + deathCrosses >= 2, "Should have trend changes (crosses)");
        Assert.True(strongBullish > 0 && strongBearish > 0, "Should have both bullish and bearish periods");
        Assert.True(bullishSAR > 100 && bearishSAR > 100, "Should have sustained trend periods");
        
        _output.WriteLine($"Commodity Trend Analysis:");
        _output.WriteLine($"- Golden crosses: {goldenCrosses}, Death crosses: {deathCrosses}");
        _output.WriteLine($"- MACD: {strongBullish} bullish, {strongBearish} bearish periods");
        _output.WriteLine($"- SAR: {bullishSAR} bullish, {bearishSAR} bearish bars");
    }

    [Fact]
    public void RealData_FlashCrash_ResilienceTest()
    {
        // Arrange - Simulate flash crash scenario (like May 6, 2010)
        var inputs = GenerateFlashCrashData();
        
        var rsiParams = new PRSI<double, double> { Period = 14 };
        var bbParams = new PBollingerBands<double, BollingerBandsResult> { Period = 20, StandardDeviations = 2 };
        var vwapParams = new PVWAP<HLCV, double> { Period = 50 };
        
        var rsi = new RSI_QC<double, double>(rsiParams);
        var bb = new BollingerBands_QC<double, BollingerBandsResult>(bbParams);
        var vwap = new VWAP_QC<HLCV, double>(vwapParams);
        
        var hlcvInputs = inputs.Select((p, i) => new HLCV 
        { 
            High = p + 2, 
            Low = p - 2, 
            Close = p,
            Volume = i == 250 ? 1000000 : 10000 // Massive volume during crash
        }).ToArray();
        var priceInputs = inputs.ToArray();
        
        var rsiOutputs = new double[priceInputs.Length];
        var bbOutputs = new BollingerBandsResult[priceInputs.Length];
        var vwapOutputs = new double[hlcvInputs.Length];

        // Act
        rsi.OnBarBatch(priceInputs, rsiOutputs);
        bb.OnBarBatch(priceInputs, bbOutputs);
        vwap.OnBarBatch(hlcvInputs, vwapOutputs);

        // Assert - Flash crash characteristics
        var crashPoint = 250;
        var preeCrashRSI = rsiOutputs[crashPoint - 5];
        var crashRSI = rsiOutputs[crashPoint + 2];
        var postCrashRSI = rsiOutputs[crashPoint + 20];
        
        // BB should expand dramatically during crash
        var preCrashBB = bbOutputs[crashPoint - 5];
        var crashBB = bbOutputs[crashPoint + 5];
        var preCrashWidth = preCrashBB.UpperBand - preCrashBB.LowerBand;
        var crashWidth = crashBB.UpperBand - crashBB.LowerBand;
        
        Assert.True(crashRSI < 20, $"RSI should hit extreme oversold during crash: {crashRSI}");
        Assert.True(postCrashRSI > crashRSI + 20, "RSI should recover after crash");
        Assert.True(crashWidth > preCrashWidth * 2, "BB should expand significantly during crash");
        
        // VWAP should be affected by high volume
        var preeCrashVWAP = vwapOutputs[crashPoint - 1];
        var postCrashVWAP = vwapOutputs[crashPoint + 1];
        var vwapChange = Math.Abs(postCrashVWAP - preeCrashVWAP);
        
        Assert.True(vwapChange > 5, "VWAP should be significantly affected by crash volume");
        
        _output.WriteLine($"Flash Crash Analysis:");
        _output.WriteLine($"- RSI: Pre={preeCrashRSI:F1}, Crash={crashRSI:F1}, Post={postCrashRSI:F1}");
        _output.WriteLine($"- BB Width: Pre={preCrashWidth:F2}, Crash={crashWidth:F2}");
        _output.WriteLine($"- VWAP change: {vwapChange:F2}");
    }

    private double[] GenerateBullMarketData(int count)
    {
        var data = new double[count];
        var random = new Random(42);
        var price = 1000.0; // Starting like SPX in 2009
        var trend = 0.0003; // ~0.03% daily average growth
        
        for (int i = 0; i < count; i++)
        {
            // Add volatility cycles and occasional corrections
            var volatility = 0.015 + 0.005 * Math.Sin(i * 0.01); // Varying volatility
            var correction = i % 500 == 0 && i > 0 ? -0.1 : 0; // 10% correction every 500 days
            
            var dailyReturn = trend + random.NextGaussian() * volatility + correction;
            price *= (1 + dailyReturn);
            data[i] = Math.Max(price, 100);
        }
        
        return data;
    }

    private double[] GenerateCryptoStyleData(int count)
    {
        var data = new double[count];
        var random = new Random(42);
        var price = 10000.0; // Starting like BTC
        
        for (int i = 0; i < count; i++)
        {
            // Crypto characteristics: high volatility, occasional massive moves
            var baseVolatility = 0.04; // 4% daily base volatility
            var extremeMove = random.NextDouble() < 0.05 ? random.NextGaussian() * 0.2 : 0; // 5% chance of extreme move
            
            var dailyReturn = random.NextGaussian() * baseVolatility + extremeMove;
            price *= (1 + dailyReturn);
            data[i] = Math.Max(price, 100);
        }
        
        return data;
    }

    private HLC[] GenerateForexRangeBreakoutData()
    {
        var data = new HLC[500];
        var random = new Random(42);
        var rate = 1.2000; // EUR/USD starting rate
        
        for (int i = 0; i < data.Length; i++)
        {
            double dailyRange, trendComponent;
            
            if (i < 400) // Consolidation period
            {
                dailyRange = 0.0020 + random.NextDouble() * 0.0010; // 20-30 pip range
                trendComponent = Math.Sin(i * 0.05) * 0.0005; // Small oscillation
            }
            else // Breakout period
            {
                dailyRange = 0.0040 + random.NextDouble() * 0.0020; // 40-60 pip range
                trendComponent = (i - 400) * 0.0002; // Strong trend
            }
            
            var change = trendComponent + random.NextGaussian() * 0.0010;
            rate += change;
            
            data[i] = new HLC
            {
                High = rate + dailyRange / 2,
                Low = rate - dailyRange / 2,
                Close = rate + (random.NextDouble() - 0.5) * dailyRange
            };
        }
        
        return data;
    }

    private double[] GenerateGoldStyleData(int count)
    {
        var data = new double[count];
        var random = new Random(42);
        var price = 1200.0; // Starting gold price
        var trendPhase = 0;
        
        for (int i = 0; i < count; i++)
        {
            // Change trend every 300-500 bars
            if (i % 400 == 0) trendPhase = random.Next(-1, 2); // -1, 0, or 1
            
            var trendStrength = trendPhase * 0.0005; // Trend component
            var volatility = 0.02 + 0.01 * Math.Sin(i * 0.02); // Varying volatility
            
            var dailyReturn = trendStrength + random.NextGaussian() * volatility;
            price *= (1 + dailyReturn);
            data[i] = Math.Max(price, 500);
        }
        
        return data;
    }

    private double[] GenerateFlashCrashData()
    {
        var data = new double[300];
        var random = new Random(42);
        var price = 1100.0;
        
        for (int i = 0; i < data.Length; i++)
        {
            double dailyReturn;
            
            if (i < 248) // Normal market
            {
                dailyReturn = random.NextGaussian() * 0.01;
            }
            else if (i < 252) // Flash crash (4 bars)
            {
                dailyReturn = -0.05 - random.NextDouble() * 0.05; // 5-10% down each bar
            }
            else // Recovery
            {
                dailyReturn = 0.02 + random.NextGaussian() * 0.02; // Strong recovery
            }
            
            price *= (1 + dailyReturn);
            data[i] = Math.Max(price, 100);
        }
        
        return data;
    }
}

public static class RandomExtensions
{
    public static double NextGaussian(this Random random, double mean = 0, double stdDev = 1)
    {
        // Box-Muller transform
        double u1 = 1.0 - random.NextDouble();
        double u2 = 1.0 - random.NextDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        return mean + stdDev * randStdNormal;
    }
}