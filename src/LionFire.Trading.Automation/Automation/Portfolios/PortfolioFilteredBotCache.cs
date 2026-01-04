using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LionFire.Reactive.Persistence;
using LionFire.Data.Async.Sets;
using ReactiveUI;
using System.Reactive.Linq;
using DynamicData;
using System.Reactive.Disposables;

namespace LionFire.Trading.Automation;

// AI Generated

/// <summary>
/// Represents a bot that failed to load from disk.
/// </summary>
public class BotLoadFailure
{
    public required string BotId { get; init; }
    public required string ErrorMessage { get; init; }
    public Exception? Exception { get; init; }
}

public class PortfolioFilteredBotCache : ReactiveSubsetReader<string, BotEntity>, IObservableReaderWriter<string, BotEntity>
{
    private readonly IObservableWriter<string, BotEntity>? writer;
    private readonly Portfolio2 portfolio;
    private readonly IObservableReader<string, BotEntity> source;
    private readonly SourceCache<BotLoadFailure, string> loadFailures = new(f => f.BotId);
    private readonly CompositeDisposable disposables = new();

    /// <summary>
    /// Observable cache of bot load failures (bots that are in the portfolio but couldn't be loaded from disk).
    /// </summary>
    public IObservableCache<BotLoadFailure, string> LoadFailures => loadFailures.AsObservableCache();

    public PortfolioFilteredBotCache(IObservableReader<string, BotEntity> source, Portfolio2 portfolio)
        : base(source, portfolio.WhenAnyValue(p => p.BotIds).Select(ids => ids ?? Enumerable.Empty<string>()))
    {
        this.source = source;
        this.writer = source as IObservableWriter<string, BotEntity>;
        this.portfolio = portfolio;

        // Monitor for bots that fail to load
        SetupLoadFailureTracking();
    }

    private void SetupLoadFailureTracking()
    {
        // When portfolio bot IDs change, check which ones don't exist in source
        portfolio.WhenAnyValue(p => p.BotIds)
            .Throttle(TimeSpan.FromMilliseconds(100))
            .Subscribe(botIds =>
            {
                if (botIds == null) return;

                // Get current source keys
                var sourceKeys = source.Keys.Items.ToHashSet();

                // Find bot IDs that are in portfolio but not in source
                foreach (var botId in botIds)
                {
                    if (!sourceKeys.Contains(botId))
                    {
                        // Bot is in portfolio but doesn't exist on disk
                        if (!loadFailures.Lookup(botId).HasValue)
                        {
                            loadFailures.AddOrUpdate(new BotLoadFailure
                            {
                                BotId = botId,
                                ErrorMessage = "Bot not found on disk"
                            });
                        }
                    }
                    else
                    {
                        // Bot exists, remove from failures if it was there
                        loadFailures.RemoveKey(botId);
                    }
                }

                // Remove failures for bots no longer in portfolio
                var currentBotIds = botIds.ToHashSet();
                var failuresToRemove = loadFailures.Keys.Where(k => !currentBotIds.Contains(k)).ToList();
                loadFailures.RemoveKeys(failuresToRemove);
            })
            .DisposeWith(disposables);

        // Also monitor source keys for additions (bot might be created later)
        source.Keys.Connect()
            .Subscribe(changes =>
            {
                foreach (var change in changes)
                {
                    if (change.Reason == ChangeReason.Add)
                    {
                        // If this key was a failure, remove it
                        loadFailures.RemoveKey(change.Key);
                    }
                }
            })
            .DisposeWith(disposables);
    }

    /// <summary>
    /// Removes a failed bot ID from the portfolio (does not delete from disk since it doesn't exist anyway).
    /// </summary>
    public bool RemoveFailedBot(string botId)
    {
        var botIds = portfolio.BotIds;
        if (botIds == null || !botIds.Contains(botId)) return false;

        portfolio.BotIds = botIds.Where(id => id != botId).ToList();
        loadFailures.RemoveKey(botId);
        return true;
    }

    #region IObservableWriter Implementation

    public async ValueTask Write(string key, BotEntity value)
    {
        if (writer == null) throw new NotSupportedException("Source does not support writing.");
        await writer.Write(key, value);

        // Add to portfolio if not already present
        var botIds = portfolio.BotIds?.ToList() ?? new List<string>();
        if (!botIds.Contains(key))
        {
            botIds.Add(key);
            portfolio.BotIds = botIds;
        }
    }

    public ValueTask<bool> Remove(string key)
    {
        // Remove from portfolio's BotIds list instead of deleting from disk
        var botIds = portfolio.BotIds;
        if (botIds == null || !botIds.Contains(key))
        {
            return ValueTask.FromResult(false);
        }

        var newBotIds = botIds.Where(id => id != key).ToList();
        portfolio.BotIds = newBotIds;
        return ValueTask.FromResult(true);
    }

    public IObservable<WriteOperation<string, BotEntity>> WriteOperations => 
        writer?.WriteOperations ?? Observable.Empty<WriteOperation<string, BotEntity>>();

    public IDisposable Synchronize(IObservable<BotEntity> source, string key, WritingOptions? options = null)
    {
        if (writer == null) throw new NotSupportedException("Source does not support writing.");
        return writer.Synchronize(source, key, options);
    }

    public IDisposable Synchronize(IReactiveNotifyPropertyChanged<IReactiveObject> source, string key, WritingOptions? options = null)
    {
        if (writer == null) throw new NotSupportedException("Source does not support writing.");
        return writer.Synchronize(source, key, options);
    }

    #endregion

    #region IDisposable

    private bool disposed;

    public new void Dispose()
    {
        if (disposed) return;
        disposed = true;

        disposables.Dispose();
        loadFailures.Dispose();
        base.Dispose();
    }

    #endregion
}
