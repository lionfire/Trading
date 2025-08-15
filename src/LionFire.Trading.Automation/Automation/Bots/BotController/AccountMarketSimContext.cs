using LionFire.Trading.Automation.Bots;
using LionFire.Trading.Data;
using LionFire.Trading.Indicators.Inputs;
using System;
using System.Collections.Generic;

namespace LionFire.Trading.Automation;

// REVIEW - not sure if this is be overkill, or contains redundant properties

/// <summary>
/// Context specifically for AccountMarketSim instances, managing their lifecycle and input mappings.
/// </summary>
public sealed class AccountMarketSimContext<TPrecision> : MarketParticipantContext<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
{
    #region Identity
    
    public ExchangeSymbol ExchangeSymbol { get; }
    
    #endregion
    
    #region Relationships
    
    public SimContext<TPrecision> Sim { get; }
    public AccountMarketSim<TPrecision> MarketSim { get; }
    public IAccount2<TPrecision> Account => MarketSim.Account;
    
    #endregion
    
    #region Parameters
    
    public PAccountMarketSim<TPrecision> Parameters => (PAccountMarketSim<TPrecision>)MarketSim.Parameters;
    public TimeFrame TimeFrame { get; }
    
    #endregion
    
    #region Lifecycle
    
    public AccountMarketSimContext(
        SimContext<TPrecision> sim,
        AccountMarketSim<TPrecision> marketSim,
        ExchangeSymbol exchangeSymbol,
        TimeFrame timeFrame)
    {
        Sim = sim ?? throw new ArgumentNullException(nameof(sim));
        MarketSim = marketSim ?? throw new ArgumentNullException(nameof(marketSim));
        ExchangeSymbol = exchangeSymbol;
        TimeFrame = timeFrame;
        
        // Initialize InputMappings for this market sim
        InitializeInputMappings();
    }
    
    private void InitializeInputMappings()
    {
        // The AccountMarketSim already has its own InputMappings that it creates in its constructor
        // We just need to reference them here for use during batch processing
        InputMappings = MarketSim.InputMappings ?? new List<PInputToMappingToValuesWindowProperty>();
    }
    
    #endregion
    
    #region State
    
    public DateTimeOffset SimulatedCurrentDate => Sim.SimulatedCurrentDate;
    
    #endregion
}