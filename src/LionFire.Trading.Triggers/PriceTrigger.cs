using LionFire.Execution;
using LionFire.Instantiating;
using LionFire.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace LionFire.Trading.Triggers
{
    

    public class MarketTrigger : ITemplateInstance<TMarketTriggerBase>, IValidatable
    {
        [Dependency]
        public IAccount Account { get; set; }
        public TMarketTriggerBase Template { get; set; }
        ITemplate ITemplateInstance.Template { get => Template; set => Template = (TMarketTriggerBase)value; }

        

        [Validate]
        public void HasSymbol(ValidationContext ctx)
        {
            foreach (var sym in Template.Symbols)
            {
                if (!Account.SymbolsAvailable.Contains(sym))
                {
                    ctx.AddIssue($"Account does not provide symbol '{sym}'");
                }
            }
        }
    }

    public class PriceTrigger : IStartable, IInitializable2
    {

        [Dependency]
        public IAccount Account { get; set; }

        public Task<ValidationContext> Initialize()
        {
            new ValidationContext
                .Dep
        }

        public Task Start()
        {
            throw new NotImplementedException();
        }


    }
}
