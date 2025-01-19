#if OLD // Discard
using DynamicData;
using Hjson;
using LionFire.Data.Collections;
using LionFire.Persistence;
using LionFire.Persistence.Filesystem;
using LionFire.Referencing;
using ReactiveUI;
using System.Text;
using System.Text.Json;

namespace LionFire.Trading.Link.Blazor.Components.Pages;

public interface IObjectStore<TDocument>
  where TDocument : notnull
{
    public IObservableCache<TDocument, string> Items { get; }

    public ValueTask Write(string key, TDocument document);
    public ValueTask<TDocument> Read(string key);
    public ValueTask<bool> Rename(string oldKey, string newKey);
    public ValueTask<bool> Delete(string key);
}

//public class ObservableReadHandle<TValue, TKey> : IObservableCache<TValue, TKey>
//      where TValue : notnull
//    where TKey : notnull
//{
//}

//public class DocumentStoreVM<TDocument> : ReactiveObject
//{
//    public string VosRootPath { get; set; } = "/bots";

//    IObservable<IChangeSet<TDocument, string>> onChange = simServerRegistrarG.GetUpdates()
//        .ToObservable()
//        .Transform<ISimServerInfoG, ISimServerO, string>(simServerO => simServerO)
//        ;

//    public DocumentStoreVM()
//    {
//        FileReference r = "z:/bots".ToFileReference(); // TODO: Use VosRootPath instead
//        IReadHandle<Persistence.Metadata<IEnumerable<IListing<object>>>> hList = r.GetListingsHandle();
//        IReadHandle<Metadata<IEnumerable<IListing<BotEntity>>>> hBots = r.GetListingsHandle<BotEntity>();

//        Items = new AsyncReadOnlyKeyedFuncCollection<string, BotEntity>(async () => (await hBots.Get()), onChange);
//        Items.ObservableCache.Connect().Subscribe(changeSet =>
//        {
//            InvokeAsync(StateHasChanged);
//        });
//    }


//}
public class LionFireFsStore<TDocument> : IObjectStore<TDocument>
  where TDocument : notnull
{
    #region Parameters

    public string RootDir { get; set; } = @"";

    #endregion

    #region Lifecycle

    public LionFireFsStore(FilesystemPersister filesystemPersister)
    {
        FilesystemPersister = filesystemPersister;
    }

    #endregion

    string GetPath(string key) => Path.Combine(RootDir, key + extension);

    #region Serialization

    string extension = ".hjson";

    byte[] Serialize(TDocument document)
    {
        var json = JsonSerializer.Serialize(document);
        var hjson = Hjson.JsonValue.Parse(json).ToString(new HjsonOptions { EmitRootBraces = false });
        return UTF8Encoding.UTF8.GetBytes(hjson);
    }
    TDocument Deserialize(byte[] bytes)
    {
        var hjson = UTF8Encoding.UTF8.GetString(bytes);
        var json = HjsonValue.Parse(hjson).ToString(Stringify.Plain);
        return JsonSerializer.Deserialize<TDocument>(json)!;
    }

    #endregion

    #region Items

    class NamedDocument
    {
        public string Name { get; set; }
        public TDocument Document { get; set; }
    }

    public IObservableCache<TDocument, string> Items => items;
    private SourceCache<NamedDocument, string> items => new(nd => nd.Name);

    #endregion

    #region Persistence

    public FilesystemPersister FilesystemPersister { get; }

    #endregion


    public ValueTask<bool> Delete(string key)
    {
        FilesystemPersister.DeleteFile(key);
    }

    public ValueTask<TDocument> Read(string key)
    {
        throw new NotImplementedException();
    }

    public ValueTask<bool> Rename(string oldKey, string newKey)
    {
        throw new NotImplementedException();
    }

    public ValueTask Write(string key, TDocument document)
    {
        throw new NotImplementedException();
    }
}

#endif