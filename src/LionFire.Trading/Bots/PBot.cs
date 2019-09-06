using LionFire.Assets;
using System.Text;
using System.Reflection;
using LionFire.Execution;
using LionFire.Instantiating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using LionFire.Trading.Backtesting;
using LionFire.Threading;

namespace LionFire.Trading.Bots
{
    public class PBot : IInstantiator
    {
        #region Construction

        public PBot() { }
        public PBot(object target)
        {
            IBot bot = (IBot)target;

            Id = bot.Template.Id;
            TypeName = bot.Template.GetType().FullName;
            //DesiredExecutionStateEx = bot.DesiredExecutionStateEx;
        }
        public PBot(string id)
        {
            this.Id = id;
        }
        public static implicit operator PBot(string id)
        {
            return new PBot(id);
        }

        #endregion

        public string Id { get; set; }
        public string TypeName { get; set; }
        //public ExecutionStateEx DesiredExecutionStateEx { get; set; }

        //public BotMode Modes { get; set; }
        //    //public BotMode ModesEnabled { get; set; }

        public object Affect(object obj, InstantiationContext context = null)
        {
            if (string.IsNullOrWhiteSpace(Id)) throw new ArgumentNullException(nameof(Id));

            var tBot = (TBot)Id.Load(TypeName, context);

            if (tBot == null && !String.IsNullOrWhiteSpace(TypeName))
            {
                foreach (var assetName in  $"*id={Id}*".Find<BacktestResult>().GetResultSafe())
                {
                    var backtestResult = assetName.Load<BacktestResult>().Result;
                    tBot = (TBot)((JObject)backtestResult.Config).ToObject(TypeResolver.Resolve(TypeName, context.TypeNaming));
                    var bot = tBot.Create();
                    return bot;
                    //var bot = _AddBotForMode(br, mode, pBot); OLD
                }
            }

            if (tBot == null)
            {
                throw new NotFoundException(this.ToXamlAttribute());
            }

            return null;
        }
    }

}
