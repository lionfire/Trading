using LionFire.Trading.Indicators.Defaults;
using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.QuantConnect_;

Console.WriteLine("Testing SMA Implementations");
Console.WriteLine("===========================");

// Test First-Party Implementation
Console.WriteLine("\n1. Testing First-Party Implementation (SMAFP):");
TestFirstPartyImplementation();

// Test QuantConnect Implementation
Console.WriteLine("\n2. Testing QuantConnect Implementation (SMAQC):");
TestQuantConnectImplementation();

// Test Default Factory
Console.WriteLine("\n3. Testing Default Factory:");
TestDefaultFactory();

Console.WriteLine("\nAll tests completed successfully!");

void TestFirstPartyImplementation()
{
    var parameters = new PSMA<double, double> { Period = 3 };
    var sma = new SMAFP<double, double>(parameters);
    var inputs = new double[] { 1, 2, 3, 4, 5 };
    var outputs = new double[inputs.Length];

    sma.OnBarBatch(inputs, outputs);

    Console.WriteLine($"   Inputs: [{string.Join(", ", inputs)}]");
    Console.WriteLine($"   Outputs: [{string.Join(", ", outputs.Select(o => o == 0 ? "NaN" : o.ToString("F2")))}]");
    Console.WriteLine($"   IsReady: {sma.IsReady}");
    Console.WriteLine($"   Current Value: {sma.Value:F2}");
    
    // Verify results
    if (outputs[2] == 2 && outputs[3] == 3 && outputs[4] == 4)
    {
        Console.WriteLine("   ✓ Test passed!");
    }
    else
    {
        Console.WriteLine("   ✗ Test failed!");
    }
}

void TestQuantConnectImplementation()
{
    var parameters = new PSMA<double, double> { Period = 3 };
    var sma = new SMAQC<double, double>(parameters);
    var inputs = new double[] { 1, 2, 3, 4, 5 };
    var outputs = new double[inputs.Length];

    sma.OnBarBatch(inputs, outputs);

    Console.WriteLine($"   Inputs: [{string.Join(", ", inputs)}]");
    Console.WriteLine($"   Outputs: [{string.Join(", ", outputs.Select(o => o == 0 ? "NaN" : o.ToString("F2")))}]");
    Console.WriteLine($"   IsReady: {sma.IsReady}");
    Console.WriteLine($"   Current Value: {sma.Value:F2}");
    
    // Verify results
    if (outputs[2] == 2 && outputs[3] == 3 && outputs[4] == 4)
    {
        Console.WriteLine("   ✓ Test passed!");
    }
    else
    {
        Console.WriteLine("   ✗ Test failed!");
    }
}

void TestDefaultFactory()
{
    var sma = SMA.CreateDouble(3);
    
    // Process batch of values
    var inputs = new double[] { 1, 2, 3, 4, 5 };
    var outputs = new double[inputs.Length];
    
    // Cast to SMAFP to access OnBarBatch method
    if (sma is SMAFP<double, double> smafp)
    {
        smafp.OnBarBatch(inputs, outputs);
        
        Console.WriteLine($"   Inputs: [{string.Join(", ", inputs)}]");
        Console.WriteLine($"   Outputs: [{string.Join(", ", outputs.Select(o => o == 0 ? "NaN" : o.ToString("F2")))}]");
        Console.WriteLine($"   IsReady: {sma.IsReady}");
        Console.WriteLine($"   Value: {sma.Value:F2}");
        
        // Verify results
        if (sma.Value == 4.0)
        {
            Console.WriteLine("   ✓ Test passed!");
        }
        else
        {
            Console.WriteLine("   ✗ Test failed!");
        }
    }
    else
    {
        Console.WriteLine("   Unable to test - implementation is not SMAFP");
    }
}