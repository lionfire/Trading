using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.ValueTypes;
using Xunit;
using Xunit.Abstractions;

namespace LionFire.Trading.Indicators.Tests;

public class IntegrationTests
{
    private readonly ITestOutputHelper _output;

    public IntegrationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Integration_BollingerBands_WithRSI_OverboughtOversold()
    {
        // Arrange
        var bbParams = new PBollingerBands<double, BollingerBandsResult> { Period = 20, StandardDeviations = 2 };
        var rsiParams = new PRSI<double, double> { Period = 14 };
        
        var bb = new BollingerBands_QC<double, BollingerBandsResult>(bbParams);
        var rsi = new RSI_QC<double, double>(rsiParams);
        
        // Create data that should trigger both BB squeeze and RSI extremes
        var inputs = new double[100];
        for (int i = 0; i < 50; i++)
        {
            inputs[i] = 100 + Math.Sin(i * 0.1) * 2; // Low volatility
        }
        for (int i = 50; i < 100; i++)
        {
            inputs[i] = 110 + (i - 50) * 2; // Strong uptrend
        }
        
        var bbOutputs = new BollingerBandsResult[inputs.Length];
        var rsiOutputs = new double[inputs.Length];

        // Act
        bb.OnBarBatch(inputs, bbOutputs);
        rsi.OnBarBatch(inputs, rsiOutputs);

        // Assert
        Assert.True(bb.IsReady && rsi.IsReady);
        
        // Find BB squeeze period (narrow bands)
        var squeezeThreshold = 5.0;
        var squeezePeriods = bbOutputs.Skip(20).Take(30)
            .Where(b => b != null && (b.UpperBand - b.LowerBand) < squeezeThreshold)
            .Count();
        
        // Find RSI overbought in uptrend
        var overboughtPeriods = rsiOutputs.Skip(70)
            .Where(r => r > 70)
            .Count();
        
        Assert.True(squeezePeriods > 5, "Should detect BB squeeze");
        Assert.True(overboughtPeriods > 5, "Should detect RSI overbought");
        
        _output.WriteLine($"Detected {squeezePeriods} squeeze periods and {overboughtPeriods} overbought periods");
    }

    [Fact]
    public void Integration_MACD_WithSignalCrossover_AndPriceTrend()
    {
        // Arrange
        var macdParams = new PMACD<double, MACDResult> { FastPeriod = 12, SlowPeriod = 26, SignalPeriod = 9 };
        var smaParams = new PSMA<double, double> { Period = 50 };
        
        var macd = new MACD_QC<double, MACDResult>(macdParams);
        var sma = new SMA_QC<double, double>(smaParams);
        
        // Create data with clear trend change
        var inputs = new double[150];
        for (int i = 0; i < 75; i++)
        {
            inputs[i] = 100 - i * 0.5 + Math.Sin(i * 0.2) * 2; // Downtrend
        }
        for (int i = 75; i < 150; i++)
        {
            inputs[i] = 62.5 + (i - 75) * 0.8 + Math.Sin(i * 0.2) * 2; // Uptrend
        }
        
        var macdOutputs = new MACDResult[inputs.Length];
        var smaOutputs = new double[inputs.Length];

        // Act
        macd.OnBarBatch(inputs, macdOutputs);
        sma.OnBarBatch(inputs, smaOutputs);

        // Assert
        Assert.True(macd.IsReady && sma.IsReady);
        
        // Find MACD signal crossovers
        var bullishCrossovers = 0;
        var bearishCrossovers = 0;
        
        for (int i = 50; i < macdOutputs.Length - 1; i++)
        {
            if (macdOutputs[i] != null && macdOutputs[i + 1] != null)
            {
                var currentDiff = macdOutputs[i].MACD - macdOutputs[i].Signal;
                var nextDiff = macdOutputs[i + 1].MACD - macdOutputs[i + 1].Signal;
                
                if (currentDiff <= 0 && nextDiff > 0) bullishCrossovers++;
                if (currentDiff >= 0 && nextDiff < 0) bearishCrossovers++;
            }
        }
        
        // Check price trend vs SMA
        var aboveSMA = inputs.Skip(100).Zip(smaOutputs.Skip(100))
            .Count(pair => pair.First > pair.Second && pair.Second > 0);
        
        Assert.True(bullishCrossovers >= 1, "Should detect bullish MACD crossover");
        Assert.True(aboveSMA > 20, "Price should be above SMA in uptrend");
        
        _output.WriteLine($"Bullish: {bullishCrossovers}, Bearish: {bearishCrossovers}, Above SMA: {aboveSMA}");
    }

