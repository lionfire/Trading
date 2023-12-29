using Binance.Net.Clients;
using Binance.Net.Interfaces;
using CryptoExchange.Net.CommonObjects;
using K4os.Compression.LZ4.Streams;
using LionFire;
using LionFire.ExtensionMethods.Cloning;
using LionFire.Threading;
using LionFire.Results;
using LionFire.Serialization;
using LionFire.Trading.Binance;
using LionFire.Trading.HistoricalData.Binance;
using LionFire.Trading.HistoricalData.Serialization;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text.Json;
using System.Text.Unicode;
using YamlDotNet.Serialization;
using ZeroFormatter;
using Oakton;
using static AnyDiff.DifferenceLines;
using K4os.Compression.LZ4;
using Microsoft.Extensions.DependencyInjection;
using LionFire.ExtensionMethods.Dumping;
//using Baseline.ImTools;
using System.Collections.Generic;
using LionFire.Trading.HistoricalData.Sources;
using Baseline;

namespace LionFire.Trading.HistoricalData.Retrieval;

public class RetrieveHistoricalDataParameters : HistoricalDataJobInput
{
    [FlagAlias("force", true)]
    public bool ForceFlag { get; set; } = false;
}

/// <summary>
/// 
/// </summary>
/// <remarks>
/// TODO
///  - support 2nd exchange (abstract out per-exchange code)
///  
/// LATER
///  - % progress event
///  
/// </remarks>
[Description("Retrieve historical data from source", Name = "retrieve")]
public class RetrieveHistoricalDataJob : OaktonAsyncCommand<RetrieveHistoricalDataParameters>
{
    #region Configuration

    public const string TimeFormat = "yyyy-MM-dd HH:mm:ss";
    public const string YamlExtension = "yaml";
    public const bool InsertBlankBars = true;  // For this to be false, serialized format must include timestamps on klines.

    public Func<LZ4EncoderSettings> LZ4EncoderSettings => () => new LZ4EncoderSettings
    {
        BlockSize = 256 * 1024,
        //CompressionLevel = K4os.Compression.LZ4.LZ4Level.L12_MAX,
        CompressionLevel = (LZ4Level)Input.CompressFlag,
        ExtraMemory = 16 * 1024 * 1024,
    };

    #endregion

    #region Dependencies

    public BinanceClientProvider? BinanceClientProvider { get; set; }
    public KlineArrayFileProvider? KlineArrayFileProvider { get; set; }
    public ILogger<RetrieveHistoricalDataJob>? Logger { get; set; }
    BinanceRestClient? BinanceClient { get; set; }

    #endregion

    #region Options: Validation and Parsing

    RetrieveHistoricalDataParameters Input;

    public void ValidateAndParseOptions()
    {
        ValidateAndParse_Fields();
        Validate_Exchange();
        Validate_ExchangeArea();


        Validate_NumericType(); // after exchange+area
    }

    public void Validate_Exchange()
    {
        Input.ExchangeFlag = Input.ExchangeFlag?.ToLowerInvariant();
        switch (Input.ExchangeFlag)
        {
            case "binance":
                break;
            default:
                throw new ArgumentException($"{nameof(Input.ExchangeFlag)} not valid: {Input.ExchangeFlag}");
        }
    }

    public void Validate_ExchangeArea()
    {
        // TODO: validate per-exchange

        switch (Input.ExchangeFlag)
        {
            case "binance":
                switch (Input.ExchangeAreaFlag)
                {
                    case "futures":
                        Input.NativeNumericType = typeof(decimal);
                        return;
                    default:
                        break;
                }
                break;
            default:
                break;
        }
        throw new ArgumentException($"{nameof(Input.ExchangeAreaFlag)} not valid: {Input.ExchangeAreaFlag}");
    }

    public void ValidateAndParse_Fields()
    {
        switch (Input.FieldsFlag)
        {
            case "native":
                Input.FieldSet = FieldSet.Native;
                break;
            case "ohlcv":
                Input.FieldSet = FieldSet.Ohlcv;
                break;
            case "ohlc":
                Input.FieldSet = FieldSet.Ohlc;
                break;
            default:
                throw new ArgumentException($"Fields not valid: {Input.FieldsFlag}");
        }
    }
    public Type? NumericTypeType { get; set; }

