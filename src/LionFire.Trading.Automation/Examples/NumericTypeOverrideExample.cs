#if UNUSED
// Written by Claude Code for some reason

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LionFire.Trading.Automation.Examples;

/// <summary>
/// Example demonstrating how to use numeric type overrides for bot configurations.
/// </summary>
public static class NumericTypeOverrideExample
{
    /// <summary>
    /// Example of configuring BotHarnessOptions for different scenarios
    /// </summary>
    public static void ConfigureServices(IServiceCollection services)
    {
        // Configure global options for numeric type handling
        services.Configure<BotHarnessOptions>(options =>
        {
            // Use decimal for live trading (maximum precision)
            options.DefaultLiveNumericType = typeof(decimal);
            
            // Use double for backtesting (better performance) 
            options.DefaultBacktestNumericType = typeof(double);
            
            // Enable logging for debugging
            options.LogTypeConversions = true;
            options.WarnOnPrecisionLoss = true;
        });
    }

    /// <summary>
    /// Example of setting up a bot with specific numeric type override
    /// </summary>
    public static BotEntity CreateBotWithOverride()
    {
        var botEntity = new BotEntity
        {
            BotTypeName = "AtrBot",
            Symbol = "BTCUSDT",
            Exchange = "Binance",
            ExchangeArea = "futures",
            TimeFrame = TimeFrame.m1,
            Live = true,
            
            // Override to use decimal for this specific bot even if saved as double
            LiveNumericTypeOverride = typeof(decimal),
            
            // Parameters would normally be loaded from file (could be PAtrBot<double>)
            // Parameters = new PAtrBot<double>() // Example - would come from file deserialization
            Parameters = null // Will be loaded from configuration file
        };

        return botEntity;
    }

    /// <summary>
    /// Example scenario explanations
    /// </summary>
    public static void ScenarioExplanations()
    {
        /*
         * Scenario 1: High-Performance Backtesting
         * - Save bot parameters as PAtrBot<float> for maximum speed
         * - DefaultBacktestNumericType = typeof(float)
         * - When loading for backtesting, keeps float for performance
         * 
         * Scenario 2: Live Trading Accuracy
         * - Same bot saved as PAtrBot<float>
         * - DefaultLiveNumericType = typeof(decimal)
         * - When loading for live trading, converts to decimal for precision
         * 
         * Scenario 3: Bot-Specific Override
         * - Critical bot needs decimal precision even in backtesting
         * - Set LiveNumericTypeOverride = typeof(decimal) on that specific bot
         * - Overrides global settings for that bot only
         * 
         * Scenario 4: No Conversion Needed
         * - Bot saved as PAtrBot<decimal>
         * - Live trading uses decimal by default
         * - No conversion occurs, parameters used directly
         */
    }
}
#endif