    [Fact]
    public void Integration_VolumeAnalysis_MFI_VWAP_CMF()
    {
        // Arrange
        var mfiParams = new PMFI<HLCV, double> { Period = 14 };
        var vwapParams = new PVWAP<HLCV, double> { Period = 20 };
        var cmfParams = new PChaikinMoneyFlow<HLCV, double> { Period = 20 };
        
        var mfi = new MFI_QC<HLCV, double>(mfiParams);
        var vwap = new VWAP_QC<HLCV, double>(vwapParams);
        var cmf = new ChaikinMoneyFlow_QC<HLCV, double>(cmfParams);
        
        // Create accumulation scenario: rising prices with high volume
        var inputs = new HLCV[50];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 1.5;
            var volume = 10000 + i * 500; // Increasing volume
            inputs[i] = new HLCV
            {
                High = price + 0.8,
                Low = price - 0.3,
                Close = price + 0.6, // Close near high (accumulation)
                Volume = volume
            };
        }
        
        var mfiOutputs = new double[inputs.Length];
        var vwapOutputs = new double[inputs.Length];
        var cmfOutputs = new double[inputs.Length];

        // Act
        mfi.OnBarBatch(inputs, mfiOutputs);
        vwap.OnBarBatch(inputs, vwapOutputs);
        cmf.OnBarBatch(inputs, cmfOutputs);

        // Assert
        Assert.True(mfi.IsReady && vwap.IsReady && cmf.IsReady);
        
        var lastMFI = mfiOutputs[inputs.Length - 1];
        var lastVWAP = vwapOutputs[inputs.Length - 1];
        var lastCMF = cmfOutputs[inputs.Length - 1];
        var lastPrice = inputs[inputs.Length - 1].Close;
        
        // All should indicate accumulation
        Assert.True(lastMFI > 50, $"MFI {lastMFI} should indicate buying pressure");
        Assert.True(lastPrice > lastVWAP, $"Price {lastPrice} should be above VWAP {lastVWAP}");
        Assert.True(lastCMF > 0, $"CMF {lastCMF} should be positive (accumulation)");
        
