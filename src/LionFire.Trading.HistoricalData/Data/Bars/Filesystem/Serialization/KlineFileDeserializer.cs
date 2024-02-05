using Binance.Net.Interfaces;
using Binance.Net.Objects.Models.Futures;
using K4os.Compression.LZ4.Streams;
using LionFire.Trading.HistoricalData.Binance;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using ZeroFormatter;

namespace LionFire.Trading.HistoricalData.Serialization;

public class KlineFileDeserializer
{
    //public ILogger<KlineFileDeserializer> Logger { get; }

    //public KlineFileDeserializer(ILogger<KlineFileDeserializer> logger)
    //{
    //    Logger = logger;
    //}

    public static KlineArrayInfo? DeserializeInfo(string path)
    {
        // TODO: Detect magic at start of file
        var (info, stream) = DeserializeStream(path);
        stream?.Dispose();
        return info;
    }

    public static (KlineArrayInfo? info, IReadOnlyList<IKline>?) Deserialize(string path)
    {
        var (info, stream) = DeserializeStream(path);

        if(info == null && stream == null)
        {
            return (null, null);
        }
        Stream? decompressionStream = null;

        if (info.Compression == "LZ4") { decompressionStream = stream = LZ4Stream.Decode(stream); }

        IReadOnlyList<IKline> list;
        //IReadOnlyList<object> list;
        try
        {
            switch (info.FieldSet)
            {
                case FieldSet.Native:

                    //list = ZeroFormatterSerializer.Deserialize<List<BinanceFuturesKlineItem>>(stream);


                    DateTime openTime = info.Start;
                    TimeFrame timeFrame = TimeFrame.Parse(info.TimeFrame);
                    TimeSpan timeSpan = timeFrame.TimeSpan!.Value;

                    if (info.DataType == null || info.DataType == typeof(LionFire.Trading.HistoricalData.Binance.BinanceFuturesKlineItem).FullName)
                    {
                        var list1 = ZeroFormatterSerializer.Deserialize<List<BinanceFuturesKlineItem>>(stream);
                        //var list2 = new List<BinanceFuturesUsdtKline>(list1.Count);
                        var list2 = new List<IKline>(list1.Count);
                        //var list2 = new List<BinanceFuturesKlineItem2>(list1.Count);
                        foreach (var i in list1)
                        {
                            list2.Add(new BinanceFuturesKlineItem2
                            {
                                Code = i.Code,
                                OpenTime = openTime.Ticks,
                                CloseTime = (openTime + timeSpan - TimeSpan.FromMilliseconds(1)).Ticks,
                                Open = i.Open,
                                High = i.High,
                                Low = i.Low,
                                Close = i.Close,

                                //QuoteVolume = i.Volume,
                                TakerBuyBaseVolume = i.TakerBuyVolume,
                                //TakerBuyQuoteVolume = i.BaseVolume,
                                Volume = i.BaseVolume, // ?
                                                       //TradeCount = i.
                                                       //Volume = i.Vo
                                                       //Volume = 
                                                       //Volume = i.Volume,
                                                       //CloseTime = i.CloseTime,
                                                       //QuoteAssetVolume = i.QuoteAssetVolume,
                                                       //NumberOfTrades = i.NumberOfTrades,
                                                       //TakerBuyBaseAssetVolume = i.TakerBuyBaseAssetVolume,
                                                       //TakerBuyQuoteAssetVolume = i.TakerBuyQuoteAssetVolume,
                                                       //Ignore = i.Ignore,
                            });
                            openTime += timeSpan;
                        }
                        //list = new List<IKline>(list2.OfType<IKline>());
                        list = list2;
                    }
                    else if (info.DataType == typeof(LionFire.Trading.HistoricalData.Binance.BinanceFuturesKlineItem2).FullName)
                    {
                        var list1 = ZeroFormatterSerializer.Deserialize<List<BinanceFuturesKlineItem2>>(stream);
                        //var list2 = new List<BinanceFuturesUsdtKline>(list1.Count);
                        //var list2 = new List<IKline>(list1.Count);
                        //foreach (var i in list1)
                        //{
                        //    if (i.Code == BarStatusCodes.Missing) { list2.Add(null!); continue; }
                        //    if (i.Code != BarStatusCodes.Ok) { throw new NotImplementedException(); }

                        //    list2.Add(new BinanceFuturesKlineItem2
                        //    {
                        //        Open = i.Open,
                        //        High = i.High,
                        //        Low = i.Low,
                        //        Close = i.Close,

                        //        OpenTime = i.OpenTime,
                        //        CloseTime = i.CloseTime,

                        //        QuoteVolume = i.QuoteVolume,
                        //        TakerBuyBaseVolume = i.TakerBuyBaseVolume,
                        //        TakerBuyQuoteVolume = i.TakerBuyQuoteVolume,
                        //        Volume = i.Volume,
                        //        TradeCount = i.TradeCount,

                        //        //Ignore = i.Ignore,
                        //    });
                        //    openTime += timeSpan;
                        //}
                        list = new List<IKline>(list1.OfType<IKline>());
                    }
                    else throw new ArgumentException("Unknown DataType: " + info.DataType);
                    break;
                case FieldSet.Ohlcv:
                    throw new NotImplementedException();
                    //list = ZeroFormatterSerializer.Deserialize<List<OhlcvItem>>(stream);
                    break;
                case FieldSet.Ohlc:
                    throw new NotImplementedException();
                    //list = ZeroFormatterSerializer.Deserialize<List<OhlcDecimalItem>>(stream);
                    break;
                default:
                    throw new NotImplementedException($"{nameof(info.FieldSet)} info.FieldSet.ToString()");
            }
        }
        finally
        {
            decompressionStream?.Dispose();
            stream.Dispose();
        }
        //Trace.WriteLine($"Deserialized {list.Count} bars");
        return (info, list);
    }

