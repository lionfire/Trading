using LionFire.Execution;
using LionFire.Instantiating;
using LionFire.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace LionFire.Trading.Triggers
{
    //public class TMarketTrigger : TMarketTriggerBase<MarketTrigger>
    //{
    //    public override IEnumerable<string> Symbols { get; set; }
    //}

    public abstract class MarketTrigger<TemplateType, InstanceType> : ITemplateInstance<TemplateType>, IValidatable
        where InstanceType : class, new()
        where TemplateType : TMarketTriggerBase<InstanceType>, ITemplate
    {
        [Dependency]
        public IFeed_Old Feed { get; set; }
        public TemplateType Template { get; set; }
        //ITemplate ITemplateInstance.Template { get => Template; set => Template = (TemplateType)value; }


        [Validate]
        public void HasSymbol(ValidationContext ctx)
        {
            foreach (var sym in Template.Symbols)
            {
                if (!Feed.SymbolsAvailable.Contains(sym))
                {
                    ctx.AddIssue($"Account does not provide symbol '{sym}'");
                }
            }
        }

        public ValidationContext Validate(ValidationContext validationContext)
        {
            return validationContext;
        }

        public void Validate(bool throwMany = true) => throw new NotImplementedException();
    }

    public class PriceTrigger : MarketTrigger<TPriceTrigger, PriceTrigger>, IStartable, IInitializable2
    {

        [Dependency]
        public IAccount_Old Account { get; set; }

        public Task<ValidationContext> Initialize()
        {
            var vc = new ValidationContext();
            return Task.FromResult<ValidationContext>(vc);
        }

        public Task Start()
        {
            throw new NotImplementedException();
        }

        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
