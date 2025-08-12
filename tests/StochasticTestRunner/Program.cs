using LionFire.Trading;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.Indicators.Native;

class Program
{
    static void Main()
    {
        Console.WriteLine("=== Stochastic Oscillator Test Runner ===\n");

        // Test parameters
        var parameters = new PStochastic<double, double>
        {
            FastPeriod = 14,
            SlowKPeriod = 3,
            SlowDPeriod = 3,
            OverboughtLevel = 80,
            OversoldLevel = 20
        };

        // Generate test data - uptrend
        var uptrendData = new List<HLC<double>>();
        for (int i = 0; i < 20; i++)
        {
            double basePrice = 100 + i * 2; // Uptrend
            uptrendData.Add(new HLC<double>
            {
                High = basePrice + 2,
                Low = basePrice - 2,
                Close = basePrice + 1  // Close near high
            });
        }

        // Test QuantConnect implementation
        Console.WriteLine("Testing QuantConnect Implementation:");
        TestIndicator(new Stochastic_QC<double, double>(parameters), uptrendData, "QuantConnect");

        // Test First-Party implementation
        Console.WriteLine("\nTesting First-Party Implementation:");
        TestIndicator(new Stochastic_FP<double, double>(parameters), uptrendData, "First-Party");

        // Test default alias
        Console.WriteLine("\nTesting Default Alias (should use QuantConnect):");
        TestIndicator(new Stochastic<double, double>(parameters), uptrendData, "Default");

        // Test with downtrend data
        var downtrendData = new List<HLC<double>>();
        for (int i = 0; i < 20; i++)
        {
            double basePrice = 100 - i * 2; // Downtrend
            downtrendData.Add(new HLC<double>
            {
                High = basePrice + 2,
                Low = basePrice - 2,
                Close = basePrice - 1  // Close near low
            });
        }

        Console.WriteLine("\n=== Testing with Downtrend Data ===");
        Console.WriteLine("\nQuantConnect with downtrend:");
        TestIndicator(new Stochastic_QC<double, double>(parameters), downtrendData, "QuantConnect-Down");

        Console.WriteLine("\nFirst-Party with downtrend:");
        TestIndicator(new Stochastic_FP<double, double>(parameters), downtrendData, "FirstParty-Down");

        Console.WriteLine("\n=== All Tests Completed Successfully ===");
    }

    static void TestIndicator<T>(T indicator, List<HLC<double>> data, string name)
        where T : class
    {
        dynamic ind = indicator;
        
        // Process data
        double[] output = new double[data.Count * 2]; // 2 outputs per input
        ind.OnBarBatch(data, output);

        // Display results
        Console.WriteLine($"  {name} Results:");
        Console.WriteLine($"    IsReady: {ind.IsReady}");
        Console.WriteLine($"    %K: {ind.PercentK:F2}");
        Console.WriteLine($"    %D: {ind.PercentD:F2}");
        Console.WriteLine($"    IsOverbought: {ind.IsOverbought}");
        Console.WriteLine($"    IsOversold: {ind.IsOversold}");

        // Display last few output values
        Console.WriteLine($"    Last outputs: ");
        int startIdx = Math.Max(0, output.Length - 6);
        for (int i = startIdx; i < output.Length; i += 2)
        {
            if (i + 1 < output.Length)
            {
                Console.WriteLine($"      [%K: {output[i]:F2}, %D: {output[i + 1]:F2}]");
            }
        }

        // Validate
        if (ind.IsReady)
        {
            if (ind.PercentK < 0 || ind.PercentK > 100)
            {
                Console.WriteLine($"    ERROR: %K out of range: {ind.PercentK}");
            }
            if (ind.PercentD < 0 || ind.PercentD > 100)
            {
                Console.WriteLine($"    ERROR: %D out of range: {ind.PercentD}");
            }
        }
    }
}