    public static (KlineArrayInfo info, Stream? stream) DeserializeStream(string path)
    {
        var yamlBuilder = new StringBuilder();
        Stream stream;

        stream = File.OpenRead(path);
        if (stream.Length == 0)
        {
            stream.Close();
            stream.Dispose();
            return (null, null);
        }

        var buffer = new List<byte>();
        bool hasCarriageReturn = false;
        bool done = false;
        int i = 0;
        var expectedPreamble = UTF8Encoding.UTF8.Preamble;
        var pn = 0;
        for (; !done; i++)
        {
            int b = stream.ReadByte();

            if (i < expectedPreamble.Length && pn >= 0)
            {
                if (expectedPreamble[i] == b)
                {
                    pn++;
                    if (pn == expectedPreamble.Length)
                    {
                        buffer.Clear();
                        continue;
                    }
                }
            }

            if (b == -1) throw new IOException("End of file reached in header.");
            if (b == 13) { hasCarriageReturn = true; }
            else if (b == 10 && hasCarriageReturn)
            {
                string line = Encoding.UTF8.GetString(buffer.ToArray(), 0, buffer.Count);
                buffer.Clear();
                hasCarriageReturn = false;

                if (line.StartsWith("..."))
                {
                    stream.Seek(i + 1, SeekOrigin.Begin);
                    break;
                }
                else
                {
                    yamlBuilder.AppendLine(line);
                }
            }
            else
            {
                if (hasCarriageReturn) { buffer.Add(13); }
                hasCarriageReturn = false;
                buffer.Add((byte)b);
            }
        }
        //s1 = yamlBuilder.ToString();
        //Console.WriteLine(yamlBuilder.ToString().Length);
        //yamlBuilder.Clear();

        //if(false)
        //{
        //    var sr = new StreamReader(path);
        //    string? line;
        //    for (line = sr.ReadLine(); line != "..."; line = sr.ReadLine())
        //    {
        //        yamlBuilder.AppendLine(line);
        //    }
        //    //stream = sr.BaseStream;
        //    //Console.WriteLine(yamlBuilder.ToString().Length);

        //}

        //int x(char x) => x;

        //if (false)
        //{
        //    s2 = yamlBuilder.ToString();
        //    for (int i = 0; i < s1.Length || i < s2.Length; i++)
        //    {
        //        if (i >= s1.Length) { Console.WriteLine("s1 Missing: " + x(s2[i])); continue; }
        //        if (i >= s2.Length) { Console.WriteLine("s2 Missing: " + x(s1[i])); continue; }
        //        if (s1[i] != s2[i]) { Console.WriteLine($"{i}: {x(s1[i])} != {x(s2[i])}"); }
        //    }
        //}

        var deserializer = new YamlDotNet.Serialization.Deserializer();
        var info = deserializer.Deserialize<KlineArrayInfo>(yamlBuilder.ToString());

        return (info, stream);
    }
}

