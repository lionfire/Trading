using Binance.Net.Clients;
using Binance.Net.Interfaces;
using CryptoExchange.Net.CommonObjects;
using K4os.Compression.LZ4.Streams;
using LionFire;
using LionFire.ExtensionMethods.Cloning;
using LionFire.Threading;
using LionFire.Results;
using LionFire.Serialization;
using LionFire.Trading.Binance_;
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
using Binance.Net.Interfaces.Clients;
using LionFire.Validation;

namespace LionFire.Trading.HistoricalData.Retrieval;

public class RetrieveHistoricalDataParameters : HistoricalDataJobInput
{
    [FlagAlias("force", true)]
    public bool ForceFlag { get; set; } = false;

    public RetrieveHistoricalDataParameters() { }
    public RetrieveHistoricalDataParameters(SymbolBarsRange barsRangeReference)
    {
        SymbolBarsRange = barsRangeReference;
        IntervalFlag = barsRangeReference.TimeFrame.ToShortString();
        Symbol = barsRangeReference.Symbol;
        ExchangeFlag = barsRangeReference.Exchange;
        ExchangeAreaFlag = barsRangeReference.ExchangeArea;
        FromFlag = barsRangeReference.Start;
        ToFlag = barsRangeReference.EndExclusive;
    }
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
public class RetrieveHistoricalDataJob : OaktonAsyncCommand<RetrieveHistoricalDataParameters> // RENAME: Binance... or refactor
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
    public IBinanceRestClient? BinanceClient { get; set; }

    BarsFileSource BarsFileSource { get; set; }
    HistoricalDataChunkRangeProvider RangeProvider { get; set; }

    #endregion

    #region Lifecycle

    public RetrieveHistoricalDataJob() { }
    public RetrieveHistoricalDataJob(IBinanceRestClient? binanceClient, ILogger<RetrieveHistoricalDataJob>? logger, KlineArrayFileProvider? klineArrayFileProvider, BarsFileSource barsFileSource, HistoricalDataChunkRangeProvider historicalDataChunkRangeProvider)
    {
        BinanceClient = binanceClient;
        Logger = logger;
        KlineArrayFileProvider = klineArrayFileProvider;
        BarsFileSource = barsFileSource;
        RangeProvider = historicalDataChunkRangeProvider;
    }

    #endregion

    #region Options: Validation and Parsing

    public RetrieveHistoricalDataParameters Input { get; set; }

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
        BarsFileSource = host.Services.GetRequiredService<BarsFileSource>();
        RangeProvider = host.Services.GetRequiredService<HistoricalDataChunkRangeProvider>();
        KlineArrayFileProvider = host.Services.GetService<KlineArrayFileProvider>() ?? throw new ArgumentNullException();
        Logger = host.Services.GetService<ILogger<RetrieveHistoricalDataJob>>() ?? throw new ArgumentNullException();

        if (BinanceClient == null)
        {
            BinanceClientProvider = host.Services.GetRequiredService<BinanceClientProvider>();
            BinanceClient ??= BinanceClientProvider.GetPublicClient();
        }