        _output.WriteLine($"MFI: {lastMFI:F2}, Price vs VWAP: {lastPrice:F2} vs {lastVWAP:F2}, CMF: {lastCMF:F3}");
    }

    [Fact]
    public void Integration_TrendFollowing_SMA_EMA_TEMA_HMA()
    {
        // Arrange
        var period = 20;
        var smaParams = new PSMA<double, double> { Period = period };
        var emaParams = new PEMA<double, double> { Period = period };
        var temaParams = new PTEMA<double, double> { Period = period };
        var hmaParams = new PHullMovingAverage<double, double> { Period = period };
        
        var sma = new SMA_QC<double, double>(smaParams);
        var ema = new EMA_QC<double, double>(emaParams);
        var tema = new TEMA_QC<double, double>(temaParams);
        var hma = new HullMovingAverage_QC<double, double>(hmaParams);
        
        // Trending data with sudden direction change
        var inputs = new double[100];
        for (int i = 0; i < 50; i++)
        {
            inputs[i] = 100.0 + i * 2; // Strong uptrend
        }
        for (int i = 50; i < 100; i++)
        {
            inputs[i] = 200.0 - (i - 50) * 3; // Strong downtrend
        }
        
        var smaOutputs = new double[inputs.Length];
        var emaOutputs = new double[inputs.Length];
        var temaOutputs = new double[inputs.Length];
        var hmaOutputs = new double[inputs.Length];

        // Act
        sma.OnBarBatch(inputs, smaOutputs);
        ema.OnBarBatch(inputs, emaOutputs);
        tema.OnBarBatch(inputs, temaOutputs);
        hma.OnBarBatch(inputs, hmaOutputs);

        // Assert - Test responsiveness after trend change
        var changePoint = 60; // 10 bars after trend change
        var currentPrice = inputs[changePoint];
        
        var smaDistance = Math.Abs(currentPrice - smaOutputs[changePoint]);
        var emaDistance = Math.Abs(currentPrice - emaOutputs[changePoint]);
        var temaDistance = Math.Abs(currentPrice - temaOutputs[changePoint]);
        var hmaDistance = Math.Abs(currentPrice - hmaOutputs[changePoint]);
        
        // TEMA and HMA should be more responsive than SMA
        Assert.True(temaDistance < smaDistance, "TEMA should be more responsive than SMA");
        Assert.True(hmaDistance < smaDistance, "HMA should be more responsive than SMA");
        Assert.True(emaDistance < smaDistance, "EMA should be more responsive than SMA");
        
        _output.WriteLine($"Distances from price: SMA={smaDistance:F2}, EMA={emaDistance:F2}, TEMA={temaDistance:F2}, HMA={hmaDistance:F2}");
    }

    [Fact]
    public void Integration_OscillatorDivergence_RSI_Stochastic_CCI()
    {
        // Arrange
        var rsiParams = new PRSI<double, double> { Period = 14 };
        var stochParams = new PStochastic<HLC, StochasticResult> { KPeriod = 14, DPeriod = 3 };
        var cciParams = new PCCI<HLC, double> { Period = 20 };
        
        var rsi = new RSI_QC<double, double>(rsiParams);
        var stoch = new Stochastic_QC<HLC, StochasticResult>(stochParams);
        var cci = new CCI_QC<HLC, double>(cciParams);
        
        // Create divergence scenario: price makes higher high, oscillators make lower high
        var inputs = new double[80];
        var hlcInputs = new HLC[80];
        
        for (int i = 0; i < 40; i++)
        {
            var price = 100.0 + Math.Sin(i * 0.1) * 20; // First peak
            inputs[i] = price;
            hlcInputs[i] = new HLC { High = price + 1, Low = price - 1, Close = price };
        }
        
        for (int i = 40; i < 80; i++)
        {
            // Second peak slightly higher in price but weaker momentum
            var price = 105.0 + Math.Sin((i - 40) * 0.1) * 15;
            inputs[i] = price;
            hlcInputs[i] = new HLC { High = price + 0.5, Low = price - 0.5, Close = price };
        }
        
        var rsiOutputs = new double[inputs.Length];
        var stochOutputs = new StochasticResult[hlcInputs.Length];
        var cciOutputs = new double[hlcInputs.Length];

        // Act
        rsi.OnBarBatch(inputs, rsiOutputs);
        stoch.OnBarBatch(hlcInputs, stochOutputs);
        cci.OnBarBatch(hlcInputs, cciOutputs);

        // Assert
        Assert.True(rsi.IsReady && stoch.IsReady && cci.IsReady);
        
        // Find peaks in each oscillator
        var rsiPeak1 = rsiOutputs.Skip(20).Take(20).Max();
        var rsiPeak2 = rsiOutputs.Skip(60).Take(15).Max();
        
        var stochPeak1 = stochOutputs.Skip(20).Take(20).Where(s => s != null).Max(s => s.K);
        var stochPeak2 = stochOutputs.Skip(60).Take(15).Where(s => s != null).Max(s => s.K);
        
        var cciPeak1 = cciOutputs.Skip(20).Take(20).Max();
        var cciPeak2 = cciOutputs.Skip(60).Take(15).Max();
        
        // Price made higher high but oscillators should show bearish divergence
        var pricePeak1 = inputs.Skip(20).Take(20).Max();
        var pricePeak2 = inputs.Skip(60).Take(15).Max();
        
        Assert.True(pricePeak2 > pricePeak1, "Price should make higher high");
        
        // At least one oscillator should show bearish divergence
        var rsiDivergence = rsiPeak2 < rsiPeak1;
        var stochDivergence = stochPeak2 < stochPeak1;
        var cciDivergence = cciPeak2 < cciPeak1;
        
        var divergenceCount = (rsiDivergence ? 1 : 0) + (stochDivergence ? 1 : 0) + (cciDivergence ? 1 : 0);
        Assert.True(divergenceCount >= 1, "At least one oscillator should show bearish divergence");
        
        _output.WriteLine($"Price peaks: {pricePeak1:F2} -> {pricePeak2:F2}");
        _output.WriteLine($"RSI peaks: {rsiPeak1:F2} -> {rsiPeak2:F2} (divergence: {rsiDivergence})");
        _output.WriteLine($"Stoch peaks: {stochPeak1:F2} -> {stochPeak2:F2} (divergence: {stochDivergence})");
        _output.WriteLine($"CCI peaks: {cciPeak1:F2} -> {cciPeak2:F2} (divergence: {cciDivergence})");
    }

    [Fact]
    public void Integration_ChannelBreakout_BollingerBands_KeltnerChannels_DonchianChannels()
    {
        // Arrange
        var bbParams = new PBollingerBands<double, BollingerBandsResult> { Period = 20, StandardDeviations = 2 };
        var kcParams = new PKeltnerChannels<HLC, KeltnerChannelsResult> { Period = 20, Multiplier = 2.0, AtrPeriod = 10 };
        var dcParams = new PDonchianChannels<HL, DonchianChannelsResult> { UpperPeriod = 20, LowerPeriod = 20 };
        
        var bb = new BollingerBands_QC<double, BollingerBandsResult>(bbParams);
        var kc = new KeltnerChannels_QC<HLC, KeltnerChannelsResult>(kcParams);
        var dc = new DonchianChannels_QC<HL, DonchianChannelsResult>(dcParams);
        
        // Create consolidation followed by breakout
        var priceInputs = new double[60];
        var hlcInputs = new HLC[60];
        var hlInputs = new HL[60];
        
        // Consolidation phase
        for (int i = 0; i < 40; i++)
        {
            var price = 100.0 + Math.Sin(i * 0.3) * 3; // Tight range
            priceInputs[i] = price;
            hlcInputs[i] = new HLC { High = price + 0.5, Low = price - 0.5, Close = price };
            hlInputs[i] = new HL { High = price + 0.5, Low = price - 0.5 };
        }
        
        // Breakout phase
        for (int i = 40; i < 60; i++)
        {
            var price = 103.0 + (i - 40) * 2; // Strong breakout
            priceInputs[i] = price;
            hlcInputs[i] = new HLC { High = price + 0.8, Low = price - 0.2, Close = price + 0.6 };
            hlInputs[i] = new HL { High = price + 0.8, Low = price - 0.2 };
        }
        
        var bbOutputs = new BollingerBandsResult[priceInputs.Length];
        var kcOutputs = new KeltnerChannelsResult[hlcInputs.Length];
        var dcOutputs = new DonchianChannelsResult[hlInputs.Length];

        // Act
        bb.OnBarBatch(priceInputs, bbOutputs);
        kc.OnBarBatch(hlcInputs, kcOutputs);
        dc.OnBarBatch(hlInputs, dcOutputs);

        // Assert
        Assert.True(bb.IsReady && kc.IsReady && dc.IsReady);
        
        // Check for squeeze during consolidation
        var consolidationBB = bbOutputs[35];
        var consolidationKC = kcOutputs[35];
        var bbWidth = consolidationBB.UpperBand - consolidationBB.LowerBand;
        var kcWidth = consolidationKC.UpperBand - consolidationKC.LowerBand;
        
        // Check for breakout
        var breakoutPrice = priceInputs[55];
        var breakoutBB = bbOutputs[55];
        var breakoutDC = dcOutputs[55];
        
        var bbBreakout = breakoutPrice > breakoutBB.UpperBand;
        var dcBreakout = breakoutPrice > breakoutDC.UpperBand;
        
        Assert.True(bbWidth < 8, "Should show squeeze in consolidation");
        Assert.True(bbBreakout || dcBreakout, "Should detect breakout in at least one channel");
        
        _output.WriteLine($"Consolidation BB width: {bbWidth:F2}, KC width: {kcWidth:F2}");
        _output.WriteLine($"Breakout price: {breakoutPrice:F2}, BB upper: {breakoutBB.UpperBand:F2}, DC upper: {breakoutDC.UpperBand:F2}");
        _output.WriteLine($"BB breakout: {bbBreakout}, DC breakout: {dcBreakout}");
    }

    [Fact]
    public void Integration_MultiTimeframeAlignment()
    {
        // Arrange - Simulate different timeframe data
        var shortTF = new PEMA<double, double> { Period = 10 }; // Short timeframe
        var mediumTF = new PEMA<double, double> { Period = 30 }; // Medium timeframe
        var longTF = new PEMA<double, double> { Period = 90 }; // Long timeframe
        
        var emaShort = new EMA_QC<double, double>(shortTF);
        var emaMedium = new EMA_QC<double, double>(mediumTF);
        var emaLong = new EMA_QC<double, double>(longTF);
        
        // Strong trending data
        var inputs = new double[200];
        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i] = 100.0 + i * 0.8 + Math.Sin(i * 0.1) * 2;
        }
        
        var shortOutputs = new double[inputs.Length];
        var mediumOutputs = new double[inputs.Length];
        var longOutputs = new double[inputs.Length];

        // Act
        emaShort.OnBarBatch(inputs, shortOutputs);
        emaMedium.OnBarBatch(inputs, mediumOutputs);
        emaLong.OnBarBatch(inputs, longOutputs);

        // Assert - Check for alignment in uptrend
        var currentPrice = inputs[inputs.Length - 1];
        var shortEMA = shortOutputs[inputs.Length - 1];
        var mediumEMA = mediumOutputs[inputs.Length - 1];
        var longEMA = longOutputs[inputs.Length - 1];
        
        // In uptrend: Price > Short EMA > Medium EMA > Long EMA
        var bullishAlignment = currentPrice > shortEMA && 
                              shortEMA > mediumEMA && 
                              mediumEMA > longEMA;
        
        Assert.True(bullishAlignment, "Should show bullish multi-timeframe alignment");
        
        _output.WriteLine($"Price: {currentPrice:F2}, Short EMA: {shortEMA:F2}, Medium EMA: {mediumEMA:F2}, Long EMA: {longEMA:F2}");
        _output.WriteLine($"Bullish alignment: {bullishAlignment}");
    }
}