﻿using LionFire.Ontology;
using LionFire.Trading.Automation;
using Newtonsoft.Json;
using Nito.Disposables;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using AliasAttribute = LionFire.Ontology.AliasAttribute;

namespace LionFire.Trading.Automation;


[Alias("Bot")]
//[Plural("Bots")]
public partial class BotEntity : ReactiveObject
{
    public PBotHarness? PBotHarness { get; set; }

    [Reactive]
    bool _enabled;

    [Reactive]
    private string? _name;

    [Reactive]
    private string? _comments;

    //[JsonProperty("Description")]
    public string? Description { get; set; }

    void X() { Comments = "a"; }

}
