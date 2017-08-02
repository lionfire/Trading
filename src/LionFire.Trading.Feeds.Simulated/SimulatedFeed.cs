using LionFire.Trading.Accounts;
using System;
using System.Collections.Generic;
using System.Text;
using LionFire.Trading.Accounts;
using LionFire.Execution;
using System.Threading.Tasks;
using LionFire.Instantiating;
using System.Threading;

namespace LionFire.Trading.Feeds.Simulated
{
    public class ScriptOptions
    {
        public int MaxRepetitions { get; set; }

    }

    public class ScriptEvent
    {

        public ScriptEvent() { }
        public ScriptEvent(TimeSpan TimeAfterLastEvent, object eventData) { this.TimeAfterLastEvent = TimeAfterLastEvent; this.EventData = eventData; }

        //public DateTime? HistoricalTime { get; set; }
        public TimeSpan TimeAfterLastEvent { get; set; }
        //public string SequenceChannel { get; set; }

        public object EventData { get; set; }
    }

    public class TScript : ITemplate<Script>
    {
        public ScriptOptions ScriptOptions { get; set; }

        //public SortedList<DateTime, ScriptEvent> HistoricalEvents { get; set; }
        //public List<ScriptEvent> SequentialEvents { get; set; }
        public Dictionary<string, List<ScriptEvent>> SequentialEvents { get; set; }

        public static TScript Default
        {
            get
            {
                var result = new TScript();
                var dict = new Dictionary<string, List<ScriptEvent>>();

                var duration = TimeSpan.FromMinutes(5);

                dict.Add("GBPUSD", TickScriptGenerator.GenerateTicks("GBPUSD", duration, startingValue: 1.28, ticksPerSecond: 0.9, pipValue: 0.0001, averageSpread: 0.3, maxSpread: 5));
                dict.Add("EURUSD", TickScriptGenerator.GenerateTicks("EURUSD", duration, startingValue: 1.12, ticksPerSecond: 1.25, pipValue: 0.0001, averageSpread: 0.1, maxSpread: 2));

                return result;
            }
        }

        public DateTime? StartTime { get; set; }
    }

    public class TickScriptGenerator
    {
        public static List<ScriptEvent> GenerateTicks(string symbol, TimeSpan? duration = null, double startingValue = 1, double ticksPerSecond = 1.25, double pipValue = 0.0001, double averageSpread = double.NaN, double maxSpread = double.NaN, double tickTimingVariancePercent = 80.0)
        {
            if (!duration.HasValue) duration = TimeSpan.FromHours(1);
            if (double.IsNaN(averageSpread)) averageSpread = 0.4;
            if (double.IsNaN(maxSpread)) maxSpread = 8.0;
            var list = new List<ScriptEvent>();


            var increment = TimeSpan.FromSeconds(1.0 / ticksPerSecond);


            var lastTickTime = TimeSpan.FromSeconds(0);

            double bid = startingValue;
            double spread = averageSpread;

            for (TimeSpan clock = TimeSpan.FromSeconds(0); clock < duration.Value; clock += increment)
            {
                // TODO: tickTimingVariancePercent
                //var tickTime = clock.
                var tickTime = clock;

                double ask = bid + averageSpread;

                list.Add(new ScriptEvent(tickTime - lastTickTime, new SymbolTick { Symbol = symbol, Bid = bid, Ask = ask });
                lastTickTime = tickTime;
            }

            return list;
        }
    }

    public class ScriptPlaybackChannel
    {
        public List<ScriptEvent> SequentialEvents { get; set; }

        public int Index { get; set; }

        //public int MillisecondsUntilNext

    }
    [HasDependencies]
    public class Script : ITemplateInstance<TScript>, IStartable
    {
        public TScript Template { get; set; }

        public Action<object> EventOccurred;

        Timer timer;
        TimeSpan elapsed;
        DateTime clock;

        public Task Start()
        {
            if (timer != null) { timer.Dispose(); timer = null; }
            elapsed = TimeSpan.Zero;
            clock = Template.StartTime ?? DateTime.UtcNow;

            WaitForNextEvent();

            return Task.CompletedTask;
        }


        private void WaitForNextEvent()
        {
            var millisecondsUntilNextEvent = 0;
            foreach (var kvp in Template.SequentialEvents)
            {
            }
            timer = new Timer(OnTimer, null, millisecondsUntilNextEvent, Timeout.Infinite);
        }

        private void OnTimer(object state)
        {
        }

    }

    

    public class SimulatedFeed : FeedBase<TScript>, IStartable
    {
        public override bool IsSimulation => throw new NotImplementedException();

        public override IEnumerable<string> SymbolsAvailable => throw new NotImplementedException();

        public override DateTime ExtrapolatedServerTime => throw new NotImplementedException();

        public override DateTime ServerTime { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }

        public override MarketSeries CreateMarketSeries(string symbol, TimeFrame timeFrame)
        {
            throw new NotImplementedException();
        }

        protected override Symbol CreateSymbol(string symbolCode)
        {
        }
    }

    public class SimulatedSymbol : SymbolBase
    {
        public SimulatedSymbol(string symbol)
        {
        }
    }
}
