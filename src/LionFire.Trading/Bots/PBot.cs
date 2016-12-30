//using LionFire.Execution;
//using LionFire.Templating;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace LionFire.Trading.Bots
//{
//    public interface ITemplateParameterization<TemplateType,InstanceType>
//        //where InstanceType : class, new()
//    {
//        TemplateType Template { get; set; }
//    }

//    public class ParameterBase<TemplateType, InstanceType> : ITemplateParameterization<TemplateType, InstanceType>
//    {
//        public TemplateType Template { get; set; }

//        public InstanceType Create
//    }

//    public class PBot : ParameterBase<TBot, IBot>, IControllableExecutable
//    {
         

//        public ExecutionState DesiredState { get; set; }
//    }
//}
