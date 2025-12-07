# BotVM.Value Null Issue - Root Cause Analysis

**Date:** 2025-12-07
**Issue:** Bots grid shows rows with correct keys but all text cells are empty because `BotVM.Value` is null

## Summary

The `BotVM.Value` property was always null due to a timing issue with reactive subscriptions where late subscribers missed already-emitted values.

## Root Cause

The issue was in `DirectoryReaderRx.GetValueObservable` which used `Publish().RefCount()`:

```csharp
public IObservable<TValue?> GetValueObservable(TKey key)
    => valueObservables.GetOrAdd(key, k => CreateValueObservable(k)
        .Publish()
        .RefCount());
```

`Publish().RefCount()` creates a shared subscription but **does not replay past values** to late subscribers.

## Detailed Flow

1. **Source reader (`HjsonFsDirectoryReaderRx`)** discovers keys and immediately subscribes to `GetValueObservable(key)` for each key (in `StartListeningToAllKeys`)
2. This triggers async file loading via `CreateValueObservable`
3. When the file loads, `observer.OnNext(value)` is emitted
4. The source reader's subscription receives the value and updates its internal `values` cache
5. **Later**, `ReactiveSubsetReader.EnsureValueLoaded` subscribes to the same `GetValueObservable(key)`
6. Because `Publish().RefCount()` does not replay, the late subscriber never receives the already-emitted value
7. `valueCache` in `ReactiveSubsetReader` remains at `Optional.None`
8. VMs are created with `Value = null` and are never updated

## The Fix

Change `Publish().RefCount()` to `Replay(1).RefCount()` in `DirectoryReaderRx.cs`:

```csharp
// File: /mnt/c/src/Core/src/LionFire.Reactive.Framework/IO/Reactive/DirectoryReaderRx.cs
// Lines 508-511

public IObservable<TValue?> GetValueObservable(TKey key)
    => valueObservables.GetOrAdd(key, k => CreateValueObservable(k)
        .Replay(1)  // Cache the most recent value for late subscribers
        .RefCount());
```

`Replay(1).RefCount()` caches the most recent value and replays it immediately to any new subscriber, ensuring late subscribers receive the current value.

## Files Involved

- `/mnt/c/src/Core/src/LionFire.Reactive.Framework/IO/Reactive/DirectoryReaderRx.cs` - Contains the fix
- `/mnt/c/src/Core/src/LionFire.Reactive/Reactive/Persistence/Read/ReactiveSubsetReader.cs` - `EnsureValueLoaded` subscribes late
- `/src/tp/Trading/src/LionFire.Trading.Automation.Blazor/Bots/Bots.razor` - UI component showing the grid
- `/mnt/c/src/Core/src/LionFire.Data.Async.Mvvm/Data/Async/Collections/DynamicData_/ObservableDataVM.cs` - Creates VMs from data

## Key Insight

When using `Publish().RefCount()` for shared observables that emit values asynchronously, consider whether late subscribers need to receive the current/latest value. If so, use `Replay(1).RefCount()` or `ReplaySubject` instead.