        return await Execute2(Input);
    }

    public async Task<bool> Execute2(RetrieveHistoricalDataParameters input)
    {
        Input = input;
        ArgumentNullException.ThrowIfNull(input);

        var barsRangeReference = input.SymbolBarsRange;
        //barsRangeReference ??= new BarsRangeReference(Input.ExchangeFlag, Input.ExchangeAreaFlag, Input.Symbol, Input.TimeFrame, Input.FromFlag, Input.ToFlag);
        barsRangeReference.ThrowIfInvalid();

        //Input.IntervalFlag ??= barsRangeReference?.TimeFrame.ToShortString();
        if (!input.KlineInterval.HasValue) throw new ArgumentNullException(nameof(input.KlineInterval));

        DateTime start, endExclusive;
        KlineArrayInfo? info = null;

        BarsInfo? barsInfo = await BarsFileSource.LoadBarsInfo(barsRangeReference);
        var local = await BarsFileSource.List(barsRangeReference);

        bool retrievedSomething = false;

        bool reverse = true;
        var NextDate = reverse ? barsRangeReference.EndExclusive : barsRangeReference.Start;
        do
        {
            (start, endExclusive) = RangeProvider.RangeForDate(NextDate, barsRangeReference.TimeFrame);

            var chunks = local.Chunks.Where(c => c.Start == start && c.EndExclusive == endExclusive).FirstOrDefault();

            if (start > DateTime.UtcNow)
            {
                Logger.LogDebug($"Skipping chunk because it is in the future: {start.ToString(TimeFormat)} to {endExclusive.ToString(TimeFormat)} because it starts in the future");
            }
            else if (!input.ForceFlag && chunks != null && chunks.ExpectedBars.HasValue && chunks.ExpectedBars == chunks.Bars)
            {
                Logger.LogInformation($"Already have chunk: {start.ToString(TimeFormat)} to {endExclusive.ToString(TimeFormat)}");
            }
            else if (!input.ForceFlag && info != null && info.MissingBarsOnlyAtStart && info.FirstOpenTime > start)
            {
                Logger.LogInformation("Not retrieving before detected FirstOpenTime time: {time}",
                    info.FirstOpenTime);
                break;
            }
            else if (chunks != null && !input.ForceFlag && barsInfo != null && barsInfo.FirstOpenTime > start)
            {
                Logger.LogInformation("Not retrieving before FirstOpenTime time that was loaded from {path}: {firstOpenTime}",
                    BarsFileSource.BarsInfoPath(input.ExchangeFlag, input.ExchangeAreaFlag, input.Symbol, input.TimeFrame), barsInfo.FirstOpenTime);
                break;
            }
            else
            {
                (start, endExclusive, info) = await RetrieveForDate(NextDate);
                retrievedSomething = true;
                Logger.LogInformation($"Retrieved chunk {NextDate.ToString(TimeFormat)}: {start.ToString(TimeFormat)} to {endExclusive.ToString(TimeFormat)}");
            }

            if (RangeProvider.IsValidLongRange(input.TimeFrame, start, endExclusive))
            {
                foreach (var shortRange in RangeProvider.ShortRangesForLongRange(start, input.TimeFrame))
                {
                    var range = barsRangeReference with { Start = shortRange.start, EndExclusive = shortRange.endExclusive };
                    var path = BarsFileSource.HistoricalDataPaths.GetExistingPath(range);
                    //var path = BarsFileSource.HistoricalDataPaths.GetExistingPath(barsRangeReference, shortRange.start, shortRange.endExclusive);

                    if (path != null)
                    {
                        if (input.DeleteExtraFilesFlag)
                        {
                            Logger.LogInformation("Deleting extra file: {path}", path);
                            File.Delete(path);
                        }
                        else
                        {
                            Logger.LogWarning("Extra file: {path}", path);
                        }
                    }
                }
            }

            if (reverse)
            {
                NextDate = start - input.TimeFrame.TimeSpanApproximation;
            }
            else
            {
                NextDate = endExclusive + input.TimeFrame.TimeSpanApproximation;
            }
        } while (reverse ? ReverseCondition() : ForwardCondition());

        bool ForwardCondition() => (endExclusive < input.ToFlag && (NextDate + input.TimeFrame.TimeSpanApproximation < DateTime.UtcNow));
        bool ReverseCondition() => (NextDate >= input.FromFlag);

        return retrievedSomething;
    }

    private async Task<(DateTime start, DateTime end, KlineArrayInfo info)> RetrieveForDate(DateTime date)
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
                Logger.LogInformation($"Retrieving chunk from {info.Start.ToString(TimeFormat)} to {info.EndExclusive.ToString(TimeFormat)}");
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

            BarsInfo? barsInfo = await BarsFileSource.LoadBarsInfo(Input.ExchangeFlag, Input.ExchangeAreaFlag, Input.Symbol, Input.TimeFrame);

            do
            {
                Logger.LogInformation($"{nextStartTime} - {requestTo}");
                CryptoExchange.Net.Objects.WebCallResult<IEnumerable<IBinanceKline>> result = (await BinanceClient!.UsdFuturesApi.ExchangeData.GetKlinesAsync(Input.Symbol, Input.KlineInterval ?? throw new ArgumentNullException(), startTime: nextStartTime, endTime: requestTo, limit: Input.LimitFlag)) ?? throw new Exception("retrieve returned null");

                await CheckWeight_Binance(result?.ResponseHeaders);

                if (!result.Success)
                {
                    if (result.Error != null)
                    {
                        if (result.Error.Code == -1121) throw new ArgumentException(result.Error.Message);
                    }
                    throw new Exception(result.Error?.Message ?? result.Error?.Code.ToString());
                }

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

                //if (reverse)
                //{
                //    nextStartTime = lastKline.OpenTime - Input.TimeFrame.TimeSpan!.Value;
                //}
                //else
                //{
                nextStartTime = lastKline.OpenTime + Input.TimeFrame.TimeSpan!.Value;
                //}
            } while (lastKline != null && (nextStartTime + Input.TimeFrame.TimeSpan!.Value < DateTime.UtcNow) && lastKline.OpenTime != expectedLastBar);
            bool ForwardCondition() => lastKline != null && (nextStartTime + Input.TimeFrame.TimeSpan!.Value < DateTime.UtcNow) && lastKline.OpenTime != expectedLastBar;
            //bool ReverseCondition() => firstKline != null && (nextStartTime >= info.Start) && !detectedStart.HasValue;

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
            var noGaps = info.Gaps == null || info.Gaps.Count == 0;

            info.MissingBarsOnlyAtStart = noGaps && (firstKline != null && firstKline.OpenTime != info.Start)
                && (lastKline != null && lastKline.OpenTime + Input.TimeFrame.TimeSpan == info.EndExclusive);

            info.MissingBarsOnlyAtEnd = noGaps && (firstKline != null && firstKline.OpenTime == info.Start)
                          && (lastKline != null && lastKline.OpenTime + Input.TimeFrame.TimeSpan != info.EndExclusive);

            //info.IsComplete = noGaps && (firstKline != null && firstKline.OpenTime == info.Start)
            //&& (lastKline != null && lastKline.OpenTime + Input.TimeFrame.TimeSpan == info.EndExclusive);
            info.IsComplete = noGaps && !info.MissingBarsOnlyAtStart && !info.MissingBarsOnlyAtEnd;

            info.FieldSet = Input.FieldSet;
            info.NumericType = NumericTypeType!.Name;
            info.Bars = bars;
            info.RetrieveTime = DateTime.UtcNow;

            bool shouldSave = true;

            if (!Input.SaveEmptyFlag && file.Info.Bars == 0)
            {
                Logger.LogInformation("Not saving empty file");
                shouldSave = false;
                file.ShouldSave = false;
            }

            if (shouldSave)
            {
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

                if (info.IsComplete || info.MissingBarsOnlyAtStart) { file.IsComplete = true; }

                if (!Input.QuietFlag)
                {
                    Logger.LogInformation(serializer.Serialize(file.Info));
                    Logger.LogInformation($"Saved {bars} bars to {file.CompletePath}");
                }
            }
            if (info.MissingBarsOnlyAtStart)
            {
                //detectedStart = info.Start;
                Logger.LogInformation("Detected start of data at {start}", info.FirstOpenTime);
                //if (!Input.NoUpdateInfo)
                {
                    bool save = true;
                    if (barsInfo != null)
                    {
                        save = false;
                        if (barsInfo.FirstOpenTime == info.FirstOpenTime)
                        {
                            Logger.LogInformation("{path} detected start of data at '{start}', which matches already saved start date.", BarsFileSource.BarsInfoPath(Input.ExchangeFlag, Input.ExchangeAreaFlag, Input.Symbol, Input.TimeFrame), info.FirstOpenTime);
                        }
                        else
                        {
                            Logger.LogWarning("{path} detected start of data at '{start}' but saved start date is set to '{infoStart}'", BarsFileSource.BarsInfoPath(Input.ExchangeFlag, Input.ExchangeAreaFlag, Input.Symbol, Input.TimeFrame),
                               info.FirstOpenTime, barsInfo.FirstOpenTime);
                        }
                    }

                    if (save /*|| forceSaveInfo*/)
                    {
                        barsInfo = new BarsInfo
                        {
                            FirstOpenTime = info.FirstOpenTime
                        };
                        await BarsFileSource.SaveBarsInfo(Input.SymbolBarsRange, barsInfo);
                    }
                }
            }
        }

        if (!Input.NoVerifyFlag)
        {
            Verify();
        }
        return (info.Start, info.EndExclusive, info);
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

