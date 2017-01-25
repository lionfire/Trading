#define DEBUG_BARSCOPIED
//#define BarStruct
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Reactive.Subjects;
using LionFire.Validation;
using LionFire.Instantiating;
using LionFire;
using System.Collections.Concurrent;
using LionFire.Execution;
using LionFire.Execution.Jobs;
#if BarStruct
using BarType = LionFire.Trading.TimedBarStruct;
#else
using BarType = LionFire.Trading.TimedBar;
#endif
using System.Threading.Tasks;
using TickType = LionFire.Trading.Tick;


namespace LionFire.Trading
{
    public class TMarketTickSeries : TMarketSeriesBase, ITemplate<MarketTickSeries>, IValidatesCreate
    {
        public ValidationContext ValidateCreate(ValidationContext context)
        {
            context.MemberNonNull(Account, nameof(Account));
            if (TimeFrame != "t1")
            {
                context.AddIssue(new ValidationIssue
                {
                    Message = "Only t1 supported for TMarketTickSeries.  Use TMarketSeries instead for other timeframes.",
                    MemberName = nameof(TimeFrame),
                    Kind = ValidationIssueKind.InvalidConfiguration | ValidationIssueKind.ParameterOutOfRange,
                });
            }

            return context;
        }
    }

    public sealed class MarketTickSeries : MarketSeriesBase<TMarketTickSeries, Tick>, ITemplateInstance<TMarketTickSeries>

    {

        #region Construction

        public MarketTickSeries() { }
        public MarketTickSeries(IAccount account, string symbol) : base(account, symbol, TimeFrame.t1)
        {
        }

        #endregion

        #region Data

        public TickType this[DateTime time]
        {
            get
            {
                var index = FindIndex(time);
                if (index < 0) return default(TickType);
                return this[index];
            }
        }

        public override TickType this[int index]
        {
            get
            {
                return new TickType
                {
                    Time = openTime[index],
                    Bid = bid[index],
                    Ask = ask[index],
                };
            }
            set
            {
                openTime[index] = value.Time;
                bid[index] = value.Bid;
                ask[index] = value.Ask;
            }
        }

        public DataSeries Bid
        {
            get { return bid; }
        }
        private DataSeries bid = new DataSeries();
        public DataSeries Ask
        {
            get { return ask; }
        }
        private DataSeries ask = new DataSeries();

        #endregion


        protected override void Add(Tick tick)
        {
            this.openTime.Add(tick.Time);
            this.bid.Add(tick.Bid);
            this.ask.Add(tick.Ask);
        }

        public override string DataPointName { get { return "ticks"; } }

        
    }
}
