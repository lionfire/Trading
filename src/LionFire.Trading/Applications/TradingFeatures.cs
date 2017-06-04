namespace LionFire.Trading
{
    public enum TradingFeatures
    {
        None = 0,

        HistoricalData = 1 << 0,
        RealTimeTickData = 1 << 1,

        MonitorPositions = 1 << 2,

        OpenPositions = 1 << 3,
        ClosePositions = 1 << 4,
        LiveBots = 1 << 5,
        DemoBots = 1 << 6,
        Scanners = 1 << 7,
        LiveAccounts = 1 << 8,
        DemoAccounts = 1 << 9,
        Feeds = 1 << 10,

        // Indicators ?

        WorkspaceInterface = 1 << 15,

        AllLive = LiveAccounts | LiveBots,
        Accounts = LiveAccounts | DemoAccounts,
        Bots = LiveBots | DemoBots,
        Participants = Bots | Scanners | Accounts,
        All = Participants | Feeds | HistoricalData | RealTimeTickData | MonitorPositions | OpenPositions | ClosePositions | Bots,
    }
}