    public void Validate_NumericType()
    {
        switch (Input.ExchangeFlag)
        {
            case "binance":
                Input.NativeNumericType = typeof(decimal);
                //switch (ExchangeArea)
                //{
                //    case "futures":
                //        return;
                //    default:
                //        break;
                //}
                break;
            default:
                break;
        }
        if (Input.NativeNumericType == null) { throw new ArgumentException($"NativeNumericType could not be determined for exchange+area"); }

        switch (Input.NumericTypeFlag)
        {
            case "native":
                NumericTypeType = Input.NativeNumericType;
                break;
            case "decimal":
                NumericTypeType = typeof(decimal);
                break;
            case "double":
                NumericTypeType = typeof(double);
                break;
            case "float":
                NumericTypeType = typeof(float);
                break;
            default:
                break;
        }

        if (NumericTypeType == null)
        {
            throw new ArgumentException($"NumericType '{Input.NumericTypeFlag}' not valid, or not supported for exchange+area {Input.ExchangeFlag}+{Input.ExchangeAreaFlag}");
        }
    }

    #endregion

    #region Execute

    public override async Task<bool> Execute(RetrieveHistoricalDataParameters input)
    {
        Input = input;

        var host = input.BuildHost();
        var barsFileSource = host.Services.GetRequiredService<BarsFileSource>();
        var RangeProvider = host.Services.GetRequiredService<HistoricalDataChunkRangeProvider>();
        BinanceClientProvider = host.Services.GetService<BinanceClientProvider>() ?? throw new ArgumentNullException();
        KlineArrayFileProvider = host.Services.GetService<KlineArrayFileProvider>() ?? throw new ArgumentNullException();
        Logger = host.Services.GetService<ILogger<RetrieveHistoricalDataJob>>() ?? throw new ArgumentNullException();

        BinanceClient = BinanceClientProvider.GetPublicClient();

        if (input.FromFlag == default) throw new ArgumentNullException(nameof(input.FromFlag));
        if (input.ToFlag == default) throw new ArgumentNullException(nameof(input.ToFlag));
        if (!input.KlineInterval.HasValue) throw new ArgumentNullException(nameof(input.KlineInterval));

        DateTime start, endExclusive;
        var NextDate = input.FromFlag;

        var local = await barsFileSource.List(input.ExchangeFlag, input.ExchangeAreaFlag, input.Symbol, input.TimeFrame);
        do
        {
            (start, endExclusive) = RangeProvider.RangeForDate(NextDate, input.TimeFrame);

            if (!input.ForceFlag && local.Chunks.Where(c => c.Start == start && c.EndExclusive == endExclusive).Any())
            {
                Console.WriteLine($"Already have chunk {NextDate.ToString(TimeFormat)}: {start.ToString(TimeFormat)} to {endExclusive.ToString(TimeFormat)}");
            }
            else
            {
                (start, endExclusive) = await RetrieveForDate(NextDate);
                Console.WriteLine($"Retrieved chunk {NextDate.ToString(TimeFormat)}: {start.ToString(TimeFormat)} to {endExclusive.ToString(TimeFormat)}");
            }
            NextDate = endExclusive + Input.TimeFrame.TimeSpanApproximation;
        } while (endExclusive < Input.ToFlag && (NextDate + Input.TimeFrame.TimeSpanApproximation < DateTime.UtcNow));

        return true;
    }

    private async Task<(DateTime start, DateTime end)> RetrieveForDate(DateTime date)
    {
        if (Input.VerboseFlag) { DumpParameters(); }
        ValidateAndParseOptions();

        KlineArrayFileOptions klineArrayFileOptions = new();

        switch (Input.FieldSet)
        {
            case FieldSet.Native:
                break;
            case FieldSet.Ohlcv:
            case FieldSet.Ohlc:
                klineArrayFileOptions.FileExtension += "." + Input.FieldSet.ToString().ToLower();
                break;
            //case FieldSet.Unspecified:
            default:
                break;
        }

        if (Input.NativeNumericType != NumericTypeType) { klineArrayFileOptions.FileExtension += "." + NumericTypeType!.Name.ToLower(); }

        klineArrayFileOptions.FileExtension += "." + YamlExtension;

        KlineArrayInfo info;
        string path;
        long bars = 0;
        long blankBars = 0;

        var serializer = new SerializerBuilder().ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults).Build();
        //var serializer = new YamlDotNet.Serialization.Serializer();

