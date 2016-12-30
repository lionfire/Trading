using LionFire.Assets;
using LionFire.Execution;
using LionFire.Templating;
using LionFire.Trading.Bots;
using LionFire.Trading.Proprietary.Bots;
using LionFire.Trading.Spotware.Connect;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using LionFire.Reactive;
using LionFire.Reactive.Subjects;

namespace LionFire.Trading
{

    public class TypeResolverTemp : ITypeResolver
    {
        public static void Register()
        {
            LionFire.Structures.ManualSingleton<ITypeResolver>.Instance = new TypeResolverTemp();
        }

        public  Type GetTemplateType(string type)
        {
            if (type == "LionTrender") { return typeof(TLionTrender); }
            return null;
        }
        public  Type GetType(string type)
        {
            if (type == "LionTrender") { return typeof(LionTrender); }
            return null;
        }

        public  IAccount CreateAccount(string name)
        {
            var tAccount = name.Load<TCTraderAccount>();
            if (tAccount == null) return null;
            var account = tAccount.Create();
            return account;
        }
    }


}
