using K4os.Compression.LZ4.Streams;
using LionFire.Trading.HistoricalData.Binance;
using System.Collections;
using System.Text;
using ZeroFormatter;

namespace LionFire.Trading.HistoricalData.Serialization;

public class KlineFileDeserializer
{
    public static KlineArrayInfo DeserializeInfo(string path)
    {
        // TODO: Detect magic at start of file
        var (info, stream) = DeserializeStream(path);
        stream.Dispose();
        return info;
    }

    public static (KlineArrayInfo info, IList) Deserialize(string path)
    {
        var (info, stream) = DeserializeStream(path);

        Stream? decompressionStream = null;

        if (info.Compression == "LZ4")
        {
            decompressionStream = stream = LZ4Stream.Decode(stream);
        }

        Console.WriteLine($"stream position: {stream.Position}");
        IList list;
        try
        {
            switch (info.FieldSet)
            {
                case FieldSet.Native:
                    list = ZeroFormatterSerializer.Deserialize<List<BinanceFuturesKlineItem>>(stream);
                    break;
                case FieldSet.Ohlcv:
                    list = ZeroFormatterSerializer.Deserialize<List<OhlcvItem>>(stream);
                    break;
                case FieldSet.Ohlc:
                    list = ZeroFormatterSerializer.Deserialize<List<OhlcDecimalItem>>(stream);
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
        Console.WriteLine($"Deserialized {list.Count} bars");
        return (info, list);
    }

    public static (KlineArrayInfo info, Stream stream) DeserializeStream(string path)
    {
        var yamlBuilder = new StringBuilder();
        Stream stream;

        stream = File.OpenRead(path);

        var buffer = new List<byte>();
        bool hasCarriageReturn = false;
        bool done = false;
        int i = 0;
        var p = UTF8Encoding.UTF8.Preamble;
        var pn = 0;
        for (; !done; i++)
        {
            int b = stream.ReadByte();

            if (i < p.Length && pn >= 0)
            {
                if (p[i] == b)
                {
                    pn++;
                    if (pn == p.Length)
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
                    stream.Seek(i+1, SeekOrigin.Begin);
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
        
        //Console.WriteLine("Data index: " + i);
        return (info, stream);
    }
}

//public class KlineArrayFileReader : IDisposable
//{
//    public FileStream FileStream { get; protected set; }

//    //byte[] ReadAllBytes(string fileName)
//    //{
//    //    //var length = (int)new Sharpen.IO.File(fileName).Length();
//    //    var raf = new RandomAccessFile(fileName, "rw");
//    //    var buffer = new byte[length];
//    //    raf.Read(buffer);
//    //    raf.Close();
//    //    return buffer;
//    //}
//    public void Dispose()
//    {
//        FileStream?.Dispose();

//    }
//}