        using (var file = KlineArrayFileProvider.GetFile(Input.ExchangeFlag, Input.ExchangeAreaFlag, Input.Symbol, Input.TimeFrame, date, klineArrayFileOptions))
        {
            info = file.Info;

            if (!Input.QuietFlag)
            {
                Console.WriteLine($"Retrieving chunk from {info.Start.ToString(TimeFormat)} to {info.EndExclusive.ToString(TimeFormat)}");
            }

            // TODO: Break this up into multiple calls if it will be above limit

            var interval = Input.TimeFrame.TimeSpan;

            var requestFrom = info.Start;
            var requestTo = Input.GetEffectiveTo(info.EndExclusive);
            if (info.EndExclusive > requestTo) { requestTo = info.EndExclusive; }

            var expectedLastBar = Input.TimeFrame.GetOpenTimeBefore(info.EndExclusive);

            DateTime? first = null;
            IBinanceKline? firstKline = null;
            IBinanceKline? lastKline = null;
            List<BinanceFuturesKlineItem> listNative = new();
            List<OhlcvItem> listOhlcv = new();
            List<OhlcDecimalItem> listOhlc = new();

            DateTime lastOpenTime = default;

            var nextStartTime = requestFrom;

            path = file.FileStream.Name;
            //if (Compact) { info.DataType = typeof(OhlcvDecimalItem).FullName!; }

            do
            {
                Logger.LogInformation($"{nextStartTime} - {requestTo}");
                CryptoExchange.Net.Objects.WebCallResult<IEnumerable<IBinanceKline>> result = (await BinanceClient!.UsdFuturesApi.ExchangeData.GetKlinesAsync(Input.Symbol, Input.KlineInterval ?? throw new ArgumentNullException(), startTime: nextStartTime, endTime: requestTo, limit: Input.LimitFlag)) ?? throw new Exception("retrieve returned null");

                await CheckWeight_Binance(result?.ResponseHeaders);

                foreach (var kline in result!.Data)
                {
                    if (!first.HasValue) { first = kline.OpenTime; }
                    if (firstKline == null) { firstKline = kline; }
                    if (kline.OpenTime >= info.EndExclusive) break;

                    bars++;

                    if (info.High == null || kline.HighPrice > info.High) { info.High = kline.HighPrice; }
                    if (info.Low == null || kline.LowPrice < info.Low) { info.Low = kline.LowPrice; }

                    var expectedNextOpenTime = lastOpenTime == default || interval == null ? default : Input.TimeFrame.GetNextOpenTimeForOpen(lastOpenTime);

                    if (expectedNextOpenTime != default)
                    {
                        if (expectedNextOpenTime != kline.OpenTime)
                        {
                            info.Gaps ??= new();
                            info.Gaps.Add((expectedNextOpenTime, kline.OpenTime - Input.TimeFrame.TimeSpan!.Value));
                            Console.WriteLine($"[WARN] GAP DETECTED - Unexpected jump from {lastOpenTime.ToString(TimeFormat)} to {kline.OpenTime.ToString(TimeFormat)}");

                            if (InsertBlankBars)
                            {
                                var blankBarsToInsert = (kline.OpenTime - expectedNextOpenTime) / Input.TimeFrame.TimeSpan!.Value;
                                Console.WriteLine($"Inserting {blankBarsToInsert} blank bars");
                                for (; blankBarsToInsert > 1; blankBarsToInsert--)
                                {
                                    bars++;
                                    blankBars++;
                                    listNative.Add(BinanceFuturesKlineItem.Missing);
                                }
                            }
                        }
                    }
                    lastOpenTime = kline.OpenTime;

                    switch (Input.FieldSet)
                    {
                        case FieldSet.Native:
                            listNative.Add(new BinanceFuturesKlineItem(kline.OpenPrice, kline.HighPrice, kline.LowPrice, kline.ClosePrice, kline.TakerBuyBaseVolume, kline.TakerBuyBaseVolume));
                            break;
                        case FieldSet.Ohlcv:
                            listOhlcv.Add(new OhlcvItem(kline.OpenPrice, kline.HighPrice, kline.LowPrice, kline.ClosePrice, kline.Volume));
                            break;
                        case FieldSet.Ohlc:
                            listOhlc.Add(new OhlcDecimalItem(kline.OpenPrice, kline.HighPrice, kline.LowPrice, kline.ClosePrice));
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                    if (Input.VerboseFlag) { Console.WriteLine(kline.Dump()); }
                    lastKline = kline;
                }

                if (lastKline == null)
                {
                    //throw new Exception("Failed to retreive any bars");
                    break;
                }
                nextStartTime = lastKline.OpenTime + Input.TimeFrame.TimeSpan!.Value;
            } while (lastKline != null && (nextStartTime + Input.TimeFrame.TimeSpan!.Value < DateTime.UtcNow) && lastKline.OpenTime != expectedLastBar);

            if (first.HasValue)
            {
                info.FirstOpenTime = first.Value;
            }
            if (firstKline != null) { info.Open = firstKline.OpenPrice; }
            if (lastKline != null)
            {
                info.LastOpenTime = lastKline.OpenTime;
                info.Close = lastKline.ClosePrice;
            }

            if (InsertBlankBars)
            {
                info.MissingBarsIncluded = true;
            }
            info.IsComplete = (firstKline != null && firstKline.OpenTime == info.Start)
                && (lastKline != null && lastKline.OpenTime + Input.TimeFrame.TimeSpan == info.EndExclusive);
            info.FieldSet = Input.FieldSet;
            info.NumericType = NumericTypeType!.Name;
            info.Bars = bars;
            info.RetrieveTime = DateTime.UtcNow;

            string compressionMethod = "LZ4";

            if (Input.CompressFlag > 0) { info.Compression = compressionMethod; }

            //var json = Newtonsoft.Json.JsonConvert.SerializeObject(file.Info, Newtonsoft.Json.Formatting.Indented);
            //var infoString = Environment.NewLine + System.Text.Json.JsonSerializer.Serialize(file.Info, new JsonSerializerOptions
            //{
            //    WriteIndented = true,
            //    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault,
            //}) + Environment.NewLine;


            Stream stream = file.FileStream;
            using (var streamWriter = new StreamWriter(stream, System.Text.Encoding.UTF8, -1, true))
            {
                streamWriter.WriteLine("---");
                serializer.Serialize(streamWriter, file.Info);
                streamWriter.WriteLine("...");
            }
            stream.FlushAsync().FireAndForget();

            Stream? compressionStream = null;
            try
            {
                if (Input.CompressFlag > 0) { compressionStream = stream = LZ4Stream.Encode(stream, LZ4EncoderSettings(), true); }

                byte[] bytes;

                switch (Input.FieldSet)
                {
                    case FieldSet.Native:
                        bytes = ZeroFormatterSerializer.Serialize(listNative);
                        break;
                    case FieldSet.Ohlcv:
                        bytes = ZeroFormatterSerializer.Serialize(listOhlcv);
                        break;
                    case FieldSet.Ohlc:
                        bytes = ZeroFormatterSerializer.Serialize(listOhlc);
                        break;
                    default:
                        throw new NotImplementedException();
                }

                stream.Write(bytes, 0, bytes.Length);
            }
            finally
            {
                if (compressionStream != null) { compressionStream.Dispose(); }
            }

            if (info.IsComplete) { file.IsComplete = true; }

            if (!Input.QuietFlag)
            {
                Console.WriteLine($"Saved {bars} bars to {file.CompletePath}" + Environment.NewLine);
                Console.Write(serializer.Serialize(file.Info));
            }
        }

        if (!Input.NoVerifyFlag)
        {
            Verify();
        }
        return (info.Start, info.EndExclusive);
    }

