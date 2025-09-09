using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using LionFire.Hosting;
using System.IO;

namespace LionFire.Trading.Hosting.Configuration
{
    /// <summary>
    /// Trading-specific configuration extensions
    /// </summary>
    public static class TradingConfigurationExtensions
    {
        /// <summary>
        /// Configures the application to use standard configuration sources with .env file support for trading applications
        /// </summary>
        /// <param name="builder">The host application builder</param>
        /// <param name="args">Command line arguments</param>
        /// <param name="envFilePath">Path to .env file (defaults to /srv/trading/.env)</param>
        /// <returns>The builder for chaining</returns>
        public static IHostApplicationBuilder ConfigureTradingConfiguration(
            this IHostApplicationBuilder builder,
            string[] args,
            string envFilePath = "/srv/trading/.env")
        {
            // Add .env file configuration
            builder.Configuration.AddEnvFile(args, envFilePath);
            
            // Force override of empty environment variables with .env values
            var envValues = ReadEnvFileValues(envFilePath);
            if (envValues.Any())
            {
                // Log at debug level only - will be suppressed at Warning level
                System.Diagnostics.Debug.WriteLine($"Adding {envValues.Count} values from .env file to memory configuration:");
                foreach (var kvp in envValues)
                {
                    if (kvp.Key.Contains("Phemex"))
                    {
                        var displayValue = kvp.Key.Contains("ApiKey") || kvp.Key.Contains("ApiSecret") ? 
                            $"[{kvp.Value?.Length ?? 0} chars]" : kvp.Value;
                        System.Diagnostics.Debug.WriteLine($"  {kvp.Key} = {displayValue}");
                    }
                }
                builder.Configuration.AddInMemoryCollection(envValues);
            }
            
            return builder;
        }
        
        private static Dictionary<string, string?> ReadEnvFileValues(string envFilePath)
        {
            var values = new Dictionary<string, string?>();
            
            if (!File.Exists(envFilePath))
                return values;
            
            try
            {
                foreach (var line in File.ReadAllLines(envFilePath))
                {
                    var trimmedLine = line.Trim();
                    if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith('#'))
                        continue;

                    var equalIndex = trimmedLine.IndexOf('=');
                    if (equalIndex == -1)
                        continue;

                    var key = trimmedLine.Substring(0, equalIndex).Trim();
                    var value = trimmedLine.Substring(equalIndex + 1).Trim();

                    // Convert __ to : for .NET configuration hierarchy
                    var configKey = key.Replace("__", ":");
                    values[configKey] = value;
                }
            }
            catch
            {
                // If we can't read the file, return empty dictionary
            }
            
            return values;
        }
        
        /// <summary>
        /// Configures the configuration builder with standard sources and .env file support for trading applications
        /// </summary>
        /// <param name="builder">The configuration builder</param>
        /// <param name="environmentName">The environment name (Development, Production, etc.)</param>
        /// <param name="args">Command line arguments</param>
        /// <param name="envFilePath">Path to .env file (defaults to /srv/trading/.env)</param>
        /// <returns>The configuration builder for chaining</returns>
        public static IConfigurationBuilder AddTradingConfiguration(
            this IConfigurationBuilder builder,
            string environmentName,
            string[] args,
            string envFilePath = "/srv/trading/.env")
        {
            return builder
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
                .AddEnvFile(args, envFilePath)
                .AddEnvironmentVariables()
                .AddCommandLine(args);
        }
    }
}