//#define BacktestAccountSlottedParameters // FUTURE Maybe, though I think we just typically need 1 hardcoded slot for the bars
using CryptoExchange.Net.CommonObjects;
using DynamicData;
using LionFire.Assets;
using LionFire.Threading;
using LionFire.Trading.Automation.Bots;
using LionFire.Trading.Backtesting;
using LionFire.Trading.Journal;
using LionFire.Trading.ValueWindows;
using Polly;
using System.Diagnostics;
using System.Numerics;

namespace LionFire.Trading.Automation;






