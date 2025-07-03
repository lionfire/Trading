using CsvHelper;
using CsvHelper.Configuration;
using LionFire;
using LionFire.Trading.Automation;
using NLog.Targets;
using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using static BacktestsJournal;

public class BacktestBatchJournalCsvSerialization //: IAsyncDisposable
{

    public static readonly CsvConfiguration CsvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
    {
        //Delimiter = ",",

        //HasHeaderRecord = true,
        //NewLine = "\n",
        NewLine = "\r\n",
        //ShouldQuote = args => false,
    };

    #region Options

    public IOptionsMonitor<BacktestRepositoryOptions> BacktestRepositoryOptionsMonitor { get; }

    #region Convenience

    public bool ZipOnDispose => BacktestRepositoryOptionsMonitor.CurrentValue.ZipOutput;

    #endregion

    #endregion

    #region Lifecycle

    public BacktestBatchJournalCsvSerialization(IOptionsMonitor<BacktestRepositoryOptions> backtestRepositoryOptionsMonitor)
    {
        BacktestRepositoryOptionsMonitor = backtestRepositoryOptionsMonitor;
    }

    public Stream GetCsvStreamFromZip(string zipPath, string fileName)
    {
        using (ZipArchive archive = ZipFile.OpenRead(zipPath))
        {
            ZipArchiveEntry? entry = archive.GetEntry(fileName);
            if (entry == null)
            {
                throw new FileNotFoundException($"{fileName} not found in the zip file.");
            }
            return entry.Open();
        }
    }


    internal IPBot2 LoadBacktest(Type pBotType, string dir, OptimizationBacktestReference obr)
    {
        var fileName = BacktestsJournal.DefaultJournalFilename + ".csv";
        var csvPath = Path.Combine(dir, fileName);

        Stream? stream = null;

        if (File.Exists(csvPath))
        {
            stream = File.OpenRead(csvPath);
        }
        else
        {
            var zip = dir + ".zip";
            if (File.Exists(zip))
            {
                stream = GetCsvStreamFromZip(zip, fileName);
            }
        }
        if (stream == null)
        {
            throw new NotFoundException($"dir: {dir}, obr: {obr}");
        }

        using (CsvReader csv = new CsvHelper.CsvReader(new StreamReader(stream), CsvConfiguration))
        {
            csv.Context.RegisterClassMap(new ParametersMapper(pBotType));

            csv.Read(); // Read header
            csv.ReadHeader();

            var desiredBatchId = obr.BatchId.ToString();
            var desiredBacktestId = obr.BacktestId.ToString();

            while (csv.Read())
            {
                var batchId = csv.GetField<string>("BatchId");
                var id = csv.GetField<string>("Id");

                if (batchId == desiredBatchId && id == desiredBacktestId)
                {
                    var entry = csv.GetRecord<BacktestBatchJournalEntry>();
                    Debug.WriteLine("TODO - deserialize PMultiSim: " + entry.Parameters);

                    //var row = csv.MultiSimContext.Reader.GetRecord<string>();
                    //return row; 
                }
            }
            throw new NotFoundException($"Backtest {obr.BatchId}-{obr.BacktestId} not found in OptimizationRun location: {dir}");
        }
    }

    #endregion

}