    #endregion

    public void Verify()
    {

    }

    #region Binance

    private async Task<int?> CheckWeight_Binance(IEnumerable<KeyValuePair<string, IEnumerable<string>>>? responseHeaders)
    {
        int? result = null;
        if (!Input.QuietFlag)
        {
            foreach (var h in responseHeaders ?? Enumerable.Empty<KeyValuePair<string, IEnumerable<string>>>())
            {
                if (h.Key.ToUpperInvariant().Contains("WEIGHT") && h.Key != "X-MBX-USED-WEIGHT-1M")
                {
                    foreach (var v in h.Value)
                    {
                        Console.WriteLine($"TODO - {h.Key} = {v}");
                    }
                }

                if (h.Key != "X-MBX-USED-WEIGHT-1M") continue;
                foreach (var v in h.Value)
                {
                    Console.WriteLine($"{h.Key} = {v}");
                    if (int.TryParse(v, out var weight))
                    {
                        result = weight;

                        if (weight > EmergencyMaxWeight)
                        {
                            Logger.LogInformation($"API rate limiting: weight of {weight} is above local threshold of {EmergencyMaxWeight}.  Waiting.");
                            await Task.Delay(60 * 1000);
                        }

                        if (weight > MaxWeight)
                        {
                            Logger.LogInformation($"API rate limiting: weight of {weight} is above local threshold of {MaxWeight}.  Waiting.");
                            await Task.Delay(3 * 1000);
                        }
                    }
                }
            }
        }
        return result;
    }

    public int MaxWeight = 800;
    public int EmergencyMaxWeight = 900;

    #endregion

    public void DumpParameters()
    {
        Console.WriteLine($"NoVerify: {Input.NoVerifyFlag}");
        Console.WriteLine($"EffectiveTo: {Input.EffectiveTo}");
    }
}

